using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using WpfXplay.bean;

namespace WpfXplay
{
    public class Peanut
    {
        Thread tHeartbeat;
        Thread tDownload;
        //public Thread tSnapshot;
        public static BoxProfile profile;
        public static XCallback callback;
        public static Dictionary<string, string> conf;
        public static PingResp pingResp;

        public Peanut() {
            conf = PeanutModule.readIni("config.ini");
        }

        public double getDoubleConf(string key)
        {
            if (conf.ContainsKey(key))
            {
                return double.Parse(conf[key]);
            }
            return 0;
        }

        public void saveConf()
        {
            PeanutModule.writeIni("config.ini", Peanut.conf);
        }

        public void Start()
        {
            WebRequest.DefaultWebProxy = null;

            tHeartbeat = new Thread(new HeartBeatThread().Run);
            tHeartbeat.IsBackground = true;
            tHeartbeat.Start();

            tDownload = new Thread(new DownloadThread().Run);
            tDownload.IsBackground = true;
            tDownload.Start();

            //tSnapshot = new Thread(new SnapshotThread().Run);
            //tSnapshot.IsBackground = true;
            //tSnapshot.Start();
        }

        public static void buildPlaySchele()
        {

        }
    }

    public abstract class PeanutRunner
    {
        public void Run()
        {
            while (true)
            {
                try
                {
                    Exec();
                } catch (Exception e)
                {
                    Console.Out.WriteLine(e.ToString());
                }
                Thread.Sleep(5 * 1000);
            }
        }

        public abstract void Exec();
        
    }

    public class HeartBeatThread : PeanutRunner
    {
        public override void Exec()
        {
            if (Peanut.profile == null && !Peanut.conf.ContainsKey("domainName"))
            {
                Dictionary<string, string> _params = new Dictionary<string, string>();
                _params["macid"] = PeanutModule.GetMacId();
                _params["macType"] = "win32";
                using (WebClient webClient = PeanutModule.newWebClient())
                {
                    string url = "https://cherry.danoolive.com/api/root/easyreg/check";
                    string resp = webClient.DownloadString(url + "?macid=" + PeanutModule.GetMacId());
                    Console.Out.WriteLine("resp=" + resp);
                    EasyCheckResp er = Json.fromJson<EasyCheckResp>(resp);
                    if (er == null)
                    {
                        Console.Out.WriteLine("resp ERROR");
                        Thread.Sleep(60 * 1000);
                        return;
                    }
                    if (er.easyreg != null)
                    {
                        // 需要注册
                        var scene_url = er.easyreg["scene_url"].ToString();
                        Console.Out.WriteLine("scene_url=" + scene_url);
                        PlayObj pobj = new PlayObj();
                        pobj.type = "play";
                        pobj.start = -1;
                        pobj.libName = "pic";
                        pobj._params = new Dictionary<string, object>();
                        pobj._params["path"] = "https://nutz.cn/qrcode/get?w=256&h=256&data=" + Uri.EscapeUriString(scene_url);
                        pobj._params["height"] = "512";
                        pobj._params["width"] = "512";
                        pobj._params["top"] = "100";
                        pobj._params["left"] = "100";
                        pobj._params["zIndex"] = 8;
                        Peanut.callback.HandlePlayObject(pobj);
                        Thread.Sleep(15 * 1000);
                    } else if (er.box_conf != null)
                    {
                        Peanut.conf["domainName"] = er.box_conf["domainName"].ToString();
                        PeanutModule.writeIni("config.ini", Peanut.conf);
                    }
                }
                Thread.Sleep(5000);
                return;
            }
            using (WebClient webClient = PeanutModule.newWebClient())
            {
                Console.Out.WriteLine("ping ...");
                var tmp = webClient.DownloadString("https://cherry.danoolive.com/ping?ex=");
                Peanut.pingResp = Json.fromJson<PingResp>(tmp);
                Console.Out.WriteLine("ping done=" + tmp);
            }
        }
    }

    public class DownloadThread : PeanutRunner
    {
        public override void Exec()
        {
            
        }
    }

    public class SnapshotThread : PeanutRunner
    {
        public override void Exec()
        {
            if (Peanut.conf.ContainsKey("domainName"))
            {
                var data = PeanutModule.GetSnapshot();
                var tld = new byte[data.Length + 4];
                var head = new byte[] { 4, 1, 31, 0 };
                head.CopyTo(tld, 0);
                data.CopyTo(tld, 4);
                using (WebClient webClient = PeanutModule.newWebClient())
                {
                    Console.Out.WriteLine("upload snapshot ...");
                    webClient.Headers["Content-Type"] = "application/octet-stream";
                    webClient.UploadData("https://cherry.danoolive.com/upload?tp=snapshot", tld);

                    Console.Out.WriteLine("upload snapshot done");
                }
                Thread.Sleep(60 * 1000);
                return;
            }

        }
    }
}
