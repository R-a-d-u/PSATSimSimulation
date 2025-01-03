
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


            // Create CPU configurations based on the provided XML data

            // Initialize PSAtSimWrapper with paths to the executable and DLL
            string exePath = @"C:\PSATSim\psatsim_con.exe"; // Replace with actual path
            string dllPath = @"C:\GTK\bin"; // Replace with actual path


            var orchestrator = new SMPSOOrchestrator();
            var domination =
                DominationConfig.GetSMPSODominationConfig();
            var searchConfig = new SearchConfigSMPSO(300, 7, 100, 0.2, environmentConfig, domination);

            orchestrator.StartSearch(searchConfig, exePath, dllPath);
        }
    }
}

