using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SMPSOsimulation.dataStructures;

public class CPUConfig : IEquatable<CPUConfig?>
{
    public int Superscalar { get; set; }
    public int Rename { get; set; }
    public int Reorder { get; set; }
    public RsbArchitectureType RsbArchitecture { get; set; }
    public bool SeparateDispatch { get; set; }
    public int Iadd { get; set; }
    public int Imult { get; set; }
    public int Idiv { get; set; }
    public int Fpadd { get; set; }
    public int Fpmult { get; set; }
    public int Fpdiv { get; set; }
    public int Fpsqrt { get; set; }
    public int Branch { get; set; }
    public int Load { get; set; }
    public int Store { get; set; }
    public int Freq { get; set; }

    // Constructor to initialize the properties
    public CPUConfig(int superscalar, int rename, int reorder, RsbArchitectureType rsbArchitecture, bool separateDispatch,
                    int iadd, int imult, int idiv, int fpadd, int fpmult, int fpdiv, int fpsqrt, int branch,
                    int load, int store, int freq)
    {
        Superscalar = superscalar;
        Rename = rename;
        Reorder = reorder;
        RsbArchitecture = rsbArchitecture;
        SeparateDispatch = separateDispatch;
        Iadd = iadd;
        Imult = imult;
        Idiv = idiv;
        Fpadd = fpadd;
        Fpmult = fpmult;
        Fpdiv = fpdiv;
        Fpsqrt = fpsqrt;
        Branch = branch;
        Load = load;
        Store = store;
        Freq = freq;
        Validate();
    }

    // Method to validate the properties
    public void Validate()
    {
        if (Superscalar < 1 || Superscalar > 16) throw new ArgumentOutOfRangeException(nameof(Superscalar));
        if (Rename < 1 || Rename > 512) throw new ArgumentOutOfRangeException(nameof(Rename));
        if (Reorder < 1 || Reorder > 512) throw new ArgumentOutOfRangeException(nameof(Reorder));
        if (!Enum.IsDefined(typeof(RsbArchitectureType), RsbArchitecture)) throw new ArgumentException(nameof(RsbArchitecture));
        if (Iadd < 1 || Iadd > 8) throw new ArgumentOutOfRangeException(nameof(Iadd));
        if (Imult < 1 || Imult > 8) throw new ArgumentOutOfRangeException(nameof(Imult));
        if (Idiv < 1 || Idiv > 8) throw new ArgumentOutOfRangeException(nameof(Idiv));
        if (Fpadd < 1 || Fpadd > 8) throw new ArgumentOutOfRangeException(nameof(Fpadd));
        if (Fpmult < 1 || Fpmult > 8) throw new ArgumentOutOfRangeException(nameof(Fpmult));
        if (Fpdiv < 1 || Fpdiv > 8) throw new ArgumentOutOfRangeException(nameof(Fpdiv));
        if (Fpsqrt < 1 || Fpsqrt > 8) throw new ArgumentOutOfRangeException(nameof(Fpsqrt));
        if (Branch < 1 || Branch > 8) throw new ArgumentOutOfRangeException(nameof(Branch));
        if (Load < 1 || Load > 8) throw new ArgumentOutOfRangeException(nameof(Load));
        if (Store < 1 || Store > 8) throw new ArgumentOutOfRangeException(nameof(Store));
        if (Freq <= 0) throw new ArgumentOutOfRangeException(nameof(Freq));
    }

    public string CalculateSha256()
    {
        // Serialize the object to JSON
        string serializedConfig = JsonSerializer.Serialize(this);

        // Compute the SHA-256 hash
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(serializedConfig));

        // Convert the hash to a hex string
        StringBuilder hashString = new();
        foreach (byte b in hashBytes)
        {
            hashString.Append(b.ToString("x2"));
        }

        return hashString.ToString();
    }

    public static class CPUConfigLimits
    {
        public const int SuperscalarMin = 1;
        public const int RenameMin = 1;
        public const int ReorderMin = 1;
        public const int IaddMin = 1;
        public const int ImultMin = 1;
        public const int IdivMin = 1;
        public const int FpaddMin = 1;
        public const int FpmultMin = 1;
        public const int FpdivMin = 1;
        public const int FpsqrtMin = 1;
        public const int BranchMin = 1;
        public const int LoadMin = 1;
        public const int StoreMin = 1;

        public const int SuperscalarMax = 16;
        public const int RenameMax = 512;
        public const int ReorderMax = 512;
        public const int IaddMax = 8;
        public const int ImultMax = 8;
        public const int IdivMax = 8;
        public const int FpaddMax = 8;
        public const int FpmultMax = 8;
        public const int FpdivMax = 8;
        public const int FpsqrtMax = 8;
        public const int BranchMax = 8;
        public const int LoadMax = 8;
        public const int StoreMax = 8;

        public static int GetMin(int index)
        {
            return index switch
            {
                0 => SuperscalarMin,
                1 => RenameMin,
                2 => ReorderMin,
                3 => 0,
                4 => 0,
                5 => IaddMin,
                6 => ImultMin,
                7 => IdivMin,
                8 => FpaddMin,
                9 => FpmultMin,
                10 => FpdivMin,
                11 => FpsqrtMin,
                12 => BranchMin,
                13 => LoadMin,
                14 => StoreMin,
                15 => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }

        public static int GetMax(int index, int maxFrequency)
        {
            return index switch
            {
                0 => SuperscalarMax,
                1 => RenameMax,
                2 => ReorderMax,
                3 => Enum.GetValues(typeof(RsbArchitectureType)).Length - 1,
                4 => 1,
                5 => IaddMax,
                6 => ImultMax,
                7 => IdivMax,
                8 => FpaddMax,
                9 => FpmultMax,
                10 => FpdivMax,
                11 => FpsqrtMax,
                12 => BranchMax,
                13 => LoadMax,
                14 => StoreMax,
                15 => maxFrequency,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
    }

    public static CPUConfig GenerateRandom(int maxFrequency)
    {
        Random random = new();

        int superscalar = random.Next(CPUConfigLimits.SuperscalarMin, CPUConfigLimits.SuperscalarMax + 1);
        int rename = random.Next(CPUConfigLimits.RenameMin, CPUConfigLimits.RenameMax + 1);
        int reorder = random.Next(CPUConfigLimits.ReorderMin, CPUConfigLimits.ReorderMax + 1);
        RsbArchitectureType rsbArchitecture = (RsbArchitectureType)random.Next(0, Enum.GetValues(typeof(RsbArchitectureType)).Length);
        bool separateDispatch = random.Next(0, 2) == 1; // Randomly true or false

        int iadd = random.Next(CPUConfigLimits.IaddMin, CPUConfigLimits.IaddMax + 1);
        int imult = random.Next(CPUConfigLimits.ImultMin, CPUConfigLimits.ImultMax + 1);
        int idiv = random.Next(CPUConfigLimits.IdivMin, CPUConfigLimits.IdivMax + 1);
        int fpadd = random.Next(CPUConfigLimits.FpaddMin, CPUConfigLimits.FpaddMax + 1);
        int fpmult = random.Next(CPUConfigLimits.FpmultMin, CPUConfigLimits.FpmultMax + 1);
        int fpdiv = random.Next(CPUConfigLimits.FpdivMin, CPUConfigLimits.FpdivMax + 1);
        int fpsqrt = random.Next(CPUConfigLimits.FpsqrtMin, CPUConfigLimits.FpsqrtMax + 1);
        int branch = random.Next(CPUConfigLimits.BranchMin, CPUConfigLimits.BranchMax + 1);
        int load = random.Next(CPUConfigLimits.LoadMin, CPUConfigLimits.LoadMax + 1);
        int store = random.Next(CPUConfigLimits.StoreMin, CPUConfigLimits.StoreMax + 1);
        int freq = random.Next(1, maxFrequency + 1); // Frequency should be between 1 and maxFrequency

        return new CPUConfig(superscalar, rename, reorder, rsbArchitecture, separateDispatch,
            iadd, imult, idiv, fpadd, fpmult, fpdiv, fpsqrt, branch, load, store, freq);
    }

    public double[] GetVectorFormDouble()
    {
        return
        [
                Superscalar,
                Rename,
                Reorder,
                (int)RsbArchitecture,
                SeparateDispatch ? 1 : 0,
                Iadd,
                Imult,
                Idiv,
                Fpadd,
                Fpmult,
                Fpdiv,
                Fpsqrt,
                Branch,
                Load,
                Store,
                Freq
        ];
    }

    public int[] GetVectorFormInt()
    {
        return
        [
                Superscalar,
                Rename,
                Reorder,
                (int)RsbArchitecture,
                SeparateDispatch ? 1 : 0,
                Iadd,
                Imult,
                Idiv,
                Fpadd,
                Fpmult,
                Fpdiv,
                Fpsqrt,
                Branch,
                Load,
                Store,
                Freq
        ];
    }

    public static CPUConfig GetConfigFromVectorInt(int[] vector)
    {
        // Map the genes back to a CPUConfig
        int superscalar = vector[0];
        int rename = vector[1];
        int reorder = vector[2];
        RsbArchitectureType rsbArchitecture = (RsbArchitectureType)vector[3];
        bool separateDispatch = vector[4] == 1; // Convert back to boolean
        int iadd = vector[5];
        int imult = vector[6];
        int idiv = vector[7];
        int fpadd = vector[8];
        int fpmult = vector[9];
        int fpdiv = vector[10];
        int fpsqrt = vector[11];
        int branch = vector[12];
        int load = vector[13];
        int store = vector[14];
        int freq = vector[15];

        // Create and return a new CPUConfig object
        return new CPUConfig(superscalar, rename, reorder, rsbArchitecture, separateDispatch,
            iadd, imult, idiv, fpadd, fpmult, fpdiv, fpsqrt, branch, load, store, freq);
    }

    public static CPUConfig GetConfigFromVectorDouble(double[] vector)
    {
        int[] intArray = vector.Select(d => (int)Math.Round(d)).ToArray();
        return GetConfigFromVectorInt(intArray);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CPUConfig);
    }

    public bool Equals(CPUConfig? other)
    {
        return other is not null &&
               Superscalar == other.Superscalar &&
               Rename == other.Rename &&
               Reorder == other.Reorder &&
               RsbArchitecture == other.RsbArchitecture &&
               SeparateDispatch == other.SeparateDispatch &&
               Iadd == other.Iadd &&
               Imult == other.Imult &&
               Idiv == other.Idiv &&
               Fpadd == other.Fpadd &&
               Fpmult == other.Fpmult &&
               Fpdiv == other.Fpdiv &&
               Fpsqrt == other.Fpsqrt &&
               Branch == other.Branch &&
               Load == other.Load &&
               Store == other.Store &&
               Freq == other.Freq;
    }
}

// Enum for RsbArchitecture
public enum RsbArchitectureType
{
    centralized,
    hybrid,
    distributed,
}