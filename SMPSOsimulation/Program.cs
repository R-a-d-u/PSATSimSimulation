
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
            trace: @"C:\Users\Ionut\IdeaProjects\DesignSpaceExplorer\Traces\compress.tra"
        );

        // Create a list to hold CPU configurations
        List<Tuple<CPUConfig, int>> cpuConfigs = new List<Tuple<CPUConfig, int>>();

        // Create CPU configurations based on the provided XML data
        cpuConfigs.Add(new Tuple<CPUConfig, int>(new CPUConfig(15, 353, 468, RsbArchitectureType.distributed, true, 6, 7, 4, 3, 6, 6, 4, 2, 8, 8, 600), 0));
        cpuConfigs.Add(new Tuple<CPUConfig, int>(new CPUConfig(11, 205, 73, RsbArchitectureType.distributed, true, 7, 6, 5, 6, 5, 7, 1, 7, 7, 7, 600), 1));
        cpuConfigs.Add(new Tuple<CPUConfig, int>(new CPUConfig(7, 438, 49, RsbArchitectureType.distributed, false, 3, 7, 2, 6, 6, 8, 1, 7, 8, 8, 600), 2));
        cpuConfigs.Add(new Tuple<CPUConfig, int>(new CPUConfig(5, 51, 23, RsbArchitectureType.hybrid, true, 5, 4, 1, 7, 1, 3, 7, 6, 2, 2, 600), 3));

        // Initialize PSAtSimWrapper with paths to the executable and DLL
        string exePath = @"path\to\psatsim_con.exe"; // Replace with actual path
        string dllPath = @"E:\PSATSIM2\GTK\bin"; // Replace with actual path
        PSAtSimWrapper psatSimWrapper = new PSAtSimWrapper(exePath, dllPath);

        // Evaluate the configurations and get results
        List<Tuple<double[], int>> results = psatSimWrapper.Evaluate(cpuConfigs, environmentConfig);

        // Display the results
        foreach (var result in results)
        {
            double[] metrics = result.Item1;
            int configIndex = result.Item2;
            Console.WriteLine($"Configuration {configIndex}: IPC = {metrics[0]}, Energy = {metrics[1]}");
        }
    }
    }
}

