using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SMPSOsimulation
{
    [XmlRoot("psatsim")]
    public class PsatSim
    {
        [XmlElement("config")]
        public List<Configuration> ?Configurations { get; set; }
    }
}
