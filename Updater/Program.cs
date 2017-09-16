using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking_New.exe"))
                {
                    //Exit the process
                    Process[] processes = Process.GetProcessesByName("Networking");

                    if (processes.Length > 0)
                    {
                        Console.WriteLine("Networking Processes: [" + processes.Length + "]");

                        foreach (Process process in processes)
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error: Failed to close Networking");
                                Console.WriteLine(ex.ToString());

                                RecoverError();
                            }
                        }

                        Console.WriteLine("Closed all Networking Processes");

                        Thread.Sleep(5000);
                    }

                    //Check and Delete older Networking
                    if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.exe"))
                    {
                        try
                        {
                            Console.WriteLine("Deleting Networking");

                            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.exe");

                            Console.WriteLine("Deleted Networking");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: Failed to delete Networking");
                            Console.WriteLine(ex.ToString());

                            RecoverError();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Missing Networking.exe");
                    }

                    //Rename new networking
                    Console.WriteLine("Renaming Networking_New -> Networking");

                    File.Move(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking_New.exe", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.exe");

                    Console.WriteLine("Renamed Networking_New");

                    Thread.Sleep(500);

                    if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.exe"))
                    {
                        Process process = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            WorkingDirectory = Environment.CurrentDirectory,
                            UseShellExecute = false,
                            FileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.exe",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true
                        };
                        process.StartInfo = startInfo;
                        process.Start();
                    }
                }
                else
                {
                    Console.WriteLine("Update Missing");

                    RecoverError();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                RecoverError();
            }

            Console.ReadLine();
        }

        public static void RecoverError()
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                FileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo;
            process.Start();

            //Environment.Exit(0);
        }
    }
}
