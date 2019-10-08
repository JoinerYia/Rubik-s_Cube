using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagicCube
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void LoadForm(object sender, EventArgs e)
        {
            o = new Point(this.Width / 2, this.Height / 2);
            bmp = new Bitmap(this.Width, this.Height);
            g = Graphics.FromImage(bmp);
            pori = new int[,]
            {
                {len,len,len },{-len,len,len},
                {len,-len,len },{-len,-len,len},
                {len,-len,-len },{-len,-len,-len},
                {len,len,-len },{-len,len,-len}
            };
            brushes = new Brush[]{
                new SolidBrush(Color.FromArgb(light,0,0)),//Red
                new SolidBrush(Color.FromArgb(light,light*6/10,0)),//Orange
                new SolidBrush(Color.FromArgb(light,light,0)),//Yellow
                new SolidBrush(Color.FromArgb(light*7/8,0,light*7/8)),//Purple
                new SolidBrush(Color.FromArgb(0,0,light)),//Blue
                new SolidBrush(Color.FromArgb(0,light*7/8,0)),//Green
            };
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    cube[i, j] = i;
                }
            }
            DoPaint();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            o = new Point(this.Width / 2, this.Height / 2);
            bmp = new Bitmap(this.Width, this.Height);
            g = Graphics.FromImage(bmp);
            DoPaint();
        }

        Graphics g;
        Point o, mouse;
        Point[] front = new Point[4];
        Pen[] pens = { new Pen(Color.Red, 5), new Pen(Color.Blue, 5), new Pen(Color.Green, 5), new Pen(Color.Black, 5), new Pen(Color.Black, 2) };
        Brush[] brushes;
        Bitmap bmp;
        int len = 200, pcou = 8, light = 200, touch = 0, frontind, lmaxind, lminind;
        double pi = Math.PI;
        double[] ro = new double[] { 0, 0, 0 };
        int[,] cube = new int[6, 9],
            pori,
            pss = { {1,3,2,0},{5,7,6,4},
            {3,5,4,2},{0,6,7,1},
            {2,4,6,0},{1,7,5,3} };
        bool rotation;

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox1.MouseMove -= Form1_MouseMove;
            if (rotation) return;
            Point va = e.Location, vb1 = front[lminind + 1], vb2 = front[lmaxind + 1];
            va = Point.Subtract(va, (Size)mouse);
            vb1 = Point.Subtract(vb1, (Size)front[0]);
            vb2 = Point.Subtract(vb2, (Size)front[0]);
            if (Math.Pow(va.X, 2) + Math.Pow(va.Y, 2) < 10000) return;
            double angle, angle1, angle2;
            int a1, a2;
            string test = "";
            angle = Math.Atan2(va.Y, va.X) / conv;
            angle = (angle + 360) % 360;

            angle1 = Math.Atan2(vb1.Y, vb1.X) / conv - angle;
            angle1 = (angle1 + 720) % 360;
            a1 = (int)(angle1 / 20) * 20;
            if (a1 > 180) a1 = 360 - a1;
            test += a1 + "/";
            a1 = a1 > 90 ? 0 : 1;
            a1 += lminind == 0 ? 1 - (frontind + 2) % 4 / 3 : ((frontind * 2 + lminind + lmaxind) % 4 / 3 + frontind / 4) % 2;
            if (angle1 > 180) angle1 = (angle1 + 180) % 360;
            if (angle1 > 90) angle1 = 180 - angle1;

            angle2 = Math.Atan2(vb2.Y, vb2.X) / conv - angle;
            angle2 = (angle2 + 720) % 360;
            a2 = (int)(angle2 / 20) * 20;
            if (a2 > 180) a2 = 360 - a2;
            test += a2 + "/";
            a2 = a2 > 90 ? 0 : 1;
            a2 += lminind == 0 ? (frontind + 1) % 4 / 3 : ((frontind * 2 + lminind + lmaxind + 3) % 4 / 3 + frontind / 4) % 2;
            if (angle2 > 180) angle2 = (angle2 + 180) % 360;
            if (angle2 > 90) angle2 = 180 - angle2;
            int stop = lminind == 0 ? (frontind / 2) % 2 : (lminind + lmaxind + 1) % 2;
            stop += angle1 < angle2 ? 1 : 0;
            stop %= 2;
            a1 = angle1 < angle2 ? a1 : a2;
            a1 %= 2;

            label1.Text += $"/{"-+"[a1]}{"XY"[stop]}";

            TurnTheCube(touch, a1, stop);
            DoPaint();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouse = e.Location;
            pictureBox1.MouseMove += Form1_MouseMove;
            Color c = bmp.GetPixel(e.X, e.Y);
            int stop = c.R + c.G + c.B, cube, stop2 = 0;
            rotation = (stop == 0 || stop == 765);
            if (!rotation)
            {
                double[] sangle = new double[3];
                Point p;
                double dstop;
                //求分界角度
                p = Point.Subtract(e.Location, (Size)front[0]);
                dstop = Math.Atan2(p.Y, p.X) / pi * 180;
                for (int i = 0; i < 3; i++)
                {
                    p = Point.Subtract(front[i + 1], (Size)front[0]);
                    sangle[i] = Math.Atan2(p.Y, p.X) / pi * 180;
                    if (sangle[i] < 0) sangle[i] += 360;
                    sangle[i] = Math.Abs(sangle[i] + 360 - dstop) % 360;
                }
                //求觸碰面
                int minind = 0, maxind = 0;
                for (int i = 1; i < 3; i++)
                {
                    if (sangle[minind] > sangle[i])
                    {
                        minind = i;
                    }
                    else if (sangle[maxind] < sangle[i])
                    {
                        maxind = i;
                    }
                }
                //轉換成對應面
                //排序兩邊 01/12/20
                //stop2=x/ysign/xsign
                //stop2=y/xsign/ysign
                stop = minind + maxind;
                if (stop == 1)
                {
                    minind = 0;
                    maxind = 1;
                    stop2 = (frontind / 2) % 2;
                    stop2 = stop2 * 2 + 1 - (frontind + 2) % 4 / 3;
                    stop2 = stop2 * 2 + (frontind + 1) % 4 / 3;

                    stop = frontind / 4;
                }
                else if ((frontind / 2) % 2 + stop == 3)
                {
                    minind = 4 - stop;
                    maxind = stop - minind;
                    stop2 = (stop + 1) % 2;
                    stop2 = stop2 * 2 + ((frontind * 2 + stop) % 4 / 3 + frontind / 4) % 2;
                    stop2 = stop2 * 2 + ((frontind * 2 + stop + 3)% 4 / 3 + frontind / 4)% 2;
                    stop = (frontind % 2) + 4;
                }
                else
                {
                    minind = 4 - stop;
                    maxind = stop - minind;
                    stop2 = (stop + 1) % 2;
                    stop2 = stop2 * 2 + ((frontind * 2 + stop) % 4 / 3 + frontind / 4) % 2;
                    stop2 = stop2 * 2 + ((frontind * 2 + stop + 3) % 4 / 3 + frontind / 4) % 2;
                    stop = ((frontind + 6) / 4) % 2 + 2;
                }
                lmaxind = maxind;
                lminind = minind;
                touch = stop;
                //求觸碰格
                cube = (int)(GetPoint2LineTimes(front[0], front[minind + 1], front[maxind + 1], e.Location) * 3);
                cube *= 10;
                cube += (int)(GetPoint2LineTimes(front[0], front[maxind + 1], front[minind + 1], e.Location) * 3);
                //轉換成對應格
                //x==0?x:n-x => n*0+x*((1-0)*2-1)//0+1-
                stop = (2 * (stop2 % 2) + (cube / 10) * ((1 - stop2 % 2) * 2 - 1)) * (stop2 / 4 * 2 + 1);
                stop2 /= 2;
                stop += (2 * (stop2 % 2) + (cube % 10) * ((1 - stop2 % 2) * 2 - 1)) * ((1 - stop2 / 2) * 2 + 1);
                touch = touch * 10 + stop;
                label1.Text = $"{"ROYPBG"[touch/10]}{touch%10}";
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DoPaint();
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            this.MouseMove -= Form1_MouseMove;
        }

        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            string str = e.KeyCode.ToString().FirstOrDefault(x => "0123456789O".Contains(x.ToString())).ToString();
            if (!e.Shift)
            {
                if ("12345678".Contains(str))
                {
                    int n = int.Parse(str);
                    ro[0] = ((n+2)/2%2+(n-1)/4)%2 * 270 + 45;
                    ro[1] = (3-(n-1)%4) * 90 + 45;
                    DoPaint();
                }
            }
            else
            {
                if ("01234567".Contains(str))
                {
                    int n = int.Parse(str);
                    ro[0] = (n / 2 % 2) * 270 + 45;
                    ro[1] = ((n+1)%2*3+(n/4)*(n%2*2-1)) * 90 + 45;//*/((n + 6) % 4) * 90 + 45;
                    DoPaint();
                }
            }
            if (str == "O")
            {
                ro[0] = 0;
                ro[1] = 0;
                ro[2] = 0;
                DoPaint();
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!rotation) return;
            Point newp = e.Location;
            int dx = newp.X - mouse.X, dy = newp.Y - mouse.Y;
            if (e.Button == MouseButtons.Left)
            {
                ro[0] -= (dy * 0.2);
                ro[0] = (ro[0] + 360) % 360;
                ro[0] = ((int)(ro[0] * 10)) / 10;
                ro[1] += (dx * 0.2);
                ro[1] = (ro[1] + 360) % 360;
                ro[1] = ((int)(ro[1] * 10)) / 10;
            }
            else
            {
                //return;
                ro[2] += (dy * 0.2);
                ro[2] = (ro[2] + 360) % 360;
                ro[2] = ((int)(ro[2] * 10)) / 10;
            }
            mouse = newp;
            DoPaint();
            //label1.Text = e.Location.ToString();
        }

        double conv = Math.PI / 180;

        private void DoPaint()
        {
            this.Text = $"rx = {ro[0]},ry = {ro[1]},rz = {ro[2]}";
            int[,] newp = new int[pcou, 3];
            Point[] drawp = new Point[pcou];
            int r, maxz = 0, index = -1, istop;
            double stop, angle;
            for (int i = 0; i < pcou; i++)
            {
                r = 0;
                for (int j = 0; j < 3; j++)
                {
                    newp[i, j] = pori[i, j];
                    r += newp[i, j] * newp[i, j];
                }
                for (int j = 0; j < 3; j++)
                {
                    stop = r - (newp[i, j] * newp[i, j]);
                    stop = Math.Sqrt(stop);
                    angle = Math.Atan2(newp[i, (j + 2) % 3], newp[i, (j + 1) % 3]);
                    newp[i, (j + 1) % 3] = (int)(stop * Math.Cos(ro[j] * conv + angle));
                    newp[i, (j + 2) % 3] = (int)(stop * Math.Sin(ro[j] * conv + angle));
                }
                drawp[i] = OffsetPoint(o, newp[i, 0], newp[i, 1]);
                if (newp[i, 2] > maxz)
                {
                    maxz = newp[i, 2];
                    index = i;
                }
            }
            frontind = index;
            front[0] = drawp[index];
            istop = index / 4 * 4 + ((index + 3) / 2) % 2;
            front[1] = drawp[istop];
            istop = index / 4 * 4 + ((index + 1) / 2) % 2 + 2;
            front[2] = drawp[istop];
            istop = index % 2 * 2 + 6 - index;
            front[3] = drawp[istop];

            g.Clear(Color.White);
            istop = (index % 2) + 4;
            DrawASurface(new Point[] { drawp[pss[istop, 0]], drawp[pss[istop, 1]], drawp[pss[istop, 2]], drawp[pss[istop, 3]] }, istop);
            index = (index / 2) * 2;
            istop = index / 4 + ((index / 2) % 2) * 2;
            DrawASurface(new Point[] { drawp[pss[istop, 0]], drawp[pss[istop, 1]], drawp[pss[istop, 2]], drawp[pss[istop, 3]] }, istop);
            istop = (index + 6) % 8 / 4 + 2 - ((index / 2) % 2) * 2;
            DrawASurface(new Point[] { drawp[pss[istop, 0]], drawp[pss[istop, 1]], drawp[pss[istop, 2]], drawp[pss[istop, 3]] }, istop);
            /*for (int i = 0; i < 8; i++)
            {
                g.DrawString(i.ToString(), label1.Font, Brushes.Black, drawp[i]);
            }//*/
            pictureBox1.Image = bmp;
        }

        private Point OffsetPoint(Point p, int dx, int dy)
        {
            return new Point(p.X + dx, p.Y + dy);
        }
        
        private void DrawASurface( Point[] ps, int n)
        {
            Point[,] cubep = new Point[4, 4];
            //求面上的各點
            for (int i = 0; i < 4; i++)
            {
                cubep[(i / 2) * 3, (((i + 1) / 2) % 2) * 3] = ps[i];
                cubep[(i / 2) * 3, (i % 2) + 1] = InsidePoint(ps[i / 2 * 3], ps[i / 2 + 1], (i % 2) + 1, 2 - (i % 2));
                cubep[(i % 2) + 1, (i / 2) * 3] = InsidePoint(ps[i / 2], ps[3 - i / 2], (i % 2) + 1, 2 - (i % 2));
            }
            for (int i = 1; i <= 2; i++)
            {
                cubep[1, i] = InsidePoint(cubep[0, i], cubep[3, i], 1, 2);
                cubep[2, i] = InsidePoint(cubep[0, i], cubep[3, i], 2, 1);
            }
            //畫面
            for (int i = 0; i < 9; i++)
            {
                g.FillPolygon(brushes[cube[n,i]],
                    new Point[] { cubep[i / 3, i % 3], cubep[i / 3, i % 3 + 1], cubep[i / 3 + 1, i % 3 + 1], cubep[i / 3 + 1, i % 3] });
                //g.DrawString($"{i}", label1.Font, Brushes.Aqua, InsidePoint(cubep[i / 3, i % 3], cubep[i / 3 + 1, i % 3 + 1], 1, 1));
            }
            //畫線
            for (int i = 0; i < 4; i++)
            {
                g.DrawLine(pens[3 + ((i + 1) / 2) % 2], cubep[0, i], cubep[3, i]);//3443
                g.DrawLine(pens[3 + ((i + 1) / 2) % 2], cubep[i, 0], cubep[i, 3]);
            }
            /*for (int i = 0; i < 16; i++)
            {
                g.DrawString("" + i, label1.Font, Brushes.Black, cubep[i / 4, i % 4]);
            }//*/
        }

        private Point InsidePoint(Point p1, Point p2, int a, int b)
        {
            return new Point((p1.X * b + p2.X * a) / (a + b), (p1.Y * b + p2.Y * a) / (a + b));
        }

        private double GetPoint2LineTimes(Point lp1, Point lp2, Point po,Point p)
        {
            //0=mx-y-mx0+y0
            double m,times;
            m = lp1.Y - lp2.Y;
            m /= (lp1.X - lp2.X + 0.0001);
            times = m * (p.X - lp1.X) - (p.Y - lp1.Y);
            times /= m * (po.X - lp1.X) - (po.Y - lp1.Y);
            return times < 0 ? -times : times;
        }

        private void TurnTheCube(int touched_surface,int isPositive,int isY)
        {
            int face = touched_surface / 10, x = (touched_surface % 10) % 3, y = (touched_surface % 10) / 3, upper_face = -1, rotate = isPositive, stop, d, n;
            int[] surface = new int[4];
            int[,] turn_cube = new int[4, 3];
            surface[0] = isY == 0 ? face / 4 * 2 : 2 - (face / 2 + 1) / 2;
            surface[0] = (surface[0] * 2) % 6;
            surface[1] = (surface[0] + 2) % 6;
            surface[2] = surface[0] + 1;
            surface[3] = surface[1] + 1;
            switch (surface[0] / 2)
            {
                case 0:
                    if (face == 3) y = 3 - y;
                    d = 1;

                    if (y != 1) upper_face = 5 - y / 2;
                    rotate = (rotate + face / 3) % 2;

                    for (int i = 0; i < 4; i++)
                    {
                        if (i == 3)
                        {
                            y = 3 - y;
                            d = -1;
                        }
                        for (int j = 0; j < 3; j++)
                        {
                            turn_cube[i, j] = y * 3 + j * d - (i / 3);
                        }
                    }
                    break;
                case 1:
                    if (x != 1) upper_face = x / 2;

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            turn_cube[i, j] = j * 3 + x;
                        }
                    }
                    break;
                case 2:
                    y = isY == 0 ? y : x;
                    d = (face / 4 + face % 2 + 1) % 2;
                    y = d * 2 - y * (d * 2 - 1);
                    x = 2 - y;
                    d = 0;

                    if (y != 1) upper_face = 2 + y / 2;
                    rotate = (rotate + face % 2 + 1) % 2;

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            turn_cube[i * 2, j] = y * 3 + (2 + (d * 2 - 1) * j + d) % 3;
                            turn_cube[i * 2 + 1, j] = (2 + (d * 2 - 1) * j + d) % 3 * 3 + x;
                        }
                        d = 1 - d;
                        x = 2 - x;
                        y = 2 - y;
                    }
                    break;
                default:
                    break;
            }
            for (int i = 0; i < 3; i++)
            {
                stop = cube[surface[0], turn_cube[0, i]];
                d = rotate * 2 + 1;
                for (int j = 0; j < 3; j++)
                {
                    cube[surface[(d * j) % 4], turn_cube[(d * j) % 4, i]] = cube[surface[(d * (j + 1)) % 4], turn_cube[(d * (j + 1)) % 4, i]];
                }
                cube[surface[3 - rotate * 2], turn_cube[3 - rotate * 2, i]] = stop;
            }

            if (upper_face != -1)
            {
                isPositive = (rotate + upper_face) % 2;
                for (int j = 0; j < 2; j++)
                {
                    stop = cube[upper_face, 0];
                    d = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        n = d + -(i / 4 * 2 - 1) * ((i / 2 + isPositive) % 2 * 2 + 1);
                        cube[upper_face, d] = cube[upper_face, n];
                        d = n;
                    }
                    cube[upper_face, d] = stop;
                }
            }
        }
    }
}
