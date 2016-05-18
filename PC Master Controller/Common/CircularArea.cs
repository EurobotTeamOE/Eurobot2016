using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;

namespace Common
{
    public class CircularArea : Element
    {
        float startAngle, endAngle, width, height;
        SolidBrush brush;

        public CircularArea(float x, float y, float width, float height, Color color)
            : base(x, y)
        {
            this.width = width;
            this.height = height;
            this.startAngle = 0;
            this.endAngle = 360;
            this.brush = new SolidBrush(color);
        }

        public CircularArea(float x, float y, float width, float height, float startAngle, float endAngle, Color color)
            : base(x, y)
        {
            this.width = width;
            this.height = height;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.brush = new SolidBrush(color);
        }

        public override void Draw(Graphics g, Camera cam)
        {
            PointF pos = cam.WorldToScreen(x, y);
            float width = cam.Scale(this.width);
            float height = cam.Scale(this.height);
            g.FillPie(brush, pos.X - width / 2, pos.Y - height / 2, width, height, startAngle, endAngle - startAngle);
        }
    }
}
