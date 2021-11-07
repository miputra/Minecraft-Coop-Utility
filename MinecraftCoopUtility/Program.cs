using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace MinecraftCheckOnline
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static bool socketError = false;
        static int maxTimeout = 10; //in seconds
        static int pid = 0;
        static string launcher = "server.jar";
        static string cloudService = "onedrive.live.com";
        static bool useOnedrive = true;

        static void Main(string[] args)
        {
            if (!File.Exists(AppContext.BaseDirectory + "/server.jar"))
            {
                Console.WriteLine("There is no server.jar file in directory");
                Console.ReadLine();
                return;
            }
            

            if (File.Exists(AppContext.BaseDirectory + "/launcher.conf"))
            {
                foreach (string line in System.IO.File.ReadLines(AppContext.BaseDirectory + "/launcher.conf"))
                {
                    try
                    {
                        string new_line = line.Replace(" ", "");
                        string[] c = new_line.Split('=');
                        if (c[0].ToLower() == "launcher")
                        {
                            launcher = c[1];
                        }
                        if (c[0].ToLower() == "use_zerotier" && c[1].ToLower() == "true")
                        {
                            if (!CheckZeroTier())
                            {
                                Console.WriteLine("Zerotier is offline. Turn on zerotier");
                                Console.ReadLine();
                                return;
                            }
                        }
                        if (c[0].ToLower() == "service_link")
                        {
                            if (c[1].Length > 0)
                            {
                                if (!CheckService(c[1]))
                                {
                                    Console.WriteLine("Cannot connect to service. Check your connection");
                                    Console.ReadLine();
                                    return;
                                }
                            }
                        }
                        if (c[0].ToLower() == "use_onedrive" )
                        {
                            if (c[1].ToLower() == "true")
                            {
                                useOnedrive = true;
                            }
                            else
                            {
                                useOnedrive = false;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }

            if (File.Exists(AppContext.BaseDirectory + "/PUT YOUR SERVER IP HERE.txt"))
            {
                Console.WriteLine("Please wait. Checking all of server online");
                List<string> onlineIPs = new List<string>();
                foreach (string line in System.IO.File.ReadLines(AppContext.BaseDirectory + "/PUT YOUR SERVER IP HERE.txt"))
                {
                    //IPs.Add(line);
                    try
                    {
                        Int32 port = 25565;
                        TcpClient client = new TcpClient();
                        client.ReceiveTimeout = 1000;
                        client.SendTimeout = 1000;
                        client.Connect(line, port);
                        client.Close();
                        onlineIPs.Add(line);
                    }
                    catch
                    {

                    }
                    if (onlineIPs.Count > 0)
                    {
                        Console.WriteLine("This server is online. Connect to this server instead:");
                        foreach(string ip in onlineIPs)
                        {
                            Console.WriteLine(ip);
                        }
                        Console.ReadLine();
                        return;
                    }

                }
            } else
            {
                File.Create(AppContext.BaseDirectory + "/PUT YOUR SERVER IP HERE.txt");
            }

            if (File.Exists(AppContext.BaseDirectory + "/ONLINE"))
            {
                Console.WriteLine("Cannot open server because one of server still online.");
                Console.WriteLine("Klik enter to close application");
                Console.ReadLine();                
                return;
            }
            else
            {
                if (useOnedrive)
                {
                    
                    var oneDrivePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Microsoft/OneDrive/OneDrive.exe";

                    int counter = 0;
                    foreach (Process processes in Process.GetProcessesByName("Microsoft OneDrive"))
                    {
                        counter++;
                    }
                    foreach (Process processes in Process.GetProcessesByName("OneDrive"))
                    {
                        counter++;
                    }
                    if (counter <= 0)
                    {
                        Process od_process = new Process();
                        od_process.StartInfo.FileName = oneDrivePath;
                        try
                        {
                            
                            od_process.Start();
                            Console.WriteLine("Your one drive is off, so we run them for you. \n" +
                                "The program will close after you hit enter\n" +
                                "Check if all file already in sync, then run this launcher again");
                            Console.ReadLine();
                            return;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error " + e.Message);
                            Console.WriteLine("File location " + oneDrivePath);
                            Console.WriteLine("Cannot run onedrive. Make sure you already install it");
                            Console.ReadLine();
                            return;
                        }
                        return; //just to make sure. This doesnt run
                    }
                }

                var handle = GetConsoleWindow();

                // Hide
                ShowWindow(handle, SW_HIDE);

                File.Create(AppContext.BaseDirectory + "/ONLINE");

                Process process = new Process();
                process.StartInfo.FileName = launcher;
                try
                {
                    process.Start();
                    pid = process.Id;
                }
                catch
                {
                    File.Delete(AppContext.BaseDirectory + "/ONLINE");
                    return;
                }


                while (ProcessExists(pid))
                {

                }

                File.Delete(AppContext.BaseDirectory + "/ONLINE");
            }
        }
       

        private static bool ProcessExists(int id)
        {
            return Process.GetProcesses().Any(x => x.Id == id);
        }

        static bool CheckZeroTier()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.Name.Contains("ZeroTier"))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckService(string address)
        {
            //string address = cloudService;
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(address);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
    }  


}