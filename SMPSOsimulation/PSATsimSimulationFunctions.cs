using System.Diagnostics;
using FlaUI.Core;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SMPSOsimulation
{
    internal class PSATsimSimulationFunctions
    {



        private string exePath = @"E:\PSATSIM\psatsim_con.exe";
        private string configFile = "FisierConfigBun.xml";
        private string outputFile = "output.xml";
        private string workingDirectory = @"E:\PSATSIM";

        // Function to start the simulator with basic settings
        public void StartSimulator()
        {
            RunProcess($"{configFile} {outputFile}");
        }

        public void SimulationOptionsFunction(String command)
        {
            RunProcess($"{configFile} {outputFile} -{command}");
        }

        // Function to set thread count
        public void StartWithThreadCount(int threadCount)
        {
            RunProcess($"{configFile} {outputFile} -t {threadCount}");
        }

        // Function to print all sections
        public void StartWithPrintAll()
        {
            RunProcess($"{configFile} {outputFile} -a");
        }

        // Function to suppress all print sections
        public void StartWithSuppressAll()
        {
            RunProcess($"{configFile} {outputFile} -A");
        }

        // Function to run with custom print options
        public void StartWithCustomPrintOptions(string printOptions)
        {
            RunProcess($"{configFile} {outputFile} {printOptions}");
        }

        // Core function to run the process with specified arguments
        public void RunProcess(string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/K \"cd /d {workingDirectory} && {exePath} {arguments}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "PSATSim Error");
            }
        }
    }



}

