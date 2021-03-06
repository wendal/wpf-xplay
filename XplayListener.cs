﻿
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
                ThreadPool.QueueUserWorkItem(ReceiveMessage, client);
            }
        }

        public void ReceiveMessage(object obj)
        {
            try
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
            } catch (Exception e)
            {
                Console.Out.WriteLine(e);
            }
            
        }

        public void HandleMsg(string msg)
        {
            //Console.Out.WriteLine("msg="+msg);
            PlayObj pobj = Json.fromJson<PlayObj>(msg);
            callback?.HandlePlayObject(pobj);
        }
    }
}
