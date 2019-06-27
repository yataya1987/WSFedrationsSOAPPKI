// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Quartz.NetCore
{
    using System;
    using System.Threading.Tasks;
    using global::Funq;
    using global::Quartz;
    using ServiceStack.Quartz.ServiceInterface;

    // SAMPLE: GettingStarted-AppHost
    public class AppHostSample : AppHostBase
    {
        public AppHostSample() : base("Quartz Sample", typeof(CronDBService).Assembly)
        {
        }

        public override void Configure(Container container)
        {
            var quartzFeature = new QuartzFeature();

            // create a simple job trigger to repeat every minute 
            quartzFeature.RegisterJob<MyJob>(
                trigger =>
                    trigger.WithSimpleSchedule(s =>
                            s.WithInterval(TimeSpan.FromSeconds(3))
                              .RepeatForever()
                        )
                        .Build()
            );

            // register the plugin
            Plugins.Add(quartzFeature);
        }
    }
    // ENDSAMPLE
    
    // SAMPLE: GettingStarted-BasicJob
    public class MyJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {

            string fileName = "DBAdv.mdb";
            string sourcePath = @"C:\Users\Yousef.Ataya\Desktop\shared";
            string targetPath = @"C:\Users\Yousef.Ataya\Desktop\shared\backups-db";

            // Use Path class to manipulate file and directory paths.
            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            // To copy a folder's contents to a new location:
            // Create a new target folder. 
            // If the directory already exists, this method does not create a new directory.
            System.IO.Directory.CreateDirectory(targetPath);

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            System.IO.File.Copy(sourceFile, destFile, true);

            // To copy all the files in one directory to another directory.
            // Get the files in the source folder. (To recursively iterate through
            // all subfolders under the current directory, see
            // "How to: Iterate Through a Directory Tree.")
            // Note: Check for target path was performed previously
            //       in this code example.
            if (System.IO.Directory.Exists(sourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    fileName = System.IO.Path.GetFileName(s);
                    destFile = System.IO.Path.Combine(targetPath, DateTime.Now + fileName);
                    System.IO.File.Copy(s, destFile, true);
                }
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
            }


            return context.AsTaskResult();
        }
    }
    // ENDSAMPLE
}