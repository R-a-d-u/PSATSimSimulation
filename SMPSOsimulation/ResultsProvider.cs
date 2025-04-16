using SMPSOsimulation.dataStructures;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace SMPSOsimulation;

public class ResultsProvider
{
    private readonly EnvironmentConfig env;
    private readonly string envHash;
    private readonly ResultsDB cachedb = new();
    private readonly string simOutorderExePath;
    private readonly List<string> tracePaths;
    private readonly List<string> traceHashes;

    public ResultsProvider(EnvironmentConfig env, string simOutorderExePath, List<string> tracePaths)
    {
        this.env = env;
        this.envHash = env.CalculateSha256();
        this.simOutorderExePath = simOutorderExePath;
        this.tracePaths = tracePaths;
        this.traceHashes = new List<string>();
        foreach (string path in tracePaths)
        {
            if (File.Exists(path)) // Ensure the file exists
            {
                using (FileStream stream = File.OpenRead(path))
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    traceHashes.Add(hashString);
                }
            }
            else
            {
                throw new Exception("Nam gasit traceul ala gen");
            }
        }
    }

    private static bool BreaksConstraints(CPUConfig config)
    {
        if (config.CacheIl1.BlockOrPageSize.GetValueInt() > config.CacheIl2.BlockOrPageSize.GetValueInt()) return true;
        if (config.CacheDl1.BlockOrPageSize.GetValueInt() > config.CacheDl2.BlockOrPageSize.GetValueInt()) return true;
        return false;
    }

    public List<(double CPI, double Energy)> Evaluate(List<CPUConfig> configs)
    {
        List<List<(double CPI, double Energy)>> resultsTraces = new();
        for (int traceIndex =  0; traceIndex < tracePaths.Count; traceIndex++)
        {
            resultsTraces.Add(EvaluateForTrace(configs, (tracePaths[traceIndex], traceHashes[traceIndex])));
        }

        int innerListSize = resultsTraces[0].Count; // Assuming all inner lists have the same size
        int numberOfLists = resultsTraces.Count;
        List<(double CPI, double Energy)> means = new List<(double CPI, double Energy)>();

        for (int j = 0; j < innerListSize; j++)
        {
            double totalCPI = 0.0;
            double totalEnergy = 0.0;

            foreach (var innerList in resultsTraces)
            {
                totalCPI += innerList[j].CPI;
                totalEnergy += innerList[j].Energy;
            }

            means.Add((totalCPI / numberOfLists, totalEnergy / numberOfLists));
        }

        return means;
    }

    private List<(double CPI, double Energy)> EvaluateForTrace(List<CPUConfig> configs, (string path, string hash) trace)
    {
        List<(double CPI, double Energy)> results = Enumerable.Repeat((0.0, 0.0), configs.Count).ToList();
        List<(CPUConfig config, int originIndex)> toEvaluate = [];
        var simoutorder = new SimOutorderWrapper(simOutorderExePath, trace.path);

        for (int i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            var hash = config.CalculateSha256();
            var dbhit = cachedb.GetEntry(hash, envHash, trace.hash);
            if (dbhit is not null)
            {
                results[i] = (dbhit.Value.CPI, dbhit.Value.Energy);
            }
            else if (BreaksConstraints(config))
            {
                results[i] = (double.PositiveInfinity, double.PositiveInfinity);
            }
            else
            {
                toEvaluate.Add((config, i));
            }
        }

        if (toEvaluate.Count > 0)
        {
            var newEvaluations = simoutorder.Evaluate(toEvaluate, env);

            foreach (var eval in newEvaluations)
            {
                results[eval.originalIndex] = (eval.cpi, eval.energy);
                var hash = configs[eval.originalIndex].CalculateSha256();
                cachedb.AddEntry(hash, envHash, trace.hash, eval.cpi, eval.energy);
            }
        }

        return results;
    }
}