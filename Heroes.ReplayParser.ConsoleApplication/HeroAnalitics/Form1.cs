using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MpqLib.Mpq;
using System;
using System.Linq;
using Heroes.ReplayParser;
using System.Collections.Generic;

namespace HeroAnalitics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static readonly int[] HeroDeathTimersByTeamLevelInSecondsForTalentLevels = new[] {
            9,  //  1
            12, //  4
            16, //  7
            23, // 10
            32, // 13
            44, // 16
            65  // 20
        };

        private void Form1_Load(object sender, EventArgs e)
        {
            var heroesAccountsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Heroes of the Storm\Accounts");
            var randomReplayFileName = Directory.GetFiles(heroesAccountsFolder, "*.StormReplay", SearchOption.AllDirectories).OrderBy(i => Guid.NewGuid()).First();

            // Use temp directory for MpqLib directory permissions requirements
          //  ParceReply(randomReplayFileName);
        }

        public class Heroy
        {
            public System.Drawing.Point WhereAmI;
            public string MyName;
            public string MyHeroName;
            public bool inited;
            public bool dead;
            public int team;
            public Heroy(System.Drawing.Point w, string mn, string mhn, bool d,int t)
            {
                inited=true;
                WhereAmI = w;
                MyName = mn;
                MyHeroName = mhn;
                dead = d;
                team = t;
            }
            public Heroy(Heroy h)
            {
                inited = h.inited;
                WhereAmI = h.WhereAmI;
                MyName = h.MyName;
                MyHeroName = h.MyHeroName;
                dead = h.dead;
                team = h.team;
            }
            public Heroy()
            {
                inited=false;
            }
        }

        public class HeroesNotDie
        {
            public Heroy[] H;
            public double[] HeroR;
            public TimeSpan ts;
            public PointF M0;
            public PointF M1;
            public PointF D0;
            public PointF D1;
            public HeroesNotDie()
            {
                H = new Heroy[10];
                HeroR = new double[10];
            }
            public HeroesNotDie(TimeSpan t)
            {
                ts = t;
            }
            public bool findMO()
            {
                M0 = new PointF(0, 0);
                M1 = new PointF(0, 0);
                for (int i = 0; i < 10; i++)
                {
                    if (H[i] == null)
                    {
                        M0 = new PointF(0, 0);
                        M1 = new PointF(0, 0);
                        return false;
                    }
                    else
                    {
                        if (!H[i].dead)
                            if (H[i].team == 0)
                            {

                                M0.X += H[i].WhereAmI.X;
                                M0.Y += H[i].WhereAmI.Y;
                            }
                            else
                            {
                                M1.X += H[i].WhereAmI.X;
                                M1.Y += H[i].WhereAmI.Y;
                            }
                    }
                }
                M0.X = M0.X / 5;
                M0.Y = M0.Y / 5;
                M1.X = M1.X / 5;
                M1.Y = M1.Y / 5;
                return true;
            }
            public bool findDisp()
            {
                D0 = new PointF(0, 0);
                D1 = new PointF(0, 0);
                for (int i = 0; i < 10; i++)
                {
                    if (H[i] == null)
                    {
                        D0 = new PointF(0, 0);
                        D1 = new PointF(0, 0);
                        return false;
                    }
                    else
                    {
                        if (!H[i].dead)
                            if (H[i].team == 0)
                            {
                                D0.X += (H[i].WhereAmI.X - M0.X) * (H[i].WhereAmI.X - M0.X);
                                D0.Y += (H[i].WhereAmI.X - M0.Y) * (H[i].WhereAmI.X - M0.Y);
                            }
                            else
                            {
                                D1.X += (H[i].WhereAmI.X - M1.X) * (H[i].WhereAmI.X - M1.X);
                                D1.Y += (H[i].WhereAmI.X - M1.Y) * (H[i].WhereAmI.X - M1.Y);
                            }
                    }
                }
                D0.X = D0.X / 5;
                D0.Y = D0.Y / 5;
                D1.X = D1.X / 5;
                D1.Y = D1.Y / 5;
                D0.X = (float)Math.Sqrt(D0.X);
                D0.Y = (float)Math.Sqrt(D0.Y);
                D1.X = (float)Math.Sqrt(D1.X);
                D1.Y = (float)Math.Sqrt(D1.Y);

                return true;
            }
            public bool findHeroR()
            {
                for (int i=0;i<10;i++)
                {
                    HeroR[i] = 0;
                    if (!H[i].dead)
                        if (H[i].team==0)
                    {
                        double dx = H[i].WhereAmI.X - M0.X;
                        double dy = H[i].WhereAmI.Y - M0.Y;
                        HeroR[i] = Math.Sqrt(dx * dx + dy * dy);
                    }
                    else
                        {
                            double dx = H[i].WhereAmI.X - M1.X;
                            double dy = H[i].WhereAmI.Y - M1.Y;
                            HeroR[i] = Math.Sqrt(dx * dx + dy * dy);
                        }
                }
                return true;
            }
        }

        HeroesNotDie[] Her = new HeroesNotDie[100];
        int[] ListForNames1 = new int[5];
        int[] ListForNames2 = new int[5];
        Double[] HerouMarching = new Double[10];
        Double[] HerouMarchingWay = new Double[10];
        double[] SumOfDisp = new double[2];
        double[] TeamWay = new double[2];

        Team[] Table = new Team[2];

        public class Team
        {
            public List<Player> Disp;
            public List<Player> Dist;
            public void calculate()
            {
                Disp.Sort();
                Dist.Sort();
                Dist.Reverse();
            }
            public Team()
            {
                Dist = new System.Collections.Generic.List<Player>();
                Disp = new System.Collections.Generic.List<Player>();
                
            }
        }
        public class Player: IComparable<Player>
        {
            public string Name;
            public double Characteristic;
            public Player(string N, double d)
            {
                Name = N;
                Characteristic = d;
            }
            public int CompareTo(Player obj)
            {
                if (obj.Characteristic > this.Characteristic)
                    return -1;
                else
                    return 1;
            }
        }
       
        private void ParceReply(string randomReplayFileName)
        {
            var tmpPath = Path.GetTempFileName();
            File.Copy(randomReplayFileName, tmpPath, true);
            if (inthread == 0)
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
            }
            HerouMarching = new Double[10];
            SumOfDisp[0] = 0;
            SumOfDisp[1] = 0;
            HerouMarchingWay = new double[10];
            TeamWay[0] = 0;
            TeamWay[1] = 0;
            Table[0] = new Team();
            Table[1] = new Team();
            try
            {
                // Create our Replay object: this object will be filled as you parse the different files in the .StormReplay archive
                var replay = new Replay();
                MpqHeader.ParseHeader(replay, tmpPath);
                List<Unit.HeroPosList> u = new System.Collections.Generic.List<Unit.HeroPosList>();
                try
                {
                    using (var archive = new CArchive(tmpPath))
                    {
                        ReplayInitData.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.initData"));
                        ReplayDetails.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.details"));
                        ReplayTrackerEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.tracker.events"));
                        ReplayAttributeEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.attributes.events"));
                        if (replay.ReplayBuild >= 32455)
                            ReplayGameEvents.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.game.events"));
                        ReplayServerBattlelobby.Parse(replay, GetMpqArchiveFileBytes(archive, "replay.server.battlelobby"));
                        u = Unit.ParseUnitData(replay);
                    }
                }
                catch
                {
                    if (inthread == 0)
                    listBox1.Items.Add("Error!");
                }
                if (u != null)
                {
                    if (u.Count == 10)
                    {
                        int timemax = 0;
                        for (int i = 0; i < u.Count;i++)
                        {
                            if (timemax < u[i].TP[u[i].TP.Count - 1].ts.TotalSeconds+1)
                                timemax = (int)(u[i].TP[u[i].TP.Count - 1].ts.TotalSeconds+1);
                        }
                        //HeroesNotDie[]
                            Her = new HeroesNotDie[timemax];
                        for (int i = 0; i < Her.Length;i++ )
                        {
                            Her[i] = new HeroesNotDie();
                        }
                            //Оценим суммарное передвижение за игру
                            //Вычислять его будем как 
                        int li1 = 0;
                        int li2 = 0;
                            for (int i = 0; i < u.Count; i++)
                            {
                                double way = 0;
                                int first = 0;
                                bool dead = false;
                                PointF DeadPlace = new PointF(0,0);
                                for (int j = 1; j < u[i].TP.Count - 1; j++)
                                {
                                    int x = u[i].TP[j].x - u[i].TP[first].x;
                                    int y = u[i].TP[j].y - u[i].TP[first].y;
                                    //Типо скорость
                                    //Расстояние нельзя использовать, так как бывают метки идущие не через секунду. Нужна нормировка расстояния к времени
                                    double s = Math.Sqrt(x * x + y * y) / (u[i].TP[j].ts.TotalSeconds - u[i].TP[first].ts.TotalSeconds);
                                    //Типо расстояние
                                    double r = Math.Sqrt(x * x + y * y);
                                    //Смотрим резкие превышения скорости
                                    if (s < 20)
                                    {
                                        if (r < 20)
                                        {
                                            if (!dead)
                                            {
                                                way += r;
                                                Her[(int)u[i].TP[j].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, false, u[i].team);
                                            }
                                            else
                                            {
                                                if ((u[i].TP[j].x == u[i].TP[j - 1].x) && (u[i].TP[j].y == u[i].TP[j - 1].y))
                                                    Her[(int)u[i].TP[j].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, true, u[i].team);
                                                else
                                                {
                                                    dead = false;
                                                    way += r;
                                                    Her[(int)u[i].TP[j].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, false, u[i].team);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (first>30)
                                            if ((u[i].TP[j].ts.TotalSeconds - u[i].TP[first].ts.TotalSeconds) >= 10)
                                            {
                                                way += 0;
                                                dead = true;
                                                DeadPlace = new PointF(u[i].TP[j].x, u[i].TP[j].y);
                                                Her[(int)u[i].TP[j].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, true, u[i].team);
                                                if (Her[(int)u[i].TP[first].ts.TotalSeconds].H[i]!=null)
                                                Her[(int)u[i].TP[first].ts.TotalSeconds].H[i].dead = true;
                                                else
                                                    Her[(int)u[i].TP[first].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, true, u[i].team);
                                            }
                                            else
                                            {
                                                way += r;
                                                Her[(int)u[i].TP[j].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, false, u[i].team);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Если превысили
                                        if (j + 8 < u[i].TP.Count)
                                        {
                                            //И ещё достаточно данных,
                                            //Проверим что это именно смерть и апосля неё не двигается чел
                                            if ((u[i].TP[j].x == u[i].TP[j + 8].x) && (u[i].TP[j].y == u[i].TP[j + 8].y))
                                            {
                                                way += 0;
                                                dead = true;
                                                DeadPlace = new PointF(u[i].TP[j].x,u[i].TP[j].y);
                                                Her[(int)u[i].TP[j].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, true, u[i].team);
                                            }
                                            else
                                            {
                                                if ((u[i].TP[j + 5].ts - u[i].TP[j].ts).TotalSeconds>10)
                                                {
                                                    way += 0;
                                                    dead = true;
                                                    DeadPlace = new PointF(u[i].TP[j].x, u[i].TP[j].y);
                                                    Her[(int)u[i].TP[j].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, true, u[i].team);
                                                }
                                                else
                                                {
                                                way += r;
                                                Her[(int)u[i].TP[j].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, false, u[i].team);
                                                }
                                            }
                                        }
                                        else
                                            Her[(int)u[i].TP[j].ts.TotalSeconds].H[i] = new Heroy(new System.Drawing.Point(u[i].TP[j].x, u[i].TP[j].y), u[i].Player, u[i].Name, true, u[i].team);
                                    }

                                    if (u[i].TP[j + 1].ts.TotalSeconds - u[i].TP[j].ts.TotalSeconds != 0)
                                        first = j;
                                }
                                Table[u[i].team].Dist.Add(new Player(u[i].Player, way));
                                if (u[i].team == 0)
                                {
                                    if (inthread==0)
                                    listBox1.Items.Add(u[i].Player + " " + u[i].Name + " " + way.ToString());
                                    ListForNames1[li1] = i;
                                    li1++;
                                    TeamWay[u[i].team] += way;
                                    
                                }
                                else
                                {
                                    if (inthread == 0)
                                    listBox2.Items.Add(u[i].Player + " " + u[i].Name + " " + way.ToString());
                                    ListForNames2[li2] = i;
                                    li2++;
                                    TeamWay[u[i].team] += way;
                                }
                                HerouMarchingWay[i] = way;
                            }
                            if (inthread == 0)
                            { 
                            label7.Text = TeamWay[0].ToString();
                            label6.Text = TeamWay[1].ToString();
                    }
                        //Дозаполним пропуски
                        for (int i=0;i<Her.Length-1;i++)
                        {
                           for (int j=0;j<Her[i].H.Length;j++)
                           {
                               if (Her[i].H[j] != null)
                               {
                                   int k = i + 1;
                                   while ((k < Her.Length) && (Her[k].H[j] == null))
                                   {
                                       Her[k].H[j] =new Heroy(Her[i].H[j]);
                                   }
                               }
                           }
                        }
                        for (int i = 0; i < Her.Length; i++)
                        {
                            if (Her[i].findMO())
                                if (Her[i].findDisp())
                                    Her[i].findHeroR();
                        }
                        //Bitmap BMP = new Bitmap(200, Her.Length);
                        List<PointF> f = new System.Collections.Generic.List<PointF>();
                        List<PointF> f2 = new System.Collections.Generic.List<PointF>();
                        for (int i = 0; i < Her.Length; i++)
                            if (Her[i].H[0]!=null)
                           //     if (!Her[i].H[0].dead)
                                {
                                //    f.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1 - Her[i].HeroR[0])));
                                   // f2.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1 - Her[i].HeroR[6])));
                                    f.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1-Math.Sqrt(Her[i].D0.X * Her[i].D0.X + Her[i].D0.Y * Her[i].D0.Y))));
                                    f2.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1 - Math.Sqrt(Her[i].D1.X * Her[i].D1.X + Her[i].D1.Y * Her[i].D1.Y))));
                                }
                        for (int i = 0; i < Her.Length; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                HerouMarching[j] += Her[i].HeroR[j];

                            }
                            SumOfDisp[0] += Math.Sqrt(Her[i].D0.X * Her[i].D0.X + Her[i].D0.Y * Her[i].D0.Y);
                            SumOfDisp[1] += Math.Sqrt(Her[i].D1.X * Her[i].D1.X + Her[i].D1.Y * Her[i].D1.Y);
                            
                        }
                        for (int i = 0; i < 10; i++)
                        {
                            if (Her[100] != null)
                                if (Her[100].H[i] != null)
                                    Table[Her[100].H[i].team].Disp.Add(new Player(Her[100].H[i].MyName, HerouMarching[i]));
                                

                        }
                        if (inthread == 0)
                        {
                            label1.Text = SumOfDisp[0].ToString();
                            label2.Text = SumOfDisp[1].ToString();
                        }
                      //  Graphics g = pictureBox1.CreateGraphics();
                        Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                        using (Graphics g = Graphics.FromImage(b))
                        {
                            g.DrawLines(new Pen(Color.Red, 1), f.ToArray());
                            g.DrawLines(new Pen(Color.Blue, 1), f2.ToArray());
                        }
                        if (inthread == 0)
                        pictureBox1.Image = b;
                    //    b.Save("b.bmp");
                    }
                }
                Table[0].calculate();
                Table[1].calculate();


            }
            finally
            {
                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);
            }
        }

        private static byte[] GetMpqArchiveFileBytes(CArchive archive, string archivedFileName)
        {
            var buffer = new byte[archive.FindFiles(archivedFileName).Single().Size];
            archive.ExportFile(archivedFileName, buffer);
            return buffer;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ParceReply(ofd.FileName);//@"C:\Users\Anton\Documents\Heroes of the Storm\Accounts\113017754\2-Hero-1-410976\Replays\Multiplayer\Проклятая лощина (19).StormReplay");
            }
        }
        
        private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            int Selected = ListForNames1[listBox1.SelectedIndex];
            List<PointF> f = new System.Collections.Generic.List<PointF>();
          //  List<PointF> f2 = new System.Collections.Generic.List<PointF>();
            for (int i = 0; i < Her.Length; i++)
                if (Her[i].H[Selected] != null)
                 //   if (!Her[i].H[Selected].dead)
                    {
                        f.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height - 1 - Her[i].HeroR[Selected])));
                       // f2.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height - 1 - Her[i].HeroR[6])));
                        //     f.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1-Math.Sqrt(Her[i].D0.X * Her[i].D0.X + Her[i].D0.Y * Her[i].D0.Y))));
                        //       f2.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1 - Math.Sqrt(Her[i].D1.X * Her[i].D1.X + Her[i].D1.Y * Her[i].D1.Y))));
                    }
                   // else
                   // {
                   //     f.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length,0));
                   // }
            label3.Text = HerouMarching[Selected].ToString();
            label8.Text = HerouMarchingWay[Selected].ToString();
            //  Graphics g = pictureBox1.CreateGraphics();
            Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawLines(new Pen(Color.Red, 1), f.ToArray());
             //   g.DrawLines(new Pen(Color.Blue, 1), f2.ToArray());
            }

            pictureBox1.Image = b;
        //    b.Save("b.bmp");
        }

        private void listBox2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            int Selected = ListForNames2[listBox2.SelectedIndex];
            List<PointF> f = new System.Collections.Generic.List<PointF>();
            //  List<PointF> f2 = new System.Collections.Generic.List<PointF>();
            for (int i = 0; i < Her.Length; i++)
                if (Her[i].H[Selected] != null)
                //    if (!Her[i].H[Selected].dead)
                    {
                        f.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height - 1 - Her[i].HeroR[Selected])));
                        // f2.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height - 1 - Her[i].HeroR[6])));
                        //     f.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1-Math.Sqrt(Her[i].D0.X * Her[i].D0.X + Her[i].D0.Y * Her[i].D0.Y))));
                        //       f2.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1 - Math.Sqrt(Her[i].D1.X * Her[i].D1.X + Her[i].D1.Y * Her[i].D1.Y))));
                    }
            label3.Text = HerouMarching[Selected].ToString();
            label8.Text = HerouMarchingWay[Selected].ToString();
            //  Graphics g = pictureBox1.CreateGraphics();
            Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawLines(new Pen(Color.Red, 1), f.ToArray());
                //   g.DrawLines(new Pen(Color.Blue, 1), f2.ToArray());
            }

            pictureBox1.Image = b;
      //      b.Save("b.bmp");
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            List<PointF> f = new System.Collections.Generic.List<PointF>();
            List<PointF> f2 = new System.Collections.Generic.List<PointF>();
            for (int i = 0; i < Her.Length; i++)
                if (Her[i].H[0] != null)
                //     if (!Her[i].H[0].dead)
                {
                    //    f.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1 - Her[i].HeroR[0])));
                    // f2.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height-1 - Her[i].HeroR[6])));
                    f.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height - 1 - Math.Sqrt(Her[i].D0.X * Her[i].D0.X + Her[i].D0.Y * Her[i].D0.Y))));
                    f2.Add(new PointF((float)pictureBox1.Width * i / (float)Her.Length, (float)(pictureBox1.Height - 1 - Math.Sqrt(Her[i].D1.X * Her[i].D1.X + Her[i].D1.Y * Her[i].D1.Y))));
                }
            Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawLines(new Pen(Color.Red, 1), f.ToArray());
                g.DrawLines(new Pen(Color.Blue, 1), f2.ToArray());
            }

            pictureBox1.Image = b;
        }
        public class HeroHistory
        {
            public string Name;
            public int[] PosDisp;// List<int> PosDisp;
            public int[] PosDist;// List<int> PosDist;
            public HeroHistory(string N)
            {
                Name = N;
                PosDisp = new int[5];//new System.Collections.Generic.List<int>();
                PosDist = new int[5]; //new System.Collections.Generic.List<int>();
            }
            
        }
        List<HeroHistory> HH = new System.Collections.Generic.List<HeroHistory>();
        int inthread = 0;
        private void button3_Click(object sender, System.EventArgs e)
        {
             FolderBrowserDialog FSD = new FolderBrowserDialog();
            FSD.SelectedPath = Environment.CurrentDirectory;
            if (FSD.ShowDialog() == DialogResult.OK)
            {
                FILENAME = FSD.SelectedPath;
                backgroundWorker1.RunWorkerAsync();
            }
        }
        string FILENAME = "";
        int filescount = 0;
        int filesnumber = 0;
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            inthread = 1;
            string[] NA = File.ReadAllLines("NAMES.txt");
            List<string> Names = new System.Collections.Generic.List<string>();
            for (int i = 0; i < NA.Length;i++ )
            {
                Names.Add(NA[i]);
            }
                /* Names.Add("Wormkeeper");
                 Names.Add("harphazard");
                 Names.Add("Rulock");
                 Names.Add("Deyn");
                 Names.Add("Morganall");
                 Names.Add("lachancie");
                 Names.Add("zeppelin");
                 Names.Add("Коля");
                 Names.Add("DarkFlame");*/
                for (int i = 0; i < Names.Count; i++)
                {
                    HH.Add(new HeroHistory(Names[i]));
                }

           
                DirectoryInfo d = new DirectoryInfo(FILENAME);
                FileInfo[] FI = d.GetFiles();
                filescount = FI.Length;
                for (int i = 0; i < FI.Length; i++)
                {
                    filesnumber = i;
                    ParceReply(FI[i].FullName);
                    if ((Table[0].Dist.Count == 5)&&(Table[0].Disp.Count == 5))
                        if ((Table[1].Dist.Count == 5) && (Table[1].Disp.Count == 5))
                        for (int k = 0; k < 2; k++)
                            for (int j = 0; j < 5; j++)
                                for (int l = 0; l < Names.Count; l++)
                                {
                                    if (Table[k].Disp[j].Name == HH[l].Name)
                                    {
                                        HH[l].PosDisp[j]++;//.Add(j);
                                    }
                                    if (Table[k].Dist[j].Name == HH[l].Name)
                                    {
                                        HH[l].PosDist[j]++;//.Add(j);
                                    }
                                }

                }
                for (int i = 0; i < Names.Count; i++)
                {
                    string s = HH[i].Name + ".txt";
                    if (File.Exists(s))
                        File.Delete(s);
                    for (int j = 0; j < HH[i].PosDisp.Length;j++)//.Count; j++)
                    {
                        File.AppendAllText(s, HH[i].PosDisp[j].ToString() + " " + HH[i].PosDist[j].ToString() + "\r\n");
                    }
                }
                inthread = 0;
            
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if (inthread==1)
            {
                this.Text = filescount.ToString() +" "+ filesnumber.ToString();
            }

        }
    }
}
