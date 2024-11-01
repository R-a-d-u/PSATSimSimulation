using System.Xml;
using System.Xml.Serialization;


namespace SMPSOsimulation
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());

            //import fisier configuri
            var configurations = ConfigurationFactory.CreateConfigurations();
            var psatSim = new PsatSim { Configurations = configurations };
           
            //sterge versiune xml si altele
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true 
            };

            string outputPath = @"E:\PSATSIM\test.xml";

            // Create XmlSerializerNamespaces with no namespaces
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            // Serialize to XML without declaration and namespaces
            using (var writer = XmlWriter.Create(outputPath, settings))
            {
                var serializer = new XmlSerializer(typeof(PsatSim));
                serializer.Serialize(writer, psatSim, emptyNamespaces);
            }

            Console.WriteLine($"Configurations saved to XML at {outputPath}");
        }
    }
}

