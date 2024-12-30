namespace SMPSOsimulation.dataStructures;

using System;

public enum MemoryArchEnum
{
    system,
    l1,
    l2
}

public class EnvironmentConfig
{
    public double Vdd { get; set; }
    public MemoryArchEnum MemoryArch { get; set; }
    public float L1CodeHitrate { get; set; }
    public int L1CodeLatency { get; set; }
    public float L1DataHitrate { get; set; }
    public int L1DataLatency { get; set; }
    public float L2Hitrate { get; set; }
    public int L2Latency { get; set; }
    public int SystemMemLatency { get; set; }
    public string Trace {get; set;}

    public EnvironmentConfig(
        double vdd,
        MemoryArchEnum memoryArch,
        float l1CodeHitrate,
        int l1CodeLatency,
        float l1DataHitrate,
        int l1DataLatency,
        float l2Hitrate,
        int l2Latency,
        int systemMemLatency,
        string trace)
    {
        if (vdd <= 0)
            throw new ArgumentException("Vdd must be greater than 0.");
        
        if (l1CodeHitrate <= 0 || l1CodeHitrate >= 1)
            throw new ArgumentException("L1CodeHitrate must be between 0 and 1 (exclusive).");

        if (l1CodeLatency <= 0)
            throw new ArgumentException("L1CodeLatency must be greater than 0.");

        if (l1DataHitrate <= 0 || l1DataHitrate >= 1)
            throw new ArgumentException("L1DataHitrate must be between 0 and 1 (exclusive).");

        if (l1DataLatency <= 0)
            throw new ArgumentException("L1DataLatency must be greater than 0.");

        if (l2Hitrate <= 0 || l2Hitrate >= 1)
            throw new ArgumentException("L2Hitrate must be between 0 and 1 (exclusive).");

        if (l2Latency <= 0)
            throw new ArgumentException("L2Latency must be greater than 0.");

        if (systemMemLatency <= 0)
            throw new ArgumentException("SystemMemLatency must be greater than 0.");

        Vdd = vdd;
        MemoryArch = memoryArch;
        L1CodeHitrate = l1CodeHitrate;
        L1CodeLatency = l1CodeLatency;
        L1DataHitrate = l1DataHitrate;
        L1DataLatency = l1DataLatency;
        L2Hitrate = l2Hitrate;
        L2Latency = l2Latency;
        SystemMemLatency = systemMemLatency;
        Trace = trace;
    }
}
