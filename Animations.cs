using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Media;
using System.Runtime.InteropServices;
namespace SlidingWindow
{
    public class Animations
    {
        
        //        private static ThicknessAnimation marginAnimations;
        public const int step = 66;
        public const int path = 397;
        public const int duration = 3;
        public static Thickness temp = new Thickness();
        public static void Up(Image i,int bgt) //控制小球向上移动的动画
        {
            ThicknessAnimation marginAnimations = new ThicknessAnimation
            {
                BeginTime = new TimeSpan(0,0,0,bgt/1000,bgt%1000),
                From = new Thickness(i.Margin.Left, i.Margin.Top, i.Margin.Right, i.Margin.Bottom),
                To = new Thickness(i.Margin.Left, i.Margin.Top - path, i.Margin.Right, i.Margin.Bottom),
                Duration = TimeSpan.FromSeconds(duration),
                FillBehavior = FillBehavior.Stop
            };

            i.BeginAnimation(Image.MarginProperty, marginAnimations,HandoffBehavior.Compose);
        
            //i.BeginAnimation(Image.OpacityProperty, myDoubleAnimation);
            i.Visibility = Visibility.Visible;
            Appear(i, bgt);
        }

        public static void Down(Image i,int bgt)//控制小球向下移动的动画
        {

            ThicknessAnimation marginAnimations = new ThicknessAnimation
            {
                BeginTime = new TimeSpan(0, 0, 0, bgt / 1000, bgt % 1000),
                From = new Thickness(i.Margin.Left, i.Margin.Top, i.Margin.Right, i.Margin.Bottom),
                To = new Thickness(i.Margin.Left, i.Margin.Top + path, i.Margin.Right, i.Margin.Bottom),
                Duration = TimeSpan.FromSeconds(duration),
                FillBehavior = FillBehavior.Stop
            };
            Appear(i, bgt);
            i.BeginAnimation(Image.MarginProperty, marginAnimations,HandoffBehavior.Compose);

 //           Storyboard.SetTargetName(marginAnimations, i.Name);
 //           Storyboard.SetTargetProperty(marginAnimations, new PropertyPath(Image.MarginProperty));
 //           s.Children.Add(marginAnimations);
 //           s.Begin(i);
        }

        public static void Shift(Image i,int bgt,int n) //控制窗口滑动的动画
        {
            //           Storyboard s = new Storyboard();
 
            //           double right = (i.Margin.Right - n* step )< 76 ? 96 : i.Margin.Right - n * step; //防止窗口右侧越界
            double right = (i.Margin.Right - n * step);
            double left = i.Margin.Left + n * step;
            ThicknessAnimation marginAnimations = new ThicknessAnimation
            {
                BeginTime = new TimeSpan(0, 0, 0, bgt / 1000, bgt % 1000),
                From = new Thickness(i.Margin.Left + (n-1) * step, i.Margin.Top, i.Margin.Right - (n - 1) * step, i.Margin.Bottom),
                To = new Thickness(left, i.Margin.Top , right, i.Margin.Bottom),
                Duration = TimeSpan.FromMilliseconds(100),
                FillBehavior = FillBehavior.HoldEnd,
                
            };
            marginAnimations.Completed += (o, s) =>
            {
                temp = new Thickness(left, i.Margin.Top, right, i.Margin.Bottom);
                //i.BeginAnimation(Image.MarginProperty, null);
            };


            i.BeginAnimation(Image.MarginProperty, marginAnimations, HandoffBehavior.Compose);
            //            Storyboard.SetTargetName(marginAnimations, i.Name);
            //            Storyboard.SetTargetProperty(marginAnimations, new PropertyPath(Image.MarginProperty));
            //            s.Children.Add(marginAnimations);
            //            s.Begin(i);
            

          

        }

        public static void TurnGreen(Image i,int bgt) //控制小球变色的动画
        {

            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                BeginTime = new TimeSpan(0, 0, 0, bgt / 1000, bgt % 1000),
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                FillBehavior = FillBehavior.Stop
            };
           
            myDoubleAnimation.Completed += turn;
            i.BeginAnimation(Image.OpacityProperty, myDoubleAnimation,HandoffBehavior.Compose);
            void turn(object sender, EventArgs eventArgs)
            {
                i.Source = (new BitmapImage(new Uri("resources/Greenball.png", UriKind.RelativeOrAbsolute)));
            }
        }

        public static void Lost(Image i,int bgt) //控制小球中途丢失的动画
        {
            int pth = (i.Margin.Top - path) > 0 ? (-path / 2) : (path / 2);
            ThicknessAnimation marginAnimations = new ThicknessAnimation
            {
                BeginTime = new TimeSpan(0, 0, 0, bgt / 1000, bgt % 1000),
                From = new Thickness(i.Margin.Left, i.Margin.Top, i.Margin.Right, i.Margin.Bottom),
                To = new Thickness(i.Margin.Left, i.Margin.Top + pth, i.Margin.Right, i.Margin.Bottom),
                Duration = TimeSpan.FromSeconds(duration),
                FillBehavior = FillBehavior.Stop
            };
            Appear(i, bgt);
            i.BeginAnimation(Image.MarginProperty, marginAnimations,HandoffBehavior.Compose);

   
        }

        public static void Appear(Image i,int bgt) //控制小球出现的动画
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                BeginTime =new TimeSpan(0, 0, 0, bgt / 1000, bgt % 1000),
                From = 0.0,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                FillBehavior = FillBehavior.Stop
            };
            i.BeginAnimation(Image.OpacityProperty, myDoubleAnimation, HandoffBehavior.Compose);
        }

        public static void Disappear(Image i) //控制小球消失的动画
        {
            DoubleAnimation myDoubleAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(10)),
                FillBehavior = FillBehavior.HoldEnd
            };
            i.BeginAnimation(Image.OpacityProperty, myDoubleAnimation, HandoffBehavior.Compose);
        }

        
    }
}
