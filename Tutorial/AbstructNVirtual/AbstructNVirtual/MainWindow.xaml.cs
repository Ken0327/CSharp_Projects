using System;
using System.Windows;

namespace AbstructNVirtual
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Employee e1 = new Employee();
            e1.printName();
            Worker e2 = new Worker();
            e2.printName();

            Employee e3 = new Employee();
            e3.printName();
            Employee e4 = new Worker();
            e4.printName();
        }

        class Employee
        {
            public virtual string Name
            {
                get
                {
                    return "EmployeeName";
                }
                set
                {
                    Name = value;
                }
            }
            public virtual void printName()
            {
                Console.WriteLine("Employee Name is " + Name);
            }
        }

        class Worker : Employee
        {
            public override string Name
            {
                get
                {
                    return "WorkerName";
                }
                set
                {
                    Name = value;
                }
            }
            public override void printName()
            {
                Console.WriteLine("Worker Name is " + Name);
            }
        }
    }
}
