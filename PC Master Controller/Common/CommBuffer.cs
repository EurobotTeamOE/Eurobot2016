using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;

namespace Common
{
    public class CommBuffer
    {
        int capacity, writePos, readPos, toRead;
        byte[] buffer = null;

        public CommBuffer(int capacity = 1024)
        {
            this.capacity = capacity;
            this.buffer = new byte[this.capacity];
            this.writePos = this.readPos = 0;
            this.toRead = 0;
        }

        public void Write(byte data)
        {
            if (toRead >= capacity) throw new OutOfMemoryException("Buffer Full!");

            buffer[writePos] = data;
            writePos = (writePos + 1) % capacity;
            toRead++;
            if (data == 255)
            {
                readPos = (writePos - 1 + capacity) % capacity;
                toRead = 1;
            }
        }

        public void Write(byte[] data)
        {
            for (int i = 0; i < data.Length; i++) Write(data[i]);
        }

        public byte Seek(int pos)
        {
            int ind = (readPos + pos) % capacity;
            return buffer[ind];
        }

        public bool CommandReady()
        {
            if (toRead < 3) return false;
            int length = Seek(2);
            if (toRead == length + 3) return true;
            return false;
        }

        public byte Read()
        {
            if (toRead <= 0) throw new Exception("Buffer Empty!");
            byte ret = buffer[readPos];
            readPos = (readPos + 1) % capacity;
            toRead--;
            return ret;
        }

        public byte[] ReadCommand()
        {
            if (!CommandReady()) return null;
            int length = Seek(2);
            byte[] data = new byte[length + 3];

            byte chkSum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Read();
                if (i > 0 && i < data.Length - 1) chkSum += data[i];
            }
            chkSum &= 0x7F;
            if (chkSum != data[data.Length - 1]) return null;
            return data;
        }

        public byte[] MakeMesage()
        {
            byte chkSum = 0;
            byte[] data = new byte[toRead + 1];

            for (int i = 0; i < data.Length - 1; i++)
            {
                data[i] = Read();
                if (i > 0) chkSum += data[i];
            }
            chkSum &= 0x7F;
            data[data.Length - 1] = chkSum;

            return data;
        }
    }
}
