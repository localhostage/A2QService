using System.Diagnostics;
using System.Text.RegularExpressions;
using WebApplication1.Model;

namespace WebApplication1.Managers;

public class JobManager
{
    private Queue<Job> JobQueue { get; } = new Queue<Job>();

    public int TotalJobs => JobQueue.Count;
    
    public JobManager()
    {
    }

    public string AddJob(string url)
    {
        Console.WriteLine("Adding job for {0}", url);

        // generate a Task
        var job = new Job
        {
            Id = Guid.NewGuid().ToString(),
            Url = url,
            Created = DateTime.Now,
            Started = DateTime.MinValue,
            Finished = DateTime.MinValue,
            Progress = 0,
            Status = Job.StatusEnum.Waiting
        };

        // add the task to the queue
        JobQueue.Enqueue(job);

        // start the task
        TryRunJob();

        return job.Id;
    }

    private void TryRunJob()
    {
        // if there are no tasks in the queue, return
        if (JobQueue.Count == 0)
        {
            return;
        }

        // get the first task in the queue
        var job = JobQueue.Peek();

        // if the task is already running, return
        if (job.Status == Job.StatusEnum.Running)
        {
            return;
        }

        // start the task
        job.Started = DateTime.Now;
        job.Status = Job.StatusEnum.Running;

        // run the task
        RunJob(job);
    }

    private void RunJob(Job job)
    {
        // regex
        var regex = new Regex(@"(\d*\.?\d+)?% of");

        // run task in new thread
        var task = Task.Run(() =>
        {
            // Create new process start info
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "D:\\Tools\\yt-dlp.exe",
                Arguments =
                    $"-f bv+ba -P D:\\Output -P \"temp:tmp\" -P \"subtitle:subs\" --embed-subs --write-auto-sub --sub-lang \"en.*\" {job.Url}", 
                UseShellExecute = false, 
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true 
            };
            
            startInfo.EnvironmentVariables["PATH"] += @";D:\Tools\ffmpeg-2023-07-19-git-efa6cec759-full_build\bin";

            // Create a new process 
            Process process = new Process
            {
                StartInfo = startInfo // set the start info
            };

            // Register event handler for output
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    var output = e.Data;
                    Console.WriteLine($"[{job.Id}] {output}");
                }
            };
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    var output = e.Data;
                    Console.WriteLine($"[{job.Id}] {output}");
                    
                    // check if the output contains a progress update
                    var match = regex.Match(output);
                    if (match.Success)
                    {
                        // get the progress
                        var progressStr = match.Groups[1].Value;
                        if (double.TryParse(progressStr, out var progress))
                        {
                            // update the progress
                            job.Progress = progress;
                            Console.WriteLine("Progress: {0}", progress);
                        }
                    }
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

            // mark the task as finished
            job.Finished = DateTime.Now;
            job.Status = Job.StatusEnum.Finished;
        });
        
        // log message after task finished
        task.ContinueWith(t =>
        {
            Console.WriteLine($"[{job.Id}] Task finished");
            
            // remove the task from the queue
            JobQueue.Dequeue();

            // try to run the next task
            TryRunJob();
        });
    }
}