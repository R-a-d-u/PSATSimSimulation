
using System.Diagnostics;
using SMPSOsimulation.dataStructures;

namespace SMPSOsimulation
{
    internal static class Program
    {
		 public static void Main(string[] args)
    {
        // Create the environment configuration
        EnvironmentConfig environmentConfig = new EnvironmentConfig(
            vdd: 2.2,
            memoryArch: MemoryArchEnum.system,
            l1CodeHitrate: 0.8860734360429738f,
            l1CodeLatency: 88,
            l1DataHitrate: 0.5771835586482472f,
            l1DataLatency: 30,
            l2Hitrate: 0.3713664058038628f,
            l2Latency: 53,
            systemMemLatency: 39,
            trace: @"C:\PSATSim\Traces\compress.tra",
            maxFrequency: 10000
        );

        // Create a list to hold CPU configurations
        List<CPUConfig> cpuConfigs = new List<CPUConfig>();

        // Create CPU configurations based on the provided XML data
        cpuConfigs.Add(new CPUConfig(15, 353, 468, RsbArchitectureType.distributed, true, 6, 7, 4, 3, 6, 6, 4, 2, 8, 8, 700));
        cpuConfigs.Add(new CPUConfig(11, 205, 73, RsbArchitectureType.distributed, true, 7, 6, 5, 6, 5, 7, 1, 7, 7, 7, 700));
        cpuConfigs.Add(new CPUConfig(7, 438, 49, RsbArchitectureType.distributed, false, 3, 7, 2, 6, 6, 8, 1, 7, 8, 8, 700));
        cpuConfigs.Add(new CPUConfig(5, 51, 23, RsbArchitectureType.hybrid, true, 5, 4, 1, 7, 1, 3, 7, 6, 2, 2, 700));

        // Initialize PSAtSimWrapper with paths to the executable and DLL
        string exePath = @"C:\PSATSim\psatsim_con.exe"; // Replace with actual path
        string dllPath = @"C:\GTK\bin"; // Replace with actual path
        ResultsProvider resultsProvider = new ResultsProvider(environmentConfig, exePath, dllPath);
        
        // Evaluate the configurations and get results
        Stopwatch stopwatch = Stopwatch.StartNew();
        double[][] results = resultsProvider.Evaluate(cpuConfigs);
        stopwatch.Stop();
            Console.WriteLine($"Elapsed {stopwatch.ElapsedMilliseconds}ms");


            // Display the results
            for (int i = 0; i < results.Length; i++)
        {
            Console.WriteLine($"Configuration {i}: IPC = {1 / results[i][0]}, Energy = {results[i][1]}");
        }
    }
    }
}

