using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Common
{
    public abstract class Element
    {
        public float x, y;

        public Element(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public virtual void Update()
        {
        }

        public abstract void Draw(Graphics g, Camera cam);

        public virtual void Destroy()
        {
        }
    }
}
