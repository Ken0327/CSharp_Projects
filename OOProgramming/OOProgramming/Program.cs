using System;

namespace OOProgramming
{
    class Program
    {
        private delegate void SomeDelegate(string text);

        static void Main(string[] args)
        {
            bool isRun = true;
            while (isRun)
            {
                Console.WriteLine("C# Object-Oriented Programming Demo:");
                Console.WriteLine("(1) Run OOP Programming - AccountingSheet");
                Console.WriteLine("(2) Run Abstract vs Virtual - Comsuption");
                Console.WriteLine("(3) Run Methods parameter: ref, out, params");
                Console.WriteLine("(4) Run Delegate method");
                Console.WriteLine("Please insert number:");
                var result = Console.ReadLine();
                switch (result)
                {
                    case "1":
                        AccountingSheet();
                        break;
                    case "2":
                        ComsumptionHistory();
                        break;
                    case "3":
                        MethodsParameter();
                        break;
                    case "4":
                        RunDelegate();
                        break;
                }
                Console.WriteLine("Do you want to continue? Yes=1, No=0");
                if (Console.ReadLine() !="1")
                {
                    isRun = false;
                }
            }
        }

        private static void AccountingSheet()
        {
            Console.WriteLine("Start (1) Run OOP Programming - AccountingSheet");
            // OOP - Abstraction, Encapsulation, Inheritance ,Polymorphism 
            IntroToClasses();

            // <FirstTests>
            var giftCard = new GiftCardAccount("gift card", 100, 50);
            giftCard.MakeWithdrawal(20, DateTime.Now, "get expensive coffee");
            giftCard.MakeWithdrawal(50, DateTime.Now, "buy groceries");
            giftCard.PerformMonthEndTransactions();
            // can make additional deposits:
            giftCard.MakeDeposit(27.50m, DateTime.Now, "add some additional spending money");
            Console.WriteLine(giftCard.GetAccountHistory());

            var savings = new InterestEarningAccount("savings account", 10000);
            savings.MakeDeposit(750, DateTime.Now, "save some money");
            savings.MakeDeposit(1250, DateTime.Now, "Add more savings");
            savings.MakeWithdrawal(250, DateTime.Now, "Needed to pay monthly bills");
            savings.PerformMonthEndTransactions();
            Console.WriteLine(savings.GetAccountHistory());
            // </FirstTests>

            // <TestLineOfCredit>
            var lineOfCredit = new LineOfCreditAccount("line of credit", 0, 2000);
            // How much is too much to borrow?
            lineOfCredit.MakeWithdrawal(1000m, DateTime.Now, "Take out monthly advance");
            lineOfCredit.MakeDeposit(50m, DateTime.Now, "Pay back small amount");
            lineOfCredit.MakeWithdrawal(5000m, DateTime.Now, "Emergency funds for repairs");
            lineOfCredit.MakeDeposit(150m, DateTime.Now, "Partial restoration on repairs");
            lineOfCredit.PerformMonthEndTransactions();
            Console.WriteLine(lineOfCredit.GetAccountHistory());
            // </TestLineOfCredit>
        }

        private static void IntroToClasses()
        {
            var account = new BankAccount("<name>", 1000);
            Console.WriteLine($"Account {account.Number} was created for {account.Owner} with {account.Balance} balance.");

            account.MakeWithdrawal(500, DateTime.Now, "Rent payment");
            Console.WriteLine(account.Balance);
            account.MakeDeposit(100, DateTime.Now, "friend paid me back");
            Console.WriteLine(account.Balance);

            Console.WriteLine(account.GetAccountHistory());

            // Test that the initial balances must be positive:
            try
            {
                var invalidAccount = new BankAccount("invalid", -55);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine("Exception caught creating account with negative balance");
                Console.WriteLine(e.ToString());
            }

            // Test for a negative balance
            try
            {
                account.MakeWithdrawal(750, DateTime.Now, "Attempt to overdraw");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("Exception caught trying to overdraw");
                Console.WriteLine(e.ToString());
            }
        }

        private static void ComsumptionHistory()
        {
            // Abstract
            Console.WriteLine("Start (2) Run Abstract vs Virtual - Comsuption");
            Man_abstract man_abstract = new Man_abstract();

            man_abstract.consumption(200, (byte)1);

            Woman_abstract woman_abstract = new Woman_abstract();

            woman_abstract.consumption(150, (byte)1);

            foreach (var item in man_abstract.GetconsumptionList())
            {
                Console.Write(item + "\n");
            }
            Console.Write("男生剩下" + man_abstract.getMoney() + "元\n");
            foreach (var item in woman_abstract.GetconsumptionList())
            {
                Console.Write(item + "\n");
            }
            Console.Write("女生剩下" + woman_abstract.getMoney() + "元\n");
            Console.ReadLine();

            // Virtual class
            Man man = new Man();

            man.consumption(200, (byte)1);

            Woman woman = new Woman();

            woman.consumption(150, (byte)1);

            foreach (var item in man.GetconsumptionList())
            {
                Console.Write(item + "\n");
            }
            Console.Write("男生剩下" + man.getMoney() + "元\n");
            foreach (var item in woman.GetconsumptionList())
            {
                Console.Write(item + "\n");
            }
            Console.Write("女生剩下" + woman.getMoney() + "元\n");
            Console.ReadLine();
        }

        private static void MethodsParameter()
        {
            // Methods parameter: ref, out, params
            Console.WriteLine("Start (3) Run Methods parameter: ref, out, params");
            int a = 5;
            int b = 5;
            Add(ref a);
            b = Add2(b);
            Console.Write(a);
            Console.Write(b);
            Console.ReadLine();

            TestClass A = new TestClass();
            Console.WriteLine("初始值x=" + A.x);
            Add(A);
            Console.WriteLine("執行完Add方法後，x=" + A.x);
            Console.ReadLine();

            // add ref
            Console.WriteLine("Add ref parameter:");
            A = new TestClass();
            Console.WriteLine("初始值x=" + A.x);
            Add(ref A);
            Console.WriteLine("執行完Add方法後，x=" + A.x);
            Console.ReadLine();

            Console.WriteLine("Add out parameter:");
            // out:只出不進的方式，當參數加上out代表會由方法內將此參數傳出。
            TestClass B;
            Add2(out B);
            Console.WriteLine("執行完Add方法後，x=" + B.x);
            Console.ReadLine();
        }

        public static void Add(ref int x)
        {
            x = x + 1;
        }

        public static int Add2(int x)
        {
            return x = x + 1;
        }

        public static TestClass Add(TestClass B)
        {
            Console.WriteLine("進入Add方法後，x=" + B.x);
            B.x = 10;
            Console.WriteLine("B.x修改值為10，x=" + B.x);
            B = new TestClass();
            Console.WriteLine("B new新的class，x=" + B.x);
            return B;
        }

        public class TestClass
        {
            public int x = 0;
        }

        public static TestClass Add(ref TestClass B)
        {
            Console.WriteLine("進入Add方法後，x=" + B.x);
            B.x = 10;
            Console.WriteLine("B.x修改值為10，x=" + B.x);
            B = new TestClass();
            Console.WriteLine("B new新的class，x=" + B.x);
            return B;
        }

        public static void Add2(out TestClass B)
        {
            B = new TestClass();
        }

        private static void RunDelegate()
        {
            // 委派：可以將工作加入到某個等待被執行的委派當中，如下程式碼，在SomeDelegate method 代表method是一個委派可以加入相同參數的方法，
            // 如程式碼中的Method1、Method2、Method3，都是丟給method來執行。
            Console.WriteLine("委派：可以將工作加入到某個等待被執行的委派當中，如下程式碼，在SomeDelegate method 代表method是一個委派可以加入相同參數的方法，如程式碼中的Method1、Method2、Method3，都是丟給method來執行。");
            Console.WriteLine("Start (4) Run Delegate method");

            SomeDelegate method = Method1;
            method += Method2;
            method += Method3;
            method.Invoke("HI");
            Console.ReadLine();
        } 

        public static void Method1(string text)
        {
            Console.WriteLine("Show Method1:" + text);
        }
        public static void Method2(string text)
        {
            Console.WriteLine("Show Method2:" + text);
        }
        public static void Method3(string text)
        {
            Console.WriteLine("Show Method3:" + text);
        }
    }
}
