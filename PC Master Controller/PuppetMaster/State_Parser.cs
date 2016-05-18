using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Common;

namespace PuppetMaster
{
    public class State_Parser : State
    {
        Queue<Command> commands;

        public State_Parser(Controller c, string fileName)
            : base(c)
        {
            commands = new Queue<Command>();

            StreamReader sr = new StreamReader(c.stratFolder + fileName);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line == "" || line.StartsWith("//")) continue;

                if (line.StartsWith("DEFINE")) Expression.Define(c.r, line);
                else commands.Enqueue(ParseCommand(line));
            }

            sr.Close();
        }

        Command ParseCommand(string line)
        {
            string upper = line.ToUpper();

            if (upper.StartsWith("MOVE")) return Command_Move.Parse(c, line);
            if (upper.StartsWith("ROTATE")) return Command_Rotate.Parse(c, line);
            if (upper.StartsWith("TAKE")) return Command_PickUp.Parse(c, line);
            if (upper.StartsWith("LETGO")) return Command_LetGo.Parse(c, line);
            if (upper.StartsWith("WAIT")) return Command_Wait.Parse(c, line);
            if (upper.StartsWith("SETANGLE")) return Command_SetAngle.Parse(c, line);
            if (upper.StartsWith("LOOKAT")) return Command_LookAt.Parse(c, line);
            if (upper.StartsWith("MOVETO")) return Command_MoveTo.Parse(c, line);
            if (upper.StartsWith("SETPRESCALER")) return Command_SetPrescaler.Parse(c, line);
            if (upper.StartsWith("SERVO_MOVE")) return Command_MoveServo.Parse(c, line);
            if (upper.StartsWith("DELAY")) return Command_Delay.Parse(c, line);
            if (upper.StartsWith("KILLSENSORS")) return new Command_KillSensors(c);
            if (upper.StartsWith("ULTRASOUND_ON")) return new Command_UltrasoundOn(c);
            if (upper.StartsWith("ULTRASOUND_OFF")) return new Command_UltrasoundOff(c);

            throw new Exception("Invalid command!");
        }

        public override string Name()
        {
            return "Parser";
        }

        public override State DoStuff()
        {
            Robot r = c.r;

            if (r.Ready || c.isUmbrellaDeployed)
            {
                SerialComm serialComm = r.comm;
                CommBuffer buffer = serialComm.outputBuffer;

                Command comm = commands.Peek();

                bool send, free;
                if (comm.Send(buffer, out send, out free))
                {
                    if (send) serialComm.ForceSendMessage();
                    //if (send) serialComm.SendMessage();

                    if (free)
                    {
                        commands.Dequeue();

                        if (commands.Count == 0) return c.GetNextState();
                    }
                }

                Thread.Sleep(100);
            }

            return this;
        }

        public static State Parse(Controller c, string line)
        {
            string[] split = line.Split(' ');

            return new State_Parser(c, split[1]);
        }
    }
}
