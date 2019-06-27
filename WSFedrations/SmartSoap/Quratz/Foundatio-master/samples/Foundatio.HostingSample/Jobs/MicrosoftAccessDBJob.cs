using System.Threading;
using System.Threading.Tasks;
using Foundatio.Jobs;
using Microsoft.Extensions.Logging;
using System;

namespace Foundatio.HostingSample {
    [Job(Description = "Sample 1 job", Interval = "5s", IterationLimit = 5)]
    public class Sample1Job : IJob {
        private readonly ILogger _logger;
        private int _iterationCount = 0;
        private static int counter = Foundatio.HostingSample.Program.counter;
        public Sample1Job(ILoggerFactory loggerFactory) {
            _logger = loggerFactory.CreateLogger<Sample1Job>();
        }

        public Task<JobResult> RunAsync(CancellationToken cancellationToken = default) {
            Interlocked.Increment(ref _iterationCount);

            string fileName = "DBAdv.mdb";
            string sourcePath = @"C:\Users\Yousef.Ataya\Desktop\shared";
            string targetPath = @"C:\Users\Yousef.Ataya\Desktop\shared\backups-db";

            // Use Path class to manipulate file and directory paths.
            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            //var fileNameExtra = DateTime.Now.ToString()+"---" + fileName;
            
            var selectPATH  = DateTime.Now.ToString("yyyy-MM-dd") + "(" + counter++ + ")" + fileName;
            string destFile = null;
            if (!System.IO.File.Exists(selectPATH)) {
                 destFile = System.IO.Path.Combine(targetPath, selectPATH);
            }

            // To copy a folder's contents to a new location:
            // Create a new target folder. 
            // If the directory already exists, this method does not create a new directory.
            System.IO.Directory.CreateDirectory(targetPath);

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            if (!System.IO.File.Exists(selectPATH)) {
                System.IO.File.Copy(sourceFile, destFile, true);
            }
            // To copy all the files in one directory to another directory.
            // Get the files in the source folder. (To recursively iterate through
            // all subfolders under the current directory, see
            // "How to: Iterate Through a Directory Tree.")
            // Note: Check for target path was performed previously
            //       in this code example.
            if (System.IO.Directory.Exists(sourcePath)) {
                string[] files = System.IO.Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files) {
                    // Use static Path methods to extract only the file name from the path.
                    fileName = "DBAdv.mdb";
                    fileName = DateTime.Now.ToString("yyyy-MM-dd") + "(" + counter++ + ")" + fileName;
                    destFile = System.IO.Path.Combine(targetPath, fileName);
                    System.IO.File.Copy(sourceFile,  destFile, true);
                }
            } else {
                Console.WriteLine("Source path does not exist!");
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogTrace("Sample1Job Run #{IterationCount} Thread={ManagedThreadId}", _iterationCount, Thread.CurrentThread.ManagedThreadId);
            selectPATH = null;
            fileName = null;
            sourcePath = null;
            targetPath = null;
            return Task.FromResult(JobResult.Success);
        }
    }
}