using System.Diagnostics;


public class SimOutorderWrapper {
    private Process? currentProcess = null; // Variable to store the current process
    private string workingDirectory;
    private string exePath;

    public SimOutorderWrapper(string exePath) {
        this.workingDirectory = Path.GetDirectoryName(exePath)!;
        this.exePath = exePath;
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
                Arguments = $"/K \"cd /d {workingDirectory} && {exePath} {arguments} && exit\"",
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
}