using SMPSOsimulation.dataStructures;

namespace SMPSOsimulation;

public class ResultsProvider
{
    private EnvironmentConfig env;
    private ResultsDB cachedb;
    private PSAtSimWrapper psatsim;

    public ResultsProvider(EnvironmentConfig env, string psatsimExePath, string gtkBinPath)
    {
        this.env = env;
        this.cachedb = new ResultsDB();
        this.psatsim = new PSAtSimWrapper(psatsimExePath, gtkBinPath);
    }

    private bool BreaksConstraints(CPUConfig config)
    {
        if (config.RsbArchitecture == RsbArchitectureType.distributed && config.Load != config.Store)
            return true;
        return false;
    }

    public double[][] Evaluate(List<CPUConfig> configs)
    {
        double[][] results = new double[configs.Count][];
        List<Tuple<CPUConfig, int>> toEvaluate = new List<Tuple<CPUConfig, int>>();

        for (int i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            var hash = config.CalculateSha256();
            var dbhit = cachedb.GetEntry(hash);
            if (dbhit is not null)
            {
                results[i] = [dbhit.Value.CPI, dbhit.Value.Energy];
            }
            else if(BreaksConstraints(config))
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
                cachedb.AddEntry(hash, eval.Item1[0], eval.Item1[1]);
            }
        }

        return results;
    }
}