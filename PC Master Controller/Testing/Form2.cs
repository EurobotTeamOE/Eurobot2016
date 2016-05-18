using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Testing
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        TcpListener listener;

        private void Form2_Load(object sender, EventArgs e)
        {
            listener = new TcpListener(IPAddress.Any, 2539);
            listener.Start();

            Thread t = new Thread(new ThreadStart(Listen));
            t.Start();
        }

        void Listen()
        {
            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();

                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(client);
                }
                catch
                {
                }
            }
        }

        void HandleClient(object tcpClient)
        {
            TcpClient client = (TcpClient)tcpClient;
            BinaryReader br = new BinaryReader(client.GetStream());

            List<byte> data = new List<byte>();

            while (true)
            {
                try
                {
                    byte recv = br.ReadByte();

                    if (recv == 255) data.Clear();
                    data.Add(recv);

                    if (data[0] == 255 && data.Count == 65)
                    {
                        string str = "";

                        Point[] points = new Point[4];

                        for (int i = 0; i < 4; i++)
                        {
                            int posX = 0, posY = 0;

                            for (int j = 0; j < 8; j++)
                            {
                                posX |= data[i * 16 + j + 1] << (j * 4);
                                posY |= data[i * 16 + j + 9] << (j * 4);
                            }

                            points[i] = new Point(posX, posY);
                        }

                        for (int i = 0; i < 4; i++) str += points[i] + " ";

                        data.Clear();
                        toAdd.Enqueue(str);
                    }
                }
                catch
                {
                }
            }
        }

        Queue<string> toAdd = new Queue<string>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            while (toAdd.Count > 0) listBox1.Items.Add(toAdd.Dequeue());

            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = -1;
        }
    }
}
