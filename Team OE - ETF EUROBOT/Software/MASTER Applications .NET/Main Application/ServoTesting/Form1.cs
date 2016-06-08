using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using PuppetMaster;

namespace ServoTesting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<NumericUpDown> nums;
        Tuple<string, string>[] strats;
        Controller controller;

        private void Form1_Load(object sender, EventArgs e)
        {
            nums = new List<NumericUpDown>();

            for (int i = 0; i < 16; i++)
            {
                NumericUpDown numUd = new NumericUpDown();

                numUd.Maximum = 1000;
                int x = (i < 8) ? 50 : Width - 150;
                int y = (i < 8) ? i % 8 * 50 + 50 : (7 - i % 8) * 50 + 50;

                numUd.Left = x;
                numUd.Top = y;
                numUd.Increment = 5;

                Controls.Add(numUd);
                numUd.ValueChanged += numUd_ValueChanged;
                nums.Add(numUd);
            }

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

            foreach (Tuple<string, string> strat in stratList)
            {
                comboBox1.Items.Add(strat.Item1);
            }

            comboBox1.SelectedIndex = 0;
            strats = stratList.ToArray();

            controller = new Controller("COM6", true, true);
        }

        void numUd_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numUd = (NumericUpDown)sender;
            int index = nums.IndexOf(numUd);

            Command moveServo = new Command_MoveServo(controller, index + 1, Convert.ToInt32(numUd.Value));
            controller.IssueSingleCommand(moveServo);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            controller.InitStrategy(comboBox1.Text);
            controller.Run();
            controller = new Controller("COM3", true, true);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
