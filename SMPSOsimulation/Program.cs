using System.Configuration;
using SMPSOsimulation.dataStructures;

namespace SMPSOsimulation
{
    internal static class Program
    {
        public static void Main(string[] _)
        {
            var cpuconf = new CPUConfig(
                branchPredictorType: BranchPredictorTypeEnum.twolev,
                bpredBimodTableSize: 128,
                bpred2LevConfig: new Predictor2LevConfig(
                    l1: 64,
                    l2: 64,
                    historySize: 8,
                    useXor: false
                ),
                bpredCombMetaTableSize: 128,
                bpredReturnAddressStackSize: 32,
                bpredBtbConfig: new BtbConfig(
                    numSets: 8,
                    associativity: 2
                ),
                bpredSpeculativeUpdate: SpeculativePredictorUpdateStageEnum.ID,
                cacheLoadPredictorType: CacheLoadPredictorTypeEnum.taken,
                cpredBimodTableSize: 64,
                cpred2LevConfig: new Predictor2LevConfig(
                    l1: 128,
                    l2: 128,
                    historySize: 4,
                    useXor: true
                ),
                cpredCombMetaTableSize: 120,
                cpredReturnAddressStackSize: 30,
                cpredBtbConfig: new BtbConfig(
                    numSets: 10,
                    associativity: 3
                ),
                decodeWidth: 32,
                issueWidth: 16,
                issueInOrder: false,
                commitWidth: 8,
                reorderBufferSize: 64,
                issueQueueSize: 16,
                registerFileSize: 32,
                loadStoreQueueSize: 32,
                cacheDl1: new CacheTlbConfig(
                    name: "dl1",
                    numSets: 10,
                    blockSize: 32,
                    associativity: 4,
                    replacementPolicy: ReplacementPolicyEnum.f
                ),
                cacheDl2: new CacheTlbConfig(
                    name: "dl2",
                    numSets: 6,
                    blockSize: 64,
                    associativity: 8,
                    replacementPolicy: ReplacementPolicyEnum.r
                ),
                cacheIl1: new CacheTlbConfig(
                    name: "il1",
                    numSets: 6,
                    blockSize: 64,
                    associativity: 8,
                    replacementPolicy: ReplacementPolicyEnum.r
                ),
                cacheIl2: new CacheTlbConfig(
                    name: "il1",
                    numSets: 6,
                    blockSize: 64,
                    associativity: 8,
                    replacementPolicy: ReplacementPolicyEnum.r
                ),
                tlbItlb: new CacheTlbConfig(
                    name: "itlb",
                    numSets: 6,
                    blockSize: 64,
                    associativity: 8,
                    replacementPolicy: ReplacementPolicyEnum.r
                ),
                tlbDtlb: new CacheTlbConfig(
                    name: "dl1",
                    numSets: 10,
                    blockSize: 32,
                    associativity: 4,
                    replacementPolicy: ReplacementPolicyEnum.l
                ),
                memBusWidth: 120,
                resIntegerAlu: 10,
                resIntegerMultDiv: 11,
                resMemoryPorts: 12,
                resFpAlu: 13,
                resFpMultDiv: 14
            );

            var env = new EnvironmentConfig(
                maxInstructions: 50000,
                fastForwardInstructions: 1000,
                fetchSpeed: 1,
                issueWrongPath: true,
                issueExecDelay: 6,
                cacheDl1Latency: 7,
                cacheDl2Latency: 8,
                cacheIl1Latency: 9,
                cacheIl2Latency: 10,
                cacheFlushOnSyscall: false,
                cacheInstructionCompress: true,
                memLatency: new MemLatencyConfig(
                    firstChunkLatency: 11,
                    interChunkLatency: 12
                ),
                tlbMissLatency: 13,
                fetchRenameDelay: 14,
                renameDispatchDelay: 15
            );

            var runner = new SimOutorderWrapper("/home/cepoiuradu/currentstorage/Documents/Projects/SimpleSimSMPSO/m-sim_v2.0/sim-outorder", "/home/cepoiuradu/currentstorage/Documents/Projects/SimpleSimSMPSO/m-sim_v2.0/bzip2.arg");
            var results = runner.Evaluate([(cpuconf, 14)], env);
            Console.WriteLine($"cpi {results[0].cpi} | energy {results[0].energy} | position {results[0].originalIndex}");
        }
    }
}

