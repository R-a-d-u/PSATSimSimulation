using System.Diagnostics;
using static SMPSOsimulation.StructPSATSimOutput;
using System.Xml.Serialization;

namespace SMPSOsimulation
{
    internal class PSAtSimFunctions
    {
        private const string exeName = "psatsim_con.exe";
        private const string configFile = "Test.xml";
        private const string outputFile = "output.xml";
        
        private string exePath;
        private string workingDirectory;
        private Process? currentProcess = null; // Variable to store the current process

        public PSAtSimFunctions(string exePath)
        {
            // Check if it's a valid path and ends with .exe
            if (string.IsNullOrWhiteSpace(exePath) || !exePath.EndsWith(exeName, StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(exePath))
            {
                throw new ArgumentException("This is not a valid path to an psatsim_con.exe file! I'm not dealing with this nonsense!");
            }

            this.exePath = exePath;
            this.workingDirectory = Path.GetDirectoryName(exePath)!;
        }

        private void RunSimExe(string? arguments = null)
        {
            string command = $"{configFile} {outputFile} " + (arguments is null ? "" : $"--{arguments}");
            RunProcess(command);
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
