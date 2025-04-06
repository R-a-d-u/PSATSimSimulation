using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;


public class SimOutorderWrapper {
    private Process? currentProcess = null; // Variable to store the current process
    private string workingDirectory;
    private string exePath;
    private const string simout = "result/simout.res";
    private const string progout = "result/progout.res";

    private string tracePath;

    public SimOutorderWrapper(string exePath, string tracePath) {
        this.workingDirectory = Path.GetDirectoryName(exePath)!;
        this.exePath = exePath;
        this.tracePath = tracePath;
    }
    
    private void RunProcess(string arguments)
    {
        // --- Terminate existing process (generally cross-platform) ---
        if (currentProcess != null && !currentProcess.HasExited)
        {
            try
            {
                // Use Kill(true) on Windows to attempt killing child processes as well.
                // On Unix, Kill sends SIGKILL, which doesn't inherently kill children
                // unless they are part of the same process group and handled appropriately.
                bool killTree = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                currentProcess.Kill(killTree);
                currentProcess.WaitForExit(5000); // Wait up to 5 seconds for termination
            }
            catch (InvalidOperationException e)
            {
                // Process might have already exited between check and Kill
                throw new Exception($"Previous process already exited. (exception: {e.Message})");
            }
            catch (Exception ex)
            {
                // Log failure but continue, maybe the process is already gone or unstoppable
                throw new Exception($"Warning: Failed to terminate previous process: {ex.Message}");
            }
            finally
            {
                currentProcess.Dispose();
                currentProcess = null;
            }
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = false, // Crucial for cross-platform and redirection
                CreateNoWindow = true,   // Hide window on Windows, no effect on Linux usually
                WorkingDirectory = workingDirectory, // Set the process's working dir
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            string commandToExecute;
            // Basic quoting for paths, might need more robust solution if paths contain quotes themselves
            string safeExePath = $"\"{exePath}\"";
            string safeTracePath = $"\"{tracePath}\"";
            string safeWorkingDirectory = $"\"{workingDirectory}\"";


            // --- OS-Specific Shell and Arguments ---
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                startInfo.FileName = "cmd.exe";
                // Use /C to execute command then exit. Safer than /K with CreateNoWindow=true.
                // Command: cd /d "WORKDIR" && "EXECPATH" arguments "TRACEPATH"
                commandToExecute = $"cd /d {safeWorkingDirectory} && {safeExePath} {arguments} {safeTracePath}";
                startInfo.Arguments = $"/C \"{commandToExecute}\""; // Pass the command string to cmd.exe
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Use /bin/sh for better compatibility than /bin/bash on some systems
                startInfo.FileName = "/bin/sh";
                // Command: cd "WORKDIR" && "EXECPATH" arguments "TRACEPATH"
                // Note: Ensure exePath is executable (chmod +x) on Linux/macOS.
                commandToExecute = $"cd {safeWorkingDirectory} && {safeExePath} {arguments} {safeTracePath}";
                startInfo.Arguments = $"-c \"{commandToExecute}\""; // Pass the command string to sh/bash
            }
            else
            {
                throw new PlatformNotSupportedException($"Operating system not supported: {RuntimeInformation.OSDescription}");
            }

            currentProcess = new Process { StartInfo = startInfo };
            currentProcess.Start();

        }
        catch (Exception ex) // Catch Win32Exception, PlatformNotSupportedException, etc.
        {
            // Provide more context in the thrown exception
            throw new ApplicationException($"Error starting process '{exePath}' on {RuntimeInformation.OSDescription}. Details: {ex.Message}", ex);
        }
    }

    private (CPI cpi, Energy energy) GetResultsFromSimout()
    {
        // Combine path safely (handles different OS separators)
        string filePath = Path.Combine(workingDirectory, simout);

        double? foundCpi = null;
        double? foundEnergy = null;

        const string cpiKey = "sim_CPI";
        const string energyKey = "Total Power Consumption:";

        try
        {
            // Use File.ReadLines for better memory efficiency with potentially large files
            // compared to File.ReadAllLines()
            foreach (string line in File.ReadLines(filePath))
            {
                // --- Check for CPI ---
                if (!foundCpi.HasValue && line.Contains(cpiKey))
                {
                    // Example: "sim_CPI             1.4022 # cycles per instruction"
                    // Split the line by whitespace, removing empty entries
                    string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    // Expecting format like: [ "sim_CPI", "1.4022", "#", "cycles", ... ]
                    if (parts.Length > 1 && parts[0] == cpiKey)
                    {
                        // Try parsing the second part using InvariantCulture (expects '.')
                        if (double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double cpiValue))
                        {
                            foundCpi = cpiValue;
                        }
                        else
                        {
                            // Found the line but couldn't parse the number - Scream and Shout!
                            throw new FormatException($"Could not parse CPI value from line: '{line}' in file '{filePath}'. Expected a double after '{cpiKey}'.");
                        }
                    }
                    // Optional: Add warning or error if line contains key but format is unexpected
                }

                // --- Check for Energy ---
                else if (!foundEnergy.HasValue && line.Contains(energyKey))
                {
                    // Example: "Total Power Consumption: 95.1592"
                    int valueStartIndex = line.IndexOf(':') + 1;
                    if (valueStartIndex > 0) // Ensure ':' was found
                    {
                        string valueString = line.Substring(valueStartIndex).Trim();
                        // Try parsing the extracted string using InvariantCulture (expects '.')
                        if (double.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out double energyValue))
                        {
                            foundEnergy = energyValue;
                        }
                        else
                        {
                            // Found the line but couldn't parse the number - Scream and Shout!
                            throw new FormatException($"Could not parse Energy value from line: '{line}' in file '{filePath}'. Expected a double after '{energyKey}'.");
                        }
                    }
                    // Optional: Add warning or error if line contains key but format is unexpected
                }

                // --- Optimization: Exit early if both values are found ---
                if (foundCpi.HasValue && foundEnergy.HasValue)
                {
                    break; // Stop reading the rest of the file
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException($"Simulation output file '{filePath}' not found.", filePath, ex);
        }
        catch (IOException ex)
        {
            throw new IOException($"An error occurred while reading the file '{filePath}'.", ex);
        }
        // Catch other potential exceptions during processing if necessary
        // catch (Exception ex) { ... }


        // --- Final Checks - Scream and Shout if values were not found ---
        if (!foundCpi.HasValue)
        {
            throw new InvalidDataException($"Could not find the '{cpiKey}' line in the simulation output file '{filePath}'.");
        }

        if (!foundEnergy.HasValue)
        {
            throw new InvalidDataException($"Could not find the '{energyKey}' line in the simulation output file '{filePath}'.");
        }

        // --- Return the results ---
        // We know the values are non-null here due to the checks above
        return (new CPI(foundCpi.Value), new Energy(foundEnergy.Value));
    }

    private (CPI cpi, Energy energy) Evaluate(CPUConfig config, EnvironmentConfig environmentConfig) {
        var simOutorderConfig = new SimOutorderConfig(config, environmentConfig);
        simOutorderConfig.RedirectSimOutput = simout;
        simOutorderConfig.RedirectProgOutput = progout;
        simOutorderConfig.PowerPrintStats = true;
        RunProcess(simOutorderConfig.ToCommandLineString());
        currentProcess!.WaitForExit();
        // *** Check Exit Code and Capture STREAMS on Error ***
        if (currentProcess.ExitCode != 0)
        {
            StringBuilder errorDetailsBuilder = new StringBuilder();
            errorDetailsBuilder.AppendLine($"External simulation process failed with ExitCode {currentProcess.ExitCode}.");

            try
            {
                // --- Read Captured Standard Error ---
                // ReadToEnd() is safe here because the process has already exited.
                string stdErr = currentProcess.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    errorDetailsBuilder.AppendLine("--- Standard Error Output ---");
                    // Limit output length in exception message if necessary
                    const int maxLen = 2048;
                    errorDetailsBuilder.AppendLine(stdErr.Length <= maxLen ? stdErr.Trim() : stdErr.Substring(0, maxLen).Trim() + "...");
                    // errorDetailsBuilder.AppendLine(stdErr.Trim()); // Use this if length isn't a concern
                    errorDetailsBuilder.AppendLine("--- End Standard Error Output ---");
                }
                else
                {
                    errorDetailsBuilder.AppendLine("(No output captured on Standard Error stream.)");
                }

                // --- Read Captured Standard Output ---
                string stdOut = currentProcess.StandardOutput.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(stdOut))
                {
                    errorDetailsBuilder.AppendLine("--- Standard Output ---");
                    // Limit output length in exception message if necessary
                    const int maxLen = 2048;
                    errorDetailsBuilder.AppendLine(stdOut.Length <= maxLen ? stdOut.Trim() : stdOut.Substring(0, maxLen).Trim() + "...");
                    // errorDetailsBuilder.AppendLine(stdOut.Trim()); // Use this if length isn't a concern
                    errorDetailsBuilder.AppendLine("--- End Standard Output ---");
                }
                else
                {
                    errorDetailsBuilder.AppendLine("(No output captured on Standard Output stream.)");
                }
            }
            catch (Exception streamEx)
            {
                // Unlikely after process exit, but possible
                errorDetailsBuilder.AppendLine($"\n(An error occurred while reading process output streams: {streamEx.Message})");
            }

            // Attempt cleanup of potentially unused/incomplete redirected files (simout/progout)
            try { File.Delete(Path.Combine(workingDirectory, simout)); } catch { /* Ignore */ }
            try { File.Delete(Path.Combine(workingDirectory, progout)); } catch { /* Ignore */ }

            throw new ApplicationException(errorDetailsBuilder.ToString());
        }
        var results = GetResultsFromSimout();
        try
        {
            File.Delete(Path.Combine(workingDirectory, simout));
            File.Delete(Path.Combine(workingDirectory, progout));
        }
        catch (Exception ex)
        {
            throw new Exception($"Warning: Successfully read results, but failed to delete files in '{workingDirectory}'. Error: {ex.Message}");
        }
        return results;
    }

    public List<(double cpi, double energy, int originalIndex)> Evaluate(List<(CPUConfig config, int originalIndex)> configs, EnvironmentConfig environmentConfig) {
        List<(double cpi, double energy, int originalIndex)> retlist = new();
        foreach (var config in configs) {
            var result = Evaluate(config.config, environmentConfig);
            retlist.Add((result.cpi.Value, result.energy.Value, config.originalIndex));
        }
        return retlist;
    }
}