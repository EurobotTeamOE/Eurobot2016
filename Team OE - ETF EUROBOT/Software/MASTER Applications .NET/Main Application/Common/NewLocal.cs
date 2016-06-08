using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.IO;
using KalmanDemo;

namespace Common
{
    public class NewLocal : Localisation
    {
        public NewLocal(SerialComm comm)
            : base(comm)
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

            /*for (int i = 0; i < 4; i++)
            {
                kalmanX[i] = new Kalman1D();
                kalmanX[i].Reset(0.1, 0.1, 0.1, 400, -500);
                kalmanY[i] = new Kalman1D();
                kalmanY[i].Reset(0.1, 0.1, 0.1, 400, -500);
            }*/

            this.comm = comm;

            sw = new StreamWriter("file.txt", false);
        }

        public void CloseStream()
        {
            sw.Close();
        }

        ~NewLocal()
        {
        }

        //Kalman1D kalmanX, kalmanY;
        //Kalman1D[] kalmanX = new Kalman1D[4];
        //Kalman1D[] kalmanY = new Kalman1D[4];

        SerialComm comm;
        StreamWriter sw;

        byte triang_status;
        double xtemp, ytemp;
        double SIN_ALPHA;
        double COS_ALPHA;
        public double[] r = new double[3];
        double D, D_SQUARED;
        double A_SQUARED;
        double ALPHA;
        double MAX_R0;
        double MAX_R1;
        double MAX_R2;

        //NOVE PROMENLJIVE
        //double X_MAX;
        //double Y_MAX;

        //KONSTANTE I TRESHOLDI KOJE JE POTREBNO PODESAVATI U ZAVISNOSTI KOLIKO BRLJA I KAKVU PRECIZNOST ZELIMO
        double RADIUS_MIN = 0.025; //0.25 cm minimalni poluprecnik(sve sto je ispod ove vrednosti zaokrzuje se na ovu vrednot)
        double RADIUS_MAX = 0.15; //15 cm poluprecnik maksimalnog kruga u kojem ga trazi
        double TRESHOLD_BEACON = 0.40; //Max rastojanje izmedju kordinata pojedinih beacona

        //double TRESHOLD = 0.20; //Stara promenljiva, sada je ne koristimo, prag iznad kojeg se sigurno izgubio i potreban je reset, 20 cm

        int init_counter = 0; //Pocetni counter za inicijalizaciju, dok jos nije popunio kalmana
        int reset_counter = 0; //Counter za reset kalmana

        double dt = 0.1;
        //Vreme za koje pretpostavljamo naredne kordinate, 0.1 sec posto podatak o lokalizaciji dobijamo na svaku 0.1 sec, mozda kao konstanta 0.1, mozda kao prethodno vreme
        //dt se automatski odredjuje programu, a ako ne radi mozemo staviti kao konstantu. To se radi tako sto zakomentarisemo pri kraju ovog algoritma deo:
        //dt = (DateTime.Now - prev).TotalSeconds;
        double scale_constant_write_text = 100; //Skaliranje u cm kada upisuje u file.txt


        //Prebacivanje primljene poruke u metre
        double SCALE = 0.0172; //Konstanta koja se empirijski podesava i za standardne dimenzije terena 2m x 3m iznosi 0.0172
        //double SCALE = 0.0129; //Za mali teren

        //Dimenzije terena [m]
        new double A = 2;
        //public double A = 0.57; //Za mali teren
        new double B = 3;
        //public double B = 0.87; //Za mali teren

        new public float a, b, c;

        //int min = 10, max = 50;

        public override void Update(byte[] data)
        {
            base.Update(data);
            return;
            //for (int i = 0; i < 4; i++)
            {
                int i = 0;
                double[] x = new double[5];
                double[] y = new double[5];
                byte[] position = new byte[8];
                position[0] = (byte)((data[3 + 8 * i] << 4) + data[4 + 8 * i]);
                position[1] = (byte)((data[5 + 8 * i] << 4) + data[6 + 8 * i]);
                position[2] = (byte)((data[7 + 8 * i] << 4) + data[8 + 8 * i]);


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


                //Ukoliko postoje sve tri vrste koordinata
                if (triang_status == 31)
                {
                    PointF mid = new PointF((float)x[0], (float)y[0]);

                    PointF left1 = new PointF((float)x[1], (float)y[1]);
                    PointF left2 = new PointF((float)x[2], (float)y[2]);

                    PointF right1 = new PointF((float)x[3], (float)y[3]);
                    PointF right2 = new PointF((float)x[4], (float)y[4]);

                    //Mozda potrebno promeniti pametnije? Za sad je ok. Ideja poredjenje sa prekitivnom
                    PointF left = (distance(left1, mid) < distance(left2, mid)) ? left1 : left2;
                    PointF right = (distance(right1, mid) < distance(right2, mid)) ? right1 : right2;


                    //
                    //"PAMETNI" ALGORITAM ZA ODREDJIVANJE I PREDIKCIJU POZICIJE
                    //

                    PointF? T = null; //Konacna pozicija
                    bool reset = false;
                    PointF prediction = GetPrediction(dt);
                    PointF previous = Position;
                    double dis = distance(mid, left);
                    if (init_counter < 5)
                    {
                        //Pocetno popunjavanje i inicijalizacija Kalmana

                        //Prvih 5 tacaka se odredjuju kao srednja vrednost od dobijenih tacaka iz beacona i provlace se kroz Kalmana
                        if ((distance(mid, left) < TRESHOLD_BEACON) && (distance(mid, right) < TRESHOLD_BEACON) && (distance(left, right) < TRESHOLD_BEACON))
                        {
                            T = new PointF((mid.X + left.X + right.X) / 3, (mid.Y + left.Y + right.Y) / 3);
                            if (this.x == -500 || this.y == -500)
                            {
                                init_counter--;
                                reset = true;
                            }
                            init_counter++;
                        }
                        else
                        {
                            T = new PointF(-500, -500);
                            reset = true;
                        }
                    }
                    else
                    {
                        //Ostale tacke se utvrdjuju na osnovu algoritma objasnjenog u onom papiru

                        double distance_previous = distance(prediction, previous);
                        if (distance_previous < RADIUS_MIN) distance_previous = RADIUS_MIN;

                        //Proverava koje se tacke nalaze u okolini predict-ivne tacke
                        int[] inside_prediction = inside_indicators(mid, left, right, prediction, distance_previous); //Niz indikatora koji pokazuje koje su tacke unutar radijusa prediktivne tacke
                        int num_inside_prediction = inside_prediction.Sum(); //Broj tacaka koje su unutar radijusa prediktivne tacke

                        switch (num_inside_prediction)
                        {
                            case 0:
                                //Ukoliko nijedna nije unutar radijusa prediktivne tacke
                                //Sirimo radijus na maksimalni i trazimo u tom krugu tacke
                                int[] inside_prediction0 = inside_indicators(mid, left, right, prediction, RADIUS_MAX);
                                int num_inside_prediction0 = inside_prediction0.Sum();
                                //Ukoliko smo nasli 2 ili 3 tacke onda odlucujemo
                                if (num_inside_prediction0 >= 2)
                                {
                                    T = new PointF((inside_prediction0[0] * mid.X + inside_prediction0[1] * left.X + inside_prediction0[2] * right.X) / num_inside_prediction0, (inside_prediction0[0] * mid.Y + inside_prediction0[1] * left.Y + inside_prediction0[2] * right.Y) / num_inside_prediction0);
                                    reset_counter = 0;
                                    break;
                                }
                                else
                                {
                                    //Ukoliko nismo ispitujemo maksimalni radijus u okolini prethodne tacke
                                    int[] inside_previous = inside_indicators(mid, left, right, previous, RADIUS_MAX);
                                    int num_inside_previous = inside_previous.Sum();
                                    //Ukoliko smo nasli 2 ili 3 tacke onda odlucujemo
                                    if (num_inside_previous >= 2)
                                    {
                                        T = new PointF((inside_previous[0] * mid.X + inside_previous[1] * left.X + inside_previous[2] * right.X) / num_inside_previous, (inside_previous[0] * mid.Y + inside_previous[1] * left.Y + inside_previous[2] * right.Y) / num_inside_previous);
                                        reset_counter = 0;
                                        break;
                                    }
                                    else
                                    {
                                        T = null;
                                        break;
                                    }
                                }

                            case 1:
                                //Ukoliko je samo jedna unutar radijusa prediktivne tacke
                                //Sirimo radijus na maksimalni i trazimo u tom krugu tacke
                                reset_counter = 0;
                                int[] inside_prediction1 = inside_indicators(mid, left, right, prediction, RADIUS_MAX);
                                int num_inside_prediction1 = inside_prediction1.Sum();
                                if (num_inside_prediction1 >= 2)
                                {
                                    //Ukoliko smo nasli 2 ili 3 tacke onda odlucujemo
                                    T = new PointF((inside_prediction1[0] * mid.X + inside_prediction1[1] * left.X + inside_prediction1[2] * right.X) / num_inside_prediction1, (inside_prediction1[0] * mid.Y + inside_prediction1[1] * left.Y + inside_prediction1[2] * right.Y) / num_inside_prediction1);
                                    break;
                                }
                                //Ukoliko nismo ispitujemo MAX_RADIUS oko prethodne tacke
                                else
                                {
                                    int[] inside_previous = inside_indicators(mid, left, right, previous, RADIUS_MAX);
                                    int num_inside_previous = inside_previous.Sum();
                                    if (num_inside_previous >= 2)
                                    {
                                        //Ukoliko smo nasli 2 ili 3 tacke onda odlucujemo
                                        T = new PointF((inside_previous[0] * mid.X + inside_previous[1] * left.X + inside_previous[2] * right.X) / num_inside_previous, (inside_previous[0] * mid.Y + inside_previous[1] * left.Y + inside_previous[2] * right.Y) / num_inside_previous);
                                        break;
                                    }
                                    else
                                    {
                                        //Ukoliko nismo onda proglasavamo tu jednu koju smo nasli
                                        //(moguce je da jedan senzor brlja i da zbog toga imamo slucaj da samo jedan par senzora dobro radi, zato nju proglasavamo a ne odbacujemo)
                                        if (inside_prediction[0] == 1) T = mid;
                                        else if (inside_prediction[1] == 1) T = left;
                                        else if (inside_prediction[2] == 1) T = right;
                                        else T = null;
                                        break;
                                    }
                                }

                            case 2:
                                //Ako su 2 unutar kruga racunaj srednju vrednost te 2
                                T = new PointF((inside_prediction[0] * mid.X + inside_prediction[1] * left.X + inside_prediction[2] * right.X) / num_inside_prediction, (inside_prediction[0] * mid.Y + inside_prediction[1] * left.Y + inside_prediction[2] * right.Y) / num_inside_prediction);
                                reset_counter = 0;
                                break;

                            case 3:
                                //Ako su sve 3 unutar kruga racunaj srednju vrednost te 3 + prediction
                                //Ipak mozda je bolje odlucivanje ako je najbliza proglasena
                                T = new PointF((mid.X + left.X + right.X + prediction.X) / 4, (mid.Y + left.Y + right.Y + prediction.Y) / 4);
                                reset_counter = 0;
                                break;

                            default:
                                T = null;
                                break;
                        }
                    }

                    //if null onda ... poredi medjusobnu distance od tacaka koje daju beaconi posle par puta, iskuliraj, iskliraj, i onda ako nema jos podatka o tacnoj poziciji tacke, nadji novu, resetuj kalmana, ponovo inizijalizacija itd
                    if (T == null)
                    {
                        //Trazimo tacke unutar prethodne i sirimo precnik stalno, ne brisemo kalmana
                        int[] inside_previous = inside_indicators(mid, left, right, previous, (reset_counter + 1) * RADIUS_MAX);
                        int num_inside_previous = inside_previous.Sum();
                        if (num_inside_previous == 3)
                        {
                            if ((distance(mid, left) < TRESHOLD_BEACON) && (distance(mid, right) < TRESHOLD_BEACON) && (distance(left, right) < TRESHOLD_BEACON))
                            {
                                T = new PointF((mid.X + left.X + right.X) / 3, (mid.Y + left.Y + right.Y) / 3);
                                reset_counter = 0;
                            }
                            else
                            {
                                T = new PointF(-500, -500);
                                reset_counter++;
                            }
                        }
                        else
                        {
                            if (reset_counter < 5)
                            {
                                T = new PointF(-500, -500);
                                reset_counter++;
                            }
                            else
                            {
                                //Moguce da je pobrljala skroz, mozda slucaj za to, trazi srednju vrednost, resetuj kalmana
                                if ((distance(mid, left) < TRESHOLD_BEACON) && (distance(mid, right) < TRESHOLD_BEACON) && (distance(left, right) < TRESHOLD_BEACON))
                                {
                                    T = new PointF((mid.X + left.X + right.X) / 3, (mid.Y + left.Y + right.Y) / 3);
                                    reset = true;
                                }
                                else
                                {
                                    T = new PointF(-500, -500);
                                    reset = true;
                                }
                            }
                        }
                    };
                    //PITAJ STEVANA ZAR NIJE OBRNUTO TJ this.x = T.X*1000
                    //Pre je bilo obrnuto:
                    //this.x = T.Value.Y;
                    //this.y = T.Value.X;
                    //Ali je brljalo, sad ok radi. Tacka 0,0 je kod prvog beacona(broj 1). Kordinatni sistem ide na desno i na dole.
                    //Dakle ovo ispod(nezakomentarisano) je ok i ne treba ga dirati ako nema neke preterane potrebe :D
                    this.x = T.Value.X;
                    this.y = T.Value.Y;
                    if (reset)
                    {
                        kalmanX.Reset(0.1, 0.1, 0.1, 400, this.x);
                        kalmanY.Reset(0.1, 0.1, 0.1, 400, this.y);
                        init_counter = 0;
                        reset_counter = 0;
                    }
                    else
                    {
                        if (this.x != -500 && this.y != -500)
                        {
                            dt = (DateTime.Now - prev).TotalSeconds; //Ukoliko zelimo konstantno dt od 0.1 sec onda treba da zakomentarisemo ovaj deo koda dt = (DateTime.Now - prev).TotalSeconds; 
                            kalmanX.Update(this.x, dt);
                            kalmanY.Update(this.y, dt);
                        }
                    }
                    prev = DateTime.Now;

                    //Uzimati kordinate iz Kalmana(one koje su ispeglane i provucene kroz Kalmanov filtar) jer one daju manju gresku cini mi se.
                    //Dakle dodati u kod na ovom mestu da se vraca tj. uzimaju kordinate:
                    //kalmanX.Value i kalmanY.Value
                    //Medjutim treba voditi racuna i o tome da se pozicija odbacuje ukoliko su kordinate tacke -500,-500
                    //Dakle ukoliko glavni program dobija podatke o poziciji preko this.x i this.y onda na ovom mestu treba da stoji:
                    if ((this.x != -500) && (this.y != -500))
                    {
                        this.x = (float)kalmanX.Value;
                        this.y = (float)kalmanY.Value;
                    }

                    //Pisanje u fajl zbog testiranja cisto
                    //Prvo se ispisuju vrednosti Kalmana, onda stvarne Pozicije, onda pozicije od svakog Beacona posebno
                    //sw.WriteLine("Kalman: " + (kalmanX.Value * scale_constant_write_text).ToString("00.00") + " " + (kalmanY.Value * scale_constant_write_text).ToString("00.00"));
                    //sw.WriteLine("POSITION: " + (this.x * scale_constant_write_text).ToString("00.00") + " " + (this.y * scale_constant_write_text).ToString("00.00") + "\n");
                    //sw.WriteLine("MID: " + (mid.X * scale_constant_write_text).ToString("00.00") + " " + (mid.Y * scale_constant_write_text).ToString("00.00"));
                    //sw.WriteLine("LEFT: " + (left.X * scale_constant_write_text).ToString("00.00") + " " + (left.Y * scale_constant_write_text).ToString("00.00"));
                    //sw.WriteLine("RIGHT: " + (right.X * scale_constant_write_text).ToString("00.00") + " " + (right.Y * scale_constant_write_text).ToString("00.00") + "\n");
                }
            }
        }

        DateTime prev;

        float distance(PointF A, PointF B)
        {
            return (float)Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2));
        }

        int[] inside_indicators(PointF M, PointF L, PointF R, PointF Ref, double distance_radius)
        {
            //Funkcija za odredjivanje koje tacke spadaju unutar odredjenog radijusa
            double distance_mid = distance(Ref, M);
            double distance_left = distance(Ref, L);
            double distance_right = distance(Ref, R);
            int[] inside = new int[3]; //Niz koji je indikator da li je tacka dobijena iz lokalizacije unutar prediktivnog kruga
            inside[0] = (distance_mid < distance_radius) ? 1 : 0;
            inside[1] = (distance_left < distance_radius) ? 1 : 0;
            inside[2] = (distance_right < distance_radius) ? 1 : 0;
            return inside;
        }

        public override void Update()
        {
            base.Update();
        }

        public PointF GetPrediction(DateTime time)
        {
            return new PointF((float)kalmanX.Predicition((time - prev).TotalSeconds), (float)kalmanY.Predicition((time - prev).TotalSeconds));
            //return new PointF((float)kalmanY.Predicition((time - prev).TotalSeconds), (float)kalmanX.Predicition((time - prev).TotalSeconds));
        }

        public PointF GetPrediction(double dt)
        {
            return new PointF((float)kalmanX.Predicition(dt), (float)kalmanY.Predicition(dt));
        }

        public override void Draw(Graphics g, Camera cam)
        {
            base.Draw(g, cam);
            return;

            //Iscrtavanje, Stevan treba da se poigra sa ovim :)
            float x = (float)kalmanY.Value;
            float y = (float)kalmanX.Value;
            if (x > -250 && y > -250)
            {
                PointF pos = cam.WorldToScreen(x, y - 0.5f);
                float size = cam.Scale(0.1f);
                g.FillEllipse(Brushes.Cyan, pos.X - size, pos.Y - size, size * 2, size * 2);
                g.DrawEllipse(new Pen(Color.Red, cam.Scale(0.007f)), pos.X - size, pos.Y - size, size * 2, size * 2);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
