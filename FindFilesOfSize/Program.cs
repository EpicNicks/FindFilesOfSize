using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace FindFilesOfSize
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                while (true)
                {
                    Console.WriteLine("Do you wish to display ALL files: (y/n)");
                    string input = Console.ReadLine();

                    if (input == "y")
                    {
                        string completeLog = BuildFileList(0, out string selectedPath);

                        Console.WriteLine("All files in " + selectedPath + " with size greater than " + args[0] + " " + args[1]);
                        Console.WriteLine(completeLog);

                        OutputToFile(completeLog, 0);
                        break;
                    }
                    else if (input == "n")
                    {
                        Console.WriteLine("Bye");
                        break;
                    }
                    Console.WriteLine("Invalid input");
                }

            }
            //where args[0] is the file size to filter by
            else if(args.Length == 2)
            {
                //determine input:
                long fileSizeThreshold = ConvertDataChunk(args[1], long.Parse(args[0]));
                
                string completeLog = BuildFileList(fileSizeThreshold, out string selectedPath);

                Console.WriteLine("All files in " + selectedPath + " with size greater than " + args[0] + " " + args[1]);
                Console.WriteLine(completeLog);

                OutputToFile(completeLog, fileSizeThreshold);
            }
            else
            {
                Console.WriteLine(
                    "Invalid amount of arguments entered\n"
                    + "------------------------------------------------------------\n"
                    + "Acceptable argument lengths: 0, 2\n"
                    + "------------------------------------------------------------\n"
                    + "2 arguments: findfilesofsize \"numeric size\" \"units\"\n"
                    + "Acceptable units: gb: gigabytes, mb: megabytes, kb: kilobytes, anything else: bytes\n"
                    + "------------------------------------------------------------\n"
                    + "0 arguments: findfilesofsize\n"
                    + "Displays all files regardless of size\n"
                    );
            }
        }

        #region Extracted Methods
        static long ConvertDataChunk(string chunk, long numToConvert)
        {
            if (chunk.ContainsIgnoreCase("kb"))
            {
                numToConvert *= 1024;
            }
            else if (chunk.ContainsIgnoreCase("mb"))
            {
                numToConvert *= 1024 * 1024;
            }
            else if (chunk.ContainsIgnoreCase("gb"))
            {
                numToConvert *= 1024 * 1024 * 1024;
            }
            return numToConvert;
        }

        static string BuildFileList(long fileSizeThreshold, out string selectedPath)
        {
            DirectoryInfo dInfo;
            using (FolderBrowserDialog ofd = new FolderBrowserDialog())
            {
                ofd.Description = "Select folder to search";
                DialogResult dRes = ofd.ShowDialog();
                if (dRes == DialogResult.OK)
                {
                    selectedPath = ofd.SelectedPath;
                    dInfo = new DirectoryInfo(ofd.SelectedPath);
                }
                else
                {
                    //Quit application
                    Console.WriteLine("Exiting Application...");
                    Environment.Exit(1);
                    selectedPath = null;
                    return null;
                }
            }

            IEnumerable<FileInfo> fileList = FindAllFiles(dInfo.FullName);

            List<FileInfo> largestFiles = new List<FileInfo>();
            foreach (var file in fileList)
            {
                if (file.Length > fileSizeThreshold)
                {
                    largestFiles.Add(file);
                }
            }

            StringBuilder sBuilder = new StringBuilder();
            foreach (var file in largestFiles)
            {
                sBuilder.Append("FileName: " + file.Name
                    + "\nFileSize: " + file.Length + " bytes"
                    + "\nFullPath: " + file.FullName + "\n\n");
            }

            return sBuilder.ToString();
        }

        static void OutputToFile(string completeLog, long fileSizeThreshold)
        {
            while (true)
            {
                Console.WriteLine("Dump list to .txt file? (overwrites old dump if same location) (y/n)");
                string input = Console.ReadLine();
                if (input == "y")
                {
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = "Select Output Folder";
                        DialogResult diagRes = fbd.ShowDialog();
                        if (diagRes == DialogResult.OK)
                        {
                            string selectedPath = fbd.SelectedPath;
                            string outputFileName = selectedPath + @"\foundFilesOfSizeGreaterThan" + fileSizeThreshold + "b.txt";
                            FileStream outputFile = File.Create(outputFileName);
                            outputFile.Close();
                            File.WriteAllText(outputFileName, completeLog);
                            Console.WriteLine(
                                "Output created!\nOutput dumped to\\ " + outputFileName
                                );
                            return;
                        }
                        else
                        {
                            //Quit application
                            Console.WriteLine("Exiting Application...");
                            return;
                        }
                    }
                }
                else if (input == "n")
                {
                    Console.WriteLine("\nbye");
                    return;
                }
                Console.WriteLine("Invalid Input\n");
            }
        }
        #endregion
        #region Helper Methods
        static bool ContainsIgnoreCase(this string source, string substring)
        {
            return source.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static List<FileInfo> FindAllFiles(string rootDir)
        {
            var pathsToSearch = new Queue<DirectoryInfo>();
            var foundFiles = new List<FileInfo>();

            pathsToSearch.Enqueue(new DirectoryInfo(rootDir));

            while (pathsToSearch.Count > 0)
            {
                var dir = pathsToSearch.Dequeue().FullName;

                try
                {
                    var files = new DirectoryInfo(dir).GetFiles();
                    foreach (var file in files)
                    {
                        foundFiles.Add(file);
                    }

                    foreach (var subDir in Directory.GetDirectories(dir))
                    {
                        pathsToSearch.Enqueue(new DirectoryInfo(subDir));
                    }

                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
            }

            return foundFiles;
        }
        #endregion
    }
}
