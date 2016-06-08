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
    class Program
    {

        static void Main(string[] args)
        {
            Program main = new Program();
            main.main(args);
        }

        void main(string[] args)
        {
            Controller c = new Controller("", "NewStrat");
            c.Run();
        }
    }
}
