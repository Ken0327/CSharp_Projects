using System.Windows;
using static Interface.MainWindow;

namespace Interface
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Practice1
        public interface IAction
        {
            //新增
            void Add();
        }

        class StudentRepository : IAction
        {
            void IAction.Add()
            {
                MessageBox.Show("已新增學生");
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            IAction _memberRepository = new MemberRepository();
            _memberRepository.Add();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            IAction _studentRepository = new StudentRepository();
            _studentRepository.Add();
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            IAction _teacherRepository = new TeacherRepository();
            _teacherRepository.Add();
        }

        //Practice2
        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            ImyInterface obj = new MyClass();

            // Call the member.
            obj.SampleMethod();
        }
        interface ImyInterface
        {
            void SampleMethod();
        }

        class MyClass : ImyInterface
        {
            // Explicit interface member implementation: 
            void ImyInterface.SampleMethod()
            {
                MessageBox.Show(nameof(ImyInterface.SampleMethod));
            }
        }

        //Practice3 
        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            Point p = new Point(2, 3);
            MessageBox.Show("My Point: ");
            PrintPoint(p);
        }

        static void PrintPoint(IPoint p)
        {
            MessageBox.Show("x = " + p.x.ToString() + " , y = "+ p.y.ToString());
        }

        interface IPoint
        {
            // Property signatures:
            int x
            {
                get;
                set;
            }

            int y
            {
                get;
                set;
            }
        }

        class Point : IPoint
        {
            // Fields:
            private int _myX;
            private int _myY;

            // Constructor:
            public Point(int a, int b)
            {
                _myX = a;
                _myY = b;
            }
            public int x
            {
                get
                {
                    return _myX;
                }

                set
                {
                    _myX = value;
                }
            }

            public int y
            {
                get
                {
                    return _myY;
                }
                set
                {
                    _myY = value;
                }
            }
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            SampleClass sc = new SampleClass();
            IControl ctrl = (IControl)sc;
            ISurface srfc = (ISurface)sc;

            // The following lines all call the same method.
            sc.Paint();
            ctrl.Paint();
            srfc.Paint();
        }

        interface IControl
        {
            void Paint();
        }
        interface ISurface
        {
            void Paint();
        }
        class SampleClass : IControl, ISurface
        {
            // Both ISurface.Paint and IControl.Paint call this method. 
            public void Paint()
            {
                MessageBox.Show("Paint method in SampleClass");
            }
        }
    }

    class MemberRepository : IAction
    {
        void IAction.Add()
        {
            MessageBox.Show("已新增會員");
        }
    }
}
