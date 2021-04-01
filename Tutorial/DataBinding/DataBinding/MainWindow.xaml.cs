using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DataBinding
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataBinding();
        }

        int sec = 0;
        bool result;
        private DispatcherTimer timer1 = new DispatcherTimer();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!timer1.IsEnabled)
            {
                timer1.Interval = TimeSpan.FromMilliseconds(1000);
                timer1.Tick += Parsing;
                timer1.IsEnabled = true;
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }

        }


        void Parsing(object sender, EventArgs e)
        {
            sec++;
            m_sec.TextMsg = sec.ToString();
            //second.Text = sec.ToString();
            m_MyData.TextMsg = "資料分析中...";
            var ss = new Random();
            var result = ss.Next(0,2) == 1 ? true : false;
            if (result)
                Pass();
            else
                Fail();

            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    m_MyData.TextMsg = "等待下一筆資料...";
            //})); 
        }

        void Pass()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                m_MyData.TextMsg = "此筆資料 Pass";
            }));
        }

        void Fail()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                m_MyData.TextMsg = "此筆資料 Fail";
            })); 
        }


        private static BindingData m_MyData, m_sec;

        private void DataBinding()
        {
            m_MyData = new BindingData();
            m_sec = new BindingData();
            m_MyData.TextMsg = "處理進度中...";
            Binding BindingTxtBlk = new Binding() { Source = m_MyData, Path = new PropertyPath("TextMsg") };
            Binding BindingTxtBlk1 = new Binding() { Source = m_sec, Path = new PropertyPath("TextMsg") };
            message.SetBinding(TextBlock.TextProperty, BindingTxtBlk);
            second.SetBinding(TextBlock.TextProperty, BindingTxtBlk1);
        }


        public class BindingData : INotifyPropertyChanged
        {
            private string m_TextMsg;

            public string TextMsg
            {
                set
                {
                    if (m_TextMsg != value)
                    {
                        m_TextMsg = value;
                        OnPropertyChanged("TextMsg");
                    }
                }
                get
                {
                    return m_TextMsg;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

    }
}
