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
using System.Text.RegularExpressions;

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
        //static int maxTimeout = 10; //in seconds
        static float time_check = 2.5f * 60 * 1000; //miliseconds
        static int pid = 0;
        static string launcher = "server.jar";
        static string cloudService = "onedrive.live.com";
        static bool useOnedrive = true;
        static string adapter_name = "null";
        //static bool useZeroTier = true;
        //static string id_adapter = "null";

        static List<string> activeIps = new List<string>();

        static void Main(string[] args)
        {
            if (!File.Exists(AppContext.BaseDirectory + "/server.jar"))
            {
                Console.WriteLine("There is no server.jar file in directory");
                Console.ReadLine();
                return;
            }

            SetConfiguration();           
            
            if (File.Exists(AppContext.BaseDirectory + "/PUT YOUR SERVER IP HERE.txt"))
            {
                Console.WriteLine("Please wait. Checking all of server online");
                

                if (!CheckSelfServer())
                {
                    Console.WriteLine("Your IP is not registered as a server. Add your ip to 'PUT YOUR SERVER IP HERE.txt'");
                    Console.WriteLine("Hit enter to close the launcher");
                    Console.ReadLine();
                    return;
                }

                if (!CheckEveryServer())
                {
                    return;
                }
                
            } else
            {
                File.Create(AppContext.BaseDirectory + "/PUT YOUR SERVER IP HERE.txt");
                Console.WriteLine("The file of your server ip is not exist. We create them for you, and you need to fill that for server");
                Console.WriteLine("The console will close when you hit enter");
                Console.ReadLine();
                return;
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
                if (!CheckOneDrive())
                {
                    return;
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

                DateTime last = DateTime.Now;
                while (ProcessExists(pid))
                {
                    if ((DateTime.Now - last).Milliseconds >= time_check)
                    {
                        last = DateTime.Now;
                        if (!CheckConnectionStatus())
                        {
                            process.Kill();
                            ShowWindow(handle, SW_SHOW);
                            Console.WriteLine("There is something wrong with your connection");
                            Console.WriteLine("Process is terminated");
                            Console.WriteLine("Run the launcher again when your connection stable");
                            Console.ReadLine();
                            File.Delete(AppContext.BaseDirectory + "/ONLINE");
                            return;
                        }
                    }
                }
                File.Delete(AppContext.BaseDirectory + "/ONLINE");
            }
        }

        static bool CheckConnectionStatus()
        {
            //[x] is order sensitive
            if(
                !CheckAdapter(adapter_name) ||//[x].
                !CheckSelfServer() || //[x] ||
                !CheckEveryServer(true) || //[x]
                !CheckOneDrive() ||
                !CheckService(cloudService)
                
            )
            {
                return false ;
            }
            return true;
        }

        static bool CheckOneDrive()
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
                        return false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error " + e.Message);
                        Console.WriteLine("File location " + oneDrivePath);
                        Console.WriteLine("Cannot run onedrive. Make sure you already install it");
                        Console.ReadLine();
                        return false;
                    }
                    return false; //just to make sure. This doesnt run
                }                
            }
            return true;
        }

        static bool CheckEveryServer(bool checkSelf = false)
        {
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
                    if (!checkSelf)
                    {
                        onlineIPs.Add(line);
                    } else
                    {
                        int c = 0;
                        foreach(string ip in activeIps)
                        {
                            if (ip == line)
                                c++;
                        }
                        if (c > 0)
                            continue;
                    }
                }
                catch
                {

                }
                if (onlineIPs.Count > 0)
                {
                    Console.WriteLine("There are other server online. Avoid multiple instances");
                    foreach (string ip in onlineIPs)
                    {
                        Console.WriteLine(ip);
                    }
                    Console.ReadLine();
                    return false;
                }                
            }
            return true;
        }

        static bool CheckSelfServer()
        {
            foreach (string line in System.IO.File.ReadLines(AppContext.BaseDirectory + "/PUT YOUR SERVER IP HERE.txt"))
            {
                foreach (string ip in activeIps)
                {
                    if (ip == line)
                        return true;
                    }
            }
            return false;
        }
        
        static public void SetConfiguration()
        {
            if (File.Exists(AppContext.BaseDirectory + "/launcher.conf"))
            {
                foreach (string line in System.IO.File.ReadLines(AppContext.BaseDirectory + "/launcher.conf"))
                {
                    try
                    {
                        //string new_line = line.Replace(" ", "");

                        //copied entirely from https://stackoverflow.com/questions/6111749/replace-whitespace-outside-quotes-using-regular-expression
                        //because I hate regex
                        string new_line = Regex.Replace(line,
                                   @"(?<=       # Assert that the string up to the current position matches...
                                    ^            # from the start of the string
                                     [^""]*      # any non-quote characters
                                     (?:         # followed by...
                                      ""[^""]*   # one quote, followed by 0+ non-quotes
                                      ""[^""]*   # a second quote and 0+ non-quotes
                                     )*          # any number of times, ensuring an even number of quotes
                                    )            # End of lookbehind
                                    [ ]          # Match a space (brackets for legibility)",
                                   "", RegexOptions.IgnorePatternWhitespace);


                        string[] c = new_line.Split('=');
                        if (c[0].ToLower() == "launcher")
                        {
                            launcher = c[1];
                        }
                        if(c[0].ToLower() == "network_adapter_name")
                        {
                            adapter_name = c[1];
                            if (!CheckAdapter(c[1]))
                            {
                                Console.WriteLine("Cannot find the adapter. Makesure your adapter in the configuration is running");
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
                        if (c[0].ToLower() == "use_onedrive")
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
        }

        private static bool ProcessExists(int id)
        {
            return Process.GetProcesses().Any(x => x.Id == id);
        }

        static bool CheckAdapter(string adapter_name)
        {
            int counter = 0;
            activeIps.Clear();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.Name.Contains(adapter_name))
                {
                    foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            activeIps.Add(ip.Address.ToString());
                            counter++;
                        }
                    }                    
                }
            }
            if (counter > 0)
                return true;
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