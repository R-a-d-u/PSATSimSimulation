using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SMPSOsimulation
{
    public class StructPSATSimOutput
    {
        [XmlRoot("psatsim_results")]
        public class PsatSimResults
        {
            [XmlElement("variation")]
            public List<Variation> Variations { get; set; }
        }

        public class Variation
        {
            [XmlAttribute("configuration")]
            public string Configuration { get; set; }

            [XmlAttribute("variation")]
            public int VariationNumber { get; set; }

            [XmlAttribute("started")]
            public string Started { get; set; }

            [XmlAttribute("runtime")]
            public double Runtime { get; set; }

            [XmlElement("general")]
            public GeneralResults General { get; set; }
        }

        public class GeneralResults
        {
            [XmlAttribute("cycles")]
            public int Cycles { get; set; }

            [XmlAttribute("instructions")]
            public int Instructions { get; set; }

            [XmlAttribute("fetches")]
            public int Fetches { get; set; }

            [XmlAttribute("ipc")]
            public double IPC { get; set; }

            [XmlAttribute("energy")]
            public double Energy { get; set; }

            [XmlAttribute("power")]
            public double Power { get; set; }

            [XmlElement("regfile")]
            public Component RegFile { get; set; }

            [XmlElement("resultbus")]
            public Component ResultBus { get; set; }

            [XmlElement("clockbus")]
            public Component ClockBus { get; set; }
        }

        public class Component
        {
            [XmlAttribute("total")]
            public int Total { get; set; }

            [XmlAttribute("utilization_avg")]
            public double UtilizationAvg { get; set; }

            [XmlAttribute("capacitance")]
            public double Capacitance { get; set; }

            [XmlAttribute("activity")]
            public double Activity { get; set; }

            [XmlAttribute("energy")]
            public double Energy { get; set; }

            [XmlAttribute("power")]
            public double Power { get; set; }

            [XmlAttribute("power_percent")]
            public double PowerPercent { get; set; }
        }
    }
}

