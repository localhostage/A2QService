namespace WebApplication1;

using System;
using System.Diagnostics;

public static class ProcessRunner
{
    public static void RunExecutable(string executablePath, string arguments = "")
    {
        // Create new process start info
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = executablePath, // the path of the executable
            Arguments = arguments, // arguments to pass to the executable, if any
            UseShellExecute = false, // don't use shell execute
            RedirectStandardOutput = true, // redirect standard output
            CreateNoWindow = true // don't create a window
        };

        // Create a new process
        Process process = new Process
        {
            StartInfo = startInfo // set the start info
        };

        // Register event handler for output
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine(e.Data);
            }
        };

        // Start the process
        process.Start();

        // Begin reading the output
        process.BeginOutputReadLine();

        // Wait for the process to finish
        process.WaitForExit();

        // Close the process
        process.Close();
    }
}