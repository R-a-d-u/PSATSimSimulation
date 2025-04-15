using System.Security.Cryptography;
using System.Text;

public class CPUConfig : IEquatable<CPUConfig?>
{

    /// <summary> Branch predictor type ("nottaken", "taken", "perfect", "bimod", "2lev", "comb"). </summary>
    public BranchPredictorTypeEnum BranchPredictorType { get; set; } // -bpred <string>

    /// <summary>
    /// bimodal predictor config ("<table size>")
    /// </summary>
    public int BpredBimodTableSize { get; set; } // -bpred:bimod <int>

    /// <summary>
    /// 2-level predictor config (<l1size> <l2size> <hist_size> <xor>)
    /// </summary>
    public Predictor2LevConfig Bpred2LevConfig { get; set; } // -bpred:2lev <int list...>

    /// <summary>
    /// combining predictor config (<meta_table_size>)
    /// </summary>
    public int BpredCombMetaTableSize { get; set; } // -bpred:comb <int>

    /// <summary>
    /// return address stack size (0 for no return stack)
    /// </summary>
    public int BpredReturnAddressStackSize { get; set; } // -bpred:ras <int>

    /// <summary>
    /// BTB config (<num_sets> <associativity>)
    /// </summary>
    public BtbConfig BpredBtbConfig { get; set; } // -bpred:btb <int list...>

    /// <summary> Speculative predictor update stage ("ID", "WB", or null for non-speculative). </summary>
    public SpeculativePredictorUpdateStageEnum BpredSpeculativeUpdate { get; set; } // -bpred:spec_update <string>

    /// <summary> Cache load-latency predictor type ("nottaken", "taken", "perfect", "bimod", "2lev", "comb"). </summary>
    public CacheLoadPredictorTypeEnum CacheLoadPredictorType { get; set; } // -cpred <string>

    /// <summary>
    /// cache load-latency bimodal predictor config (<table size>)
    /// </summary>
    public int CpredBimodTableSize { get; set; } // -cpred:bimod <int>

    /// <summary>
    /// cache load-latency 2-level predictor config (<l1size> <l2size> <hist_size> <xor>)
    /// </summary>
    public Predictor2LevConfig Cpred2LevConfig { get; set; } // -cpred:2lev <int list...>

    /// <summary>
    /// cache load-latency combining predictor config (<meta_table_size>)
    /// </summary>
    public int CpredCombMetaTableSize { get; set; } // -cpred:comb <int>

    /// <summary>
    /// return address stack size (0 for no return stack)
    /// </summary>
    public int CpredReturnAddressStackSize { get; set; } // -cpred:ras <int> (Note: Default is 0)

    /// <summary>
    /// cache load-latency BTB config (<num_sets> <associativity>)
    /// </summary>
    public BtbConfig CpredBtbConfig { get; set; } // -cpred:btb <int list...>

    /// <summary>
    /// instruction decode B/W (insts/cycle)
    /// </summary>
    public int DecodeWidth { get; set; } // -decode:width <int>

    /// <summary>
    /// instruction issue B/W (insts/cycle)
    /// </summary>
    public int IssueWidth { get; set; } // -issue:width <int>

    /// <summary>
    /// run pipeline with in-order issue
    /// </summary>
    public bool IssueInOrder { get; set; } // -issue:inorder <true|false>

    /// <summary>
    /// instruction commit B/W (insts/cycle)
    /// </summary>
    public int CommitWidth { get; set; } // -commit:width <int>

    /// <summary>
    /// reorder buffer (ROB) size
    /// </summary>
    public int ReorderBufferSize { get; set; } // -rob:size <int>

    /// <summary>
    /// issue queue (IQ) size
    /// </summary>
    public int IssueQueueSize { get; set; } // -iq:size <int>

    /// <summary>
    /// register file (RF) size for each the INT and FP physical register file)
    /// </summary>
    public int RegisterFileSize { get; set; } // -rf:size <int> (Size for *each* INT and FP)

    /// <summary>
    /// load/store queue (LSQ) size
    /// </summary>
    public int LoadStoreQueueSize { get; set; } // -lsq:size <int>

    /// <summary> l1 data cache config, i.e., {<config>|none} </summary>
    public CacheTlbConfig CacheDl1 { get; set; } // -cache:dl1 <string>

    /// <summary> l2 data cache config, i.e., {<config>|none} </summary>
    public CacheTlbConfig CacheDl2 { get; set; } // -cache:dl2 <string>

    /// <summary> L1 Instruction Cache Config. Can be "none", "dl1", "dl2", or CacheTlbConfig. </summary>
    public CacheTlbConfig CacheIl1 { get; set; } // -cache:il1 <string>

    /// <summary> L2 Instruction Cache Config. Can be "none", "dl2", or CacheTlbConfig. </summary>
    public CacheTlbConfig CacheIl2 { get; set; } // -cache:il2 <string>

    /// <summary>
    /// memory access bus width (in bytes)s
    /// </summary>
    public int MemBusWidth { get; set; } // -mem:width <int>

    /// <summary> Instruction TLB Config. Can be "none" or CacheTlbConfig. </summary>
    public CacheTlbConfig TlbItlb { get; set; } // -tlb:itlb <string>
    /// <summary> Data TLB Config. Can be "none" or CacheTlbConfig. </summary>
    public CacheTlbConfig TlbDtlb { get; set; } // -tlb:dtlb <string>

    /// <summary>
    /// total number of integer ALU's available
    /// </summary>
    public int ResIntegerAlu { get; set; } // -res:ialu <int>

    /// <summary>
    /// total number of integer multiplier/dividers available
    /// </summary>
    public int ResIntegerMultDiv { get; set; } // -res:imult <int>

    /// <summary>
    /// total number of memory system ports available (to CPU)
    /// </summary>
    public int ResMemoryPorts { get; set; } // -res:memport <int>

    /// <summary>
    /// total number of floating point ALU's available
    /// </summary>
    public int ResFpAlu { get; set; } // -res:fpalu <int>

    /// <summary>
    /// total number of floating point multiplier/dividers available
    /// </summary>
    public int ResFpMultDiv { get; set; } // -res:fpmult <int>

    public CPUConfig(
        BranchPredictorTypeEnum branchPredictorType,
        int bpredBimodTableSize,
        Predictor2LevConfig bpred2LevConfig,
        int bpredCombMetaTableSize,
        int bpredReturnAddressStackSize,
        BtbConfig bpredBtbConfig,
        SpeculativePredictorUpdateStageEnum bpredSpeculativeUpdate,
        CacheLoadPredictorTypeEnum cacheLoadPredictorType,
        int cpredBimodTableSize,
        Predictor2LevConfig cpred2LevConfig,
        int cpredCombMetaTableSize,
        int cpredReturnAddressStackSize,
        BtbConfig cpredBtbConfig,
        int decodeWidth,
        int issueWidth,
        bool issueInOrder,
        int commitWidth,
        int reorderBufferSize,
        int issueQueueSize,
        int registerFileSize,
        int loadStoreQueueSize,
        CacheTlbConfig cacheDl1,
        CacheTlbConfig cacheDl2,
        CacheTlbConfig cacheIl1,
        CacheTlbConfig cacheIl2,
        int memBusWidth,
        CacheTlbConfig tlbItlb,
        CacheTlbConfig tlbDtlb,
        int resIntegerAlu,
        int resIntegerMultDiv,
        int resMemoryPorts,
        int resFpAlu,
        int resFpMultDiv
    )
    {
        void ThrowIfOutOfRange(string name, int value, int min, int max)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(name, $"Value {value} is outside range [{min}, {max}]");
        }

        ThrowIfOutOfRange(nameof(branchPredictorType), (int)branchPredictorType, CPUConfigLimits.BranchPredictorTypeMin, CPUConfigLimits.BranchPredictorTypeMax);
        ThrowIfOutOfRange(nameof(bpredBimodTableSize), bpredBimodTableSize, CPUConfigLimits.BpredBimodTableSizeMin, CPUConfigLimits.BpredBimodTableSizeMax);
        ThrowIfOutOfRange(nameof(bpredCombMetaTableSize), bpredCombMetaTableSize, CPUConfigLimits.BpredCombMetaTableSizeMin, CPUConfigLimits.BpredCombMetaTableSizeMax);
        ThrowIfOutOfRange(nameof(bpredReturnAddressStackSize), bpredReturnAddressStackSize, CPUConfigLimits.BpredReturnAddressStackSizeMin, CPUConfigLimits.BpredReturnAddressStackSizeMax);
        ThrowIfOutOfRange(nameof(bpredSpeculativeUpdate), (int)bpredSpeculativeUpdate, CPUConfigLimits.BpredSpeculativeUpdateMin, CPUConfigLimits.BpredSpeculativeUpdateMax);
        ThrowIfOutOfRange(nameof(cacheLoadPredictorType), (int)cacheLoadPredictorType, CPUConfigLimits.CacheLoadPredictorTypeMin, CPUConfigLimits.CacheLoadPredictorTypeMax);
        ThrowIfOutOfRange(nameof(cpredBimodTableSize), cpredBimodTableSize, CPUConfigLimits.CpredBimodTableSizeMin, CPUConfigLimits.CpredBimodTableSizeMax);
        ThrowIfOutOfRange(nameof(cpredCombMetaTableSize), cpredCombMetaTableSize, CPUConfigLimits.CpredCombMetaTableSizeMin, CPUConfigLimits.CpredCombMetaTableSizeMax);
        ThrowIfOutOfRange(nameof(cpredReturnAddressStackSize), cpredReturnAddressStackSize, CPUConfigLimits.CpredReturnAddressStackSizeMin, CPUConfigLimits.CpredReturnAddressStackSizeMax);
        ThrowIfOutOfRange(nameof(decodeWidth), decodeWidth, (int)Math.Pow(2, CPUConfigLimits.DecodeWidthMinLog2), (int)Math.Pow(2, CPUConfigLimits.DecodeWidthMaxLog2));
        ThrowIfOutOfRange(nameof(issueWidth), issueWidth, (int)Math.Pow(2, CPUConfigLimits.IssueWidthMinLog2), (int)Math.Pow(2, CPUConfigLimits.IssueWidthMaxLog2));
        ThrowIfOutOfRange(nameof(commitWidth), commitWidth, CPUConfigLimits.CommitWidthMin, CPUConfigLimits.CommitWidthMax);
        ThrowIfOutOfRange(nameof(reorderBufferSize), reorderBufferSize, CPUConfigLimits.ReorderBufferSizeMin, CPUConfigLimits.ReorderBufferSizeMax);
        ThrowIfOutOfRange(nameof(issueQueueSize), issueQueueSize, CPUConfigLimits.IssueQueueSizeMin, CPUConfigLimits.IssueQueueSizeMax);
        ThrowIfOutOfRange(nameof(registerFileSize), registerFileSize, CPUConfigLimits.RegisterFileSizeMin, CPUConfigLimits.RegisterFileSizeMax);
        ThrowIfOutOfRange(nameof(loadStoreQueueSize), loadStoreQueueSize, CPUConfigLimits.LoadStoreQueueSizeMin, CPUConfigLimits.LoadStoreQueueSizeMax);
        ThrowIfOutOfRange(nameof(memBusWidth), memBusWidth, CPUConfigLimits.MemBusWidthMin, CPUConfigLimits.MemBusWidthMax);
        ThrowIfOutOfRange(nameof(resIntegerAlu), resIntegerAlu, CPUConfigLimits.ResIntegerAluMin, CPUConfigLimits.ResIntegerAluMax);
        ThrowIfOutOfRange(nameof(resIntegerMultDiv), resIntegerMultDiv, CPUConfigLimits.ResIntegerMultDivMin, CPUConfigLimits.ResIntegerMultDivMax);
        ThrowIfOutOfRange(nameof(resMemoryPorts), resMemoryPorts, CPUConfigLimits.ResMemoryPortsMin, CPUConfigLimits.ResMemoryPortsMax);
        ThrowIfOutOfRange(nameof(resFpAlu), resFpAlu, CPUConfigLimits.ResFpAluMin, CPUConfigLimits.ResFpAluMax);
        ThrowIfOutOfRange(nameof(resFpMultDiv), resFpMultDiv, CPUConfigLimits.ResFpMultDivMin, CPUConfigLimits.ResFpMultDivMax);

        // Validate nested config classes
        bpred2LevConfig.Validate((int)Math.Pow(2, CPUConfigLimits.Bpred2LevConfigL1SizeMinLog2), (int)Math.Pow(2, CPUConfigLimits.Bpred2LevConfigL1SizeMaxLog2), (int)Math.Pow(2, CPUConfigLimits.Bpred2LevConfigL2SizeMinLog2), (int)Math.Pow(2, CPUConfigLimits.Bpred2LevConfigL2SizeMaxLog2), CPUConfigLimits.Bpred2LevConfigHistorySizeMin, CPUConfigLimits.Bpred2LevConfigHistorySizeMax);
        bpredBtbConfig.Validate(CPUConfigLimits.BpredBtbConfigNumSetsMin, CPUConfigLimits.BpredBtbConfigNumSetsMax, CPUConfigLimits.BpredBtbConfigAssociativityMin, CPUConfigLimits.BpredBtbConfigAssociativityMax);
        cpred2LevConfig.Validate((int)Math.Pow(2, CPUConfigLimits.Cpred2LevConfigL1SizeMinLog2), (int)Math.Pow(2, CPUConfigLimits.Cpred2LevConfigL1SizeMaxLog2), (int)Math.Pow(2, CPUConfigLimits.Cpred2LevConfigL2SizeMinLog2), (int)Math.Pow(2, CPUConfigLimits.Cpred2LevConfigL2SizeMaxLog2), CPUConfigLimits.Cpred2LevConfigHistorySizeMin, CPUConfigLimits.Cpred2LevConfigHistorySizeMax);
        cpredBtbConfig.Validate(CPUConfigLimits.CpredBtbConfigNumSetsMin, CPUConfigLimits.CpredBtbConfigNumSetsMax, CPUConfigLimits.CpredBtbConfigAssociativityMin, CPUConfigLimits.CpredBtbConfigAssociativityMax);
        cacheDl1.Validate((int)Math.Pow(2, CPUConfigLimits.CacheDl1NumSetsMinLog2), (int)Math.Pow(2, CPUConfigLimits.CacheDl1NumSetsMaxLog2), CPUConfigLimits.CacheDl1BlockOrPageSizeMin, CPUConfigLimits.CacheDl1BlockOrPageSizeMax, CPUConfigLimits.CacheDl1AssociativityMin, CPUConfigLimits.CacheDl1AssociativityMax);
        cacheDl2.Validate((int)Math.Pow(2, CPUConfigLimits.CacheDl2NumSetsMinLog2), (int)Math.Pow(2, CPUConfigLimits.CacheDl2NumSetsMaxLog2), CPUConfigLimits.CacheDl2BlockOrPageSizeMin, CPUConfigLimits.CacheDl2BlockOrPageSizeMax, CPUConfigLimits.CacheDl2AssociativityMin, CPUConfigLimits.CacheDl2AssociativityMax);
        cacheIl1.Validate((int)Math.Pow(2, CPUConfigLimits.CacheIl1NumSetsMinLog2), (int)Math.Pow(2, CPUConfigLimits.CacheIl1NumSetsMaxLog2), CPUConfigLimits.CacheIl1BlockOrPageSizeMin, CPUConfigLimits.CacheIl1BlockOrPageSizeMax, CPUConfigLimits.CacheIl1AssociativityMin, CPUConfigLimits.CacheIl1AssociativityMax);
        cacheIl2.Validate((int)Math.Pow(2, CPUConfigLimits.CacheIl2NumSetsMinLog2), (int)Math.Pow(2, CPUConfigLimits.CacheIl2NumSetsMaxLog2), CPUConfigLimits.CacheIl2BlockOrPageSizeMin, CPUConfigLimits.CacheIl2BlockOrPageSizeMax, CPUConfigLimits.CacheIl2AssociativityMin, CPUConfigLimits.CacheIl2AssociativityMax);
        tlbItlb.Validate((int)Math.Pow(2, CPUConfigLimits.TlbItlbNumSetsMinLog2), (int)Math.Pow(2, CPUConfigLimits.TlbItlbNumSetsMaxLog2), CPUConfigLimits.TlbItlbBlockOrPageSizeMin, CPUConfigLimits.TlbItlbBlockOrPageSizeMax, CPUConfigLimits.TlbItlbAssociativityMin, CPUConfigLimits.TlbItlbAssociativityMax);
        tlbDtlb.Validate((int)Math.Pow(2, CPUConfigLimits.TlbDtlbNumSetsMinLog2), (int)Math.Pow(2, CPUConfigLimits.TlbDtlbNumSetsMaxLog2), CPUConfigLimits.TlbDtlbBlockOrPageSizeMin, CPUConfigLimits.TlbDtlbBlockOrPageSizeMax, CPUConfigLimits.TlbDtlbAssociativityMin, CPUConfigLimits.TlbDtlbAssociativityMax);

        if (memBusWidth <= 0 || (memBusWidth & (memBusWidth - 1)) != 0)
        {
            throw new ArgumentException($"The memory bus width ({nameof(memBusWidth)}) must be a positive power of 2. Received value: {memBusWidth}.", nameof(memBusWidth));
        }

        BranchPredictorType = branchPredictorType;
        BpredBimodTableSize = bpredBimodTableSize;
        Bpred2LevConfig = bpred2LevConfig;
        BpredCombMetaTableSize = bpredCombMetaTableSize;
        BpredReturnAddressStackSize = bpredReturnAddressStackSize;
        BpredBtbConfig = bpredBtbConfig;
        BpredSpeculativeUpdate = bpredSpeculativeUpdate;
        CacheLoadPredictorType = cacheLoadPredictorType;
        CpredBimodTableSize = cpredBimodTableSize;
        Cpred2LevConfig = cpred2LevConfig;
        CpredCombMetaTableSize = cpredCombMetaTableSize;
        CpredReturnAddressStackSize = cpredReturnAddressStackSize;
        CpredBtbConfig = cpredBtbConfig;
        DecodeWidth = decodeWidth;
        IssueWidth = issueWidth;
        IssueInOrder = issueInOrder;
        CommitWidth = commitWidth;
        ReorderBufferSize = reorderBufferSize;
        IssueQueueSize = issueQueueSize;
        RegisterFileSize = registerFileSize;
        LoadStoreQueueSize = loadStoreQueueSize;
        CacheDl1 = cacheDl1;
        CacheDl2 = cacheDl2;
        CacheIl1 = cacheIl1;
        CacheIl2 = cacheIl2;
        MemBusWidth = memBusWidth;
        TlbItlb = tlbItlb;
        TlbDtlb = tlbDtlb;
        ResIntegerAlu = resIntegerAlu;
        ResIntegerMultDiv = resIntegerMultDiv;
        ResMemoryPorts = resMemoryPorts;
        ResFpAlu = resFpAlu;
        ResFpMultDiv = resFpMultDiv;
    }




    public string CalculateSha256()
    {
        var sb = new StringBuilder();

        sb.Append(BranchPredictorType);
        sb.Append(BpredBimodTableSize);
        sb.Append(Bpred2LevConfig);
        sb.Append(BpredCombMetaTableSize);
        sb.Append(BpredReturnAddressStackSize);
        sb.Append(BpredBtbConfig);
        sb.Append(BpredSpeculativeUpdate);
        sb.Append(CacheLoadPredictorType);
        sb.Append(CpredBimodTableSize);
        sb.Append(Cpred2LevConfig);
        sb.Append(CpredCombMetaTableSize);
        sb.Append(CpredReturnAddressStackSize);
        sb.Append(CpredBtbConfig);
        sb.Append(DecodeWidth);
        sb.Append(IssueWidth);
        sb.Append(IssueInOrder);
        sb.Append(CommitWidth);
        sb.Append(ReorderBufferSize);
        sb.Append(IssueQueueSize);
        sb.Append(RegisterFileSize);
        sb.Append(LoadStoreQueueSize);
        sb.Append(CacheDl1);
        sb.Append(CacheDl2);
        sb.Append(CacheIl1);
        sb.Append(CacheIl2);
        sb.Append(MemBusWidth);
        sb.Append(TlbItlb);
        sb.Append(TlbDtlb);
        sb.Append(ResIntegerAlu);
        sb.Append(ResIntegerMultDiv);
        sb.Append(ResMemoryPorts);
        sb.Append(ResFpAlu);
        sb.Append(ResFpMultDiv);

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes); // .NET 5+
    }

    // You should also implement Equals to be consistent with GetHashCode
    public override bool Equals(object? obj)
    {
        return Equals(obj as CPUConfig);
    }

    public bool Equals(CPUConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Compare all properties included in GetHashCode
        return BranchPredictorType == other.BranchPredictorType &&
               BpredBimodTableSize == other.BpredBimodTableSize &&
               EqualityComparer<Predictor2LevConfig>.Default.Equals(Bpred2LevConfig, other.Bpred2LevConfig) &&
               BpredCombMetaTableSize == other.BpredCombMetaTableSize &&
               BpredReturnAddressStackSize == other.BpredReturnAddressStackSize &&
               EqualityComparer<BtbConfig>.Default.Equals(BpredBtbConfig, other.BpredBtbConfig) &&
               BpredSpeculativeUpdate == other.BpredSpeculativeUpdate &&
               CacheLoadPredictorType == other.CacheLoadPredictorType &&
               CpredBimodTableSize == other.CpredBimodTableSize &&
               EqualityComparer<Predictor2LevConfig>.Default.Equals(Cpred2LevConfig, other.Cpred2LevConfig) &&
               CpredCombMetaTableSize == other.CpredCombMetaTableSize &&
               CpredReturnAddressStackSize == other.CpredReturnAddressStackSize &&
               EqualityComparer<BtbConfig>.Default.Equals(CpredBtbConfig, other.CpredBtbConfig) &&
               DecodeWidth == other.DecodeWidth &&
               IssueWidth == other.IssueWidth &&
               IssueInOrder == other.IssueInOrder &&
               CommitWidth == other.CommitWidth &&
               ReorderBufferSize == other.ReorderBufferSize &&
               IssueQueueSize == other.IssueQueueSize &&
               RegisterFileSize == other.RegisterFileSize &&
               LoadStoreQueueSize == other.LoadStoreQueueSize &&
               EqualityComparer<CacheTlbConfig>.Default.Equals(CacheDl1, other.CacheDl1) &&
               EqualityComparer<CacheTlbConfig>.Default.Equals(CacheDl2, other.CacheDl2) &&
               EqualityComparer<CacheTlbConfig>.Default.Equals(CacheIl1, other.CacheIl1) &&
               EqualityComparer<CacheTlbConfig>.Default.Equals(CacheIl2, other.CacheIl2) &&
               MemBusWidth == other.MemBusWidth &&
               EqualityComparer<CacheTlbConfig>.Default.Equals(TlbItlb, other.TlbItlb) &&
               EqualityComparer<CacheTlbConfig>.Default.Equals(TlbDtlb, other.TlbDtlb) &&
               ResIntegerAlu == other.ResIntegerAlu &&
               ResIntegerMultDiv == other.ResIntegerMultDiv &&
               ResMemoryPorts == other.ResMemoryPorts &&
               ResFpAlu == other.ResFpAlu &&
               ResFpMultDiv == other.ResFpMultDiv;
    }

    // Optional: Implement equality operators if needed
    public static bool operator ==(CPUConfig? left, CPUConfig? right)
    {
        return EqualityComparer<CPUConfig?>.Default.Equals(left, right);
    }

    public static bool operator !=(CPUConfig? left, CPUConfig? right)
    {
        return !(left == right);
    }


    public static int GenerateRandomPowerOfTwo(Random _random, int min, int max)
    {
        // --- Input Validation ---
        if (min > max)
        {
            throw new ArgumentException($"Minimum value ({min}) cannot be greater than maximum value ({max}).");
        }
        // Powers of 2 are positive integers (1, 2, 4, ...), so max must be at least 1.
        if (max < 1)
        {
            throw new ArgumentException($"Maximum value ({max}) must be at least 1 to include any power of 2.");
        }

        // --- Find Candidate Powers of 2 ---
        List<int> candidates = new List<int>();
        long currentPower = 1; // Use long to safely check against int.MaxValue before casting

        while (currentPower <= max)
        {
            // Check if the current power of 2 is within the valid int range AND the specified [min, max] range
            if (currentPower >= min && currentPower <= int.MaxValue)
            {
                candidates.Add((int)currentPower);
            }

            // Calculate the next power of 2, checking for potential overflow
            // Stop if the *next* power would exceed max or long.MaxValue
            if (currentPower > max / 2) // Optimization: Avoids overflow and unnecessary checks if next power > max
            {
                break;
            }
            // Check for potential long overflow before multiplying (highly unlikely with int range but safe)
            if (currentPower > long.MaxValue / 2)
            {
                break;
            }
            currentPower *= 2;
        }

        // --- Handle No Candidates ---
        if (candidates.Count == 0)
        {
            throw new ArgumentException($"No power of 2 found within the specified range [{min}, {max}].");
        }

        // --- Select and Return Random Candidate ---
        int randomIndex = _random.Next(candidates.Count);
        return candidates[randomIndex];
    }


    public static CPUConfig GenerateRandom()
    {
        Random random = new();

        var bpred2predcfg = new Predictor2LevConfig( // bpred2levconfig
                GenerateRandomPowerOfTwo(random, CPUConfigLimits.Bpred2LevConfigL1SizeMinLog2, CPUConfigLimits.Bpred2LevConfigL1SizeMaxLog2),
                GenerateRandomPowerOfTwo(random, CPUConfigLimits.Bpred2LevConfigL2SizeMinLog2, CPUConfigLimits.Bpred2LevConfigL2SizeMaxLog2),
                random.Next(CPUConfigLimits.Bpred2LevConfigHistorySizeMin, CPUConfigLimits.Bpred2LevConfigHistorySizeMax + 1),
                random.Next(0, 2) == 1 // useXor
            );
        
        var cpred2predcfg = new Predictor2LevConfig( // cpred2levconfig
                GenerateRandomPowerOfTwo(random, CPUConfigLimits.Cpred2LevConfigL1SizeMinLog2, CPUConfigLimits.Cpred2LevConfigL1SizeMaxLog2),
                GenerateRandomPowerOfTwo(random, CPUConfigLimits.Cpred2LevConfigL2SizeMinLog2, CPUConfigLimits.Cpred2LevConfigL2SizeMaxLog2),
                random.Next(CPUConfigLimits.Cpred2LevConfigHistorySizeMin, CPUConfigLimits.Cpred2LevConfigHistorySizeMax + 1),
                random.Next(0, 2) == 1 // useXor
            );

        return new CPUConfig(
            (BranchPredictorTypeEnum)random.Next(CPUConfigLimits.BranchPredictorTypeMin, CPUConfigLimits.BranchPredictorTypeMax + 1),
            random.Next(CPUConfigLimits.BpredBimodTableSizeMin, CPUConfigLimits.BpredBimodTableSizeMax + 1),
            bpred2predcfg,
            random.Next(CPUConfigLimits.BpredCombMetaTableSizeMin, CPUConfigLimits.BpredCombMetaTableSizeMax + 1),
            random.Next(CPUConfigLimits.BpredReturnAddressStackSizeMin, CPUConfigLimits.BpredReturnAddressStackSizeMax + 1),
            new BtbConfig( //bpredBtbConfig
                random.Next(CPUConfigLimits.BpredBtbConfigNumSetsMin, CPUConfigLimits.BpredBtbConfigNumSetsMax + 1),
                random.Next(CPUConfigLimits.BpredBtbConfigAssociativityMin, CPUConfigLimits.BpredBtbConfigAssociativityMax + 1)
            ),
            (SpeculativePredictorUpdateStageEnum)random.Next(CPUConfigLimits.BpredSpeculativeUpdateMin, CPUConfigLimits.BpredSpeculativeUpdateMax + 1),
            (CacheLoadPredictorTypeEnum)random.Next(CPUConfigLimits.CacheLoadPredictorTypeMin, CPUConfigLimits.CacheLoadPredictorTypeMax + 1),
            random.Next(CPUConfigLimits.CpredBimodTableSizeMin, CPUConfigLimits.CpredBimodTableSizeMax + 1),
            cpred2predcfg,
            random.Next(CPUConfigLimits.CpredCombMetaTableSizeMin, CPUConfigLimits.CpredCombMetaTableSizeMax + 1),
            random.Next(CPUConfigLimits.CpredReturnAddressStackSizeMin, CPUConfigLimits.CpredReturnAddressStackSizeMax + 1),
            new BtbConfig( //cpredBtbConfig
                random.Next(CPUConfigLimits.CpredBtbConfigNumSetsMin, CPUConfigLimits.CpredBtbConfigNumSetsMax + 1),
                random.Next(CPUConfigLimits.CpredBtbConfigAssociativityMin, CPUConfigLimits.CpredBtbConfigAssociativityMax + 1)
            ),
            GenerateRandomPowerOfTwo(random, CPUConfigLimits.DecodeWidthMinLog2, CPUConfigLimits.DecodeWidthMaxLog2),
            GenerateRandomPowerOfTwo(random, CPUConfigLimits.IssueWidthMinLog2, CPUConfigLimits.IssueWidthMaxLog2),
            random.Next(0, 2) == 1,
            random.Next(CPUConfigLimits.CommitWidthMin, CPUConfigLimits.CommitWidthMax + 1),
            random.Next(CPUConfigLimits.ReorderBufferSizeMin, CPUConfigLimits.ReorderBufferSizeMax + 1),
            random.Next(CPUConfigLimits.IssueQueueSizeMin, CPUConfigLimits.IssueQueueSizeMax + 1),
            random.Next(CPUConfigLimits.RegisterFileSizeMin, CPUConfigLimits.RegisterFileSizeMax + 1),
            random.Next(CPUConfigLimits.LoadStoreQueueSizeMin, CPUConfigLimits.LoadStoreQueueSizeMax + 1),
            new CacheTlbConfig(
                "dl1",
                GenerateRandomPowerOfTwo(random, 1, 9999),
                random.Next(CPUConfigLimits.CacheDl1BlockOrPageSizeMin, CPUConfigLimits.CacheDl1BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheDl1AssociativityMin, CPUConfigLimits.CacheDl1AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheDl1ReplacementPolicyMin, CPUConfigLimits.CacheDl1ReplacementPolicyMax + 1)
            ),
            new CacheTlbConfig(
                "dl2",
                GenerateRandomPowerOfTwo(random, 1, 9999),
                random.Next(CPUConfigLimits.CacheDl2BlockOrPageSizeMin, CPUConfigLimits.CacheDl2BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheDl2AssociativityMin, CPUConfigLimits.CacheDl2AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheDl2ReplacementPolicyMin, CPUConfigLimits.CacheDl2ReplacementPolicyMax + 1)
            ),
            new CacheTlbConfig(
                "il1",
                GenerateRandomPowerOfTwo(random, 1, 9999),
                random.Next(CPUConfigLimits.CacheIl1BlockOrPageSizeMin, CPUConfigLimits.CacheIl1BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheIl1AssociativityMin, CPUConfigLimits.CacheIl1AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheIl1ReplacementPolicyMin, CPUConfigLimits.CacheIl1ReplacementPolicyMax + 1)
            ),
            new CacheTlbConfig(
                "il2",
                GenerateRandomPowerOfTwo(random, 1, 9999),
                random.Next(CPUConfigLimits.CacheIl2BlockOrPageSizeMin, CPUConfigLimits.CacheIl2BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheIl2AssociativityMin, CPUConfigLimits.CacheIl2AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheIl2ReplacementPolicyMin, CPUConfigLimits.CacheIl2ReplacementPolicyMax + 1)
            ),
            GenerateRandomPowerOfTwo(random, CPUConfigLimits.MemBusWidthMin, CPUConfigLimits.MemBusWidthMax),
            new CacheTlbConfig(
                "itlb",
                GenerateRandomPowerOfTwo(random, CPUConfigLimits.TlbItlbNumSetsMinLog2,CPUConfigLimits.TlbItlbNumSetsMaxLog2 + 1),
                random.Next(CPUConfigLimits.TlbItlbBlockOrPageSizeMin, CPUConfigLimits.TlbItlbBlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.TlbItlbAssociativityMin, CPUConfigLimits.TlbItlbAssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.TlbItlbReplacementPolicyMin, CPUConfigLimits.TlbItlbReplacementPolicyMin + 1)
            ),
            new CacheTlbConfig(
                "dtlb",
                GenerateRandomPowerOfTwo(random, CPUConfigLimits.TlbDtlbNumSetsMinLog2,CPUConfigLimits.TlbDtlbNumSetsMaxLog2 + 1),
                random.Next(CPUConfigLimits.TlbDtlbBlockOrPageSizeMin, CPUConfigLimits.TlbDtlbBlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.TlbDtlbAssociativityMin, CPUConfigLimits.TlbDtlbAssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.TlbDtlbReplacementPolicyMin, CPUConfigLimits.TlbDtlbReplacementPolicyMin + 1)
            ),
            random.Next(CPUConfigLimits.ResIntegerAluMin, CPUConfigLimits.ResIntegerAluMax + 1),
            random.Next(CPUConfigLimits.ResIntegerMultDivMin, CPUConfigLimits.ResIntegerMultDivMax + 1),
            random.Next(CPUConfigLimits.ResMemoryPortsMin, CPUConfigLimits.ResMemoryPortsMax + 1),
            random.Next(CPUConfigLimits.ResFpAluMin, CPUConfigLimits.ResFpAluMax + 1),
            random.Next(CPUConfigLimits.ResFpMultDivMin, CPUConfigLimits.ResFpMultDivMax + 1)
        );
    }


    public double[] GetVectorFormDouble()
    {
        return [
            (int)BranchPredictorType,
            BpredBimodTableSize,
            Math.Log2(Bpred2LevConfig.L1Size),
            Math.Log2(Bpred2LevConfig.L2Size),
            Bpred2LevConfig.HistorySize,
            Bpred2LevConfig.UseXor ? 1 : 0,
            BpredCombMetaTableSize,
            BpredReturnAddressStackSize,
            BpredBtbConfig.NumSets,
            BpredBtbConfig.Associativity,
            (int)BpredSpeculativeUpdate,
            (int)CacheLoadPredictorType,
            CpredBimodTableSize,
            Math.Log2(Cpred2LevConfig.L1Size),
            Math.Log2(Cpred2LevConfig.L2Size),
            Cpred2LevConfig.HistorySize,
            Cpred2LevConfig.UseXor ? 1 : 0,
            CpredCombMetaTableSize,
            CpredReturnAddressStackSize,
            CpredBtbConfig.NumSets,
            CpredBtbConfig.Associativity,
            Math.Log2(DecodeWidth),
            Math.Log2(IssueWidth),
            IssueInOrder ? 1 : 0,
            CommitWidth,
            ReorderBufferSize,
            IssueQueueSize,
            RegisterFileSize,
            LoadStoreQueueSize,
            Math.Log2(CacheDl1.NumSets),
            CacheDl1.BlockOrPageSize,
            CacheDl1.Associativity,
            (int)CacheDl1.ReplacementPolicy,
            Math.Log2(CacheDl2.NumSets),
            CacheDl2.BlockOrPageSize,
            CacheDl2.Associativity,
            (int)CacheDl2.ReplacementPolicy,
            Math.Log2(CacheIl1.NumSets),
            CacheIl1.BlockOrPageSize,
            CacheIl1.Associativity,
            (int)CacheIl1.ReplacementPolicy,
            Math.Log2(CacheIl2.NumSets),
            CacheIl2.BlockOrPageSize,
            CacheIl2.Associativity,
            (int)CacheIl2.ReplacementPolicy,
            Math.Log2(MemBusWidth),
            Math.Log2(TlbItlb.NumSets),
            TlbItlb.BlockOrPageSize,
            TlbItlb.Associativity,
            (int)TlbItlb.ReplacementPolicy,
            Math.Log2(TlbDtlb.NumSets),
            TlbDtlb.BlockOrPageSize,
            TlbDtlb.Associativity,
            (int)TlbDtlb.ReplacementPolicy,
            ResIntegerAlu,
            ResIntegerMultDiv,
            ResMemoryPorts,
            ResFpAlu,
            ResFpMultDiv,
        ];
    }


    public static CPUConfig GetConfigFromVectorDouble(double[] vector)
    {
        int idx = 0;

        var branchPredictorType = (BranchPredictorTypeEnum)(int)vector[idx++];
        var bpredBimodTableSize = (int)vector[idx++];
        var bpred2LevConfig = new Predictor2LevConfig
        (
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)vector[idx++],
            vector[idx++] != 0
        );
        var bpredCombMetaTableSize = (int)vector[idx++];
        var bpredReturnAddressStackSize = (int)vector[idx++];
        var bpredBtbConfig = new BtbConfig
        (
            (int)vector[idx++],
            (int)vector[idx++]
        );
        var bpredSpeculativeUpdate = (SpeculativePredictorUpdateStageEnum)(int)vector[idx++];
        var cacheLoadPredictorType = (CacheLoadPredictorTypeEnum)(int)vector[idx++];
        var cpredBimodTableSize = (int)vector[idx++];
        var cpred2LevConfig = new Predictor2LevConfig
        (
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)vector[idx++],
            vector[idx++] != 0
        );
        var cpredCombMetaTableSize = (int)vector[idx++];
        var cpredReturnAddressStackSize = (int)vector[idx++];
        var cpredBtbConfig = new BtbConfig
        (
            (int)vector[idx++],
            (int)vector[idx++]
        );
        var decodeWidth = (int)Math.Pow(2, (int)vector[idx++]);
        var issueWidth = (int)Math.Pow(2, (int)vector[idx++]);
        var issueInOrder = vector[idx++] != 0;
        var commitWidth = (int)vector[idx++];
        var reorderBufferSize = (int)vector[idx++];
        var issueQueueSize = (int)vector[idx++];
        var registerFileSize = (int)vector[idx++];
        var loadStoreQueueSize = (int)vector[idx++];
        var cacheDl1 = new CacheTlbConfig
        (
            "dl1",
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++] // assuming the ReplacementPolicy is an enum of type CacheReplacementPolicyEnum
        );
        var cacheDl2 = new CacheTlbConfig
        (
            "dl2",
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++]
        );
        var cacheIl1 = new CacheTlbConfig
        (
            "il1",
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++]
        );
        var cacheIl2 = new CacheTlbConfig
        (
            "il2",
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++]
        );
        var memBusWidth = (int)Math.Pow(2, (int)vector[idx++]);
        var tlbItlb = new CacheTlbConfig
        (
            "itlb",
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++]
        );
        var tlbDtlb = new CacheTlbConfig
        (
            "dtlb",
            (int)Math.Pow(2, (int)vector[idx++]),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++]
        );
        var resIntegerAlu = (int)vector[idx++];
        var resIntegerMultDiv = (int)vector[idx++];
        var resMemoryPorts = (int)vector[idx++];
        var resFpAlu = (int)vector[idx++];
        var resFpMultDiv = (int)vector[idx++];

        return new CPUConfig(
            branchPredictorType,
            bpredBimodTableSize,
            bpred2LevConfig,
            bpredCombMetaTableSize,
            bpredReturnAddressStackSize,
            bpredBtbConfig,
            bpredSpeculativeUpdate,
            cacheLoadPredictorType,
            cpredBimodTableSize,
            cpred2LevConfig,
            cpredCombMetaTableSize,
            cpredReturnAddressStackSize,
            cpredBtbConfig,
            decodeWidth,
            issueWidth,
            issueInOrder,
            commitWidth,
            reorderBufferSize,
            issueQueueSize,
            registerFileSize,
            loadStoreQueueSize,
            cacheDl1,
            cacheDl2,
            cacheIl1,
            cacheIl2,
            memBusWidth,
            tlbItlb,
            tlbDtlb,
            resIntegerAlu,
            resIntegerMultDiv,
            resMemoryPorts,
            resFpAlu,
            resFpMultDiv
        );
    }





    public static class CPUConfigLimits
    {
        public static int BranchPredictorTypeMax = (int)BranchPredictorTypeEnum.perfect - 1;
        public static int BranchPredictorTypeMin = 0;

        public static int BpredBimodTableSizeMax = 9999;
        public static int BpredBimodTableSizeMin = 1;

        public static int Bpred2LevConfigL1SizeMaxLog2 = (int)Math.Log2(9999);
        public static int Bpred2LevConfigL1SizeMinLog2 = 0;

        public static int Bpred2LevConfigL2SizeMaxLog2 = (int)Math.Log2(9999);
        public static int Bpred2LevConfigL2SizeMinLog2 = 0;

        public static int Bpred2LevConfigHistorySizeMax = 9999;
        public static int Bpred2LevConfigHistorySizeMin = 1;

        public static int Bpred2LevConfigUseXorMax = 1;
        public static int Bpred2LevConfigUseXorMin = 0;

        public static int BpredCombMetaTableSizeMax = 9999;
        public static int BpredCombMetaTableSizeMin = 1;

        public static int BpredReturnAddressStackSizeMax = 9999;
        public static int BpredReturnAddressStackSizeMin = 1;

        public static int BpredBtbConfigNumSetsMax = 9999;
        public static int BpredBtbConfigNumSetsMin = 1;

        public static int BpredBtbConfigAssociativityMax = 9999;
        public static int BpredBtbConfigAssociativityMin = 1;

        public static int BpredSpeculativeUpdateMax = 1;
        public static int BpredSpeculativeUpdateMin = 0;

        public static int CacheLoadPredictorTypeMax = (int)CacheLoadPredictorTypeEnum.perfect - 1;
        public static int CacheLoadPredictorTypeMin = 0;

        public static int CpredBimodTableSizeMax = 9999;
        public static int CpredBimodTableSizeMin = 1;

        public static int Cpred2LevConfigL1SizeMaxLog2 = (int)Math.Log2(9999);
        public static int Cpred2LevConfigL1SizeMinLog2 = 0;

        public static int Cpred2LevConfigL2SizeMaxLog2 = (int)Math.Log2(9999);
        public static int Cpred2LevConfigL2SizeMinLog2 = 0;

        public static int Cpred2LevConfigHistorySizeMax = 9999;
        public static int Cpred2LevConfigHistorySizeMin = 1;

        public static int Cpred2LevConfigUseXorMax = 1;
        public static int Cpred2LevConfigUseXorMin = 0;

        public static int CpredCombMetaTableSizeMax = 9999;
        public static int CpredCombMetaTableSizeMin = 1;

        public static int CpredReturnAddressStackSizeMax = 9999;
        public static int CpredReturnAddressStackSizeMin = 1;

        public static int CpredBtbConfigNumSetsMax = 9999;
        public static int CpredBtbConfigNumSetsMin = 1;

        public static int CpredBtbConfigAssociativityMax = 9999;
        public static int CpredBtbConfigAssociativityMin = 1;

        public static int DecodeWidthMaxLog2 = (int)Math.Log2(9999);
        public static int DecodeWidthMinLog2 = 0;

        public static int IssueWidthMaxLog2 = (int)Math.Log2(9999);
        public static int IssueWidthMinLog2 = 0;

        public static int IssueInOrderMax = 1;
        public static int IssueInOrderMin = 0;

        public static int CommitWidthMax = 9999;
        public static int CommitWidthMin = 1;

        public static int ReorderBufferSizeMax = 9999;
        public static int ReorderBufferSizeMin = 1;

        public static int IssueQueueSizeMax = 9999;
        public static int IssueQueueSizeMin = 1;

        public static int RegisterFileSizeMax = 9999;
        public static int RegisterFileSizeMin = 34;

        public static int LoadStoreQueueSizeMax = 9999;
        public static int LoadStoreQueueSizeMin = 1;


        public static int CacheDl1NumSetsMaxLog2 = (int)Math.Log2(9999);
        public static int CacheDl1NumSetsMinLog2 = 0;

        public static int CacheDl1BlockOrPageSizeMax = 9999;
        public static int CacheDl1BlockOrPageSizeMin = 1;

        public static int CacheDl1AssociativityMax = 9999;
        public static int CacheDl1AssociativityMin = 1;

        public static int CacheDl1ReplacementPolicyMax = 2;
        public static int CacheDl1ReplacementPolicyMin = 0;


        public static int CacheDl2NumSetsMaxLog2 = (int)Math.Log2(9999);
        public static int CacheDl2NumSetsMinLog2 = 0;

        public static int CacheDl2BlockOrPageSizeMax = 9999;
        public static int CacheDl2BlockOrPageSizeMin = 1;

        public static int CacheDl2AssociativityMax = 9999;
        public static int CacheDl2AssociativityMin = 1;

        public static int CacheDl2ReplacementPolicyMax = 2;
        public static int CacheDl2ReplacementPolicyMin = 0;


        public static int CacheIl1NumSetsMaxLog2 = (int)Math.Log2(9999);
        public static int CacheIl1NumSetsMinLog2 = 0;

        public static int CacheIl1BlockOrPageSizeMax = 9999;
        public static int CacheIl1BlockOrPageSizeMin = 1;

        public static int CacheIl1AssociativityMax = 9999;
        public static int CacheIl1AssociativityMin = 1;

        public static int CacheIl1ReplacementPolicyMax = 2;
        public static int CacheIl1ReplacementPolicyMin = 0;


        public static int CacheIl2NumSetsMaxLog2 = (int)Math.Log2(9999);
        public static int CacheIl2NumSetsMinLog2 = 0;

        public static int CacheIl2BlockOrPageSizeMax = 9999;
        public static int CacheIl2BlockOrPageSizeMin = 1;

        public static int CacheIl2AssociativityMax = 9999;
        public static int CacheIl2AssociativityMin = 1;

        public static int CacheIl2ReplacementPolicyMax = 2;
        public static int CacheIl2ReplacementPolicyMin = 0;


        public static int MemBusWidthMax = 9999;
        public static int MemBusWidthMin = 1;


        public static int TlbItlbNumSetsMaxLog2 = (int)Math.Log2(9999);
        public static int TlbItlbNumSetsMinLog2 = 0;

        public static int TlbItlbBlockOrPageSizeMax = 9999;
        public static int TlbItlbBlockOrPageSizeMin = 1;

        public static int TlbItlbAssociativityMax = 9999;
        public static int TlbItlbAssociativityMin = 1;

        public static int TlbItlbReplacementPolicyMax = 2;
        public static int TlbItlbReplacementPolicyMin = 0;


        public static int TlbDtlbNumSetsMaxLog2 = (int)Math.Log2(9999);
        public static int TlbDtlbNumSetsMinLog2 = 0;

        public static int TlbDtlbBlockOrPageSizeMax = 9999;
        public static int TlbDtlbBlockOrPageSizeMin = 1;

        public static int TlbDtlbAssociativityMax = 9999;
        public static int TlbDtlbAssociativityMin = 1;

        public static int TlbDtlbReplacementPolicyMax = 2;
        public static int TlbDtlbReplacementPolicyMin = 0;


        public static int ResIntegerAluMax = 8;
        public static int ResIntegerAluMin = 1;

        public static int ResIntegerMultDivMax = 8;
        public static int ResIntegerMultDivMin = 1;

        public static int ResMemoryPortsMax = 8;
        public static int ResMemoryPortsMin = 1;

        public static int ResFpAluMax = 8;
        public static int ResFpAluMin = 1;

        public static int ResFpMultDivMax = 8;
        public static int ResFpMultDivMin = 1;

        public static int GetMin(int index)
        {
            return index switch
            {
                0 => BranchPredictorTypeMin,
                1 => BpredBimodTableSizeMin,
                2 => Bpred2LevConfigL1SizeMinLog2,
                3 => Bpred2LevConfigL2SizeMinLog2,
                4 => Bpred2LevConfigHistorySizeMin,
                5 => Bpred2LevConfigUseXorMin,
                6 => BpredCombMetaTableSizeMin,
                7 => BpredReturnAddressStackSizeMin,
                8 => BpredBtbConfigNumSetsMin,
                9 => BpredBtbConfigAssociativityMin,
                10 => BpredSpeculativeUpdateMin,
                11 => CacheLoadPredictorTypeMin,
                12 => CpredBimodTableSizeMin,
                13 => Cpred2LevConfigL1SizeMinLog2,
                14 => Cpred2LevConfigL2SizeMinLog2,
                15 => Cpred2LevConfigHistorySizeMin,
                16 => Cpred2LevConfigUseXorMin,
                17 => CpredCombMetaTableSizeMin,
                18 => CpredReturnAddressStackSizeMin,
                19 => CpredBtbConfigNumSetsMin,
                20 => CpredBtbConfigAssociativityMin,
                21 => DecodeWidthMinLog2,
                22 => IssueWidthMinLog2,
                23 => IssueInOrderMin,
                24 => CommitWidthMin,
                25 => ReorderBufferSizeMin,
                26 => IssueQueueSizeMin,
                27 => RegisterFileSizeMin,
                28 => LoadStoreQueueSizeMin,
                29 => CacheDl1NumSetsMinLog2,
                30 => CacheDl1BlockOrPageSizeMin,
                31 => CacheDl1AssociativityMin,
                32 => CacheDl1ReplacementPolicyMin,
                33 => CacheDl2NumSetsMinLog2,
                34 => CacheDl2BlockOrPageSizeMin,
                35 => CacheDl2AssociativityMin,
                36 => CacheDl2ReplacementPolicyMin,
                37 => CacheIl1NumSetsMinLog2,
                38 => CacheIl1BlockOrPageSizeMin,
                39 => CacheIl1AssociativityMin,
                40 => CacheIl1ReplacementPolicyMin,
                41 => CacheIl2NumSetsMinLog2,
                42 => CacheIl2BlockOrPageSizeMin,
                43 => CacheIl2AssociativityMin,
                44 => CacheIl2ReplacementPolicyMin,
                45 => (int)Math.Log2(MemBusWidthMin),
                46 => TlbItlbNumSetsMinLog2,
                47 => TlbItlbBlockOrPageSizeMin,
                48 => TlbItlbAssociativityMin,
                49 => TlbItlbReplacementPolicyMin,
                50 => TlbDtlbNumSetsMinLog2,
                51 => TlbDtlbBlockOrPageSizeMin,
                52 => TlbDtlbAssociativityMin,
                53 => TlbDtlbReplacementPolicyMin,
                54 => ResIntegerAluMin,
                55 => ResIntegerMultDivMin,
                56 => ResMemoryPortsMin,
                57 => ResFpAluMin,
                58 => ResFpMultDivMin,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }

        public static int GetMax(int index)
        {
            return index switch
            {
                0 => BranchPredictorTypeMax,
                1 => BpredBimodTableSizeMax,
                2 => Bpred2LevConfigL1SizeMaxLog2,
                3 => Bpred2LevConfigL2SizeMaxLog2,
                4 => Bpred2LevConfigHistorySizeMax,
                5 => Bpred2LevConfigUseXorMax,
                6 => BpredCombMetaTableSizeMax,
                7 => BpredReturnAddressStackSizeMax,
                8 => BpredBtbConfigNumSetsMax,
                9 => BpredBtbConfigAssociativityMax,
                10 => BpredSpeculativeUpdateMax,
                11 => CacheLoadPredictorTypeMax,
                12 => CpredBimodTableSizeMax,
                13 => Cpred2LevConfigL1SizeMaxLog2,
                14 => Cpred2LevConfigL2SizeMaxLog2,
                15 => Cpred2LevConfigHistorySizeMax,
                16 => Cpred2LevConfigUseXorMax,
                17 => CpredCombMetaTableSizeMax,
                18 => CpredReturnAddressStackSizeMax,
                19 => CpredBtbConfigNumSetsMax,
                20 => CpredBtbConfigAssociativityMax,
                21 => DecodeWidthMaxLog2,
                22 => IssueWidthMaxLog2,
                23 => IssueInOrderMax,
                24 => CommitWidthMax,
                25 => ReorderBufferSizeMax,
                26 => IssueQueueSizeMax,
                27 => RegisterFileSizeMax,
                28 => LoadStoreQueueSizeMax,
                29 => CacheDl1NumSetsMaxLog2,
                30 => CacheDl1BlockOrPageSizeMax,
                31 => CacheDl1AssociativityMax,
                32 => CacheDl1ReplacementPolicyMax,
                33 => CacheDl2NumSetsMaxLog2,
                34 => CacheDl2BlockOrPageSizeMax,
                35 => CacheDl2AssociativityMax,
                36 => CacheDl2ReplacementPolicyMax,
                37 => CacheIl1NumSetsMaxLog2,
                38 => CacheIl1BlockOrPageSizeMax,
                39 => CacheIl1AssociativityMax,
                40 => CacheIl1ReplacementPolicyMax,
                41 => CacheIl2NumSetsMaxLog2,
                42 => CacheIl2BlockOrPageSizeMax,
                43 => CacheIl2AssociativityMax,
                44 => CacheIl2ReplacementPolicyMax,
                45 => (int)Math.Log2(MemBusWidthMax),
                46 => TlbItlbNumSetsMaxLog2,
                47 => TlbItlbBlockOrPageSizeMax,
                48 => TlbItlbAssociativityMax,
                49 => TlbItlbReplacementPolicyMax,
                50 => TlbDtlbNumSetsMaxLog2,
                51 => TlbDtlbBlockOrPageSizeMax,
                52 => TlbDtlbAssociativityMax,
                53 => TlbDtlbReplacementPolicyMax,
                54 => ResIntegerAluMax,
                55 => ResIntegerMultDivMax,
                56 => ResMemoryPortsMax,
                57 => ResFpAluMax,
                58 => ResFpMultDivMax,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }

    }
}