
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Media;
using System.Net;

namespace WpfXplay
{

    public class PeanutModule
    {
        //public static string zipFilePath = @"\danoo\content\sys_upload.zip";
        //public static int PORT = 8878;
        public static string server = "cherry.danoolive.com";

        public static string GetMacId()
        {
            SelectQuery selectQuery = new SelectQuery("Win32_DiskDrive");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectQuery);
            foreach (ManagementObject disk in searcher.Get())
            {
                if (disk["MediaType"] != null && disk["MediaType"].ToString() == "Fixed hard disk media" && disk["SerialNumber"] != null)
                {
                    var tmp = disk["SerialNumber"].ToString().Trim().ToUpper();
                    if (tmp.Contains(" "))
                        tmp = tmp.Split(' ')[0];
                    return tmp;
                }
            }
            return "NONDISK";
        }

        public static Dictionary<String, String> GetIps()
        {
            String strHostName = Dns.GetHostName();
            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

            // Enumerate IP addresses
            Dictionary<String, String> ips = new Dictionary<string, string>();
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                if (ipaddress.ToString().StartsWith("10.100."))
                {
                    ips["tun0"] = ipaddress.ToString();
                }
                else if (ipaddress.ToString().StartsWith("192.168."))
                {
                    ips["eth0"] = ipaddress.ToString();
                }
            }
            return ips;
        }

        public static void Beep()
        {
            SystemSounds.Beep.Play();
        }

        public static byte[] GetSnapshot()
        {
            ScreenCapture sc = new ScreenCapture();
            Image img = sc.CaptureScreen();
            img = ScreenCapture.ResizeImage(img, img.Width / 4 * 2, img.Height / 4 * 2);
            MemoryStream stream = new MemoryStream();
            img.Save(stream, ImageFormat.Png);
            img.Dispose();
            return stream.ToArray();
        }

        public static WebClient newWebClient()
        {
            var webClient = new WebClient();
            webClient.Headers["Mac-Id"] = PeanutModule.GetMacId();
            if (Peanut.conf.ContainsKey("domainName"))
                webClient.Headers["Domain"] = "" + Peanut.conf["domainName"];
            Console.Out.WriteLine("Mac-Id=" + webClient.Headers["Mac-Id"]);
            Console.Out.WriteLine("Domain=" + webClient.Headers["Domain"]);
            return webClient;
        }

        public static Dictionary<string, string> readIni(string path)
        {
            Dictionary<string, string> conf = new Dictionary<string, string>();
            if (File.Exists(path))
                foreach (var line in File.ReadAllLines(path))
                {
                    if (line == null)
                        continue;
                    var tmp = line.Trim();
                    if (tmp.Length < 3 || tmp.StartsWith("#") || !tmp.Contains("="))
                        continue;
                    var d = tmp.Split('=');
                    if (d.Length > 1)
                        conf[d[0].Trim()] = d[1].Trim();
                }
            return conf;
        }

        public static void writeIni(string path, Dictionary<string, string> conf)
        {
            string tmp = "";
            foreach (var key in conf.Keys)
                tmp += key + "=" + conf[key] + "\r\n";
            File.WriteAllText(path, tmp);
        }
    }
}
