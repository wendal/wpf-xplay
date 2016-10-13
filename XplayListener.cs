using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfXplay.bean;

namespace WpfXplay
{
    public class XplayListener
    {
        TcpListener tp;
        public int port = 8700;
        public XCallback callback;

        public void run()
        {
            IPAddress addr = IPAddress.Parse("0.0.0.0");
            tp = new TcpListener(addr, port);
            tp.Start();
            while (true)
            {
                TcpClient client = tp.AcceptTcpClient();
                Thread t = new Thread(ReceiveMessage);
                t.Start(client);
            }
        }

        public void ReceiveMessage(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter output = new StreamWriter(stream);
            string tmp = "";
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                tmp = reader.ReadLine();
                //Console.Out.WriteLine("line=" + tmp);
                if (tmp.StartsWith("#End"))
                {
                    HandleMsg(sb.ToString());
                    output.WriteLine("{}");
                    output.WriteLine("#End");
                    output.Flush();
                }
                else if (tmp.StartsWith("#Close"))
                {
                    client.Close();
                    break;
                }
                else
                {
                    sb.Append(tmp).AppendLine();
                }
            }
        }

        public void HandleMsg(string msg)
        {
            //Console.Out.WriteLine("msg="+msg);
            JsonSerializer s = new JsonSerializer();
            PlayObj pobj = s.Deserialize<PlayObj>(new JsonTextReader(new StringReader(msg)));
            callback?.HandlePlayObject(pobj);
        }
    }
}
