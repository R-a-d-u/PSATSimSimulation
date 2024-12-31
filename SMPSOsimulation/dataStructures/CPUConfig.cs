using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SMPSOsimulation.dataStructures;

public class CPUConfig
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
        using var sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(serializedConfig));

        // Convert the hash to a hex string
        StringBuilder hashString = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            hashString.Append(b.ToString("x2"));
        }

        return hashString.ToString();
    }
}

// Enum for RsbArchitecture
public enum RsbArchitectureType
{
    centralized,
    hybrid,
    distributed,
}