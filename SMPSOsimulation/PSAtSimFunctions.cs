using System.Diagnostics;
using static SMPSOsimulation.StructPSATSimOutput;
using System.Xml.Serialization;

namespace SMPSOsimulation
{
    internal class PSAtSimFunctions
    {
        private string exePath = @"E:\PSATSIM\psatsim_con.exe";
        private string configFile = "Test.xml";
        private string outputFile = "output.xml";
        private string workingDirectory = @"E:\PSATSIM";
        private Process currentProcess; // Variable to store the current process

        // Function to start the simulator with basic settings
        public void StartSimulator()
        {
            Console.Write(($"{configFile} {outputFile} "));

            RunProcess($"{configFile} {outputFile}");
        }

        public void SimulationOptionsFunction(string command)
        {
            Console.Write(($"{configFile} {outputFile} -{command}"));
            RunProcess($"{configFile} {outputFile} -{command}");
        }
        public List<Variation> LoadVariationsFromXml(string filePath)
        {
            var serializer = new XmlSerializer(typeof(PsatSimResults));

            using (var reader = new StreamReader(filePath))
            {
                var psatSimResults = (PsatSimResults)serializer.Deserialize(reader);
                return psatSimResults.Variations;
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
            string dllPath = @"E:\PSATSIM2\GTK\bin"; // Path to where libxml2.dll is located
            Environment.SetEnvironmentVariable("Path", dllPath + ";" + Environment.GetEnvironmentVariable("Path"));

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/K \"cd /d {workingDirectory} && {exePath} {arguments}\"",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = workingDirectory,
                    EnvironmentVariables =
            {
                ["Path"] = dllPath + ";" + Environment.GetEnvironmentVariable("Path")
            }
                };

                currentProcess = new Process { StartInfo = startInfo };
                currentProcess.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "PSATSim Error");
            }
        }
    }
}
