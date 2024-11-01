using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SMPSOsimulation
{
    public struct Configuration
    {
        // General Configuration
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("general")]
        public General General { get; set; }

        [XmlElement("execution")]
        public Execution Execution { get; set; }

        [XmlElement("memory")]
        public Memory Memory { get; set; }
    }

    public struct General
    {
        [XmlAttribute("superscalar")]
        public int Superscalar { get; set; }
        [XmlAttribute("rename")]
        public int Rename { get; set; }
        [XmlAttribute("reorder")]
        public int Reorder { get; set; }
        [XmlAttribute("seed")]
        public int Seed { get; set; }
        [XmlAttribute("frequency")]
        public int Frequency { get; set; }
        [XmlAttribute("vdd")]
        public double Vdd { get; set; }
        [XmlAttribute("trace")]
        public string Trace { get; set; }
        [XmlAttribute("speculative")]
        public bool Speculative { get; set; }
        [XmlAttribute("separate_dispatch")]
        public bool SeparateDispatch { get; set; }
        [XmlAttribute("rs_per_rsb")]
        public int RsPerRsb { get; set; }
        [XmlAttribute("rsb_architecture")]
        public string RsbArchitecture { get; set; }
        [XmlAttribute("speculation_accuracy")]
        public double SpeculationAccuracy { get; set; }
    }

    public struct Execution
    {
        [XmlAttribute("architecture")]
        public string Architecture { get; set; }
        [XmlAttribute("integer")]
        public int Integer { get; set; }
        [XmlAttribute("floating")]
        public int Floating { get; set; }
        [XmlAttribute("branch")]
        public int Branch { get; set; }
        [XmlAttribute("memory")]
        public int Memory { get; set; }
        [XmlAttribute("alu")]
        public int Alu { get; set; }
        [XmlAttribute("iadd")]
        public int IAdd { get; set; }
        [XmlAttribute("imult")]
        public int IMult { get; set; }
        [XmlAttribute("idiv")]
        public int IDiv { get; set; }
        [XmlAttribute("fpadd")]
        public int FPAdd { get; set; }
        [XmlAttribute("fpmult")]
        public int FPMult { get; set; }
        [XmlAttribute("fpdiv")]
        public int FPDiv { get; set; }
        [XmlAttribute("fpsqrt")]
        public int FPSqrt { get; set; }
        [XmlAttribute("load")]
        public int Load { get; set; }
        [XmlAttribute("store")]
        public int Store { get; set; }
    }

    public struct Memory
    {
        [XmlAttribute("architecture")]
        public string Architecture { get; set; }

        [XmlElement("system")]
        public SystemMemory System { get; set; }

        [XmlElement("l1_code")]
        public Cache L1Code { get; set; }

        [XmlElement("l1_data")]
        public Cache L1Data { get; set; }

        [XmlElement("l2")]
        public Cache L2 { get; set; }
    }

    public struct SystemMemory
    {
        [XmlAttribute("latency")]
        public double Latency { get; set; }
    }

    public struct Cache
    {
        [XmlAttribute("hitrate")]
        public double Hitrate { get; set; }
        [XmlAttribute("latency")]
        public double Latency { get; set; }
    }
}
