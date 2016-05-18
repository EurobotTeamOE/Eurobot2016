using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

namespace Common
{
    public class RectangularArea : Element
    {
        float width, height;
        SolidBrush brush;

        public RectangularArea(float x, float y, float width, float height, Color color)
            : base(x, y)
        {
            this.width = width;
            this.height = height;
            this.brush = new SolidBrush(color);
        }

        public override void Draw(Graphics g, Camera cam)
        {
            PointF pos = cam.WorldToScreen(x, y);
            float width = cam.Scale(this.width);
            float height = cam.Scale(this.height);
            g.FillRectangle(brush, pos.X - width / 2, pos.Y - height / 2, width, height);
        }
    }
}
