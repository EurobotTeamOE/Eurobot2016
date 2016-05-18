using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;

namespace Common
{
    public class Field : Element
    {
        float width, height;
        public Field(float width, float height)
            : base(width / 2, height / 2)
        {
            this.width = width;
            this.height = height;
        }

        public override void Draw(Graphics g, Camera cam)
        {
            PointF pos = cam.WorldToScreen(x, y);
            float width = cam.Scale(this.width);
            float height = cam.Scale(this.height);
            g.FillRectangle(Brushes.LightGreen, pos.X - width / 2, pos.Y - height / 2, width, height);

            float penWidth = cam.Scale(20);
            g.DrawRectangle(new Pen(Color.Green, penWidth), pos.X - width / 2 - penWidth / 2, pos.Y - height / 2 - penWidth / 2, width + penWidth, height + penWidth);
        }
    }
}
