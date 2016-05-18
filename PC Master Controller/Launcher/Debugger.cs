using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Common;

namespace Launcher
{
    class Debugger
    {
        public bool running = false;
        List<Element> elements;
        public Camera cam;
        Robot r;
        TcpClient client;
        BinaryReader br;
        BinaryWriter bw;

        public Debugger(int width, int height)
        {
            elements = new List<Element>();
            elements.Add(new Field(3000, 2000));
            elements.Add(new RectangularArea(195, 150, 390, 300, Color.Yellow));
            elements.Add(new CircularArea(0, 300, 780, 780, -90, 0, Color.Yellow));
            elements.Add(new RectangularArea(2805, 150, 390, 300, Color.Red));
            elements.Add(new CircularArea(3000, 300, 780, 780, 180, 270, Color.Red));
            cam = new Camera(width, height);

            r = new Robot(0, 0, 100);
            elements.Add(r);
        }

        public void OpenComm(string portName)
        {
            try
            {
                int port = 39000 + Convert.ToInt32(portName.Substring(3));

                client = new TcpClient();
                client.Connect(IPAddress.Loopback, port);

                br = new BinaryReader(client.GetStream());
                bw = new BinaryWriter(client.GetStream());

                running = true;
            }
            catch { }
        }

        public void Update()
        {
            try
            {
                bw.Write(576);
                byte[] data = br.ReadBytes(12);
                float x = BitConverter.ToSingle(data, 0);
                float y = BitConverter.ToSingle(data, 4);
                float rot = BitConverter.ToSingle(data, 8);

                r.SetState(x, y, rot, false, 0);
                cam.Update(r);
            }
            catch { }
        }

        public void Draw(Graphics g)
        {
            foreach (Element elem in elements) elem.Draw(g, cam);
        }
    }
}
