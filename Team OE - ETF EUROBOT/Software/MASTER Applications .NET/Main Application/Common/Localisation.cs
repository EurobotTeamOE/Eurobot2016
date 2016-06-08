using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using KalmanDemo;

namespace Common
{
    public class Localisation : Element
    {
        public Localisation(SerialComm comm)
            : base(-500, -500)
        {
            A_SQUARED = Math.Pow(A, 2);
            D_SQUARED = A_SQUARED / 4.0 + Math.Pow(B, 2.0);
            D = Math.Sqrt(D_SQUARED);
            ALPHA = Math.Atan(2.0 * B / A);
            SIN_ALPHA = Math.Sin(ALPHA);
            COS_ALPHA = Math.Cos(ALPHA);
            MAX_R0 = Math.Sqrt(Math.Pow(A, 2.0) + Math.Pow(B, 2.0));
            MAX_R1 = Math.Sqrt(Math.Pow(A, 2.0) + Math.Pow(B, 2.0));
            MAX_R2 = Math.Sqrt(Math.Pow(A, 2.0) / 4.0 + Math.Pow(B, 2.0));
            kalmanX = new Kalman1D();
            kalmanX.Reset(0.1, 0.1, 0.1, 400, -500);
            kalmanY = new Kalman1D();
            kalmanY.Reset(0.1, 0.1, 0.1, 400, -500);

            this.comm = comm;
        }

        protected Kalman1D kalmanX, kalmanY;
        SerialComm comm;

        byte triang_status;
        double xtemp, ytemp;
        double SIN_ALPHA;
        double COS_ALPHA;
        double[] r = new double[3];
        double D, D_SQUARED;
        double A_SQUARED;
        double ALPHA;
        double MAX_R0;
        double MAX_R1;
        double MAX_R2;

        double THRESHOLD = 0.2;
        //prabacivanje primljene poruke u metre
        //double SCALE = 0.0172;
        double SCALE = 0.0129;

        //dimenzije terena [m]
        public double A = 2;
        //public double A = 0.578;
        public double B = 3;
        //public double B = 0.734;

        public float a, b, c;

        //int min = 10, max = 50;

        public virtual void Update(byte[] data)
        {
            double[] x = new double[5];
            double[] y = new double[5];
            byte[] position = new byte[8];
            position[0] = (byte)((data[3] << 4) + data[4]);
            position[1] = (byte)((data[5] << 4) + data[6]);
            position[2] = (byte)((data[7] << 4) + data[8]);

            //max = Math.Max(Math.Max(position[0], position[1]), position[2]);

            /*int a = max - position[0];
            int b = max - position[1];
            int c = max - position[2];

            if (a > 0 && b > 0 && c > 0)
            {
                float wA = a * 1.0f / (a + b + c);
                float wB = b * 1.0f / (a + b + c);
                float wC = c * 1.0f / (a + b + c);

                double posX = wA * 0 + wB * 0 + wC * B;
                double posY = wA * A + wB * 0 + wC * A / 2;

                this.a = (float)posX * 100;
                this.b = (float)posY * 100;
                this.c = 0;


                bool reset = false;
                if (this.x == -500 || this.y == -500) reset = true;

                this.x = (float)posX * 1000;
                this.y = (float)posY * 1000;

                if (reset)
                {
                    kalmanX.Reset(0.1, 0.1, 0.1, 400, this.x);
                    kalmanY.Reset(0.1, 0.1, 0.1, 400, this.y);
                }
                else
                {
                    double dt = (DateTime.Now - prev).TotalSeconds;
                    kalmanX.Update(this.x, dt);
                    kalmanY.Update(this.y, dt);
                }
                prev = DateTime.Now;
            }

            return;*/

            //Form1.Log.Log(position[0] + " " + position[1] + " " + position[2]);

            r[0] = SCALE * (double)position[0];
            if (r[0] > MAX_R0)
            {
                position[0] = 255;
                r[0] = SCALE * (double)position[0];
            }
            r[1] = SCALE * (double)position[1];
            if (r[1] > MAX_R1)
            {
                position[1] = 255;
                r[1] = SCALE * (double)position[1];
            }
            r[2] = SCALE * (double)position[2];
            if (r[2] > MAX_R2)
            {
                position[2] = 255;
                r[2] = SCALE * (double)position[2];
            }

            if (position[0] < 252 && position[1] < 252 && position[2] < 252) //sva tri su navodno validna
            {
                triang_status = 31;
                x[0] = A_SQUARED - (Math.Pow(r[1], 2) - Math.Pow(r[0], 2));
                x[0] = x[0] / (2 * A);
                y[0] = Math.Abs(Math.Pow(r[0], 2) - Math.Pow(x[0], 2));
                y[0] = Math.Sqrt(y[0]);

                x[1] = D_SQUARED - (Math.Pow(r[2], 2) - Math.Pow(r[0], 2));
                x[1] = x[1] / (2.0 * D);
                y[1] = Math.Abs(Math.Pow(r[0], 2) - Math.Pow(x[1], 2));
                y[1] = -Math.Sqrt(y[1]);

                xtemp = x[1];
                ytemp = y[1];
                x[1] = xtemp * COS_ALPHA - ytemp * SIN_ALPHA;
                y[1] = xtemp * SIN_ALPHA + ytemp * COS_ALPHA;

                x[2] = xtemp * COS_ALPHA + ytemp * SIN_ALPHA;
                y[2] = xtemp * SIN_ALPHA - ytemp * COS_ALPHA;

                /*if (distance(1, 0) <= THRESHOLD)
                {
                }
                else
                {
                }*/
                x[3] = D_SQUARED - (r[2] * r[2] - r[1] * r[1]);
                x[3] = -x[3] / (2 * D);
                y[3] = -Math.Sqrt(Math.Abs(r[1] * r[1] - x[3] * x[3]));
                xtemp = x[3];
                ytemp = y[3];
                x[3] = xtemp * COS_ALPHA + ytemp * SIN_ALPHA;
                y[3] = -xtemp * SIN_ALPHA + ytemp * COS_ALPHA;
                x[3] = x[3] + A;
                x[4] = xtemp * COS_ALPHA - ytemp * SIN_ALPHA;
                y[4] = -xtemp * SIN_ALPHA - ytemp * COS_ALPHA;
                x[4] = x[4] + A;

                /*if (distance(3, 0) <= THRESHOLD)
                {
                }
                else
                {
                }*/
            }
            else //nisu sva tri validna, ili neka dva ili jedan ili nijedan
            {
                if (position[0] < 252 && position[1] < 252) //validni A i B, jedinstveno resenje
                {
                    triang_status = 1;
                    x[0] = A_SQUARED - (Math.Pow(r[1], 2) - Math.Pow(r[0], 2));
                    x[0] = x[0] / (2 * A);
                    y[0] = Math.Sqrt((Math.Pow(r[0], 2) - Math.Pow(x[0], 2)));
                }
                else if (position[0] < 252 && position[2] < 252) //validni A i C, resenje nije jedinstveno
                {
                    triang_status = 6;
                    x[1] = D_SQUARED - (Math.Pow(r[2], 2) - Math.Pow(r[0], 2));
                    x[1] = x[1] / (2.0 * D);
                    y[1] = Math.Abs(Math.Pow(r[0], 2) - Math.Pow(x[1], 2));
                    y[1] = -Math.Sqrt(y[1]);
                    xtemp = x[1];
                    ytemp = y[1];
                    x[1] = xtemp * COS_ALPHA - ytemp * SIN_ALPHA;
                    y[1] = xtemp * SIN_ALPHA + ytemp * COS_ALPHA;
                    x[2] = xtemp * COS_ALPHA + ytemp * SIN_ALPHA;
                    y[2] = xtemp * SIN_ALPHA - ytemp * COS_ALPHA;
                    /*if (distance(1, 0) <= THRESHOLD)
                    {
                    }
                    else
                    {
                    }*/
                }
                else if (position[1] < 252 && position[2] < 252) //validni B i C, resenje nije jedinstveno
                {
                    triang_status = 24;
                    x[3] = D_SQUARED - (r[2] * r[2] - r[1] * r[1]);
                    x[3] = -x[3] / (2 * D);
                    y[3] = -Math.Sqrt(Math.Abs(r[1] * r[1] - x[3] * x[3]));
                    xtemp = x[3];
                    ytemp = y[3];
                    x[3] = xtemp * COS_ALPHA + ytemp * SIN_ALPHA;
                    y[3] = -xtemp * SIN_ALPHA + ytemp * COS_ALPHA;
                    x[3] = x[3] + A;
                    x[4] = xtemp * COS_ALPHA - ytemp * SIN_ALPHA;
                    y[4] = -xtemp * SIN_ALPHA - ytemp * COS_ALPHA;
                    x[4] = x[4] + A;
                    /*if (distance(3, 0) <= THRESHOLD)
                    {
                    }
                    else
                    {
                    }*/
                }
                else //nijedan par nije validan, mogu biti validni samo pojedini ili nijedan
                { //nista ne mogu da uradim...
                    triang_status = 0;
                }
            }

            if (triang_status == 31)
            {
                PointF mid = new PointF((float)x[0], (float)y[0]);

                PointF left1 = new PointF((float)x[1], (float)y[1]);
                PointF left2 = new PointF((float)x[2], (float)y[2]);

                PointF right1 = new PointF((float)x[3], (float)y[3]);
                PointF right2 = new PointF((float)x[4], (float)y[4]);

                PointF left = (distance(left1, mid) < distance(left2, mid)) ? left1 : left2;
                PointF right = (distance(right1, mid) < distance(right2, mid)) ? right1 : right2;

                PointF T = new PointF((mid.X + left.X + right.X) / 3, (mid.Y + left.Y + right.Y) / 3);

                //if (distance(T, mid) > THRESHOLD || distance(T, left) > THRESHOLD || distance(T, right) > THRESHOLD /*|| Math.Sqrt(Math.Pow(this.x - T.Y * 1000, 2) + Math.Pow(this.y - T.X * 1000, 2)) < THRESHOLD*/)
                //{
                //    this.x = this.y = -500;
                //    kalmanX.Reset(0.1, 0.1, 0.1, 400, -500);
                //    kalmanY.Reset(0.1, 0.1, 0.1, 400, -500);
                //}
                //else
                {
                    bool reset = false;
                    if (this.x == -500 || this.y == -500) reset = true;

                    this.x = T.Y * 1000;
                    this.y = T.X * 1000;

                    //if (reset)
                    //{
                    //    kalmanX.Reset(0.1, 0.1, 0.1, 400, this.x);
                    //    kalmanY.Reset(0.1, 0.1, 0.1, 400, this.y);
                    //}
                    //else
                    {
                        double dt = (DateTime.Now - prev).TotalSeconds;
                        //kalmanX.Update(this.x, dt);
                        //kalmanY.Update(this.y, dt);

                        kalmanX.Reset(0.1, 0.1, 0.1, 400, this.x);
                        kalmanY.Reset(0.1, 0.1, 0.1, 400, this.y);
                    }
                    prev = DateTime.Now;
                }
            }
        }

        DateTime prev;

        float distance(PointF A, PointF B)
        {
            return (float)Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2));
        }

        public override void Update()
        {
            base.Update();
        }

        public void Ping()
        {
            CommBuffer buffer = comm.outputBuffer;

            /*buffer.Write(0xFF);
            buffer.Write(0x0A);
            buffer.Write(0x03);
            buffer.Write(0x08);
            buffer.Write(0x00);*/

            buffer.Write(0xFF);
            buffer.Write(0x14);
            buffer.Write(0x01);

            comm.SendMessage();
        }

        public PointF Position
        {
            get { return new PointF((float)kalmanX.Value, (float)kalmanY.Value); }
        }

        public override void Draw(Graphics g, Camera cam)
        {
            float x = (float)kalmanX.Value;
            float y = (float)kalmanY.Value;
            if (x > -250 && y > -250)
            {
                PointF pos = cam.WorldToScreen(x, y);
                float size = cam.Scale(100);
                g.FillEllipse(Brushes.Cyan, pos.X - size, pos.Y - size, size * 2, size * 2);
                g.DrawEllipse(new Pen(Color.Red, cam.Scale(7)), pos.X - size, pos.Y - size, size * 2, size * 2);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
