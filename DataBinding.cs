using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlidingWindow
{
    //Data实现动态绑定接口
    public class DataBinding : INotifyPropertyChanged
    {
        private double loss;
        private int steps;

        public double Loss
        {
            get
            {
                return loss;
            }
            set
            {
                loss = value;
                Notify("Loss");
            }
        }
        public int Steps
        {
            get
            {
                return steps;
            }
            set
            {
                steps = value;
                Notify("Steps");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void Notify(String info)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(info));
        }

    }

}
