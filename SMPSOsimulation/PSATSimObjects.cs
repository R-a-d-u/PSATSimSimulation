using SMPSOsimulation;

public static class PSATSimObjects
{
    public static List<Configuration> CreateConfigurations()
    {
        return new List<Configuration>
        {
            new Configuration
            {
                Name = "SolutionGeneratedConfig",
                General = new General
                {
                    Superscalar = 15, Rename = 353, Reorder = 468, Seed = 0, Frequency = 600,
                    Vdd = 2.2, Trace = @"E:\PSATSIM\compress.tra", Speculative = false, SeparateDispatch = true,
                    RsPerRsb = 6, RsbArchitecture = "distributed", SpeculationAccuracy = 0.6436230001842436
                },
                Execution = new Execution
                {
                    Architecture = "complex", Integer = 2, Floating = 7, Branch = 2, Memory = 4, Alu = 5,
                    IAdd = 6, IMult = 7, IDiv = 4, FPAdd = 3, FPMult = 6, FPDiv = 6, FPSqrt = 4,
                    Load = 8, Store = 8
                },
                Memory = new Memory
                {
                    Architecture = "system",
                    System = new SystemMemory { Latency = 39.04801640210498 },
                    L1Code = new Cache { Hitrate = 0.8860734360429738, Latency = 88.15517121302447 },
                    L1Data = new Cache { Hitrate = 0.5771835586482472, Latency = 30.2691248802196 },
                    L2 = new Cache { Hitrate = 0.3713664058038628, Latency = 53.229882066328884 }
                }
            },
            new Configuration
            {
                Name = "SolutionGeneratedConfig",
                General = new General
                {
                    Superscalar = 11, Rename = 205, Reorder = 73, Seed = 0, Frequency = 600,
                    Vdd = 2.2, Trace = @"E:\PSATSIM\compress.tra", Speculative = false, SeparateDispatch = true,
                    RsPerRsb = 5, RsbArchitecture = "distributed", SpeculationAccuracy = 0.3074403798432901
                },
                Execution = new Execution
                {
                    Architecture = "complex", Integer = 6, Floating = 4, Branch = 7, Memory = 4, Alu = 4,
                    IAdd = 7, IMult = 6, IDiv = 5, FPAdd = 6, FPMult = 5, FPDiv = 7, FPSqrt = 1,
                    Load = 7, Store = 7
                },
                Memory = new Memory
                {
                    Architecture = "system",
                    System = new SystemMemory { Latency = 78.37131565539839 },
                    L1Code = new Cache { Hitrate = 0.6053831354509829, Latency = 61.21089993242839 },
                    L1Data = new Cache { Hitrate = 0.02577763159848412, Latency = 45.043201338442415 },
                    L2 = new Cache { Hitrate = 0.8381162702760461, Latency = 92.730496869009 }
                }
            },
            new Configuration
            {
                Name = "SolutionGeneratedConfig",
                General = new General
                {
                    Superscalar = 7, Rename = 438, Reorder = 49, Seed = 0, Frequency = 600,
                    Vdd = 2.2, Trace = @"E:\PSATSIM\compress.tra", Speculative = false, SeparateDispatch = false,
                    RsPerRsb = 3, RsbArchitecture = "distributed", SpeculationAccuracy = 0.9243659714786319
                },
                Execution = new Execution
                {
                    Architecture = "standard", Integer = 7, Floating = 7, Branch = 7, Memory = 5, Alu = 2,
                    IAdd = 3, IMult = 2, IDiv = 2, FPAdd = 6, FPMult = 6, FPDiv = 8, FPSqrt = 1,
                    Load = 8, Store = 8
                },
                Memory = new Memory
                {
                    Architecture = "system",
                    System = new SystemMemory { Latency = 7.430097030493554 },
                    L1Code = new Cache { Hitrate = 0.7675868778066492, Latency = 23.397747737499387 },
                    L1Data = new Cache { Hitrate = 0.14439961793725486, Latency = 41.646353335121546 },
                    L2 = new Cache { Hitrate = 0.4940488444312361, Latency = 10.595230152565204 }
                }
            },
            new Configuration
            {
                Name = "SolutionGeneratedConfig",
                General = new General
                {
                    Superscalar = 5, Rename = 51, Reorder = 23, Seed = 0, Frequency = 600,
                    Vdd = 2.2, Trace = @"E:\PSATSIM\compress.tra", Speculative = true, SeparateDispatch = true,
                    RsPerRsb = 7, RsbArchitecture = "hybrid", SpeculationAccuracy = 0.8959741875162028
                },
                Execution = new Execution
                {
                    Architecture = "complex", Integer = 2, Floating = 4, Branch = 6, Memory = 3, Alu = 1,
                    IAdd = 5, IMult = 7, IDiv = 1, FPAdd = 7, FPMult = 1, FPDiv = 3, FPSqrt = 7,
                    Load = 2, Store = 2
                },
                Memory = new Memory
                {
                    Architecture = "system",
                    System = new SystemMemory { Latency = 22.13192581859431 },
                    L1Code = new Cache { Hitrate = 0.7036238304119009, Latency = 43.471623715159225 },
                    L1Data = new Cache { Hitrate = 0.8496886929492693, Latency = 33.32972216733938 },
                    L2 = new Cache { Hitrate = 0.5302133760292369, Latency = 53.38895633829666 }
                }
            }
        };
    }
}
