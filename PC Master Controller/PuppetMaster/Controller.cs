using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Xml;
using Common;

namespace PuppetMaster
{
    public class Controller
    {
        public TimeSpan time;
        public Queue<State> strategy;
        public Robot r;
        public string stratFolder;
        public static bool started = false;
        public bool isUmbrellaDeployed = false;
        public bool enginesOff = false;

        public Controller(string portName, bool movement = true, bool servos = true)
        {
            r = new Robot(0, 0, 0, portName, false);
            if (!movement) r.DisableMovement();
            if (!servos) r.DisableServos();
            DebugComm.StartDebug(this, portName);
        }

        void ReadConfig(out string path, out string port)
        {
            path = "";
            port = "";

            StreamReader sr = new StreamReader("config.cfg");
            XmlReader reader = XmlReader.Create(sr.BaseStream);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name.ToLower())
                        {
                            case "port":
                                port = reader.ReadInnerXml();
                                break;
                            case "path":
                                path = reader.ReadInnerXml();
                                break;
                        }
                        break;
                }
            }

            sr.Close();
        }

        public void InitStrategy(string strategy)
        {
            string path, port;

            ReadConfig(out path, out port);

            path = path.EndsWith("\\\\") ? path : path + "\\\\";
            strategy = strategy.EndsWith("\\\\") ? strategy : strategy + "\\\\";
            stratFolder = path + strategy;

            LoadStrategy();
        }

        public Controller(string portName, string strategy, bool movement = true, bool servos = true, bool smallBot = false)
        {
            string path, port;

            ReadConfig(out path, out port);

            portName = (portName == "") ? port : portName;
            r = new Robot(0, 0, 0, portName, true, smallBot);
            if (!movement) r.DisableMovement();
            if (!servos) r.DisableServos();

            InitStrategy(strategy);

            DebugComm.StartDebug(this, portName);
        }

        public static string GetPath()
        {
            StreamReader sr = new StreamReader("config.cfg");
            XmlReader reader = XmlReader.Create(sr.BaseStream);

            string path = "";

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name.ToLower())
                        {
                            case "path":
                                path = reader.ReadInnerXml();
                                break;
                        }
                        break;
                }
            }

            sr.Close();

            path = path.EndsWith("\\\\") ? path : path + "\\\\";

            return path;
        }

        public void Run()
        {
            DoRun();
        }

        Thread runningThread;

        //debugThread;
        //List<Thread> clientThreads;

        public void RunInNewThread()
        {
            runningThread = new Thread(new ThreadStart(DoRun));
            runningThread.Start();
        }

        static DateTime start;
        public bool running = true;

        void Watchdog()
        {
            while (running)
            {
                if ((DateTime.Now - start).TotalSeconds >= 90)
                {
                    running = false;
                }
                Thread.Sleep(500);
            }
        }

        void DoRun()
        {
            try
            {
                State state = GetNextState();

                while (!started)
                {
                    if (!r.smallBot)
                    {
                        r.Ping();
                        if (r.GetStatusBit(31)) started = true;
                    }
                }

                start = DateTime.Now;

                Thread watchdog = new Thread(new ThreadStart(Watchdog));
                watchdog.Start();

                while (running)
                {
                    r.Ping();
                    r.PingServos();
                    Console.WriteLine("Current state: " + state.Name());
                    Console.WriteLine("State: " + r.State());
                    time = DateTime.Now - start;
                    state = state.DoStuff();

                    Thread.Sleep(50);
                }

                //if (r.smallBot) state = new State_Idle(this);
                //else state = new State_Parser(this, "otvoriKisobran.servo");
                
                //state = state.DoStuff();
                state = new State_Idle(this);
                //state = state.DoStuff();

                while (true)
                {
                    state = state.DoStuff();
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                int asd = 576;
            }
        }

        void LoadStrategy()
        {
            strategy = new Queue<State>();

            StreamReader sr = new StreamReader(stratFolder + "main.strat");

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line == "" || line.StartsWith("//")) continue;

                strategy.Enqueue(ParseState(line));
            }

            sr.Close();
        }

        State ParseState(string line)
        {
            string upper = line.ToUpper();

            if (upper.StartsWith("START")) return State_Start.Parse(this, line);
            else if (upper.StartsWith("IDLE")) return new State_Idle(this);
            else if (upper.StartsWith("PARSER")) return State_Parser.Parse(this, line);

            throw new Exception("Invalid state!");
        }

        public State GetNextState()
        {
            if (strategy.Count > 0)
            {
                return strategy.Dequeue();
            }
            else
            {
                r.Kill();
                throw new Exception("No states remaining!");
            }
        }

        public void IssueSingleCommand(Command comm)
        {
            SerialComm serialComm = r.comm;
            CommBuffer buffer = serialComm.outputBuffer;

            bool send, free;

            if (comm.Send(buffer, out send, out free))
            {
                if (send) serialComm.ForceSendMessage();
            }
        }
    }
}
