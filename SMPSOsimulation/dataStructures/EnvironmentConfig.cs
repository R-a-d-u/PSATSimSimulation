

using System.Security.Cryptography;
using System.Text;

public class EnvironmentConfig {

    public int MaxSecondsPerSimulation {get; set;}
    public int MaxParallelProcesses {get; set;}
    /// <summary>
    /// maximum number of inst's to execute
    /// </summary>
    public ulong? MaxInstructions { get; set; } // -max:inst <uint> (Using ulong for <uint>)

    /// <summary>
    /// number of insts skipped before timing starts
    /// </summary>
    public ulong? FastForwardInstructions { get; set; } // -fastfwd <int> (Using ulong as count is non-negative)

    // --- Frontend ---

    /// <summary>
    /// speed of front-end of machine relative to execution core
    /// </summary>
    public int? FetchSpeed { get; set; } // -fetch:speed <int>

    /// <summary> Number of cycles between fetch and rename stages. </summary>
    public int? FetchRenameDelay { get; set; } // -fetch_rename_delay <int>

    /// <summary>
    /// issue instructions down wrong execution paths
    /// </summary>
    public bool? IssueWrongPath { get; set; } // -issue:wrongpath <true|false>

    /// <summary> Minimum cycles between issue and execution. </summary>
    public int? IssueExecDelay { get; set; } // -iq:issue_exec_delay <int>

    /// <summary>
    /// l1 data cache hit latency (in cycles)
    /// </summary>
    public int? CacheDl1Latency { get; set; } // -cache:dl1lat <int>

    /// <summary>
    /// l2 data cache hit latency (in cycles)
    /// </summary>
    public int? CacheDl2Latency { get; set; } // -cache:dl2lat <int>

    /// <summary>
    /// il1 instruction cache hit latency (in cycles)
    /// </summary>
    public int? CacheIl1Latency { get; set; } // -cache:il1lat <int>

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

    /// <summary>
    /// memory access latency (<first_chunk> <inter_chunk>)
    /// </summary>
    public MemLatencyConfig? MemLatency { get; set; } // -mem:lat <int list...>

    /// <summary>
    /// inst/data TLB miss latency (in cycles)
    /// </summary>
    public int? TlbMissLatency { get; set; } // -tlb:lat <int>

    /// <summary> Number cycles between rename and dispatch stages. </summary>
    public int? RenameDispatchDelay { get; set; } // -rename_dispatch_delay <int>


    public EnvironmentConfig(int maxParallelProcesses, int maxSecondsPerSimulation, ulong? maxInstructions = null, ulong? fastForwardInstructions = null, int? fetchSpeed = null, bool? issueWrongPath = null, int? issueExecDelay = null, int? cacheDl1Latency = null, int? cacheDl2Latency = null, int? cacheIl1Latency = null, int? cacheIl2Latency = null, bool? cacheFlushOnSyscall = null, bool? cacheInstructionCompress = null, MemLatencyConfig? memLatency = null, int? tlbMissLatency = null, int? fetchRenameDelay = null, int? renameDispatchDelay = null)
    {
        if (maxSecondsPerSimulation <= 0)
            throw new ArgumentException("maxSecondsPerSimulation must be greater than 0.");

        if (maxParallelProcesses <= 0)
            throw new ArgumentException("MaxParallelProcesses must be greater than 0.");

        if (maxInstructions <= 0)
            throw new ArgumentException("maxInstructions must be greater than 0.");

        if (fastForwardInstructions <= 0)
            throw new ArgumentException("fastForwardInstructions must be greater than 0.");

        if (fetchSpeed <= 0)
            throw new ArgumentException("fetchSpeed must be greater than 0.");

        if (issueExecDelay <= 0)
            throw new ArgumentException("issueExecDelay must be greater than 0.");

        if (cacheDl1Latency <= 0)
            throw new ArgumentException("cacheDl1Latency must be greater than 0.");

        if (cacheDl2Latency <= 0)
            throw new ArgumentException("cacheDl2Latency must be greater than 0.");

        if (cacheIl1Latency <= 0)
            throw new ArgumentException("cacheIl1Latency must be greater than 0.");

        if (cacheIl2Latency <= 0)
            throw new ArgumentException("cacheIl2Latency must be greater than 0.");

        if (memLatency is not null && memLatency.InterChunkLatency <= 0)
            throw new ArgumentException("InterChunkLatency must be greater than 0.");

        if (memLatency is not null && memLatency.FirstChunkLatency <= 0)
            throw new ArgumentException("FirstChunkLatency must be greater than 0.");

        if (tlbMissLatency <= 0)
            throw new ArgumentException("tlbMissLatency must be greater than 0.");

        if (fetchRenameDelay <= 0)
            throw new ArgumentException("fetchRenameDelay must be greater than 0.");

        if (renameDispatchDelay <= 0)
            throw new ArgumentException("renameDispatchDelay must be greater than 0.");


        MaxInstructions = maxInstructions;
        FastForwardInstructions = fastForwardInstructions;
        FetchSpeed = fetchSpeed;
        IssueWrongPath = issueWrongPath;
        IssueExecDelay = issueExecDelay;
        CacheDl1Latency = cacheDl1Latency;
        CacheDl2Latency = cacheDl2Latency;
        CacheIl1Latency = cacheIl1Latency;
        CacheIl2Latency = cacheIl2Latency;
        CacheFlushOnSyscall = cacheFlushOnSyscall;
        CacheInstructionCompress = cacheInstructionCompress;
        MemLatency = memLatency;
        TlbMissLatency = tlbMissLatency;
        FetchRenameDelay = fetchRenameDelay;
        RenameDispatchDelay = renameDispatchDelay;
        MaxSecondsPerSimulation = maxSecondsPerSimulation;
        MaxParallelProcesses = maxParallelProcesses;
    }

    public string CalculateSha256()
    {
        using var sha256 = SHA256.Create();
        var rawData = $"{MaxInstructions}-{FastForwardInstructions}-{FetchSpeed}-{IssueWrongPath}-{IssueExecDelay}-" +
                    $"{CacheDl1Latency}-{CacheDl2Latency}-{CacheIl1Latency}-{CacheIl2Latency}-{CacheFlushOnSyscall}-" +
                    $"{CacheInstructionCompress}-{MemLatency}-{TlbMissLatency}-{FetchRenameDelay}-{RenameDispatchDelay}-{MaxSecondsPerSimulation}";

        var bytes = Encoding.UTF8.GetBytes(rawData);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes); // or BitConverter.ToString(hashBytes).Replace("-", "") for older .NET
    }
}