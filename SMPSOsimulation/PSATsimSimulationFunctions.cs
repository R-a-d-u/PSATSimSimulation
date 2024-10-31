using System.Diagnostics;
using FlaUI.Core;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;

namespace SMPSOsimulation
{
    internal class PSATsimSimulationFunctions
    {
        public void startSimulator()
        {
            try
            {

                // Set up process start information
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = @"E:\PSATSIM\PSATSim.exe",
                    Arguments = "",  // Replace with actual arguments
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false,
                    Verb="runas",
                    WorkingDirectory = @"E:\PSATSIM"

                };

                
                // Start the process and capture output
                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Display output and error messages
                    MessageBox.Show($"Output: {output}\nError: {error}", "PSATSim Output");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "PSATSim Error");
            }
        }
        public void SetSimulationParameters(string traceFilePath, string outputFilePath)
        {
            using (var app =FlaUI.Core.Application.Launch("E:\\PSATSIM\\PSATSim.exe"))
            using (var automation = new UIA3Automation())
            {
                var window = app.GetMainWindow(automation);

                var traceFileBox = window.FindFirstDescendant(cf => cf.ByAutomationId("traceFileTextBox")).AsTextBox();
                traceFileBox.Text = traceFilePath;

                var outputFileBox = window.FindFirstDescendant(cf => cf.ByAutomationId("outputFileTextBox")).AsTextBox();
                outputFileBox.Text = outputFilePath;

                var applyButton = window.FindFirstDescendant(cf => cf.ByText("Apply")).AsButton();
                applyButton.Click();
            }
        }

    }
}
