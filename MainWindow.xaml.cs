
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SlidingWindow
{
   
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    
    public partial class MainWindow : Window
    {
        public const int step = 66;

        private static int cnt = 0;
        static Dictionary<int,Window> pList = new Dictionary<int, Window>();
        private DataBinding binder = new DataBinding();
        private static Result[] results;
        private System.Timers.Timer timer;

        public MainWindow()
        {         
            this.Title = "SlidingWindow";
            pList.Add(cnt,this);
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            this.DataContext = binder;
        }  

        public static void End(Image i, int bgt)
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                BeginTime = new TimeSpan(0, 0, 0, bgt / 1000, bgt % 1000),
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(2)),
                FillBehavior = FillBehavior.Stop
            };
            //myDoubleAnimation.Completed += ReStart;
            i.BeginAnimation(Image.OpacityProperty, myDoubleAnimation);

            
        }
        public static void Restart(object sender, EventArgs eventArgs)
        {
            cnt++;
            var tw = new MainWindow();
            tw.Show();
            Thread.Sleep(3000);
            pList[cnt-1].Close();


        }

        private void Start(object sender, RoutedEventArgs e)
        {

            double loss = LossRate.CurValue;//丢包率
            binder.Loss = loss;
            start.IsEnabled = false;
            //IsEnabled = false;
            LossRate.Visibility = Visibility.Hidden;
            ShowLoss.Visibility = Visibility.Visible;
            bool isFinish = false;
            results = new Simulator((int)loss, 4).Simulate(ref isFinish);
            Image[] SList = { S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14 };
            Image[] RList = { R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12, R13, R14 };
            Image[] DList = { D1, D2, D3, D4, D5, D6, D7, D8, D9, D10, D11, D12, D13, D14 };
            Image[] AList = { A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14 };
            Image[] LList = { L1, L2, L3, L4, L5, L6, L7, L8, L9, L10, L11, L12, L13, L14 };
            int i = 0, stps = 0;
            int s = 0, r = 0;
            int finish_count = 0;
            
            
 //           LossRate.DataContext = binder;



            Result result;

            while (true)
            {
                
                timer = new System.Timers.Timer(1000);
                timer.Enabled = true;
                timer.Elapsed += new System.Timers.ElapsedEventHandler((oo, ss) => {
                    stps++;
                    binder.Steps = stps/25;
                });
                timer.Start();
               
                if (results[i].seq > 13)
                {
                    i++;
                    continue;
                }

                result = results[i];

                switch (result.move)
                {
                    case Move.SEND:
                        Animations.Up(DList[result.seq], (int)result.time_start * 10);

                        break;
                    case Move.SENDACK:
                        Animations.Down(AList[result.seq], (int)result.time_start * 10);
                        break;
                    case Move.HANDIN:

                        break;
                    case Move.RCVACK:
                        Animations.TurnGreen(SList[result.seq], (int)result.time_start * 10);
                        break;
                    case Move.R_SLIDE:
                        r++;
                        r = r > 13 ? 13 : r;
                        Animations.Shift(ReceiverWindow, (int)result.time_start * 10,r);                 
                        break;
                    case Move.S_SLIDE:
                        s++;finish_count++;
                        s = s > 13 ? 13 : s;
                        Animations.Shift(SenderWindow, (int)result.time_start * 10,s);
 
                        break;
                    case Move.RECEIVE:
                        Animations.TurnGreen(RList[result.seq], (int)result.time_start * 10);
                        break;
                    case Move.LOST:
                        Animations.Lost(LList[result.seq], (int)result.time_start * 10);
                        break;
                    case Move.ACKLOST:
                        Animations.Lost(AList[result.seq], (int)result.time_start * 10);
                        break;
                }

                if (finish_count>=14||i>1900)break;                
                i++;
            }
            End(success, (int)results[i].time_start * 10);
        }



        private void Exit(object sender, RoutedEventArgs e)
        {
            Restart(sender, e);
        }
    }
}
