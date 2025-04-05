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
        ThrowIfOutOfRange(nameof(decodeWidth), decodeWidth, CPUConfigLimits.DecodeWidthMin, CPUConfigLimits.DecodeWidthMax);
        ThrowIfOutOfRange(nameof(issueWidth), issueWidth, CPUConfigLimits.IssueWidthMin, CPUConfigLimits.IssueWidthMax);
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
        bpred2LevConfig.Validate(CPUConfigLimits.Bpred2LevConfigL1SizeMin, CPUConfigLimits.Bpred2LevConfigL1SizeMax, CPUConfigLimits.Bpred2LevConfigL2SizeMin, CPUConfigLimits.Bpred2LevConfigL2SizeMax, CPUConfigLimits.Bpred2LevConfigHistorySizeMin, CPUConfigLimits.Bpred2LevConfigHistorySizeMax);
        bpredBtbConfig.Validate(CPUConfigLimits.BpredBtbConfigNumSetsMin, CPUConfigLimits.BpredBtbConfigNumSetsMax, CPUConfigLimits.BpredBtbConfigAssociativityMin, CPUConfigLimits.BpredBtbConfigAssociativityMax);
        cpred2LevConfig.Validate(CPUConfigLimits.Cpred2LevConfigL1SizeMin, CPUConfigLimits.Cpred2LevConfigL1SizeMax, CPUConfigLimits.Cpred2LevConfigL2SizeMin, CPUConfigLimits.Cpred2LevConfigL2SizeMax, CPUConfigLimits.Cpred2LevConfigHistorySizeMin, CPUConfigLimits.Cpred2LevConfigHistorySizeMax);
        cpredBtbConfig.Validate(CPUConfigLimits.CpredBtbConfigNumSetsMin, CPUConfigLimits.CpredBtbConfigNumSetsMax, CPUConfigLimits.CpredBtbConfigAssociativityMin, CPUConfigLimits.CpredBtbConfigAssociativityMax);
        cacheDl1.Validate(CPUConfigLimits.CacheDl1NumSetsMin, CPUConfigLimits.CacheDl1NumSetsMax, CPUConfigLimits.CacheDl1BlockOrPageSizeMin, CPUConfigLimits.CacheDl1BlockOrPageSizeMax, CPUConfigLimits.CacheDl1AssociativityMin, CPUConfigLimits.CacheDl1AssociativityMax);
        cacheDl2.Validate(CPUConfigLimits.CacheDl2NumSetsMin, CPUConfigLimits.CacheDl2NumSetsMax, CPUConfigLimits.CacheDl2BlockOrPageSizeMin, CPUConfigLimits.CacheDl2BlockOrPageSizeMax, CPUConfigLimits.CacheDl2AssociativityMin, CPUConfigLimits.CacheDl2AssociativityMax);
        cacheIl1.Validate(CPUConfigLimits.CacheIl1NumSetsMin, CPUConfigLimits.CacheIl1NumSetsMax, CPUConfigLimits.CacheIl1BlockOrPageSizeMin, CPUConfigLimits.CacheIl1BlockOrPageSizeMax, CPUConfigLimits.CacheIl1AssociativityMin, CPUConfigLimits.CacheIl1AssociativityMax);
        cacheIl2.Validate(CPUConfigLimits.CacheIl2NumSetsMin, CPUConfigLimits.CacheIl2NumSetsMax, CPUConfigLimits.CacheIl2BlockOrPageSizeMin, CPUConfigLimits.CacheIl2BlockOrPageSizeMax, CPUConfigLimits.CacheIl2AssociativityMin, CPUConfigLimits.CacheIl2AssociativityMax);
        tlbItlb.Validate(CPUConfigLimits.TlbItlbNumSetsMin, CPUConfigLimits.TlbItlbNumSetsMax, CPUConfigLimits.TlbItlbBlockOrPageSizeMin, CPUConfigLimits.TlbItlbBlockOrPageSizeMax, CPUConfigLimits.TlbItlbAssociativityMin, CPUConfigLimits.TlbItlbAssociativityMax);
        tlbDtlb.Validate(CPUConfigLimits.TlbDtlbNumSetsMin, CPUConfigLimits.TlbDtlbNumSetsMax, CPUConfigLimits.TlbDtlbBlockOrPageSizeMin, CPUConfigLimits.TlbDtlbBlockOrPageSizeMax, CPUConfigLimits.TlbDtlbAssociativityMin, CPUConfigLimits.TlbDtlbAssociativityMax);


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


    

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();

        // Add all properties that contribute to the object's identity.
        // HashCode.Add handles null checks for reference types and nullable value types automatically.
        hash.Add(BranchPredictorType);
        hash.Add(BpredBimodTableSize);
        hash.Add(Bpred2LevConfig);
        hash.Add(BpredCombMetaTableSize);
        hash.Add(BpredReturnAddressStackSize);
        hash.Add(BpredBtbConfig);
        hash.Add(BpredSpeculativeUpdate);
        hash.Add(CacheLoadPredictorType);
        hash.Add(CpredBimodTableSize);
        hash.Add(Cpred2LevConfig);
        hash.Add(CpredCombMetaTableSize);
        hash.Add(CpredReturnAddressStackSize);
        hash.Add(CpredBtbConfig);
        hash.Add(DecodeWidth);
        hash.Add(IssueWidth);
        hash.Add(IssueInOrder);
        hash.Add(CommitWidth);
        hash.Add(ReorderBufferSize);
        hash.Add(IssueQueueSize);
        hash.Add(RegisterFileSize);
        hash.Add(LoadStoreQueueSize);
        hash.Add(CacheDl1);
        hash.Add(CacheDl2);
        hash.Add(CacheIl1);
        hash.Add(CacheIl2);
        hash.Add(MemBusWidth);
        hash.Add(TlbItlb);
        hash.Add(TlbDtlb);
        hash.Add(ResIntegerAlu);
        hash.Add(ResIntegerMultDiv);
        hash.Add(ResMemoryPorts);
        hash.Add(ResFpAlu);
        hash.Add(ResFpMultDiv);

        return hash.ToHashCode();
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

    public static CPUConfig GenerateRandom()
    {
        Random random = new();

        return new CPUConfig(
            (BranchPredictorTypeEnum)random.Next(CPUConfigLimits.BranchPredictorTypeMin, CPUConfigLimits.BranchPredictorTypeMax + 1),
            random.Next(CPUConfigLimits.BpredBimodTableSizeMin, CPUConfigLimits.BpredBimodTableSizeMax + 1),
            new Predictor2LevConfig( // bpred2levconfig
                random.Next(CPUConfigLimits.Bpred2LevConfigL1SizeMin, CPUConfigLimits.Bpred2LevConfigL1SizeMax + 1),
                random.Next(CPUConfigLimits.Bpred2LevConfigL2SizeMin, CPUConfigLimits.Bpred2LevConfigL2SizeMax + 1),
                random.Next(CPUConfigLimits.Bpred2LevConfigHistorySizeMin, CPUConfigLimits.Bpred2LevConfigHistorySizeMax + 1),
                random.Next(0, 2) == 1 // useXor
            ),
            random.Next(CPUConfigLimits.BpredCombMetaTableSizeMin, CPUConfigLimits.BpredCombMetaTableSizeMax + 1),
            random.Next(CPUConfigLimits.BpredReturnAddressStackSizeMin, CPUConfigLimits.BpredReturnAddressStackSizeMax + 1),
            new BtbConfig( //bpredBtbConfig
                random.Next(CPUConfigLimits.BpredBtbConfigNumSetsMin, CPUConfigLimits.BpredBtbConfigNumSetsMax + 1),
                random.Next(CPUConfigLimits.BpredBtbConfigAssociativityMin, CPUConfigLimits.BpredBtbConfigAssociativityMax + 1)
            ),
            (SpeculativePredictorUpdateStageEnum)random.Next(CPUConfigLimits.BpredSpeculativeUpdateMin, CPUConfigLimits.BpredSpeculativeUpdateMax + 1),
            (CacheLoadPredictorTypeEnum)random.Next(CPUConfigLimits.CacheLoadPredictorTypeMin, CPUConfigLimits.CacheLoadPredictorTypeMax + 1),
            random.Next(CPUConfigLimits.CpredBimodTableSizeMin, CPUConfigLimits.CpredBimodTableSizeMax + 1),
            new Predictor2LevConfig( // cpred2levconfig
                random.Next(CPUConfigLimits.Cpred2LevConfigL1SizeMin, CPUConfigLimits.Cpred2LevConfigL1SizeMax + 1),
                random.Next(CPUConfigLimits.Cpred2LevConfigL2SizeMin, CPUConfigLimits.Cpred2LevConfigL2SizeMax + 1),
                random.Next(CPUConfigLimits.Cpred2LevConfigHistorySizeMin, CPUConfigLimits.Cpred2LevConfigHistorySizeMax + 1),
                random.Next(0, 2) == 1 // useXor
            ),
            random.Next(CPUConfigLimits.CpredCombMetaTableSizeMin, CPUConfigLimits.CpredCombMetaTableSizeMax + 1),
            random.Next(CPUConfigLimits.CpredReturnAddressStackSizeMin, CPUConfigLimits.CpredReturnAddressStackSizeMax + 1),
            new BtbConfig( //cpredBtbConfig
                random.Next(CPUConfigLimits.CpredBtbConfigNumSetsMin, CPUConfigLimits.CpredBtbConfigNumSetsMax + 1),
                random.Next(CPUConfigLimits.CpredBtbConfigAssociativityMin, CPUConfigLimits.CpredBtbConfigAssociativityMax + 1)
            ),
            random.Next(CPUConfigLimits.DecodeWidthMin, CPUConfigLimits.DecodeWidthMax + 1),
            random.Next(CPUConfigLimits.IssueWidthMin, CPUConfigLimits.IssueWidthMax + 1),
            random.Next(0, 2) == 1,
            random.Next(CPUConfigLimits.CommitWidthMin, CPUConfigLimits.CommitWidthMax + 1),
            random.Next(CPUConfigLimits.ReorderBufferSizeMin, CPUConfigLimits.ReorderBufferSizeMax + 1),
            random.Next(CPUConfigLimits.IssueQueueSizeMin, CPUConfigLimits.IssueQueueSizeMax + 1),
            random.Next(CPUConfigLimits.RegisterFileSizeMin, CPUConfigLimits.RegisterFileSizeMax + 1),
            random.Next(CPUConfigLimits.LoadStoreQueueSizeMin, CPUConfigLimits.LoadStoreQueueSizeMax + 1),
            new CacheTlbConfig(
                "dl1",
                random.Next(CPUConfigLimits.CacheDl1NumSetsMin, CPUConfigLimits.CacheDl1NumSetsMax + 1),
                random.Next(CPUConfigLimits.CacheDl1BlockOrPageSizeMin, CPUConfigLimits.CacheDl1BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheDl1AssociativityMin, CPUConfigLimits.CacheDl1AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheDl1ReplacementPolicyMin, CPUConfigLimits.CacheDl1ReplacementPolicyMax + 1)
            ),
            new CacheTlbConfig(
                "dl2",
                random.Next(CPUConfigLimits.CacheDl2NumSetsMin, CPUConfigLimits.CacheDl2NumSetsMax + 1),
                random.Next(CPUConfigLimits.CacheDl2BlockOrPageSizeMin, CPUConfigLimits.CacheDl2BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheDl2AssociativityMin, CPUConfigLimits.CacheDl2AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheDl2ReplacementPolicyMin, CPUConfigLimits.CacheDl2ReplacementPolicyMax + 1)
            ),
            new CacheTlbConfig(
                "il1",
                random.Next(CPUConfigLimits.CacheIl1NumSetsMin, CPUConfigLimits.CacheIl1NumSetsMax + 1),
                random.Next(CPUConfigLimits.CacheIl1BlockOrPageSizeMin, CPUConfigLimits.CacheIl1BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheIl1AssociativityMin, CPUConfigLimits.CacheIl1AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheIl1ReplacementPolicyMin, CPUConfigLimits.CacheIl1ReplacementPolicyMax + 1)
            ),
            new CacheTlbConfig(
                "il2",
                random.Next(CPUConfigLimits.CacheIl2NumSetsMin, CPUConfigLimits.CacheIl2NumSetsMax + 1),
                random.Next(CPUConfigLimits.CacheIl2BlockOrPageSizeMin, CPUConfigLimits.CacheIl2BlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.CacheIl2AssociativityMin, CPUConfigLimits.CacheIl2AssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.CacheIl2ReplacementPolicyMin, CPUConfigLimits.CacheIl2ReplacementPolicyMax + 1)
            ),
            random.Next(CPUConfigLimits.MemBusWidthMin, CPUConfigLimits.MemBusWidthMax + 1),
            new CacheTlbConfig(
                "itlb",
                random.Next(CPUConfigLimits.TlbItlbNumSetsMin, CPUConfigLimits.TlbItlbNumSetsMax + 1),
                random.Next(CPUConfigLimits.TlbItlbBlockOrPageSizeMin, CPUConfigLimits.TlbItlbBlockOrPageSizeMax + 1),
                random.Next(CPUConfigLimits.TlbItlbAssociativityMin, CPUConfigLimits.TlbItlbAssociativityMax + 1),
                (ReplacementPolicyEnum)random.Next(CPUConfigLimits.TlbItlbReplacementPolicyMin, CPUConfigLimits.TlbItlbReplacementPolicyMin + 1)
            ),
            new CacheTlbConfig(
                "dtlb",
                random.Next(CPUConfigLimits.TlbDtlbNumSetsMin, CPUConfigLimits.TlbDtlbNumSetsMax + 1),
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




    public static class CPUConfigLimits {
        public static int BranchPredictorTypeMax = (int)BranchPredictorTypeEnum.perfect - 1;
        public static int BranchPredictorTypeMin = 0;

        public static int BpredBimodTableSizeMax = 9999;
        public static int BpredBimodTableSizeMin = 1;

        public static int Bpred2LevConfigL1SizeMax = 9999;
        public static int Bpred2LevConfigL1SizeMin = 1;

        public static int Bpred2LevConfigL2SizeMax = 9999;
        public static int Bpred2LevConfigL2SizeMin = 1;

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

        public static int Cpred2LevConfigL1SizeMax = 9999;
        public static int Cpred2LevConfigL1SizeMin = 1;

        public static int Cpred2LevConfigL2SizeMax = 9999;
        public static int Cpred2LevConfigL2SizeMin = 1;

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

        public static int DecodeWidthMax = 9999;
        public static int DecodeWidthMin = 1;

        public static int IssueWidthMax = 9999;
        public static int IssueWidthMin = 1;

        public static int IssueInOrderMax = 1;
        public static int IssueInOrderMin = 0;

        public static int CommitWidthMax = 9999;
        public static int CommitWidthMin = 1;

        public static int ReorderBufferSizeMax = 9999;
        public static int ReorderBufferSizeMin = 1;

        public static int IssueQueueSizeMax = 9999;
        public static int IssueQueueSizeMin = 1;

        public static int RegisterFileSizeMax = 9999;
        public static int RegisterFileSizeMin = 1;

        public static int LoadStoreQueueSizeMax = 9999;
        public static int LoadStoreQueueSizeMin = 1;


        public static int CacheDl1NumSetsMax = 9999;
        public static int CacheDl1NumSetsMin = 1;

        public static int CacheDl1BlockOrPageSizeMax = 9999;
        public static int CacheDl1BlockOrPageSizeMin = 1;

        public static int CacheDl1AssociativityMax = 9999;
        public static int CacheDl1AssociativityMin = 1;

        public static int CacheDl1ReplacementPolicyMax = 2;
        public static int CacheDl1ReplacementPolicyMin = 0;


        public static int CacheDl2NumSetsMax = 9999;
        public static int CacheDl2NumSetsMin = 1;

        public static int CacheDl2BlockOrPageSizeMax = 9999;
        public static int CacheDl2BlockOrPageSizeMin = 1;

        public static int CacheDl2AssociativityMax = 9999;
        public static int CacheDl2AssociativityMin = 1;

        public static int CacheDl2ReplacementPolicyMax = 2;
        public static int CacheDl2ReplacementPolicyMin = 0;


        public static int CacheIl1NumSetsMax = 9999;
        public static int CacheIl1NumSetsMin = 1;

        public static int CacheIl1BlockOrPageSizeMax = 9999;
        public static int CacheIl1BlockOrPageSizeMin = 1;

        public static int CacheIl1AssociativityMax = 9999;
        public static int CacheIl1AssociativityMin = 1;

        public static int CacheIl1ReplacementPolicyMax = 2;
        public static int CacheIl1ReplacementPolicyMin = 0;


        public static int CacheIl2NumSetsMax = 9999;
        public static int CacheIl2NumSetsMin = 1;

        public static int CacheIl2BlockOrPageSizeMax = 9999;
        public static int CacheIl2BlockOrPageSizeMin = 1;

        public static int CacheIl2AssociativityMax = 9999;
        public static int CacheIl2AssociativityMin = 1;

        public static int CacheIl2ReplacementPolicyMax = 2;
        public static int CacheIl2ReplacementPolicyMin = 0;


        public static int MemBusWidthMax = 9999;
        public static int MemBusWidthMin = 1;


        public static int TlbItlbNumSetsMax = 9999;
        public static int TlbItlbNumSetsMin = 1;

        public static int TlbItlbBlockOrPageSizeMax = 9999;
        public static int TlbItlbBlockOrPageSizeMin = 1;

        public static int TlbItlbAssociativityMax = 9999;
        public static int TlbItlbAssociativityMin = 1;

        public static int TlbItlbReplacementPolicyMax = 2;
        public static int TlbItlbReplacementPolicyMin = 0;


        public static int TlbDtlbNumSetsMax = 9999;
        public static int TlbDtlbNumSetsMin = 1;

        public static int TlbDtlbBlockOrPageSizeMax = 9999;
        public static int TlbDtlbBlockOrPageSizeMin = 1;

        public static int TlbDtlbAssociativityMax = 9999;
        public static int TlbDtlbAssociativityMin = 1;

        public static int TlbDtlbReplacementPolicyMax = 2;
        public static int TlbDtlbReplacementPolicyMin = 0;


        public static int ResIntegerAluMax = 9999;
        public static int ResIntegerAluMin = 1;

        public static int ResIntegerMultDivMax = 9999;
        public static int ResIntegerMultDivMin = 1;

        public static int ResMemoryPortsMax = 9999;
        public static int ResMemoryPortsMin = 1;

        public static int ResFpAluMax = 9999;
        public static int ResFpAluMin = 1;

        public static int ResFpMultDivMax = 9999;
        public static int ResFpMultDivMin = 1;

        public static int GetMin(int index)
        {
            return index switch
            {
                0 => BranchPredictorTypeMin,
                1 => BpredBimodTableSizeMin,
                2 => Bpred2LevConfigL1SizeMin,
                3 => Bpred2LevConfigL2SizeMin,
                4 => Bpred2LevConfigHistorySizeMin,
                5 => Bpred2LevConfigUseXorMin,
                6 => BpredCombMetaTableSizeMin,
                7 => BpredReturnAddressStackSizeMin,
                8 => BpredBtbConfigNumSetsMin,
                9 => BpredBtbConfigAssociativityMin,
                10 => BpredSpeculativeUpdateMin,
                11 => CacheLoadPredictorTypeMin,
                12 => CpredBimodTableSizeMin,
                13 => Cpred2LevConfigL1SizeMin,
                14 => Cpred2LevConfigL2SizeMin,
                15 => Cpred2LevConfigHistorySizeMin,
                16 => Cpred2LevConfigUseXorMin,
                17 => CpredCombMetaTableSizeMin,
                18 => CpredReturnAddressStackSizeMin,
                19 => CpredBtbConfigNumSetsMin,
                20 => CpredBtbConfigAssociativityMin,
                21 => DecodeWidthMin,
                22 => IssueWidthMin,
                23 => IssueInOrderMin,
                24 => CommitWidthMin,
                25 => ReorderBufferSizeMin,
                26 => IssueQueueSizeMin,
                27 => RegisterFileSizeMin,
                28 => LoadStoreQueueSizeMin,
                29 => CacheDl1NumSetsMin,
                30 => CacheDl1BlockOrPageSizeMin,
                31 => CacheDl1AssociativityMin,
                32 => CacheDl1ReplacementPolicyMin,
                33 => CacheDl2NumSetsMin,
                34 => CacheDl2BlockOrPageSizeMin,
                35 => CacheDl2AssociativityMin,
                36 => CacheDl2ReplacementPolicyMin,
                37 => CacheIl1NumSetsMin,
                38 => CacheIl1BlockOrPageSizeMin,
                39 => CacheIl1AssociativityMin,
                40 => CacheIl1ReplacementPolicyMin,
                41 => CacheIl2NumSetsMin,
                42 => CacheIl2BlockOrPageSizeMin,
                43 => CacheIl2AssociativityMin,
                44 => CacheIl2ReplacementPolicyMin,
                45 => MemBusWidthMin,
                46 => TlbItlbNumSetsMin,
                47 => TlbItlbBlockOrPageSizeMin,
                48 => TlbItlbAssociativityMin,
                49 => TlbItlbReplacementPolicyMin,
                50 => TlbDtlbNumSetsMin,
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
                2 => Bpred2LevConfigL1SizeMax,
                3 => Bpred2LevConfigL2SizeMax,
                4 => Bpred2LevConfigHistorySizeMax,
                5 => Bpred2LevConfigUseXorMax,
                6 => BpredCombMetaTableSizeMax,
                7 => BpredReturnAddressStackSizeMax,
                8 => BpredBtbConfigNumSetsMax,
                9 => BpredBtbConfigAssociativityMax,
                10 => BpredSpeculativeUpdateMax,
                11 => CacheLoadPredictorTypeMax,
                12 => CpredBimodTableSizeMax,
                13 => Cpred2LevConfigL1SizeMax,
                14 => Cpred2LevConfigL2SizeMax,
                15 => Cpred2LevConfigHistorySizeMax,
                16 => Cpred2LevConfigUseXorMax,
                17 => CpredCombMetaTableSizeMax,
                18 => CpredReturnAddressStackSizeMax,
                19 => CpredBtbConfigNumSetsMax,
                20 => CpredBtbConfigAssociativityMax,
                21 => DecodeWidthMax,
                22 => IssueWidthMax,
                23 => IssueInOrderMax,
                24 => CommitWidthMax,
                25 => ReorderBufferSizeMax,
                26 => IssueQueueSizeMax,
                27 => RegisterFileSizeMax,
                28 => LoadStoreQueueSizeMax,
                29 => CacheDl1NumSetsMax,
                30 => CacheDl1BlockOrPageSizeMax,
                31 => CacheDl1AssociativityMax,
                32 => CacheDl1ReplacementPolicyMax,
                33 => CacheDl2NumSetsMax,
                34 => CacheDl2BlockOrPageSizeMax,
                35 => CacheDl2AssociativityMax,
                36 => CacheDl2ReplacementPolicyMax,
                37 => CacheIl1NumSetsMax,
                38 => CacheIl1BlockOrPageSizeMax,
                39 => CacheIl1AssociativityMax,
                40 => CacheIl1ReplacementPolicyMax,
                41 => CacheIl2NumSetsMax,
                42 => CacheIl2BlockOrPageSizeMax,
                43 => CacheIl2AssociativityMax,
                44 => CacheIl2ReplacementPolicyMax,
                45 => MemBusWidthMax,
                46 => TlbItlbNumSetsMax,
                47 => TlbItlbBlockOrPageSizeMax,
                48 => TlbItlbAssociativityMax,
                49 => TlbItlbReplacementPolicyMax,
                50 => TlbDtlbNumSetsMax,
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