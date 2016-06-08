using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Common
{
    public class Camera
    {
        float x, y, zoom, speed = 5.0f;
        int screenWidth, screenHeight;

        public Camera(int screenWidth, int screenHeight)
        {
            x = y = 0;
            zoom = 0.25f;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
        }

        public Camera(int screenWidth, int screenHeight, float x, float y)
        {
            this.x = x;
            this.y = y;
            zoom = 0.25f;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
        }

        public void Update(Robot r)
        {
            x = r.x;
            y = -r.y;
        }
        
        public void Update()
        {
            if (KeyboardState.Left) x -= speed / zoom;
            if (KeyboardState.Right) x += speed / zoom;
            if (KeyboardState.Up) y -= speed / zoom;
            if (KeyboardState.Down) y += speed / zoom;

            float prevzoom = zoom;
            if (KeyboardState.ZoomIn) zoom = Math.Min(zoom + 0.01f, 0.5f);
            if (KeyboardState.ZoomOut) zoom = Math.Max(zoom - 0.01f, 0.2f);
        }

        public PointF WorldToScreen(PointF world)
        {
            return new PointF((world.X - this.x) * zoom + screenWidth / 2, (-world.Y - this.y) * zoom + screenHeight / 2);
        }

        public PointF WorldToScreen(float x, float y)
        {
            return WorldToScreen(new PointF(x, y));
        }

        public PointF ScreenToWorld(PointF screen)
        {
            return new PointF((screen.X - screenWidth / 2) / zoom + this.x, -((screen.Y - screenHeight / 2) / zoom + this.y));
        }

        public PointF ScreenToWorld(float x, float y)
        {
            return ScreenToWorld(new PointF(x, y));
        }

        public float Scale(float val)
        {
            return val * zoom;
        }

        public float ScaleFromWorld(float val)
        {
            return val / zoom;
        }
    }
}
