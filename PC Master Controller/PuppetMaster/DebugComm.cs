using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace PuppetMaster
{
    class DebugComm
    {
        Controller c;
        int port;
        Thread debugThread;
        List<Thread> clientThreads = new List<Thread>();

        public DebugComm(Controller c, int port)
        {
            this.c = c;
            this.port = port;
        }

        public static void StartDebug(Controller c, string portName)
        {
            try
            {
                int port = 39000 + Convert.ToInt32(portName.Substring(3));
                DebugComm dbg = new DebugComm(c, port);
                dbg.Start();
            }
            catch { }
        }

        void Start()
        {
            try
            {
                debugThread = new Thread(new ThreadStart(DoListen));
                debugThread.Start();
            }
            catch { }
        }

        void DoListen()
        {
            try
            {
                TcpListener listener = new TcpListener(port);
                listener.Start();

                while (true)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();

                        Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                        t.Start(client);
                        clientThreads.Add(t);
                    }
                    catch { }
                }
            }
            catch { }
        }

        void HandleClient(object tcpClient)
        {
            try
            {
                TcpClient client = (TcpClient)tcpClient;
                BinaryReader br = new BinaryReader(client.GetStream());
                BinaryWriter bw = new BinaryWriter(client.GetStream());

                byte[] data = new byte[12];

                while (true)
                {
                    int read = br.ReadInt32();
                    if (read == 576)
                    {
                        try
                        {
                            int offset = 0;

                            byte[] convert = BitConverter.GetBytes(c.r.x);
                            for (int i = 0; i < convert.Length; i++) data[offset + i] = convert[i];
                            offset += convert.Length;

                            convert = BitConverter.GetBytes(c.r.y);
                            for (int i = 0; i < convert.Length; i++) data[offset + i] = convert[i];
                            offset += convert.Length;

                            convert = BitConverter.GetBytes(c.r.rot);
                            for (int i = 0; i < convert.Length; i++) data[offset + i] = convert[i];
                            offset += convert.Length;

                            bw.Write(data, 0, data.Length);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
    }
}
