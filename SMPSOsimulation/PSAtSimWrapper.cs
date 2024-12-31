using System.Diagnostics;
using System.Xml;
using static SMPSOsimulation.StructPSATSimOutput;
using System.Xml.Serialization;
using SMPSOsimulation.dataStructures;

namespace SMPSOsimulation
{
    internal class PSAtSimWrapper
    {
        private const string exeName = "psatsim_con.exe";
        private const string configFile = "Test.xml";
        private const string outputFile = "output.xml";
        
        private string dllPath; // for example "E:\PSATSIM2\GTK\bin"; Path to where libxml2.dll is located
        private string exePath;
        private string workingDirectory;
        private Process? currentProcess = null; // Variable to store the current process

        public PSAtSimWrapper(string exePath, string dllPath)
        {
            if (string.IsNullOrWhiteSpace(exePath) || !exePath.EndsWith(exeName, StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(exePath))
            {
                throw new ArgumentException("This is not a valid path to an psatsim_con.exe file! I'm not dealing with this nonsense!");
            }

            this.exePath = exePath;
            this.workingDirectory = Path.GetDirectoryName(exePath)!;
            
            if (string.IsNullOrWhiteSpace(dllPath) || !Directory.Exists(dllPath))
            {
                throw new ArgumentException("This is not a valid path to a dll folder! I'm not dealing with this nonsense!");
            }

            this.dllPath = dllPath;
        }

        private void RunPSATSimWithArguments(string? arguments = null)
        {
            string command = $"{configFile} {outputFile} " + (arguments is null ? "" : $"{arguments}");
            RunProcess(command);
        }

        public List<Tuple<double[], int>> Evaluate(List<Tuple<CPUConfig, int>> configs, EnvironmentConfig environmentConfig)
        {
            CreateInputXMLFile(configs, environmentConfig);
            RunPSATSimWithArguments("-g -t 16");
            currentProcess!.WaitForExit();
            List<Tuple<double[], int>> results = GetResultsFromOutputXMLFile();
            return results;
        }

        private List<Tuple<double[], int>> GetResultsFromOutputXMLFile()
        {
            string filePath = workingDirectory + "/" + outputFile;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            XmlNode? root = xmlDoc.SelectSingleNode("psatsim_results");
            if (root == null)
            {
                throw new ApplicationException("Root element 'psatsim_results' not found!");
            }

            var variations = root.SelectNodes("variation");
            if (variations == null)
            {
                throw new ApplicationException("Element 'variation' not found!");
            }
            
            var results = new List<Tuple<double[], int>>();
            
            foreach (XmlNode variation in variations)
            {
                string? configuration = variation.Attributes?["configuration"]?.Value;
                if (configuration == null)
                {
                    throw new ApplicationException("No 'configuration' in variation!");
                }

                XmlNode? general = variation.SelectSingleNode("general");
                if (general == null)
                {
                    throw new ApplicationException($"No 'general' node found for variation with configuration: {configuration}");
                }

                string? energy = general.Attributes?["energy"]?.Value;
                if (energy == null)
                {
                    throw new ApplicationException("No 'energy' in variation!");
                }
                string? ipc = general.Attributes?["ipc"]?.Value;
                if (ipc == null)
                {
                    throw new ApplicationException("No 'ipc' in variation!");
                }

                results.Add(new Tuple<double[], int>([1 / double.Parse(ipc), double.Parse(energy)], int.Parse(configuration)));
            }
            
            return results;
        }

        private void CreateInputXMLFile(List<Tuple<CPUConfig, int>> configs, EnvironmentConfig environmentConfig)
        {
            // Create an XmlWriterSettings object to configure the output
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true, // Makes the output readable
                IndentChars = "  ", // Use two spaces for indentation
                NewLineOnAttributes = false
            };

            string filePath = workingDirectory + "/" + configFile;
            
            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("psatsim");

                foreach (Tuple<CPUConfig, int> configTuple in configs)
                {
                    var cpuConfig = configTuple.Item1;
                    var indexConfig = configTuple.Item2;
                    writer.WriteStartElement("config");
                    writer.WriteAttributeString("name", indexConfig.ToString());
                    
                    writer.WriteStartElement("general");
                    writer.WriteAttributeString("superscalar", cpuConfig.Superscalar.ToString());
                    writer.WriteAttributeString("rename", cpuConfig.Rename.ToString());
                    writer.WriteAttributeString("reorder", cpuConfig.Reorder.ToString());
                    writer.WriteAttributeString("rsb_architecture", cpuConfig.RsbArchitecture.ToString());
                    writer.WriteAttributeString("separate_dispatch", cpuConfig.SeparateDispatch.ToString().ToLower());
                    writer.WriteAttributeString("seed", "0");
                    writer.WriteAttributeString("trace", environmentConfig.Trace);
                    writer.WriteAttributeString("vdd", environmentConfig.Vdd.ToString());
                    writer.WriteAttributeString("frequency", cpuConfig.Freq.ToString());
                    writer.WriteEndElement();
                    
                    writer.WriteStartElement("execution");
                    writer.WriteAttributeString("architecture", "complex");
                    writer.WriteAttributeString("iadd", cpuConfig.Iadd.ToString());
                    writer.WriteAttributeString("imult", cpuConfig.Imult.ToString());
                    writer.WriteAttributeString("idiv", cpuConfig.Idiv.ToString());
                    writer.WriteAttributeString("fpadd", cpuConfig.Fpadd.ToString());
                    writer.WriteAttributeString("fpmult", cpuConfig.Fpmult.ToString());
                    writer.WriteAttributeString("fpdiv", cpuConfig.Fpdiv.ToString());
                    writer.WriteAttributeString("fpsqrt", cpuConfig.Fpsqrt.ToString());
                    writer.WriteAttributeString("branch", cpuConfig.Branch.ToString());
                    writer.WriteAttributeString("load", cpuConfig.Load.ToString());
                    writer.WriteAttributeString("store", cpuConfig.Store.ToString());
                    writer.WriteEndElement();
                    
                    writer.WriteStartElement("memory");
                    writer.WriteAttributeString("architecture", environmentConfig.MemoryArch.ToString());
                    writer.WriteStartElement("system");
                    writer.WriteAttributeString("latency", environmentConfig.SystemMemLatency.ToString());
                    writer.WriteEndElement();
                    writer.WriteStartElement("l1_code");
                    writer.WriteAttributeString("hitrate", environmentConfig.L1CodeHitrate.ToString());
                    writer.WriteAttributeString("latency", environmentConfig.L1CodeLatency.ToString());
                    writer.WriteEndElement();
                    writer.WriteStartElement("l1_data");
                    writer.WriteAttributeString("hitrate", environmentConfig.L1DataHitrate.ToString());
                    writer.WriteAttributeString("latency", environmentConfig.L1DataLatency.ToString());
                    writer.WriteEndElement();
                    writer.WriteStartElement("l2");
                    writer.WriteAttributeString("hitrate", environmentConfig.L2Hitrate.ToString());
                    writer.WriteAttributeString("latency", environmentConfig.L2Latency.ToString());
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    
                    writer.WriteEndElement();
                }
                
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        // Core function to run the process with specified arguments
        private void RunProcess(string arguments)
        {
            // Check if there's an existing process and terminate it
            if (currentProcess != null && !currentProcess.HasExited)
            {
                currentProcess.Kill();
                currentProcess.WaitForExit();
                currentProcess.Dispose();
                currentProcess = null;
            }

            string currentPath = Environment.GetEnvironmentVariable("Path");

            try
            {
                
                string updatedPath = currentPath + ";" + dllPath;
                Environment.SetEnvironmentVariable("Path", updatedPath);


                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/K \"cd /d {workingDirectory} && {exePath} {arguments} && exit\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                    EnvironmentVariables =
                    {
                        ["Path"] =  Environment.GetEnvironmentVariable("Path") + ";" + dllPath
                    }
                };

                currentProcess = new Process { StartInfo = startInfo };
                currentProcess.Start();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error: {ex.Message}, PSATSim Error");
            }
            finally
            {
                Environment.SetEnvironmentVariable("Path", currentPath);
            }
        }
    }
}
