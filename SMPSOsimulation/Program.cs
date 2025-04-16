using System.Configuration;
using SMPSOsimulation.dataStructures;

namespace SMPSOsimulation
{
    internal static class Program
    {
        public static void Main(string[] _)
        {
            var env = new EnvironmentConfig(
                maxInstructions: 2000000,
                fastForwardInstructions: 1000000,
                fetchSpeed: 1,
                issueWrongPath: true,
                issueExecDelay: 8,
                cacheDl1Latency: 2,
                cacheDl2Latency: 4,
                cacheIl1Latency: 2,
                cacheIl2Latency: 4,
                cacheFlushOnSyscall: false,
                cacheInstructionCompress: true,
                memLatency: new MemLatencyConfig(
                    firstChunkLatency: 8,
                    interChunkLatency: 8
                ),
                tlbMissLatency: 8,
                fetchRenameDelay: 16,
                renameDispatchDelay: 16
            );

            var cfg = new SearchConfigSMPSO(
                swarmSize: 10,
                archiveSize: 10,
                maxGenerations: 10,
                turbulenceRate: 0.1,
                environment: env
            );

            var simExec = "/home/cepoiuradu/currentstorage/Documents/Projects/SimpleSimSMPSO/m-sim_v2.0/sim-outorder";
            var benchmark = "/home/cepoiuradu/currentstorage/Documents/Projects/SimpleSimSMPSO/m-sim_v2.0/bzip2.arg";

            var runner = new SMPSOOrchestrator();
            runner.GenerationChanged += OnGenerationChanged;
            var algorithmThread = new Thread(new ThreadStart(() => runner.StartSearch(cfg, simExec, [benchmark])));
            algorithmThread.Start();

            algorithmThread.Join();
        }

        private static void OnGenerationChanged(object? sender, List<(CPUConfig, double[])> leaders)
        {
            Console.WriteLine("A aparut generatie noua. Liderii sunt:");
            foreach (var leader in leaders) {
                Console.WriteLine($"{leader.Item2[0]} {leader.Item2[1]}");
            }
            Console.WriteLine("--------------");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}

