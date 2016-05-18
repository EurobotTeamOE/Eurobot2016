using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace Common
{
    public class SerialComm
    {
        Robot r;
        Localisation l;
        public SerialPort port;
        public CommBuffer inputBuffer, outputBuffer;
        public byte CommandID = (byte)(new Random()).Next(255);

        public SerialComm(Robot r, string portName)
        {
            this.r = r;
            this.port = new SerialPort(portName);
            this.port.BaudRate = 115200;
            this.port.DataBits = 8;
            this.port.ReadBufferSize = 512;
            this.port.StopBits = StopBits.One;
            this.port.Parity = Parity.None;
            this.port.Open();
            this.port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            this.inputBuffer = new CommBuffer();
            this.outputBuffer = new CommBuffer();
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[port.BytesToRead];
            port.Read(data, 0, data.Length);
            for (int i = 0; i < data.Length; i++)
                inputBuffer.Write(data[i]);

            if (inputBuffer.CommandReady())
            {
                byte[] command = inputBuffer.ReadCommand();
                if (command != null)
                {
                    switch (command[1])
                    {
                        case 0x4A:
                            if (command.Length > 10) PositionReceived(command);
                            if (command.Length == 4) CheckAcknowledge(command);
                            break;
                        case 0x54:
                            LocalisationResponse(command);
                            break;
                        case 0x49:
                            if (command.Length > 10) ServoStateReceived(command);
                            if (command.Length == 4) CheckAcknowledge(command);
                            break;
                        case 0x47:
                            if (command.Length == 4) CheckAcknowledge(command);
                            break;
                        default:
                            Console.WriteLine("INVALID RESPONSE   " + command[1]); // Throw exception? no.
                            break;
                    }
                }
            }
        }

        void CheckAcknowledge(byte[] data)
        {
            waiting = false;
        }

        void PositionReceived(byte[] data)
        {
            float x = (((data[3] & 0x0000000F) | ((data[4] & 0x0000000F) << 4) | ((data[5] & 0x0000000F) << 8) | ((data[6] & 0x0000000F) << 12) | ((data[7] & 0x0000000F) << 16) | ((data[8] & 0x0000000F) << 20) | ((data[9] & 0x0000000F) << 24) | ((data[10] & 0x0000000F) << 28))) / 11.7374f;
            float y = (((data[11] & 0x0000000F) | ((data[12] & 0x0000000F) << 4) | ((data[13] & 0x0000000F) << 8) | ((data[14] & 0x0000000F) << 12) | ((data[15] & 0x0000000F) << 16) | ((data[16] & 0x0000000F) << 20) | ((data[17] & 0x0000000F) << 24) | ((data[18] & 0x0000000F) << 28))) / 11.7374f;
            //float rot = ((data[19] & 0x0000000F) | ((data[20] & 0x0000000F) << 4) | ((data[21] & 0x0000000F) << 8) | ((data[22] & 0x0000000F) << 12));
            float rot = (((data[19] & 0x0000000F) | ((data[20] & 0x0000000F) << 4) | ((data[21] & 0x0000000F) << 8) | ((data[22] & 0x0000000F) << 12) | ((data[23] & 0x0000000F) << 16) | ((data[24] & 0x0000000F) << 20) | ((data[25] & 0x0000000F) << 24) | ((data[26] & 0x0000000F) << 28)));
            //bool ready = (data[27] == 0x01 && data[28] == 0x00) ? true : false;
            int statusBits = (data[27] & 0x0000000F) | ((data[28] & 0x0000000F) << 4) | ((data[29] & 0x0000000F) << 8) | ((data[30] & 0x0000000F) << 12) | ((data[31] & 0x0000000F) << 16) | ((data[32] & 0x0000000F) << 20) | ((data[33] & 0x0000000F) << 24) | ((data[34] & 0x0000000F) << 28);
            //int statusBits = 0;
            bool ready = (data[35] == 0x71) ? true : false;

            r.SetState(x, y, rot, ready, statusBits);
            r.upToDate = Math.Min(r.upToDate + 1, 50);
        }

        void ServoStateReceived(byte[] data)
        {
            bool L_Done = Convert.ToBoolean(data[3]);
            bool D_Done = Convert.ToBoolean(data[11]);
            bool P_Done = Convert.ToBoolean(data[19]);

            r.SetServoState(L_Done && D_Done && P_Done);
        }

        public void SetLocalisation(Localisation l)
        {
            this.l = l;
        }

        void LocalisationResponse(byte[] data)
        {
            l.Update(data);
        }

        public byte NextCommandID()
        {
            CommandID = (byte)((CommandID + 1) % 255);
            return CommandID;
        }

        public void SendMessage()
        {
            byte[] data = outputBuffer.MakeMesage();
            port.Write(data, 0, data.Length);
            Thread.Sleep(10);
        }

        bool waiting = false;
        public void WaitForResponse(int timeout) //miliseconds
        {
            DateTime start = DateTime.Now;
            while (waiting && (DateTime.Now - start).TotalMilliseconds < timeout)
            {
                Thread.Sleep(50);
            }
        }

        public void ForceSendMessage()
        {
            byte[] data = outputBuffer.MakeMesage();

            waiting = true;
            while (waiting)
            {
                port.Write(data, 0, data.Length);
                Thread.Sleep(10);
                WaitForResponse(300);
            }
        }

        public void SetPosition(float x, float y, float angle = 0)
        {
            bool success = false;

            while (!success)
            {
                outputBuffer.Write(0xFF);
                outputBuffer.Write(0x0A);
                outputBuffer.Write(26);
                outputBuffer.Write(0x01);
                //outputBuffer.Write(0x00);

                int X = (int)(x * 11.7374f);
                int Y = (int)(y * 11.7374f);
                int ANGLE = (int)angle;

                for (int i = 0; i < 8; i++) outputBuffer.Write((byte)((X >> (4 * i)) & 0x0000000F));
                for (int i = 0; i < 8; i++) outputBuffer.Write((byte)((Y >> (4 * i)) & 0x0000000F));
                for (int i = 0; i < 8; i++) outputBuffer.Write((byte)((ANGLE >> (4 * i)) & 0x0000000F));

                waiting = true;
                SendMessage();

                Thread.Sleep(100);
                if (!waiting) success = true;
            }
        }

        public void Kill()
        {
            port.Close();
        }
    }
}
