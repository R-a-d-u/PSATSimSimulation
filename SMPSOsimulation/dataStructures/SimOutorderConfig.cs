using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

/// <summary>
/// Represents the configuration for a cache or TLB structure.
/// </summary>
public class CacheTlbConfig : IEquatable<CacheTlbConfig>
{
    /// <summary>
    /// Name of the cache/TLB being defined.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Number of sets in the cache/TLB.
    /// </summary>
    public IntPowerOf2 NumSets { get; init; }

    /// <summary>
    /// Block size (cache) or page size (TLB) in bytes.
    /// </summary>
    public IntPowerOf2 BlockOrPageSize { get; init; }

    /// <summary>
    /// Associativity of the cache/TLB.
    /// </summary>
    public IntPowerOf2 Associativity { get; init; }

    /// <summary>
    /// Block replacement strategy: 'l' (LRU), 'f' (FIFO), 'r' (Random).
    /// </summary>
    public ReplacementPolicyEnum ReplacementPolicy { get; init; } // Could be an enum

    public CacheTlbConfig(string name, IntPowerOf2 numSets, IntPowerOf2 blockSize, IntPowerOf2 associativity, ReplacementPolicyEnum replacementPolicy) {
        Name = name;
        NumSets = numSets;
        BlockOrPageSize = blockSize;
        Associativity = associativity;
        ReplacementPolicy = replacementPolicy;
    }

    public override string ToString()
    {
        return $"{Name}:{NumSets}:{BlockOrPageSize}:{Associativity}:{ReplacementPolicy.ToString()}";
    }

    // --- IEquatable and Hashing ---

    public bool Equals(CacheTlbConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name &&
               NumSets == other.NumSets &&
               BlockOrPageSize == other.BlockOrPageSize &&
               Associativity == other.Associativity &&
               ReplacementPolicy == other.ReplacementPolicy;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CacheTlbConfig);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, NumSets, BlockOrPageSize, Associativity, ReplacementPolicy);
    }

    public static bool operator ==(CacheTlbConfig? left, CacheTlbConfig? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(CacheTlbConfig? left, CacheTlbConfig? right)
    {
        return !(left == right);
    }

}

/// <summary>
/// Represents the configuration for a 2-level branch/cache predictor.
/// </summary>
public class Predictor2LevConfig : IEquatable<Predictor2LevConfig>
{
    /// <summary>
    /// Level 1 table size.
    /// </summary>
    public IntPowerOf2 L1Size { get; init; }

    /// <summary>
    /// Level 2 table size.
    /// </summary>
    public IntPowerOf2 L2Size { get; init; }

    /// <summary>
    /// History register size (in bits).
    /// </summary>
    public IntPowerOf2 HistorySize { get; init; }

    /// <summary>
    /// Whether to use XOR folding for history (true) or not (false).
    /// Corresponds to the <xor> parameter (0 or non-zero).
    /// </summary>
    public bool UseXor { get; init; }

    public Predictor2LevConfig(IntPowerOf2 l1, IntPowerOf2 l2, IntPowerOf2 historySize, bool useXor) {
        L1Size = l1;
        L2Size = l2;
        HistorySize = historySize;
        UseXor = useXor;
    }

    public override string ToString()
    {
        return $"{L1Size.ToString()} {L2Size.ToString()} {HistorySize.ToString()} {(UseXor ? 1 : 0)}";
    }

    // --- IEquatable and Hashing ---

    public bool Equals(Predictor2LevConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return L1Size == other.L1Size &&
               L2Size == other.L2Size &&
               HistorySize == other.HistorySize &&
               UseXor == other.UseXor;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Predictor2LevConfig);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(L1Size, L2Size, HistorySize, UseXor);
    }

     public static bool operator ==(Predictor2LevConfig? left, Predictor2LevConfig? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Predictor2LevConfig? left, Predictor2LevConfig? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Represents the configuration for a Branch Target Buffer (BTB).
/// </summary>
public class BtbConfig : IEquatable<BtbConfig>
{
    /// <summary>
    /// Number of sets in the BTB.
    /// </summary>
    public IntPowerOf2 NumSets { get; init; }

    /// <summary>
    /// Associativity of the BTB.
    /// </summary>
    public IntPowerOf2 Associativity { get; init; }

    public BtbConfig(IntPowerOf2 numSets, IntPowerOf2 associativity) {
        NumSets = numSets;
        Associativity = associativity;
    }

    public override string ToString()
    {
        return $"{NumSets} {Associativity}";
    }

    // --- IEquatable and Hashing ---

    public bool Equals(BtbConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return NumSets == other.NumSets &&
               Associativity == other.Associativity;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as BtbConfig);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NumSets, Associativity);
    }

    public static bool operator ==(BtbConfig? left, BtbConfig? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(BtbConfig? left, BtbConfig? right)
    {
        return !(left == right);
    }
}


/// <summary>
/// Represents the memory access latency configuration.
/// </summary>
public class MemLatencyConfig : IEquatable<MemLatencyConfig>
{
    /// <summary>
    /// Latency for the first chunk accessed.
    /// </summary>
    public int FirstChunkLatency { get; init; }

    /// <summary>
    /// Latency for subsequent chunks accessed within the same request.
    /// </summary>
    public int InterChunkLatency { get; init; }

    public MemLatencyConfig(int firstChunkLatency, int interChunkLatency) {
        FirstChunkLatency = firstChunkLatency;
        InterChunkLatency = interChunkLatency;
    }

    public override string ToString()
    {
        return $"{FirstChunkLatency} {InterChunkLatency}";
    }

    // --- IEquatable and Hashing ---

    public bool Equals(MemLatencyConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return FirstChunkLatency == other.FirstChunkLatency &&
               InterChunkLatency == other.InterChunkLatency;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as MemLatencyConfig);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FirstChunkLatency, InterChunkLatency);
    }

    public static bool operator ==(MemLatencyConfig? left, MemLatencyConfig? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(MemLatencyConfig? left, MemLatencyConfig? right)
    {
        return !(left == right);
    }
}


/// <summary>
/// Represents the configuration options for the M-Sim sim-outorder simulator.
/// Null properties indicate that the parameter should not be included in the
/// command line, allowing the simulator to use its default value.
/// </summary>
public class SimOutorderConfig : IEquatable<SimOutorderConfig>
{
    // --- General Options ---

    /// <summary>
    /// load configuration from a file
    /// </summary>
    public string? ConfigFile { get; set; } // -config <string>

    /// <summary>
    /// dump configuration to a file
    /// </summary>
    public string? DumpConfigFile { get; set; } // -dumpconfig <string>

    /// <summary>
    /// print help message
    /// </summary>
    public bool? PrintHelp { get; set; } // -h <true|false>

    /// <summary>
    /// verbose operation
    /// </summary>
    public bool? Verbose { get; set; } // -v <true|false>

    /// <summary>
    /// enable debug message
    /// </summary>
    public bool? Debug { get; set; } // -d <true|false>

    /// <summary>
    /// start in Dlite debugger
    /// </summary>
    public bool? StartDebugger { get; set; } // -i <true|false>

    /// <summary>
    /// random number generator seed (0 for timer seed)
    /// </summary>
    public int? RandomSeed { get; set; } // -seed <int> (0 for timer seed)

    /// <summary>
    /// initialize and terminate immediately
    /// </summary>
    public bool? InitTerminate { get; set; } // -q <true|false>

    /// <summary>
    /// redirect simulator output to file (non-interactive only)
    /// </summary>
    public string? RedirectSimOutput { get; set; } // -redir:sim <string>

    /// <summary>
    /// redirect simulated program output to file
    /// </summary>
    public string? RedirectProgOutput { get; set; } // -redir:prog <string>

    /// <summary>
    /// simulator scheduling priority
    /// </summary>
    public int? NicePriority { get; set; } // -nice <int>

    // --- Execution Control ---

    /// <summary>
    /// maximum number of inst's to execute
    /// </summary>
    public ulong? MaxInstructions { get; set; } // -max:inst <uint> (Using ulong for <uint>)

    /// <summary>
    /// number of insts skipped before timing starts
    /// </summary>
    public ulong? FastForwardInstructions { get; set; } // -fastfwd <int> (Using ulong as count is non-negative)
    /// <summary>
    /// List of pipetrace configurations. Each string should be formatted as
    /// "<fname|stdout|stderr> <range>". Can be specified multiple times.
    /// Example entry: "stdout 0:10000"
    /// </summary>
    public List<string>? PTrace { get; set; } // -ptrace <string list...>

    // --- Frontend ---

    /// <summary>
    /// speed of front-end of machine relative to execution core
    /// </summary>
    public int? FetchSpeed { get; set; } // -fetch:speed <int>

    /// <summary> Fetch policy ("icount", "round_robin"). </summary>
    public FetchPolicyEnum? FetchPolicy { get; set; } // -fetch:policy <string>

    /// <summary> Number of cycles between fetch and rename stages. </summary>
    public int? FetchRenameDelay { get; set; } // -fetch_rename_delay <int>

    // --- Branch Predictor ---

    // TODO
    /// <summary> Branch predictor type ("nottaken", "taken", "perfect", "bimod", "2lev", "comb"). </summary>
    public BranchPredictorTypeEnum? BranchPredictorType { get; set; } // -bpred <string>

    /// <summary>
    /// bimodal predictor config ("<table size>")
    /// </summary>
    public IntPowerOf2? BpredBimodTableSize { get; set; } // -bpred:bimod <int>

    /// <summary>
    /// 2-level predictor config (<l1size> <l2size> <hist_size> <xor>)
    /// </summary>
    public Predictor2LevConfig? Bpred2LevConfig { get; set; } // -bpred:2lev <int list...>

    /// <summary>
    /// combining predictor config (<meta_table_size>)
    /// </summary>
    public IntPowerOf2? BpredCombMetaTableSize { get; set; } // -bpred:comb <int>

    /// <summary>
    /// return address stack size (0 for no return stack)
    /// </summary>
    public IntPowerOf2? BpredReturnAddressStackSize { get; set; } // -bpred:ras <int>

    /// <summary>
    /// BTB config (<num_sets> <associativity>)
    /// </summary>
    public BtbConfig? BpredBtbConfig { get; set; } // -bpred:btb <int list...>

    /// <summary> Speculative predictor update stage ("ID", "WB", or null for non-speculative). </summary>
    public SpeculativePredictorUpdateStageEnum? BpredSpeculativeUpdate { get; set; } // -bpred:spec_update <string>

    // --- Cache Load-Latency Predictor (cpred) ---

    /// <summary> Cache load-latency predictor type ("nottaken", "taken", "perfect", "bimod", "2lev", "comb"). </summary>
    public CacheLoadPredictorTypeEnum? CacheLoadPredictorType { get; set; } // -cpred <string>

    /// <summary>
    /// cache load-latency bimodal predictor config (<table size>)
    /// </summary>
    public IntPowerOf2? CpredBimodTableSize { get; set; } // -cpred:bimod <int>

    /// <summary>
    /// cache load-latency 2-level predictor config (<l1size> <l2size> <hist_size> <xor>)
    /// </summary>
    public Predictor2LevConfig? Cpred2LevConfig { get; set; } // -cpred:2lev <int list...>

    /// <summary>
    /// cache load-latency combining predictor config (<meta_table_size>)
    /// </summary>
    public IntPowerOf2? CpredCombMetaTableSize { get; set; } // -cpred:comb <int>

    /// <summary>
    /// return address stack size (0 for no return stack)
    /// </summary>
    public IntPowerOf2? CpredReturnAddressStackSize { get; set; } // -cpred:ras <int> (Note: Default is 0)

    /// <summary>
    /// cache load-latency BTB config (<num_sets> <associativity>)
    /// </summary>
    public BtbConfig? CpredBtbConfig { get; set; } // -cpred:btb <int list...>

    // --- Pipeline Widths & Core ---

    /// <summary>
    /// instruction decode B/W (insts/cycle)
    /// </summary>
    public IntPowerOf2? DecodeWidth { get; set; } // -decode:width <int>

    /// <summary>
    /// instruction issue B/W (insts/cycle)
    /// </summary>
    public IntPowerOf2? IssueWidth { get; set; } // -issue:width <int>

    /// <summary>
    /// run pipeline with in-order issue
    /// </summary>
    public bool? IssueInOrder { get; set; } // -issue:inorder <true|false>

    /// <summary>
    /// issue instructions down wrong execution paths
    /// </summary>
    public bool? IssueWrongPath { get; set; } // -issue:wrongpath <true|false>

    /// <summary>
    /// instruction commit B/W (insts/cycle)
    /// </summary>
    public int? CommitWidth { get; set; } // -commit:width <int>
    /// <summary> Number cycles between rename and dispatch stages. </summary>
    public int? RenameDispatchDelay { get; set; } // -rename_dispatch_delay <int>
    /// <summary> Minimum cycles between issue and execution. </summary>
    public int? IssueExecDelay { get; set; } // -iq:issue_exec_delay <int>

    // --- Queues & Buffers ---
    
    /// <summary>
    /// reorder buffer (ROB) size
    /// </summary>
    public int? ReorderBufferSize { get; set; } // -rob:size <int>

    /// <summary>
    /// issue queue (IQ) size
    /// </summary>
    public int? IssueQueueSize { get; set; } // -iq:size <int>
    
    /// <summary>
    /// register file (RF) size for each the INT and FP physical register file)
    /// </summary>
    public int? RegisterFileSize { get; set; } // -rf:size <int> (Size for *each* INT and FP)

    /// <summary>
    /// load/store queue (LSQ) size
    /// </summary>
    public int? LoadStoreQueueSize { get; set; } // -lsq:size <int>

    // --- Recovery Model ---

    /// <summary> Recovery model ("squash", "perfect"). Alpha squash recovery or perfect predition </summary>
    public RecoveryModelEnum? RecoveryModel { get; set; } // -recovery:model <string>

    // --- Cache Hierarchy ---
    /// <summary> l1 data cache config, i.e., {<config>|none} </summary>
    public CacheTlbConfig? CacheDl1 { get; set; } // -cache:dl1 <string>

    /// <summary>
    /// l1 data cache hit latency (in cycles)
    /// </summary>
    public int? CacheDl1Latency { get; set; } // -cache:dl1lat <int>

    /// <summary> l2 data cache config, i.e., {<config>|none} </summary>
    public CacheTlbConfig? CacheDl2 { get; set; } // -cache:dl2 <string>

    /// <summary>
    /// l2 data cache hit latency (in cycles)
    /// </summary>
    public int? CacheDl2Latency { get; set; } // -cache:dl2lat <int>

    /// <summary> L1 Instruction Cache Config. Can be "none", "dl1", "dl2", or CacheTlbConfig. </summary>
    public CacheTlbConfig? CacheIl1 { get; set; } // -cache:il1 <string>

    /// <summary>
    /// il1 instruction cache hit latency (in cycles)
    /// </summary>
    public int? CacheIl1Latency { get; set; } // -cache:il1lat <int>

    /// <summary> L2 Instruction Cache Config. Can be "none", "dl2", or CacheTlbConfig. </summary>
    public CacheTlbConfig? CacheIl2 { get; set; } // -cache:il2 <string>

    /// <summary>
    /// l2 instruction cache hit latency (in cycles)
    /// </summary>
    public int? CacheIl2Latency { get; set; } // -cache:il2lat <int>

    /// <summary>
    /// flush caches on system calls
    /// </summary>
    public bool? CacheFlushOnSyscall { get; set; } // -cache:flush <true|false>

    /// <summary>
    /// convert 64-bit inst addresses to 32-bit inst equivalents
    /// </summary>
    public bool? CacheInstructionCompress { get; set; } // -cache:icompress <true|false>

    // --- Memory System ---

    /// <summary>
    /// memory access latency (<first_chunk> <inter_chunk>)
    /// </summary>
    public MemLatencyConfig? MemLatency { get; set; } // -mem:lat <int list...>

    /// <summary>
    /// memory access bus width (in bytes)s
    /// </summary>
    public IntPowerOf2? MemBusWidth { get; set; } // -mem:width <int>

    // --- TLB ---
    /// <summary> Instruction TLB Config. Can be "none" or CacheTlbConfig. </summary>
    public CacheTlbConfig? TlbItlb { get; set; } // -tlb:itlb <string>
    /// <summary> Data TLB Config. Can be "none" or CacheTlbConfig. </summary>
    public CacheTlbConfig? TlbDtlb { get; set; } // -tlb:dtlb <string>

    /// <summary>
    /// inst/data TLB miss latency (in cycles)
    /// </summary>
    public int? TlbMissLatency { get; set; } // -tlb:lat <int>

    // --- Functional Units (Resources) ---

    /// <summary>
    /// total number of integer ALU's available
    /// </summary>
    public int? ResIntegerAlu { get; set; } // -res:ialu <int>

    /// <summary>
    /// total number of integer multiplier/dividers available
    /// </summary>
    public int? ResIntegerMultDiv { get; set; } // -res:imult <int>

    /// <summary>
    /// total number of memory system ports available (to CPU)
    /// </summary>
    public int? ResMemoryPorts { get; set; } // -res:memport <int>

    /// <summary>
    /// total number of floating point ALU's available
    /// </summary>
    public int? ResFpAlu { get; set; } // -res:fpalu <int>

    /// <summary>
    /// total number of floating point multiplier/dividers available
    /// </summary>
    public int? ResFpMultDiv { get; set; } // -res:fpmult <int>

    // --- Profiling & Power ---
    /// <summary>
    /// List of PC stats configurations. Each string represents one use of -pcstat.
    /// Format depends on SimpleScalar's stat library expectations.
    /// Example entry: "sim_num_insn"
    /// </summary>
    public List<string>? PcStat { get; set; } // -pcstat <string list...>

    /// <summary>
    /// print power statistics collected by wattch?
    /// </summary>
    public bool? PowerPrintStats { get; set; } // -power:print_stats <true|false>

    public SimOutorderConfig(CPUConfig cpuConfig, EnvironmentConfig environmentConfig)
    {
        // --- General Options ---
        // ConfigFile, DumpConfigFile, PrintHelp, Verbose, Debug, StartDebugger,
        // RandomSeed, InitTerminate, RedirectSimOutput, RedirectProgOutput,
        // NicePriority are not present in CPUConfig or EnvironmentConfig, remain null.
        
        // --- Execution Control ---
        this.MaxInstructions = environmentConfig.MaxInstructions;
        this.FastForwardInstructions = environmentConfig.FastForwardInstructions;
        // PTrace is not present in CPUConfig or EnvironmentConfig, remains null.

        // --- Frontend ---
        this.FetchSpeed = environmentConfig.FetchSpeed;
        this.FetchRenameDelay = environmentConfig.FetchRenameDelay;
        // FetchPolicy, are not present in CPUConfig or EnvironmentConfig, remain null.

        // --- Branch Predictor ---
        this.BranchPredictorType = cpuConfig.BranchPredictorType;
        this.BpredBimodTableSize = cpuConfig.BpredBimodTableSize;
        this.Bpred2LevConfig = cpuConfig.Bpred2LevConfig;
        this.BpredCombMetaTableSize = cpuConfig.BpredCombMetaTableSize;
        this.BpredReturnAddressStackSize = cpuConfig.BpredReturnAddressStackSize;
        this.BpredBtbConfig = cpuConfig.BpredBtbConfig;
        this.BpredSpeculativeUpdate = cpuConfig.BpredSpeculativeUpdate;

        // --- Cache Load-Latency Predictor (cpred) ---
        this.CacheLoadPredictorType = cpuConfig.CacheLoadPredictorType;
        this.CpredBimodTableSize = cpuConfig.CpredBimodTableSize;
        this.Cpred2LevConfig = cpuConfig.Cpred2LevConfig;
        this.CpredCombMetaTableSize = cpuConfig.CpredCombMetaTableSize;
        this.CpredReturnAddressStackSize = cpuConfig.CpredReturnAddressStackSize;
        this.CpredBtbConfig = cpuConfig.CpredBtbConfig;

        // --- Pipeline Widths & Core ---
        this.DecodeWidth = cpuConfig.DecodeWidth;
        this.IssueWidth = cpuConfig.IssueWidth;
        this.IssueInOrder = cpuConfig.IssueInOrder;
        this.IssueWrongPath = environmentConfig.IssueWrongPath;
        this.CommitWidth = cpuConfig.CommitWidth;
        this.RenameDispatchDelay = environmentConfig.RenameDispatchDelay;
        this.IssueExecDelay = environmentConfig.IssueExecDelay;

        // --- Queues & Buffers ---
        this.ReorderBufferSize = cpuConfig.ReorderBufferSize;
        this.IssueQueueSize = cpuConfig.IssueQueueSize;
        this.RegisterFileSize = cpuConfig.RegisterFileSize;
        this.LoadStoreQueueSize = cpuConfig.LoadStoreQueueSize;

        // --- Recovery Model ---
        // RecoveryModel is not present in CPUConfig or EnvironmentConfig, remains null.

        // --- Cache Hierarchy ---
        this.CacheDl1 = cpuConfig.CacheDl1;
        this.CacheDl1Latency = environmentConfig.CacheDl1Latency;
        this.CacheDl2 = cpuConfig.CacheDl2;
        this.CacheDl2Latency = environmentConfig.CacheDl2Latency;
        this.CacheIl1 = cpuConfig.CacheIl1;
        this.CacheIl1Latency = environmentConfig.CacheIl1Latency;
        this.CacheIl2 = cpuConfig.CacheIl2;
        this.CacheIl2Latency = environmentConfig.CacheIl2Latency;
        this.CacheFlushOnSyscall = environmentConfig.CacheFlushOnSyscall;
        this.CacheInstructionCompress = environmentConfig.CacheInstructionCompress;

        // --- Memory System ---
        this.MemLatency = environmentConfig.MemLatency;
        this.MemBusWidth = cpuConfig.MemBusWidth;

        // --- TLB ---
        this.TlbItlb = cpuConfig.TlbItlb;
        this.TlbDtlb = cpuConfig.TlbDtlb;
        this.TlbMissLatency = environmentConfig.TlbMissLatency;

        // --- Functional Units (Resources) ---
        this.ResIntegerAlu = cpuConfig.ResIntegerAlu;
        this.ResIntegerMultDiv = cpuConfig.ResIntegerMultDiv;
        this.ResMemoryPorts = cpuConfig.ResMemoryPorts;
        this.ResFpAlu = cpuConfig.ResFpAlu;
        this.ResFpMultDiv = cpuConfig.ResFpMultDiv;

        // --- Profiling & Power ---
        // PcStat, PowerPrintStats are not present in CPUConfig or EnvironmentConfig, remain null.
    }

    /// <summary>
    /// Generates the command-line argument string corresponding to the configured options.
    /// Only includes options that have been explicitly set (are not null).
    /// </summary>
    /// <returns>A string suitable for passing as arguments to sim-outorder.</returns>
    public string ToCommandLineString()
    {

        var sb = new StringBuilder();

        // Helper to append option if value is not null
        Action<string, object?> appendOpt = (name, value) =>
        {
            if (value != null)
            {
                string valueStr = value switch
                {
                    bool b => b ? "true" : "false",
                    // Use ToString() for mini-classes and primitives
                    _ => value.ToString() ?? string.Empty
                };
                 // Ensure CacheTlbConfig and unified cache strings like "dl1" are handled
                if (value is CacheTlbConfig cfg) valueStr = cfg.ToString();
                else if (value is string s && (s == "dl1" || s == "dl2" || s == "none")) valueStr = s; // Keep predefined strings

                if (valueStr == "twolev") valueStr = "2lev";
                sb.Append(name).Append(' ').Append(valueStr).Append(' ');
            }
        };

        // Helper for list options that can appear multiple times
        Action<string, List<string>?> appendListOpt = (name, list) =>
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                         sb.Append(name).Append(' ').Append(item).Append(' ');
                    }
                }
            }
        };

        // Append all options
        appendOpt("-config", ConfigFile);
        appendOpt("-dumpconfig", DumpConfigFile);
        appendOpt("-h", PrintHelp);
        appendOpt("-v", Verbose);
        appendOpt("-d", Debug);
        appendOpt("-i", StartDebugger);
        appendOpt("-seed", RandomSeed);
        appendOpt("-q", InitTerminate);
        appendOpt("-redir:sim", RedirectSimOutput);
        appendOpt("-redir:prog", RedirectProgOutput);
        appendOpt("-nice", NicePriority);

        appendOpt("-max:inst", MaxInstructions);
        appendOpt("-fastfwd", FastForwardInstructions);
        appendListOpt("-ptrace", PTrace);

        appendOpt("-fetch:speed", FetchSpeed);
        appendOpt("-fetch:policy", FetchPolicy);
        appendOpt("-fetch_rename_delay", FetchRenameDelay);


        appendOpt("-bpred", BranchPredictorType);
        appendOpt("-bpred:bimod", BpredBimodTableSize);
        appendOpt("-bpred:2lev", Bpred2LevConfig); // Uses ToString()
        appendOpt("-bpred:comb", BpredCombMetaTableSize);
        appendOpt("-bpred:ras", BpredReturnAddressStackSize);
        appendOpt("-bpred:btb", BpredBtbConfig); // Uses ToString()
        appendOpt("-bpred:spec_update", BpredSpeculativeUpdate);

        appendOpt("-cpred", CacheLoadPredictorType);
        appendOpt("-cpred:bimod", CpredBimodTableSize);
        appendOpt("-cpred:2lev", Cpred2LevConfig); // Uses ToString()
        appendOpt("-cpred:comb", CpredCombMetaTableSize);
        appendOpt("-cpred:ras", CpredReturnAddressStackSize);
        appendOpt("-cpred:btb", CpredBtbConfig); // Uses ToString()

        appendOpt("-decode:width", DecodeWidth);
        appendOpt("-issue:width", IssueWidth);
        appendOpt("-issue:inorder", IssueInOrder);
        appendOpt("-issue:wrongpath", IssueWrongPath);
        appendOpt("-commit:width", CommitWidth);
        appendOpt("-rename_dispatch_delay", RenameDispatchDelay);
        appendOpt("-iq:issue_exec_delay", IssueExecDelay);


        appendOpt("-rob:size", ReorderBufferSize);
        appendOpt("-iq:size", IssueQueueSize);
        appendOpt("-rf:size", RegisterFileSize);
        appendOpt("-lsq:size", LoadStoreQueueSize);

        appendOpt("-recovery:model", RecoveryModel);

        appendOpt("-cache:dl1", CacheDl1); // Handles string or CacheTlbConfig via its value
        appendOpt("-cache:dl1lat", CacheDl1Latency);
        appendOpt("-cache:dl2", CacheDl2); // Handles string or CacheTlbConfig via its value
        appendOpt("-cache:dl2lat", CacheDl2Latency);
        appendOpt("-cache:il1", CacheIl1); // Handles string or CacheTlbConfig via its value
        appendOpt("-cache:il1lat", CacheIl1Latency);
        appendOpt("-cache:il2", CacheIl2); // Handles string or CacheTlbConfig via its value
        appendOpt("-cache:il2lat", CacheIl2Latency);
        appendOpt("-cache:flush", CacheFlushOnSyscall);
        appendOpt("-cache:icompress", CacheInstructionCompress);

        appendOpt("-mem:lat", MemLatency); // Uses ToString()
        appendOpt("-mem:width", MemBusWidth);

        appendOpt("-tlb:itlb", TlbItlb); // Handles string or CacheTlbConfig via its value
        appendOpt("-tlb:dtlb", TlbDtlb); // Handles string or CacheTlbConfig via its value
        appendOpt("-tlb:lat", TlbMissLatency);

        appendOpt("-res:ialu", ResIntegerAlu);
        appendOpt("-res:imult", ResIntegerMultDiv);
        appendOpt("-res:memport", ResMemoryPorts);
        appendOpt("-res:fpalu", ResFpAlu);
        appendOpt("-res:fpmult", ResFpMultDiv);

        appendListOpt("-pcstat", PcStat);
        appendOpt("-power:print_stats", PowerPrintStats);

        return sb.ToString().TrimEnd(); // Remove trailing space
    }

    // --- IEquatable and Hashing ---

    // Note: Equality comparison includes checking all properties.
    // This can be quite long. Consider using source generators for records
    // if using C# 9+ for simpler equality implementation, though this manual
    // approach provides full control.

    public bool Equals(SimOutorderConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Compare all properties...
        return ConfigFile == other.ConfigFile &&
               DumpConfigFile == other.DumpConfigFile &&
               PrintHelp == other.PrintHelp &&
               Verbose == other.Verbose &&
               Debug == other.Debug &&
               StartDebugger == other.StartDebugger &&
               RandomSeed == other.RandomSeed &&
               InitTerminate == other.InitTerminate &&
               RedirectSimOutput == other.RedirectSimOutput &&
               RedirectProgOutput == other.RedirectProgOutput &&
               NicePriority == other.NicePriority &&
               MaxInstructions == other.MaxInstructions &&
               FastForwardInstructions == other.FastForwardInstructions &&
               EqualityComparer<List<string>>.Default.Equals(PTrace, other.PTrace) && // List comparison
               FetchSpeed == other.FetchSpeed &&
               FetchPolicy == other.FetchPolicy &&
               FetchRenameDelay == other.FetchRenameDelay &&
               BranchPredictorType == other.BranchPredictorType &&
               BpredBimodTableSize == other.BpredBimodTableSize &&
               Equals(Bpred2LevConfig, other.Bpred2LevConfig) &&
               BpredCombMetaTableSize == other.BpredCombMetaTableSize &&
               BpredReturnAddressStackSize == other.BpredReturnAddressStackSize &&
               Equals(BpredBtbConfig, other.BpredBtbConfig) &&
               BpredSpeculativeUpdate == other.BpredSpeculativeUpdate &&
               CacheLoadPredictorType == other.CacheLoadPredictorType &&
               CpredBimodTableSize == other.CpredBimodTableSize &&
               Equals(Cpred2LevConfig, other.Cpred2LevConfig) &&
               CpredCombMetaTableSize == other.CpredCombMetaTableSize &&
               CpredReturnAddressStackSize == other.CpredReturnAddressStackSize &&
               Equals(CpredBtbConfig, other.CpredBtbConfig) &&
               DecodeWidth == other.DecodeWidth &&
               IssueWidth == other.IssueWidth &&
               IssueInOrder == other.IssueInOrder &&
               IssueWrongPath == other.IssueWrongPath &&
               CommitWidth == other.CommitWidth &&
               RenameDispatchDelay == other.RenameDispatchDelay &&
               IssueExecDelay == other.IssueExecDelay &&
               ReorderBufferSize == other.ReorderBufferSize &&
               IssueQueueSize == other.IssueQueueSize &&
               RegisterFileSize == other.RegisterFileSize &&
               LoadStoreQueueSize == other.LoadStoreQueueSize &&
               RecoveryModel == other.RecoveryModel &&
               CacheDl1 == other.CacheDl1 && // Handles string comparison correctly
               CacheDl1Latency == other.CacheDl1Latency &&
               CacheDl2 == other.CacheDl2 &&
               CacheDl2Latency == other.CacheDl2Latency &&
               CacheIl1 == other.CacheIl1 &&
               CacheIl1Latency == other.CacheIl1Latency &&
               CacheIl2 == other.CacheIl2 &&
               CacheIl2Latency == other.CacheIl2Latency &&
               CacheFlushOnSyscall == other.CacheFlushOnSyscall &&
               CacheInstructionCompress == other.CacheInstructionCompress &&
               Equals(MemLatency, other.MemLatency) &&
               MemBusWidth == other.MemBusWidth &&
               TlbItlb == other.TlbItlb &&
               TlbDtlb == other.TlbDtlb &&
               TlbMissLatency == other.TlbMissLatency &&
               ResIntegerAlu == other.ResIntegerAlu &&
               ResIntegerMultDiv == other.ResIntegerMultDiv &&
               ResMemoryPorts == other.ResMemoryPorts &&
               ResFpAlu == other.ResFpAlu &&
               ResFpMultDiv == other.ResFpMultDiv &&
               EqualityComparer<List<string>>.Default.Equals(PcStat, other.PcStat) && // List comparison
               PowerPrintStats == other.PowerPrintStats;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SimOutorderConfig);
    }

    public override int GetHashCode()
    {
        // Combine hash codes of all properties.
        // Using HashCode.Combine is recommended for good distribution.
        var hc = new HashCode();
        hc.Add(ConfigFile);
        hc.Add(DumpConfigFile);
        hc.Add(PrintHelp);
        hc.Add(Verbose);
        hc.Add(Debug);
        hc.Add(StartDebugger);
        hc.Add(RandomSeed);
        hc.Add(InitTerminate);
        hc.Add(RedirectSimOutput);
        hc.Add(RedirectProgOutput);
        hc.Add(NicePriority);
        hc.Add(MaxInstructions);
        hc.Add(FastForwardInstructions);
        // Add list hash - simple approach: hash count and first few elements or combine all element hashes
        hc.Add(PTrace?.Count ?? 0);
        if (PTrace != null) foreach(var item in PTrace) hc.Add(item);

        hc.Add(FetchSpeed);
        hc.Add(FetchPolicy);
        hc.Add(FetchRenameDelay);

        hc.Add(BranchPredictorType);
        hc.Add(BpredBimodTableSize);
        hc.Add(Bpred2LevConfig);
        hc.Add(BpredCombMetaTableSize);
        hc.Add(BpredReturnAddressStackSize);
        hc.Add(BpredBtbConfig);
        hc.Add(BpredSpeculativeUpdate);

        hc.Add(CacheLoadPredictorType);
        hc.Add(CpredBimodTableSize);
        hc.Add(Cpred2LevConfig);
        hc.Add(CpredCombMetaTableSize);
        hc.Add(CpredReturnAddressStackSize);
        hc.Add(CpredBtbConfig);

        hc.Add(DecodeWidth);
        hc.Add(IssueWidth);
        hc.Add(IssueInOrder);
        hc.Add(IssueWrongPath);
        hc.Add(CommitWidth);
        hc.Add(RenameDispatchDelay);
        hc.Add(IssueExecDelay);


        hc.Add(ReorderBufferSize);
        hc.Add(IssueQueueSize);
        hc.Add(RegisterFileSize);
        hc.Add(LoadStoreQueueSize);

        hc.Add(RecoveryModel);

        hc.Add(CacheDl1);
        hc.Add(CacheDl1Latency);
        hc.Add(CacheDl2);
        hc.Add(CacheDl2Latency);
        hc.Add(CacheIl1);
        hc.Add(CacheIl1Latency);
        hc.Add(CacheIl2);
        hc.Add(CacheIl2Latency);
        hc.Add(CacheFlushOnSyscall);
        hc.Add(CacheInstructionCompress);

        hc.Add(MemLatency);
        hc.Add(MemBusWidth);

        hc.Add(TlbItlb);
        hc.Add(TlbDtlb);
        hc.Add(TlbMissLatency);

        hc.Add(ResIntegerAlu);
        hc.Add(ResIntegerMultDiv);
        hc.Add(ResMemoryPorts);
        hc.Add(ResFpAlu);
        hc.Add(ResFpMultDiv);

        // Add list hash
        hc.Add(PcStat?.Count ?? 0);
         if (PcStat != null) foreach(var item in PcStat) hc.Add(item);

        hc.Add(PowerPrintStats);

        return hc.ToHashCode();
    }

     public static bool operator ==(SimOutorderConfig? left, SimOutorderConfig? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(SimOutorderConfig? left, SimOutorderConfig? right)
    {
        return !(left == right);
    }
}

public enum FetchPolicyEnum {
    icount = 0,
    round_robin = 1,
}

public enum BranchPredictorTypeEnum {
    nottaken = 0,
    taken = 1,
    bimod = 2,
    twolev = 3,
    comb = 4,
    perfect = 5, // WARNING TO NOT USE THIS
}

public enum SpeculativePredictorUpdateStageEnum {
    ID = 0,
    WB = 1,
}

public enum CacheLoadPredictorTypeEnum {
    nottaken = 0,
    taken = 1,
    bimod = 2,
    twolev = 3,
    comb = 4,
    perfect = 5, // WARNING TO NOT USE THIS
}

public enum RecoveryModelEnum {
    squash = 0,
    perfect = 1,
}


public enum ReplacementPolicyEnum {
    l = 0,
    f = 1,
    r = 2,
}