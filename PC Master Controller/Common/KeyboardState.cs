using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Common
{
    static class KeyboardState
    {
        static bool up, down, left, right, zoomIn, zoomOut, rotateLeft, rotateRight;

        public static bool Up
        {
            get { return up; }
        }

        public static bool Down
        {
            get { return down; }
        }

        public static bool Left
        {
            get { return left; }
        }

        public static bool Right
        {
            get { return right; }
        }

        public static bool ZoomIn
        {
            get { return zoomIn; }
        }

        public static bool ZoomOut
        {
            get { return zoomOut; }
        }

        public static bool RotateLeft
        {
            get { return rotateLeft; }
        }

        public static bool RotateRight
        {
            get { return rotateRight; }
        }

        public static void KeyDown(Keys key)
        {
            switch (key)
            {
                case Keys.W: up = true;
                    break;
                case Keys.S: down = true;
                    break;
                case Keys.A: left = true;
                    break;
                case Keys.D: right = true;
                    break;
                case Keys.Up: zoomIn = true;
                    break;
                case Keys.Down: zoomOut = true;
                    break;
                case Keys.Left: rotateLeft = true;
                    break;
                case Keys.Right: rotateRight = true;
                    break;
            }
        }

        public static void KeyUp(Keys key)
        {
            switch (key)
            {
                case Keys.W: up = false;
                    break;
                case Keys.S: down = false;
                    break;
                case Keys.A: left = false;
                    break;
                case Keys.D: right = false;
                    break;
                case Keys.Up: zoomIn = false;
                    break;
                case Keys.Down: zoomOut = false;
                    break;
                case Keys.Left: rotateLeft = false;
                    break;
                case Keys.Right: rotateRight = false;
                    break;
            }
        }
    }
}
