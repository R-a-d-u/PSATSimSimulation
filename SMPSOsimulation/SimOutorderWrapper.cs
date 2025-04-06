using System.Diagnostics;
using System.Globalization;


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
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "cmd.exe",
                Arguments = $"/K \"cd /d {workingDirectory} && {exePath} {arguments} {tracePath} && exit\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory,
            };

            currentProcess = new Process { StartInfo = startInfo };
            currentProcess.Start();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Error: {ex.Message}, Msim Error");
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
            // Scream and Shout - File not found
            Console.Error.WriteLine($"ERROR: Simulation output file not found at '{filePath}'.");
            // Re-throw with more context or let it propagate
            throw new FileNotFoundException($"Simulation output file '{filePath}' not found.", filePath, ex);
        }
        catch (IOException ex)
        {
            // Scream and Shout - Error reading file (permissions, disk issue, etc.)
            Console.Error.WriteLine($"ERROR: Failed to read simulation output file '{filePath}'. Check permissions and disk space.");
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