using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SlidingWindow
{
    public  class NumericBox : TextBox
    {
        #region DependencyProperty
        private const double CURVALUE = 0; //当前值
        private const double MINVALUE = 0; //最小值
        private const double MAXVALUE = 99; //最大值
        private const int DIGITS = 15; //小数点精度

        public static readonly DependencyProperty CurValueProperty;
        public static readonly DependencyProperty MinValueProperty;
        public static readonly DependencyProperty MaxValueProperty;
        public static readonly DependencyProperty DigitsProperty;

        public double CurValue
        {
            get
            {
                return (double)GetValue(CurValueProperty);
            }
            set
            {
                double v = value;
                if (value < MinValue)
                {
                    v = MinValue;
                }
                else if (value > MaxValue)
                {
                    v = MaxValue;
                }
                v = Math.Round(v, Digits);

                SetValue(CurValueProperty, v);
                // if do not go into OnCurValueChanged then force update ui
                if (v != value)
                {
                    this.Text = v.ToString();
                }
            }
        }
        public double MinValue
        {
            get
            {
                return (double)GetValue(MinValueProperty);
            }
            set
            {
                SetValue(MinValueProperty, value);
            }
        }
        public double MaxValue
        {
            get
            {
                return (double)GetValue(MaxValueProperty);
            }
            set
            {
                SetValue(MaxValueProperty, value);
            }
        }
        public int Digits
        {
            get
            {
                return (int)GetValue(DigitsProperty);
            }
            set
            {
                int digits = value;
                if (digits <= 0)
                {
                    digits = 0;
                }
                if (digits > 15)
                {
                    digits = 15;
                }
                SetValue(DigitsProperty, value);
            }
        }

        static NumericBox()
        {
            FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(CURVALUE, new PropertyChangedCallback(OnCurValueChanged));
            CurValueProperty = DependencyProperty.Register("CurValue", typeof(double), typeof(NumericBox), metadata);

            metadata = new FrameworkPropertyMetadata(MINVALUE, new PropertyChangedCallback(OnMinValueChanged));
            MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(NumericBox), metadata);

            metadata = new FrameworkPropertyMetadata(MAXVALUE, new PropertyChangedCallback(OnMaxValueChanged));
            MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(NumericBox), metadata);

            metadata = new FrameworkPropertyMetadata(DIGITS, new PropertyChangedCallback(OnDigitsChanged));
            DigitsProperty = DependencyProperty.Register("Digits", typeof(int), typeof(NumericBox), metadata);
        }

        private static void OnCurValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            double value = (double)e.NewValue;
            NumericBox numericBox = (NumericBox)sender;
            numericBox.Text = value.ToString();
        }
        private static void OnMinValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            double minValue = (double)e.NewValue;
            NumericBox numericBox = (NumericBox)sender;
            numericBox.MinValue = minValue;
        }
        private static void OnMaxValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            double maxValue = (double)e.NewValue;
            NumericBox numericBox = (NumericBox)sender;
            numericBox.MaxValue = maxValue;
        }
        private static void OnDigitsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            int digits = (int)e.NewValue;
            NumericBox numericBox = (NumericBox)sender;
            numericBox.CurValue = Math.Round(numericBox.CurValue, digits);
            numericBox.MinValue = Math.Round(numericBox.MinValue, digits);
            numericBox.MaxValue = Math.Round(numericBox.MaxValue, digits);
        }
        #endregion

        public NumericBox()
        {
            this.TextChanged += NumericBox_TextChanged;
            this.PreviewKeyDown += NumericBox_KeyDown;
            this.LostFocus += NumericBox_LostFocus;
            DataObject.AddPastingHandler(this, NumericBox_Pasting);
        }

        void NumericBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumericBox numericBox = sender as NumericBox;
            if (string.IsNullOrEmpty(numericBox.Text))
            {
                return;
            }

            TrimZeroStart();

            double value = MinValue;
            if (!Double.TryParse(numericBox.Text, out value))
            {
                return;
            }

            if (value != this.CurValue)
            {
                this.CurValue = value;
            }
        }

        void NumericBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (IsControlKeys(key))
            {
                return;
            }
            else if (IsDigit(key))
            {
                return;
            }
            else if (IsSubtract(key)) //-
            {
                TextBox textBox = sender as TextBox;
                string str = textBox.Text;
                if (str.Length > 0 && textBox.SelectionStart != 0)
                {
                    e.Handled = true;
                }
            }
            else if (IsDot(key)) //point
            {
                if (this.Digits > 0)
                {
                    TextBox textBox = sender as TextBox;
                    string str = textBox.Text;
                    if (str.Contains('.') || str == "-")
                    {
                        e.Handled = true;
                    }
                }
                else
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        void NumericBox_LostFocus(object sender, RoutedEventArgs e)
        {
            NumericBox numericBox = sender as NumericBox;
            if (string.IsNullOrEmpty(numericBox.Text))
            {
                numericBox.Text = this.CurValue.ToString();
            }
        }

        private void NumericBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
        }

        private static readonly List<Key> _controlKeys = new List<Key>
                                                             {
                                                                 Key.Back,
                                                                 Key.CapsLock,
                                                                 Key.Down,
                                                                 Key.End,
                                                                 Key.Enter,
                                                                 Key.Escape,
                                                                 Key.Home,
                                                                 Key.Insert,
                                                                 Key.Left,
                                                                 Key.PageDown,
                                                                 Key.PageUp,
                                                                 Key.Right,
                                                                 Key.Tab,
                                                                 Key.Up
                                                             };
        public static bool IsControlKeys(Key key)
        {
            return _controlKeys.Contains(key);
        }

        public static bool IsDigit(Key key)
        {
            bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool retVal;
            if (key >= Key.D0 && key <= Key.D9 && !shiftKey)
            {
                retVal = true;
            }
            else
            {
                retVal = key >= Key.NumPad0 && key <= Key.NumPad9;
            }
            return retVal;
        }


        public static bool IsDot(Key key)
        {
            bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool flag = false;
            if (key == Key.Decimal)
            {
                flag = true;
            }
            if (key == Key.OemPeriod && !shiftKey)
            {
                flag = true;
            }
            return flag;
        }
        public static bool IsSubtract(Key key)
        {
            bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool flag = false;
            if (key == Key.Subtract)
            {
                flag = true;
            }
            if (key == Key.OemMinus && !shiftKey)
            {
                flag = true;
            }
            return flag;
        }

        private void TrimZeroStart()
        {
            if (this.Text.Length == 1)
            {
                return;
            }
            string resultText = this.Text;
            int zeroCount = 0;
            foreach (char c in this.Text)
            {
                if (c == '0') { zeroCount++; }
                else { break; }
            }
            if (zeroCount == 0)
            {
                return;
            }

            if (this.Text.Contains('.'))
            {
                if (this.Text[zeroCount] != '.')
                {
                    resultText = this.Text.TrimStart('0');
                }
                else if (zeroCount > 1)
                {
                    resultText = this.Text.Substring(zeroCount - 1);
                }
            }
            else if (zeroCount > 0)
            {
                resultText = this.Text.TrimStart('0');
            }
        }

    }
}
