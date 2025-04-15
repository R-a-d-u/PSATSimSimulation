using System.Configuration;
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
    public IntPowerOf2 DecodeWidth { get; set; } // -decode:width <int>

    /// <summary>
    /// instruction issue B/W (insts/cycle)
    /// </summary>
    public IntPowerOf2 IssueWidth { get; set; } // -issue:width <int>

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
    public IntPowerOf2 MemBusWidth { get; set; } // -mem:width <int>

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
        IntPowerOf2 decodeWidth,
        IntPowerOf2 issueWidth,
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
        IntPowerOf2 memBusWidth,
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
        ThrowIfOutOfRange(nameof(commitWidth), commitWidth, CPUConfigLimits.CommitWidthMin, CPUConfigLimits.CommitWidthMax);
        ThrowIfOutOfRange(nameof(reorderBufferSize), reorderBufferSize, CPUConfigLimits.ReorderBufferSizeMin, CPUConfigLimits.ReorderBufferSizeMax);
        ThrowIfOutOfRange(nameof(issueQueueSize), issueQueueSize, CPUConfigLimits.IssueQueueSizeMin, CPUConfigLimits.IssueQueueSizeMax);
        ThrowIfOutOfRange(nameof(registerFileSize), registerFileSize, CPUConfigLimits.RegisterFileSizeMin, CPUConfigLimits.RegisterFileSizeMax);
        ThrowIfOutOfRange(nameof(loadStoreQueueSize), loadStoreQueueSize, CPUConfigLimits.LoadStoreQueueSizeMin, CPUConfigLimits.LoadStoreQueueSizeMax);
        ThrowIfOutOfRange(nameof(resIntegerAlu), resIntegerAlu, CPUConfigLimits.ResIntegerAluMin, CPUConfigLimits.ResIntegerAluMax);
        ThrowIfOutOfRange(nameof(resIntegerMultDiv), resIntegerMultDiv, CPUConfigLimits.ResIntegerMultDivMin, CPUConfigLimits.ResIntegerMultDivMax);
        ThrowIfOutOfRange(nameof(resMemoryPorts), resMemoryPorts, CPUConfigLimits.ResMemoryPortsMin, CPUConfigLimits.ResMemoryPortsMax);
        ThrowIfOutOfRange(nameof(resFpAlu), resFpAlu, CPUConfigLimits.ResFpAluMin, CPUConfigLimits.ResFpAluMax);
        ThrowIfOutOfRange(nameof(resFpMultDiv), resFpMultDiv, CPUConfigLimits.ResFpMultDivMin, CPUConfigLimits.ResFpMultDivMax);

        // Validate nested config classes
        bpredBtbConfig.Validate(CPUConfigLimits.BpredBtbConfigNumSetsMin, CPUConfigLimits.BpredBtbConfigNumSetsMax, CPUConfigLimits.BpredBtbConfigAssociativityMin, CPUConfigLimits.BpredBtbConfigAssociativityMax);
        cpredBtbConfig.Validate(CPUConfigLimits.CpredBtbConfigNumSetsMin, CPUConfigLimits.CpredBtbConfigNumSetsMax, CPUConfigLimits.CpredBtbConfigAssociativityMin, CPUConfigLimits.CpredBtbConfigAssociativityMax);
        cacheDl1.Validate(CPUConfigLimits.CacheDl1BlockOrPageSizeMin, CPUConfigLimits.CacheDl1BlockOrPageSizeMax, CPUConfigLimits.CacheDl1AssociativityMin, CPUConfigLimits.CacheDl1AssociativityMax);
        cacheDl2.Validate(CPUConfigLimits.CacheDl2BlockOrPageSizeMin, CPUConfigLimits.CacheDl2BlockOrPageSizeMax, CPUConfigLimits.CacheDl2AssociativityMin, CPUConfigLimits.CacheDl2AssociativityMax);
        cacheIl1.Validate(CPUConfigLimits.CacheIl1BlockOrPageSizeMin, CPUConfigLimits.CacheIl1BlockOrPageSizeMax, CPUConfigLimits.CacheIl1AssociativityMin, CPUConfigLimits.CacheIl1AssociativityMax);
        cacheIl2.Validate(CPUConfigLimits.CacheIl2BlockOrPageSizeMin, CPUConfigLimits.CacheIl2BlockOrPageSizeMax, CPUConfigLimits.CacheIl2AssociativityMin, CPUConfigLimits.CacheIl2AssociativityMax);
        tlbItlb.Validate(CPUConfigLimits.TlbItlbBlockOrPageSizeMin, CPUConfigLimits.TlbItlbBlockOrPageSizeMax, CPUConfigLimits.TlbItlbAssociativityMin, CPUConfigLimits.TlbItlbAssociativityMax);
        tlbDtlb.Validate(CPUConfigLimits.TlbDtlbBlockOrPageSizeMin, CPUConfigLimits.TlbDtlbBlockOrPageSizeMax, CPUConfigLimits.TlbDtlbAssociativityMin, CPUConfigLimits.TlbDtlbAssociativityMax);

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

    private static IntPowerOf2 GetRandomIntPowerOf2(Random random, IntPowerOf2 min, IntPowerOf2 max) {
        return new(new IntLog2(random.Next((int)min.GetValueLog2().Value, (int)max.GetValueLog2().Value + 1)));
    }

    public static CPUConfig GenerateRandom()
    {
        Random random = new();

        var bpred2predcfg = new Predictor2LevConfig( // bpred2levconfig
                GetRandomIntPowerOf2(random, CPUConfigLimits.Bpred2LevConfigL1SizeMin, CPUConfigLimits.Bpred2LevConfigL1SizeMax),
                GetRandomIntPowerOf2(random, CPUConfigLimits.Bpred2LevConfigL2SizeMin, CPUConfigLimits.Bpred2LevConfigL2SizeMax),
                GetRandomIntPowerOf2(random, CPUConfigLimits.Bpred2LevConfigHistorySizeMin, CPUConfigLimits.Bpred2LevConfigHistorySizeMax),
                random.Next(0, 2) == 1 // useXor
        );
        
        var cpred2predcfg = new Predictor2LevConfig( // cpred2levconfig
                GetRandomIntPowerOf2(random, CPUConfigLimits.Cpred2LevConfigL1SizeMin, CPUConfigLimits.Cpred2LevConfigL1SizeMax),
                GetRandomIntPowerOf2(random, CPUConfigLimits.Cpred2LevConfigL2SizeMin, CPUConfigLimits.Cpred2LevConfigL2SizeMax),
                GetRandomIntPowerOf2(random, CPUConfigLimits.Cpred2LevConfigHistorySizeMin, CPUConfigLimits.Cpred2LevConfigHistorySizeMax),
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
            GetRandomIntPowerOf2(random, CPUConfigLimits.DecodeWidthMin, CPUConfigLimits.DecodeWidthMax),
            GetRandomIntPowerOf2(random, CPUConfigLimits.IssueWidthMin, CPUConfigLimits.IssueWidthMax),
            random.Next(0, 2) == 1,
            random.Next(CPUConfigLimits.CommitWidthMin, CPUConfigLimits.CommitWidthMax + 1),
            random.Next(CPUConfigLimits.ReorderBufferSizeMin, CPUConfigLimits.ReorderBufferSizeMax + 1),
            random.Next(CPUConfigLimits.IssueQueueSizeMin, CPUConfigLimits.IssueQueueSizeMax + 1),
            random.Next(CPUConfigLimits.RegisterFileSizeMin, CPUConfigLimits.RegisterFileSizeMax + 1),
            random.Next(CPUConfigLimits.LoadStoreQueueSizeMin, CPUConfigLimits.LoadStoreQueueSizeMax + 1),
            new CacheTlbConfig(
                "dl1",
                GetRandomIntPowerOf2(random, CPUConfigLimits.CacheDl1NumSetsMin, CPUConfigLimits.CacheDl1NumSetsMax),
                random.Next(CPUConfigLimits.CacheDl1BlockOrPageSizeMin, CPUConfigLimits.CacheDl1BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheDl1AssociativityMin, CPUConfigLimits.CacheDl1AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheDl1ReplacementPolicyMin, CPUConfigLimits.CacheDl1ReplacementPolicyMax + 1)
            ),
            new CacheTlbConfig(
                "dl2",
                GetRandomIntPowerOf2(random, CPUConfigLimits.CacheDl2NumSetsMin, CPUConfigLimits.CacheDl2NumSetsMax),
                random.Next(CPUConfigLimits.CacheDl2BlockOrPageSizeMin, CPUConfigLimits.CacheDl2BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheDl2AssociativityMin, CPUConfigLimits.CacheDl2AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheDl2ReplacementPolicyMin, CPUConfigLimits.CacheDl2ReplacementPolicyMax + 1)
            ),
            new CacheTlbConfig(
                "il1",
                GetRandomIntPowerOf2(random, CPUConfigLimits.CacheIl1NumSetsMin, CPUConfigLimits.CacheIl1NumSetsMax),
                random.Next(CPUConfigLimits.CacheIl1BlockOrPageSizeMin, CPUConfigLimits.CacheIl1BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheIl1AssociativityMin, CPUConfigLimits.CacheIl1AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheIl1ReplacementPolicyMin, CPUConfigLimits.CacheIl1ReplacementPolicyMax + 1)
            ),
            new CacheTlbConfig(
                "il2",
                GetRandomIntPowerOf2(random, CPUConfigLimits.CacheIl2NumSetsMin, CPUConfigLimits.CacheIl2NumSetsMax),
                random.Next(CPUConfigLimits.CacheIl2BlockOrPageSizeMin, CPUConfigLimits.CacheIl2BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheIl2AssociativityMin, CPUConfigLimits.CacheIl2AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheIl2ReplacementPolicyMin, CPUConfigLimits.CacheIl2ReplacementPolicyMax + 1)
            ),
            GetRandomIntPowerOf2(random, CPUConfigLimits.MemBusWidthMin, CPUConfigLimits.MemBusWidthMax),
            new CacheTlbConfig(
                "itlb",
                GetRandomIntPowerOf2(random, CPUConfigLimits.TlbItlbNumSetsMin,CPUConfigLimits.TlbItlbNumSetsMax),
                random.Next(CPUConfigLimits.TlbItlbBlockOrPageSizeMin, CPUConfigLimits.TlbItlbBlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.TlbItlbAssociativityMin, CPUConfigLimits.TlbItlbAssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.TlbItlbReplacementPolicyMin, CPUConfigLimits.TlbItlbReplacementPolicyMin + 1)
            ),
            new CacheTlbConfig(
                "dtlb",
                GetRandomIntPowerOf2(random, CPUConfigLimits.TlbDtlbNumSetsMin,CPUConfigLimits.TlbDtlbNumSetsMax),
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
            Bpred2LevConfig.L1Size.GetValueLog2().Value,
            Bpred2LevConfig.L2Size.GetValueLog2().Value,
            Bpred2LevConfig.HistorySize.GetValueLog2().Value,
            Bpred2LevConfig.UseXor ? 1 : 0,
            BpredCombMetaTableSize,
            BpredReturnAddressStackSize,
            BpredBtbConfig.NumSets,
            BpredBtbConfig.Associativity,
            (int)BpredSpeculativeUpdate,
            (int)CacheLoadPredictorType,
            CpredBimodTableSize,
            Cpred2LevConfig.L1Size.GetValueLog2().Value,
            Cpred2LevConfig.L2Size.GetValueLog2().Value,
            Cpred2LevConfig.HistorySize.GetValueLog2().Value,
            Cpred2LevConfig.UseXor ? 1 : 0,
            CpredCombMetaTableSize,
            CpredReturnAddressStackSize,
            CpredBtbConfig.NumSets,
            CpredBtbConfig.Associativity,
            DecodeWidth.GetValueLog2().Value,
            IssueWidth.GetValueLog2().Value,
            IssueInOrder ? 1 : 0,
            CommitWidth,
            ReorderBufferSize,
            IssueQueueSize,
            RegisterFileSize,
            LoadStoreQueueSize,
            CacheDl1.NumSets.GetValueLog2().Value,
            CacheDl1.BlockOrPageSize,
            CacheDl1.Associativity,
            (int)CacheDl1.ReplacementPolicy,
            CacheDl2.NumSets.GetValueLog2().Value,
            CacheDl2.BlockOrPageSize,
            CacheDl2.Associativity,
            (int)CacheDl2.ReplacementPolicy,
            CacheIl1.NumSets.GetValueLog2().Value,
            CacheIl1.BlockOrPageSize,
            CacheIl1.Associativity,
            (int)CacheIl1.ReplacementPolicy,
            CacheIl2.NumSets.GetValueLog2().Value,
            CacheIl2.BlockOrPageSize,
            CacheIl2.Associativity,
            (int)CacheIl2.ReplacementPolicy,
            MemBusWidth.GetValueLog2().Value,
            TlbItlb.NumSets.GetValueLog2().Value,
            TlbItlb.BlockOrPageSize,
            TlbItlb.Associativity,
            (int)TlbItlb.ReplacementPolicy,
            TlbDtlb.NumSets.GetValueLog2().Value,
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
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.Bpred2LevConfigL1SizeMin, CPUConfigLimits.Bpred2LevConfigL1SizeMax),
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.Bpred2LevConfigL2SizeMin, CPUConfigLimits.Bpred2LevConfigL2SizeMax),
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.Bpred2LevConfigHistorySizeMin, CPUConfigLimits.Bpred2LevConfigHistorySizeMax),
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
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.Cpred2LevConfigL1SizeMin, CPUConfigLimits.Cpred2LevConfigL1SizeMax),
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.Cpred2LevConfigL2SizeMin, CPUConfigLimits.Cpred2LevConfigL2SizeMax),
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.Cpred2LevConfigHistorySizeMin, CPUConfigLimits.Cpred2LevConfigHistorySizeMax),
            vector[idx++] != 0
        );
        var cpredCombMetaTableSize = (int)vector[idx++];
        var cpredReturnAddressStackSize = (int)vector[idx++];
        var cpredBtbConfig = new BtbConfig
        (
            (int)vector[idx++],
            (int)vector[idx++]
        );
        IntPowerOf2 decodeWidth = new(new IntLog2((int)vector[idx++]), CPUConfigLimits.DecodeWidthMin, CPUConfigLimits.DecodeWidthMax);
        IntPowerOf2 issueWidth = new(new IntLog2((int)vector[idx++]), CPUConfigLimits.IssueWidthMin, CPUConfigLimits.IssueWidthMax);
        var issueInOrder = vector[idx++] != 0;
        var commitWidth = (int)vector[idx++];
        var reorderBufferSize = (int)vector[idx++];
        var issueQueueSize = (int)vector[idx++];
        var registerFileSize = (int)vector[idx++];
        var loadStoreQueueSize = (int)vector[idx++];
        var cacheDl1 = new CacheTlbConfig
        (
            "dl1",
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.CacheDl1NumSetsMin, CPUConfigLimits.CacheDl1NumSetsMax),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++] // assuming the ReplacementPolicy is an enum of type CacheReplacementPolicyEnum
        );
        var cacheDl2 = new CacheTlbConfig
        (
            "dl2",
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.CacheDl2NumSetsMin, CPUConfigLimits.CacheDl2NumSetsMax),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++]
        );
        var cacheIl1 = new CacheTlbConfig
        (
            "il1",
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.CacheIl1NumSetsMin, CPUConfigLimits.CacheIl1NumSetsMax),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++]
        );
        var cacheIl2 = new CacheTlbConfig
        (
            "il2",
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.CacheIl2NumSetsMin, CPUConfigLimits.CacheIl2NumSetsMax),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++]
        );
        IntPowerOf2 memBusWidth = new(new IntLog2((int)vector[idx++]), CPUConfigLimits.MemBusWidthMin, CPUConfigLimits.MemBusWidthMax);
        var tlbItlb = new CacheTlbConfig
        (
            "itlb",
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.TlbItlbNumSetsMin, CPUConfigLimits.TlbItlbNumSetsMax),
            (int)vector[idx++],
            (int)vector[idx++],
            (ReplacementPolicyEnum)(int)vector[idx++]
        );
        var tlbDtlb = new CacheTlbConfig
        (
            "dtlb",
            new(new IntLog2((int)vector[idx++]), CPUConfigLimits.TlbDtlbNumSetsMin, CPUConfigLimits.TlbDtlbNumSetsMax),
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

        public static IntPowerOf2 Bpred2LevConfigL1SizeMax = new(8192);
        public static IntPowerOf2 Bpred2LevConfigL1SizeMin = new(1);

        public static IntPowerOf2 Bpred2LevConfigL2SizeMax = new(8192);
        public static IntPowerOf2 Bpred2LevConfigL2SizeMin = new(1);

        public static IntPowerOf2 Bpred2LevConfigHistorySizeMax = new(8192);
        public static IntPowerOf2 Bpred2LevConfigHistorySizeMin = new(1);

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

        public static IntPowerOf2 Cpred2LevConfigL1SizeMax = new(8192);
        public static IntPowerOf2 Cpred2LevConfigL1SizeMin = new(1);

        public static IntPowerOf2 Cpred2LevConfigL2SizeMax = new(8192);
        public static IntPowerOf2 Cpred2LevConfigL2SizeMin = new(1);

        public static IntPowerOf2 Cpred2LevConfigHistorySizeMax = new(8192);
        public static IntPowerOf2 Cpred2LevConfigHistorySizeMin = new(1);

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

        public static IntPowerOf2 DecodeWidthMax = new(8192);
        public static IntPowerOf2 DecodeWidthMin = new(1);

        public static IntPowerOf2 IssueWidthMax = new(8192);
        public static IntPowerOf2 IssueWidthMin = new(1);

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


        public static IntPowerOf2 CacheDl1NumSetsMax = new(8192);
        public static IntPowerOf2 CacheDl1NumSetsMin = new(1);

        public static int CacheDl1BlockOrPageSizeMax = 9999;
        public static int CacheDl1BlockOrPageSizeMin = 1;

        public static int CacheDl1AssociativityMax = 9999;
        public static int CacheDl1AssociativityMin = 1;

        public static int CacheDl1ReplacementPolicyMax = 2;
        public static int CacheDl1ReplacementPolicyMin = 0;


        public static IntPowerOf2 CacheDl2NumSetsMax = new(8192);
        public static IntPowerOf2 CacheDl2NumSetsMin = new(1);

        public static int CacheDl2BlockOrPageSizeMax = 9999;
        public static int CacheDl2BlockOrPageSizeMin = 1;

        public static int CacheDl2AssociativityMax = 9999;
        public static int CacheDl2AssociativityMin = 1;

        public static int CacheDl2ReplacementPolicyMax = 2;
        public static int CacheDl2ReplacementPolicyMin = 0;


        public static IntPowerOf2 CacheIl1NumSetsMax = new(8192);
        public static IntPowerOf2 CacheIl1NumSetsMin = new(1);

        public static int CacheIl1BlockOrPageSizeMax = 9999;
        public static int CacheIl1BlockOrPageSizeMin = 1;

        public static int CacheIl1AssociativityMax = 9999;
        public static int CacheIl1AssociativityMin = 1;

        public static int CacheIl1ReplacementPolicyMax = 2;
        public static int CacheIl1ReplacementPolicyMin = 0;


        public static IntPowerOf2 CacheIl2NumSetsMax = new(8192);
        public static IntPowerOf2 CacheIl2NumSetsMin = new(1);

        public static int CacheIl2BlockOrPageSizeMax = 9999;
        public static int CacheIl2BlockOrPageSizeMin = 1;

        public static int CacheIl2AssociativityMax = 9999;
        public static int CacheIl2AssociativityMin = 1;

        public static int CacheIl2ReplacementPolicyMax = 2;
        public static int CacheIl2ReplacementPolicyMin = 0;


        public static IntPowerOf2 MemBusWidthMax = new(8192);
        public static IntPowerOf2 MemBusWidthMin = new(1);


        public static IntPowerOf2 TlbItlbNumSetsMax = new(8192);
        public static IntPowerOf2 TlbItlbNumSetsMin = new(1);

        public static int TlbItlbBlockOrPageSizeMax = 9999;
        public static int TlbItlbBlockOrPageSizeMin = 1;

        public static int TlbItlbAssociativityMax = 9999;
        public static int TlbItlbAssociativityMin = 1;

        public static int TlbItlbReplacementPolicyMax = 2;
        public static int TlbItlbReplacementPolicyMin = 0;


        public static IntPowerOf2 TlbDtlbNumSetsMax = new(8192);
        public static IntPowerOf2 TlbDtlbNumSetsMin = new(1);

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
                2 => (int)Bpred2LevConfigL1SizeMin.GetValueLog2().Value,
                3 => (int)Bpred2LevConfigL2SizeMin.GetValueLog2().Value,
                4 => (int)Bpred2LevConfigHistorySizeMin.GetValueLog2().Value,
                5 => Bpred2LevConfigUseXorMin,
                6 => BpredCombMetaTableSizeMin,
                7 => BpredReturnAddressStackSizeMin,
                8 => BpredBtbConfigNumSetsMin,
                9 => BpredBtbConfigAssociativityMin,
                10 => BpredSpeculativeUpdateMin,
                11 => CacheLoadPredictorTypeMin,
                12 => CpredBimodTableSizeMin,
                13 => (int)Cpred2LevConfigL1SizeMin.GetValueLog2().Value,
                14 => (int)Cpred2LevConfigL2SizeMin.GetValueLog2().Value,
                15 => (int)Cpred2LevConfigHistorySizeMin.GetValueLog2().Value,
                16 => Cpred2LevConfigUseXorMin,
                17 => CpredCombMetaTableSizeMin,
                18 => CpredReturnAddressStackSizeMin,
                19 => CpredBtbConfigNumSetsMin,
                20 => CpredBtbConfigAssociativityMin,
                21 => (int)DecodeWidthMin.GetValueLog2().Value,
                22 => (int)IssueWidthMin.GetValueLog2().Value,
                23 => IssueInOrderMin,
                24 => CommitWidthMin,
                25 => ReorderBufferSizeMin,
                26 => IssueQueueSizeMin,
                27 => RegisterFileSizeMin,
                28 => LoadStoreQueueSizeMin,
                29 => (int)CacheDl1NumSetsMin.GetValueLog2().Value,
                30 => CacheDl1BlockOrPageSizeMin,
                31 => CacheDl1AssociativityMin,
                32 => CacheDl1ReplacementPolicyMin,
                33 => (int)CacheDl2NumSetsMin.GetValueLog2().Value,
                34 => CacheDl2BlockOrPageSizeMin,
                35 => CacheDl2AssociativityMin,
                36 => CacheDl2ReplacementPolicyMin,
                37 => (int)CacheIl1NumSetsMin.GetValueLog2().Value,
                38 => CacheIl1BlockOrPageSizeMin,
                39 => CacheIl1AssociativityMin,
                40 => CacheIl1ReplacementPolicyMin,
                41 => (int)CacheIl2NumSetsMin.GetValueLog2().Value,
                42 => CacheIl2BlockOrPageSizeMin,
                43 => CacheIl2AssociativityMin,
                44 => CacheIl2ReplacementPolicyMin,
                45 => (int)MemBusWidthMin.GetValueLog2().Value,
                46 => (int)TlbItlbNumSetsMin.GetValueLog2().Value,
                47 => TlbItlbBlockOrPageSizeMin,
                48 => TlbItlbAssociativityMin,
                49 => TlbItlbReplacementPolicyMin,
                50 => (int)TlbDtlbNumSetsMin.GetValueLog2().Value,
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
                2 => (int)Bpred2LevConfigL1SizeMax.GetValueLog2().Value,
                3 => (int)Bpred2LevConfigL2SizeMax.GetValueLog2().Value,
                4 => (int)Bpred2LevConfigHistorySizeMax.GetValueLog2().Value,
                5 => Bpred2LevConfigUseXorMax,
                6 => BpredCombMetaTableSizeMax,
                7 => BpredReturnAddressStackSizeMax,
                8 => BpredBtbConfigNumSetsMax,
                9 => BpredBtbConfigAssociativityMax,
                10 => BpredSpeculativeUpdateMax,
                11 => CacheLoadPredictorTypeMax,
                12 => CpredBimodTableSizeMax,
                13 => (int)Cpred2LevConfigL1SizeMax.GetValueLog2().Value,
                14 => (int)Cpred2LevConfigL2SizeMax.GetValueLog2().Value,
                15 => (int)Cpred2LevConfigHistorySizeMax.GetValueLog2().Value,
                16 => Cpred2LevConfigUseXorMax,
                17 => CpredCombMetaTableSizeMax,
                18 => CpredReturnAddressStackSizeMax,
                19 => CpredBtbConfigNumSetsMax,
                20 => CpredBtbConfigAssociativityMax,
                21 => (int)DecodeWidthMax.GetValueLog2().Value,
                22 => (int)IssueWidthMax.GetValueLog2().Value,
                23 => IssueInOrderMax,
                24 => CommitWidthMax,
                25 => ReorderBufferSizeMax,
                26 => IssueQueueSizeMax,
                27 => RegisterFileSizeMax,
                28 => LoadStoreQueueSizeMax,
                29 => (int)CacheDl1NumSetsMax.GetValueLog2().Value,
                30 => CacheDl1BlockOrPageSizeMax,
                31 => CacheDl1AssociativityMax,
                32 => CacheDl1ReplacementPolicyMax,
                33 => (int)CacheDl2NumSetsMax.GetValueLog2().Value,
                34 => CacheDl2BlockOrPageSizeMax,
                35 => CacheDl2AssociativityMax,
                36 => CacheDl2ReplacementPolicyMax,
                37 => (int)CacheIl1NumSetsMax.GetValueLog2().Value,
                38 => CacheIl1BlockOrPageSizeMax,
                39 => CacheIl1AssociativityMax,
                40 => CacheIl1ReplacementPolicyMax,
                41 => (int)CacheIl2NumSetsMax.GetValueLog2().Value,
                42 => CacheIl2BlockOrPageSizeMax,
                43 => CacheIl2AssociativityMax,
                44 => CacheIl2ReplacementPolicyMax,
                45 => (int)MemBusWidthMax.GetValueLog2().Value,
                46 => (int)TlbItlbNumSetsMax.GetValueLog2().Value,
                47 => TlbItlbBlockOrPageSizeMax,
                48 => TlbItlbAssociativityMax,
                49 => TlbItlbReplacementPolicyMax,
                50 => (int)TlbDtlbNumSetsMax.GetValueLog2().Value,
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