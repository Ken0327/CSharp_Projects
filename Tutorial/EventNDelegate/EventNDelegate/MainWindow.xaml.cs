using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EventNDelegate
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate int Multiply(int x, int y);
        public delegate int Timestwo(int x);     // [1] 新增一個叫做timestwo的delegate,其輸入個數 = 1, 輸出個數=1 

        public delegate string SayHi();          // [2] 新增一個叫做SayHi的delegate,其輸入個數 = 0, 輸出個數=1
        public delegate string RandSelect();     // 隨機亂數0~1, 回傳字串

        // 2. 委派是事件的基礎(event)，可以利用委派來呼叫不同的事件，以便觸發其他控制項事件來完成互動性強大的應用程式。
        // 事件語法：
        // public event ClickEventHandler ClickEvent;
        // [存取修飾詞] event 委派名稱 事件名稱(事件變數) ;
        Police p = new Police("台灣隊長");                // 美國隊長  
        Thief thief1 = new Thief("小吳");                 // 小偷1
        Thief thief2 = new Thief("阿肥");                 // 小偷2

        public delegate void SomeDelegate();
    
        public MainWindow()
        {
            InitializeComponent();

            initialEvent();
        }

        private void Test1_Click(object sender, RoutedEventArgs e)
        {
            // 1. 委派可以將方法當成參數來進行傳遞
            // 委派語法：[public|private|protected] Delegate[void | 回傳資料型態] 委派名稱([參數1, 參數2,…]);

            Timestwo timestwo = delegate (int x) { return 2 * x; };
            Multiply multiply = delegate (int x, int y) { return x * y; };

            txt_timestwo.Text = timestwo(5).ToString();
            txt_multiply.Text = multiply(5, 6).ToString();
        }

        private void Test2_Click(object sender, RoutedEventArgs e)
        {
            RandSelect randSelect = delegate ()
            {
                // 定義一個SayHi變數s1, 利用匿名方法初始化之 
                SayHi s1 = delegate () { return "Hello world!"; };
                SayHi s2 = delegate () { return "It's me, Ryan!"; };
                Random c = new Random((int)DateTime.Now.Ticks);
                double x = c.NextDouble();
                string str = x > 0.5 ? s1() : s2();
                return string.Format("{0} \r\n {1}", x, str);
            };

            test2.Text = randSelect();
        }

        private void initialEvent()
        {
            // 如果delegate委派是無參數,無回傳, 則註冊的Method也必須一樣是無參數,無回傳
            // 以下設計一個無參數, 無回傳的委派範例
            // 委派名稱: void PoliceCatchThiefHandler()
            // 事件名稱: PoliceCatchThiefEvent

            // 實例化委託事件: 分別註冊小偷1 & 2快跑RunAway
            // += 相當於Add_PoliceCatchThiefEvent
            p.PoliceCatchThiefEvent += new Police.PoliceCatchThiefHandler(thief1.RunAway);
            p.PoliceCatchThiefEvent += new Police.PoliceCatchThiefHandler(thief2.RunAway);
        }

        private void Test3_Click(object sender, RoutedEventArgs e)
        {
            // 找到壞人, 觸發事件:PoliceCatchThiefEvent
            p.FindBadGuys();
            // Console.Read();
        }

        private async void btn1_Click(object sender, RoutedEventArgs e)
        {
            await RunProcessBar();
        }

        public async Task RunProcessBar()
        {
            var progress = new Progress<double>(value => player1.Value = value);
            double percent = 0;

            await Task.Run(() => {
                for (int i = 0; i < 10; i++)
                {
                    percent = percent + 10;
                    Label1.Dispatcher.Invoke(() =>
                    {
                        Label1.Content = percent.ToString();
                    });
                    ((IProgress<double>)progress).Report(percent);
                    Thread.Sleep(200);
                }
            });
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            Thread _Thread = new Thread(new ThreadStart(SetLabel));
            _Thread.IsBackground = true;
            _Thread.Start();
        }

            void SetLabel()
        {
            //同步方式
            // player2.Dispatcher.Invoke(SetLabel_delegate);
            //非同步
            player2.Dispatcher.BeginInvoke(new SomeDelegate(SetLabel_delegate));
            // player2.Dispatcher.BeginInvoke(new Action(() => { SetLabel_delegate(); }));
        }

        public void SetLabel_delegate()
        {
            var progress = new Progress<double>(value => player2.Value = value);
            double percent = 0;
            for (int i = 0; i < 10; i++)
            {
                percent = percent + 10;
                Label2.Content = percent.ToString();
                ((IProgress<double>)progress).Report(percent);
                Thread.Sleep(200);
            }
            // ((IProgress<double>)progress).Report(0);
        }
    }
}
