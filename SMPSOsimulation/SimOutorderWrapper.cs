using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;


public class SimOutorderWrapper
{
    private Process? currentProcess = null; // Variable to store the current process
    private string workingDirectory;
    private string exePath;
    private const string simout = "results/simout.res";
    private const string progout = "results/progout.res";

    private string tracePath;

    public SimOutorderWrapper(string exePath, string tracePath)
    {
        this.workingDirectory = Path.GetDirectoryName(exePath)!;
        this.exePath = exePath;
        this.tracePath = tracePath;

        string resultPath = Path.Combine(this.workingDirectory, "results");
        if (!Directory.Exists(resultPath))
        {
            Directory.CreateDirectory(resultPath);
        }
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

    private (CPI cpi, Energy energy) Evaluate(CPUConfig config, EnvironmentConfig environmentConfig)
    {
        // Ensure working directory exists (optional, but good practice)
        Directory.CreateDirectory(workingDirectory);

        // Define unique filenames for this run if needed, or use fixed names
        // For example:
        // simout = $"sim_{Guid.NewGuid()}.out";
        // progout = $"prog_{Guid.NewGuid()}.out";

        var simOutorderConfig = new SimOutorderConfig(config, environmentConfig);
        simOutorderConfig.RedirectSimOutput = simout; // Assigns the filename
        simOutorderConfig.RedirectProgOutput = progout; // Assigns the filename
        simOutorderConfig.PowerPrintStats = true;

        Console.WriteLine($"DEBUGGGGGGGGGGGGGGG to evaluate config : {simOutorderConfig.ToCommandLineString()}");
        RunProcess(simOutorderConfig.ToCommandLineString());
        

        if (currentProcess == null)
        {
            throw new InvalidOperationException("Process was not started correctly.");
        }

        currentProcess.WaitForExit();
        Console.WriteLine("Gata evaluatu");

        // *** Check Exit Code and Capture STREAMS/FILE on Error ***
        if (currentProcess.ExitCode != 0)
        {
            StringBuilder errorDetailsBuilder = new StringBuilder();
            errorDetailsBuilder.AppendLine($"External simulation process failed with ExitCode {currentProcess.ExitCode}.");
            errorDetailsBuilder.AppendLine($"Working Directory: {Path.GetFullPath(workingDirectory)}"); // Added for clarity
            errorDetailsBuilder.AppendLine($"Command used was: {simOutorderConfig.ToCommandLineString()}");

            // --- Read Captured Standard Error ---
            try
            {
                string stdErr = currentProcess.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(stdErr))
                {
                    errorDetailsBuilder.AppendLine("\n--- Standard Error Output ---");
                    const int maxLen = 2048;
                    errorDetailsBuilder.AppendLine(stdErr.Length <= maxLen ? stdErr.Trim() : stdErr.Substring(0, maxLen).Trim() + "...");
                    errorDetailsBuilder.AppendLine("--- End Standard Error Output ---");
                }
                else
                {
                    errorDetailsBuilder.AppendLine("\n(No output captured on Standard Error stream.)");
                }
            }
            catch (Exception streamEx)
            {
                errorDetailsBuilder.AppendLine($"\n(An error occurred while reading Standard Error stream: {streamEx.Message})");
            }

            // --- Read Captured Standard Output ---
            try
            {
                string stdOut = currentProcess.StandardOutput.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(stdOut))
                {
                    errorDetailsBuilder.AppendLine("\n--- Standard Output ---");
                    string trimmedStdOut = stdOut.Trim();
                    const int maxLen = 2048;
                    // Show the *end* of the output if it's too long, as errors often appear last
                    errorDetailsBuilder.AppendLine(trimmedStdOut.Length <= maxLen ? trimmedStdOut : "..." + trimmedStdOut.Substring(trimmedStdOut.Length - maxLen));
                    errorDetailsBuilder.AppendLine("--- End Standard Output ---");
                }
                else
                {
                    errorDetailsBuilder.AppendLine("\n(No output captured on Standard Output stream.)");
                }
            }
            catch (Exception streamEx)
            {
                errorDetailsBuilder.AppendLine($"\n(An error occurred while reading Standard Output stream: {streamEx.Message})");
            }

            // --- Read 'simout' file for "fatal:" lines ---
            // Do this *before* deleting the file
            string simoutPath = Path.Combine(workingDirectory, simout);
            errorDetailsBuilder.AppendLine($"\n--- Searching for 'fatal:' lines in {simout} ({simoutPath}) ---");
            try
            {
                if (File.Exists(simoutPath))
                {
                    var fatalLinesFound = new List<string>();
                    // Use StreamReader for line-by-line reading and proper disposal
                    using (StreamReader reader = new StreamReader(simoutPath))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // Case-insensitive search for "fatal:"
                            if (line.IndexOf("fatal:", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                fatalLinesFound.Add(line.Trim());
                            }
                        }
                    } // StreamReader is disposed here

                    if (fatalLinesFound.Any())
                    {
                        errorDetailsBuilder.AppendLine("Found the following 'fatal:' lines:");
                        foreach (var fatalLine in fatalLinesFound)
                        {
                            // Optional: Limit length of each reported fatal line if they can be very long
                            const int maxFatalLineLen = 512;
                            errorDetailsBuilder.AppendLine($"  * {(fatalLine.Length <= maxFatalLineLen ? fatalLine : fatalLine.Substring(0, maxFatalLineLen) + "...")}");
                        }
                    }
                    else
                    {
                        errorDetailsBuilder.AppendLine("(No lines containing 'fatal:' found.)");
                    }
                }
                else
                {
                    errorDetailsBuilder.AppendLine($"(File not found. It might not have been created or was already deleted.)");
                }
            }
            catch (IOException ioEx) // Catch specific IO exceptions
            {
                errorDetailsBuilder.AppendLine($"\n(IO Error occurred while trying to read the simout file: {ioEx.Message})");
            }
            catch (Exception fileEx) // Catch any other potential exceptions during file read
            {
                errorDetailsBuilder.AppendLine($"\n(An unexpected error occurred while trying to read the simout file: {fileEx.Message})");
            }
            finally // Ensure end marker is added even if exceptions occur during reading
            {
                errorDetailsBuilder.AppendLine($"--- End search in {simout} ---");
            }
            // --- End reading 'simout' ---


            // Attempt cleanup of potentially unused/incomplete redirected files
            // // Now delete simout (if it exists) and progout
            // try
            // {
            //     if (File.Exists(simoutPath))
            //     {
            //         File.Delete(simoutPath);
            //     }
            // }
            // catch (Exception ex)
            // {
            //     errorDetailsBuilder.AppendLine($"\n(Warning: Failed to delete {simoutPath}: {ex.Message})");
            //     // Decide if you want to continue or stop here. Continuing might leave files behind.
            // }
            try
            {
                string progoutPath = Path.Combine(workingDirectory, progout);
                if (File.Exists(progoutPath))
                {
                    File.Delete(progoutPath);
                }
            }
            catch (Exception ex)
            {
                errorDetailsBuilder.AppendLine($"\n(Warning: Failed to delete {progout}: {ex.Message})");
            }

            // Throw the exception with all gathered details
            throw new ApplicationException(errorDetailsBuilder.ToString());
        } // End if (ExitCode != 0)

        // --- Process exited successfully ---
        var results = GetResultsFromSimout(); // Assumes this reads 'simout'

        // --- Cleanup successful run files ---
        try
        {
            string simoutPath = Path.Combine(workingDirectory, simout);
            string progoutPath = Path.Combine(workingDirectory, progout);
            //if (File.Exists(simoutPath)) File.Delete(simoutPath);
            //if (File.Exists(progoutPath)) File.Delete(progoutPath);
        }
        catch (Exception ex)
        {
            // Log this as a warning, but don't fail the whole operation since results were obtained
            Console.Error.WriteLine($"Warning: Successfully read results, but failed to delete simulation files in '{workingDirectory}'. Error: {ex.Message}");
            // Depending on requirements, you might want to re-throw or handle differently
            // throw new IOException($"Warning: Successfully read results, but failed to delete files in '{workingDirectory}'. Error: {ex.Message}", ex);
        }
        return results;
    }
    public List<(double cpi, double energy, int originalIndex)> Evaluate(List<(CPUConfig config, int originalIndex)> configs, EnvironmentConfig environmentConfig)
    {
        List<(double cpi, double energy, int originalIndex)> retlist = new();
        foreach (var config in configs)
        {
            var result = Evaluate(config.config, environmentConfig);
            retlist.Add((result.cpi.Value, result.energy.Value, config.originalIndex));
        }
        return retlist;
    }
}