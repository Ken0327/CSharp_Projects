using OOProgramming.DesignPattern;
using OOProgramming.DesignPattern.ObserverVsDelegateVsEvent;
using OOProgramming.DesignPattern.ObserverVsDelegateVsEvent.Delegate;
using System;
using System.Text;
using static OOProgramming.Constructor;
using static OOProgramming.DesignPattern.Adapter;
using static OOProgramming.DesignPattern.ObserverDesignPattern;
using static OOProgramming.DesignPattern.RegistryofSingletons;
using static OOProgramming.DesignPattern.Singleton;
using static OOProgramming.Property;

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
                Console.WriteLine("(5) Run constructor");
                Console.WriteLine("(6) Run Design Pattern - Facade");
                Console.WriteLine("(7) Run Design Pattern - Adapter");
                Console.WriteLine("(8) Run Design Pattern - Singleton");
                Console.WriteLine("(9) Run Design Pattern - Registry of Singletons");
                Console.WriteLine("(10) Run Design Pattern - Observer Design Pattern");
                Console.WriteLine("(11) Run Design Pattern - Observer Vs Delegate Vs Event");
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
                    case "5":
                        RunConstructor();
                        break;
                    case "6":
                        RunDesignPattern_Facade();
                        break;
                    case "7":
                        RunDesignPattern_Adapter();
                        break;
                    case "8":
                        RunDesignPattern_Singleton();
                        break;
                    case "9":
                        RunDesignPattern_RegistryofSingletons();
                        break;
                    case "10":
                        RunDesignPattern_ObserverDesignPattern();
                        break;
                    case "11":
                        RunDesignPattern_ObserverVsDelegateVsEvent();
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

            var description =
                "OOP - Abstraction, Encapsulation, Inheritance ,Polymorphism";
            Console.WriteLine(description);
            Console.WriteLine("------------------------------------------------------------------------");
            IntroToClasses();

            // <FirstTests>
            Console.WriteLine("------------------------");
            Console.WriteLine("<FirstTests>");
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
            Console.WriteLine("------------------------");
            Console.WriteLine("<TestLineOfCredit>");
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
            account.MakeDeposit(100, DateTime.Now, "friend paid me back");

            Console.WriteLine(account.GetAccountHistory());

            // Test that the initial balances must be positive:
            try
            {
                var invalidAccount = new BankAccount("invalid", -55);
                Console.WriteLine(invalidAccount.Owner + " Account: the initial balances must be positive");
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

            man.consumption(200, (byte)3);

            Woman woman = new Woman();

            woman.consumption(150, (byte)3);

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

        private static void RunConstructor()
        {
            Console.WriteLine("Start (5) Run constructor");
            var description = 
                "建構式: 當一個類別建立時，類別首先呼叫的函式稱為建構式，也可以當作一個類別當在初始化的方法。" + "\n" +
                "建構式有一點特別，列出以下幾點" + "\n" +
                "1. 建構式本身不能繼承。" + "\n" +
                "2. 在class中建構式名稱與class相同 " + "\n" +
                "3. 不具有回傳值 " + "\n" +
                "4. 經過編譯過後視為方法，當class使用時會先利用建構式初始化class";
            Console.WriteLine(description);
            Console.WriteLine("------------------------------------------------------------------------");
            C X = new C();
            Console.WriteLine("------------------------");
            Console.WriteLine("Add base examples:");
            E Y = new E();

            Console.WriteLine("------------------------");
            Console.WriteLine("Polymorphism methods examples:");
            F K = new F();
            Console.WriteLine(K.GetX());
            Console.WriteLine(K.GetY());
            Console.WriteLine("------------------------");
            F J = new F("有參數建構式");
            Console.WriteLine(J.GetX());
            Console.WriteLine(J.GetY());

            Console.ReadLine();
        }

        private static void RunDesignPattern_Facade()
        {
            Console.WriteLine("Start (6) Run Design Pattern - Facade");
            var description =
                "Facade(門面/外觀模式)目的：讓User透過高層的 夾層 來呼叫子系統，降低User對於子系統的依賴，也更輕鬆的操作複雜的子系統。" + "\n" +
                "夾層的實作可以是Class也可以是Interface，主要目的必須隔離User直接操作子系統。" + "\n" +
                "使用Facade的理由:" + "\n" +
                "隔離User對於子系統的依賴，避免發生日後子系統修改導致User也必須跟著修改(連動性問題)。";
            Console.WriteLine(description);
            Console.WriteLine("------------------------------------------------------------------------");
            byte[] bytes = { 3, 2, 5, 4 };
            int i = Facade.ByteToInt(bytes);
            Console.WriteLine(i);
            Console.ReadLine();
        }

        private static void RunDesignPattern_Adapter()
        {
            Console.WriteLine("Start (7) Run Design Pattern - Adapter");
            var description =
                "Adapter(轉接器模式)" + "\n" +
                "目的：將兩個以上不同的介面統一成一個介面，讓User更輕鬆維護。" + "\n" +
                "實際情境中，可能出現架構上已經設計兩套Library在專案中，突然需求需要第三個Library，這時候Adapter模式下只需要將共用介面引用至第三個Library中開發完交付給User，User只有修改new出第三套Libraray所產生的Instance就大功告成了。";
            Console.WriteLine(description);
            Console.WriteLine("------------------------------------------------------------------------");
            //Lib_1
            ICommunication Tunnel = new UdpCommunication();
            //Lib_2
            //ICommunication Tunnel = new TcpCommunication();
            //Lib_3
            //ICommunication Tunnel = new MqttCommunication();

            try
            {
                Tunnel.Connect("192.168.243.1", 3254);
                byte[] sendBuffer = GetSendBuffer();
                Tunnel.Send(sendBuffer);

                byte[] receiveBuffer = Tunnel.Receive();
                Tunnel.Disconnect();
                Console.WriteLine(GetReceiveString(receiveBuffer));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.ReadLine();
        }

        private static void RunDesignPattern_Singleton()
        {
            Console.WriteLine("Start (8) Run Design Pattern - Singleton");
            var description =
                "Singleton(單例/獨立模式)" + "\n" +
                "目的:在程式運作中，永遠只維持一份Instance在系統中，讓所有User取得相同一個Instance。" + "\n" +
                "運用情境1：在開發Socket TCP的系統中，Socket Server的連線數量對公司而言是最直接的成本因素，所以會盡量保持Client端系統永遠只保持一條連線。" + "\n" +
                "運用情境2：對於DB讀取一份資料屬於長期不變動的資料，一般系統都會做Cache，減少對DB的負擔。";
            Console.WriteLine(description);
            Console.WriteLine("------------------------------------------------------------------------");
            SocketClass.SocketObject.Connect("10.124.41.57", 3254);
            byte[] sendBuffer = GetSendBuffer();
            SocketClass.SocketObject.Send(sendBuffer);

            byte[] receiveBuffer = SocketClass.SocketObject.Receive();
            SocketClass.SocketObject.Disconnect();
            Console.WriteLine(GetReceiveString(receiveBuffer));

            Console.WriteLine("Done. 請按任意鍵繼續");
            Console.ReadLine();
        }

        private static void RunDesignPattern_RegistryofSingletons()
        {
            Console.WriteLine("Start (9) Run Design Pattern - Registry of Singletons");
            var description =
                "Registry of Singletons (Multiton pattern)" + "\n" +
                "目的：共同管理單例的Instance。" + "\n" +
                "系統運作中，單例的Instance非常多，變成須要有一個共通的地方來管理會使得取出Instance更加簡便。";
            Console.WriteLine(description);
            Console.WriteLine("------------------------------------------------------------------------");
            var o1 = SingletonRegistry.GetInstance<Class1>();
            var o2 = SingletonRegistry.GetInstance<Class1>();
            var o3 = SingletonRegistry.GetInstance<Class2>();
            Console.WriteLine(o1);
            Console.WriteLine(o2);
            Console.WriteLine(o3);

            Console.WriteLine("Compare whether o1 and o2 are equal?");
            Console.WriteLine(object.ReferenceEquals(o1, o2));
            Console.WriteLine("Compare whether o1 and o3 are equal?");
            Console.WriteLine(object.ReferenceEquals(o1, o3));

            Console.WriteLine("Done. 請按任意鍵繼續");
            Console.ReadLine();
        }

        private static byte[] GetSendBuffer()
        {
            string data = "Hi Tunnel!";
            return Encoding.UTF8.GetBytes(data);
        }

        public static string GetReceiveString(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
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

        private static void RunDesignPattern_ObserverDesignPattern()
        {
            Console.WriteLine("Start (10) Run Design Pattern - Observer Design Pattern");
            var description =
                "The observer design pattern enables a subscriber to register with and receive notifications from a provider. It is suitable for any scenario that requires push-based notification." + "\n" +
                "目的：觀察者模式是軟體設計模式的一種。在此種模式中，一個目標物件管理所有相依於它的觀察者物件，並且在它本身的狀態改變時主動發出通知。這通常透過呼叫各觀察者所提供的方法來實現。此種模式通常被用來實時事件處理系統。";
            Console.WriteLine(description);
            Console.WriteLine("------------------------------------------------------------------------");
            BaggageHandler provider = new BaggageHandler();
            ArrivalsMonitor observer1 = new ArrivalsMonitor("BaggageClaimMonitor1");
            ArrivalsMonitor observer2 = new ArrivalsMonitor("SecurityExit");

            provider.BaggageStatus(712, "Detroit", 3);
            Console.WriteLine("BaggageClaimMonitor1 Subscribe");
            observer1.Subscribe(provider);
            provider.BaggageStatus(712, "Kalamazoo", 3);
            provider.BaggageStatus(400, "New York-Kennedy", 1);
            provider.BaggageStatus(712, "Detroit", 3);
            Console.WriteLine("SecurityExit Subscribe");
            observer2.Subscribe(provider);
            provider.BaggageStatus(511, "San Francisco", 2);
            provider.BaggageStatus(712);
            Console.WriteLine("SecurityExit Unsubscribe");
            observer2.Unsubscribe();
            provider.BaggageStatus(400);
            provider.LastBaggageClaimed();

            Console.WriteLine("Done. 請按任意鍵繼續");
            Console.ReadLine();
        }


        public static void RunDesignPattern_ObserverVsDelegateVsEvent()
        {
            Console.WriteLine("Start (11) Run Design Pattern - Observer Vs Delegate Vs Event");
            var description =
                "The observer design pattern enables a subscriber to register with and receive notifications from a provider. It is suitable for any scenario that requires push-based notification." + "\n" +
                "目的：觀察者模式是軟體設計模式的一種。在此種模式中，一個目標物件管理所有相依於它的觀察者物件，並且在它本身的狀態改變時主動發出通知。這通常透過呼叫各觀察者所提供的方法來實現。此種模式通常被用來實時事件處理系統。" + "\n" +
                "模擬情境: 假設你的公司開發了一個溫度監測的機器，可以連接到各種裝置上；公司希望其他外部開發人員也可以利用這個溫度監測器來撰寫自己的應用程式；而你的工作就是開發一組.Net的SDK，當偵測到溫度變化時，能立即讓所有使用SDK的程式收到即時的溫度通知。";
            Console.WriteLine(description);
            Console.WriteLine("------------------------------------------------------------------------");

            Console.WriteLine("(1) Use Observer Pattern");
            Observer.Execute();

            Console.WriteLine("(2) Use Delegate");
            DelegateMethod.Execute();

            Console.WriteLine("(3) Use Event");
            EventMethod.Execute();

            Console.WriteLine("Done. 請按任意鍵繼續");
            Console.ReadLine();
        }
    }
}
