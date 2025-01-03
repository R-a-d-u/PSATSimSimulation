using SMPSOsimulation.dataStructures;

namespace SMPSOsimulation;

public class ResultsProvider(EnvironmentConfig env, string psatsimExePath, string gtkBinPath, string tracePath)
{
    private readonly EnvironmentConfig env = env;
    private readonly ResultsDB cachedb = new();
    private readonly PSAtSimWrapper psatsim = new(psatsimExePath, gtkBinPath, tracePath);

    private static bool BreaksConstraints(CPUConfig config)
    {
        if (config.RsbArchitecture == RsbArchitectureType.distributed && config.Load != config.Store)
            return true;
        return false;
    }

    public double[][] Evaluate(List<CPUConfig> configs)
    {
        double[][] results = new double[configs.Count][];
        List<Tuple<CPUConfig, int>> toEvaluate = [];

        for (int i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            var hash = config.CalculateSha256();
            var envHash = env.CalculateSha256();
            var dbhit = cachedb.GetEntry(hash, envHash);
            if (dbhit is not null)
            {
                results[i] = [dbhit.Value.CPI, dbhit.Value.Energy];
            }
            else if (BreaksConstraints(config))
            {
                results[i] = [double.PositiveInfinity, double.PositiveInfinity];
            }
            else
            {
                toEvaluate.Add(new Tuple<CPUConfig, int>(config, i));
            }
        }

        if (toEvaluate.Count > 0)
        {
            var newEvaluations = psatsim.Evaluate(toEvaluate, env);

            foreach (var eval in newEvaluations)
            {
                results[eval.Item2] = eval.Item1;
                var hash = configs[eval.Item2].CalculateSha256();
                var envHash = env.CalculateSha256();
                cachedb.AddEntry(hash, envHash, eval.Item1[0], eval.Item1[1]);
            }
        }

        return results;
    }
}