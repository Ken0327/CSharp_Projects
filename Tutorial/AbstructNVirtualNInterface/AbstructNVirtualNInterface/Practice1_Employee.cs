using System;

namespace AbstructNVirtualNInterface
{
    class Practice1_Employee
    {
        public class Employee
        {
            public string Name
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
            public void printName()
            {
                Console.WriteLine("Employee Name is " + Name);
            }
        }

        public class Worker : Employee
        {
            public string Name
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
            public string rank = "C";
            public void printName()
            {
                Console.WriteLine("Worker Name is " + Name);
            }
        }

        public class Employee1
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

        public class Worker1 : Employee1
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