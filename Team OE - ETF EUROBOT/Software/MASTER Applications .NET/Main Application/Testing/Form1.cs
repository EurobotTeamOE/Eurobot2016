using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;

namespace Testing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Localisation local;
        SerialComm comm;
        Camera cam;

        private void timer1_Tick(object sender, EventArgs e)
        {
            local.Ping();

            Text = local.Position.ToString();
            textBox1.Text = local.a.ToString("00000.00");
            textBox2.Text = local.b.ToString("00000.00");
            textBox3.Text = local.c.ToString("00000.00");

            pictureBox1.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comm = new SerialComm(null, "COM6");
            local = new Localisation(comm);
            comm.SetLocalisation(local);

            cam = new Camera(pictureBox1.Width, pictureBox1.Height, (float)(local.B / 2), (float)(local.A / 2));
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            local.Draw(e.Graphics, cam);
        }
    }
}
