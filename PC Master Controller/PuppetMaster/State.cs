using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public abstract class State
    {
        protected Controller c;

        public State(Controller c) { this.c = c; }
        public abstract string Name();
        public abstract State DoStuff();
    }
}
