using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Management;
using Common;
using PuppetMaster;

namespace Launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Tuple<string, string>[] ports;
        Thread[] threads;
        Image imageBT = Image.FromFile("Images\\BT.png");
        Image imageUSB = Image.FromFile("Images\\USB.png");
        Tuple<string, string>[] strats;

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Left = panel1.Width / 4 - comboBox1.Width / 2;
            pictureBox1.Left = comboBox1.Left - pictureBox1.Width - 25;
            label1.Left = comboBox1.Right + 25;
            comboBox2.Left = comboBox1.Left;
            button1.Left = panel1.Width / 2 - button1.Width / 2;
            pictureBox3.Left = pictureBox3.Top = 0;
            pictureBox3.Width = panel1.Width;
            pictureBox3.Height = panel1.Height;
            pictureBox3.SendToBack();

            dbg1 = new Debugger(pictureBox3.Width, pictureBox3.Height);

            List<Tuple<string, string>> stratList = new List<Tuple<string, string>>();
            string[] dirs = Directory.GetDirectories(Controller.GetPath());

            for (int i = 0; i < dirs.Length; i++)
            {
                string[] files = Directory.GetFiles(dirs[i]);
                for (int j = 0; j < files.Length; j++)
                {
                    if (files[j].ToLower().EndsWith("main.strat"))
                    {
                        string name = dirs[i].Substring(dirs[i].LastIndexOf('\\') + 1);
                        stratList.Add(new Tuple<string, string>(name, dirs[i]));
                    }
                }
            }

            foreach (Tuple<string, string> strat in stratList) comboBox2.Items.Add(strat.Item1);
            comboBox2.SelectedIndex = 0;
            strats = stratList.ToArray();

            List<Tuple<string, string>> portList = new List<Tuple<string, string>>();

            /*ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from WIN32_SerialPort");
            foreach (ManagementObject port in searcher.Get())
            {
                string deviceID = "";
                string deviceName = "";

                foreach (PropertyData property in port.Properties)
                {
                    string name = property.Name;
                    string value = property.Value == null ? "" : property.Value.ToString();

                    if (name == "DeviceID") deviceID = value;
                    if (name == "Name") deviceName = value;
                }

                if (deviceID.ToUpper().StartsWith("COM")) portList.Add(new Tuple<string, string>(deviceID, deviceName));
            }*/

            foreach (string port in SerialPort.GetPortNames()) portList.Add(new Tuple<string, string>(port, "UNKNOWN"));

            ports = portList.ToArray();

            for (int i=0; i<ports.Length-1; i++)
                for (int j = i + 1; j < ports.Length; j++)
                    if (ports[j].Item1.CompareTo(ports[i].Item1) < 0)
                    {
                        Tuple<string, string> tmp = ports[i];
                        ports[i] = ports[j];
                        ports[j] = tmp;
                    }

            foreach (Tuple<string, string> port in ports) comboBox1.Items.Add(port.Item1);

            threads = new Thread[ports.Length];
            for (int i = 0; i < ports.Length; i++)
            {
                threads[i] = new Thread(new ParameterizedThreadStart(TestPort));
                threads[i].Start((object)i);
            }
        }

        void TestPort(object port)
        {
            try
            {
                int index = (int)port;

                Robot r = new Robot(-9999, -9999, 0, ports[index].Item1, false);
                r.Ping();

                DateTime start = DateTime.Now;

                while ((DateTime.Now - start).TotalSeconds < 1)
                {
                    if (r.x > -5000) portIndex = index;
                    Thread.Sleep(50);
                }

                r.Kill();
            }
            catch { }
        }

        Debugger dbg1, dbg2;
        int portIndex = -1;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (portIndex > -1)
            {
                panel1.Enabled = true;
                Text = ports[portIndex].Item1;
                comboBox1.SelectedIndex = portIndex;
                label1.Text = ports[portIndex].Item2;

                if (ports[portIndex].Item2.ToUpper().Contains("BLUETOOTH")) pictureBox1.BackgroundImage = imageBT;
                else if (ports[portIndex].Item2.ToUpper().Contains("USB")) pictureBox1.BackgroundImage = imageUSB;

                portIndex = -1;
            }

            if (dbg1.running)
            {
                dbg1.Update();
            }

            try
            {
                //local.Ping();
            }
            catch
            {
            }

            pictureBox3.Invalidate();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        Localisation local = null;

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Enabled = false;
            Controller c = new Controller(comboBox1.Text, comboBox2.Text, true, true, false);
            c.RunInNewThread();
            pictureBox3.BringToFront();
            dbg1.OpenComm(comboBox1.Text);

            //local = new NewLocal(c.r.comm);
            //c.r.comm.SetLocalisation(local);
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            if (dbg1.running)
            {
                dbg1.Draw(e.Graphics);
                //local.Draw(e.Graphics, dbg1.cam);
            }
        }

        private void pictureBox4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
