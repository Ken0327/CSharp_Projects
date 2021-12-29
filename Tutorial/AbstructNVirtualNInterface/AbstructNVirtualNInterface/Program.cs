using System;
using System.Collections.Generic;
using static AbstructNVirtualNInterface.Practice3_Car;

namespace AbstructNVirtualNInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            // Practice 1 
            // 
            Console.WriteLine("Practice 1 Employee \n Virtual");
            Practice1_Employee.Employee e1 = new Practice1_Employee.Employee();
            e1.printName();
            Practice1_Employee.Worker e2 = new Practice1_Employee.Worker();
            e2.printName();

            // 將Worker的物件塞到Employee裡 則會跑 base class printName
            Practice1_Employee.Employee e3 = new Practice1_Employee.Employee();
            e3.printName();
            Practice1_Employee.Employee e4 = new Practice1_Employee.Worker();
            e4.printName();

            // Using Virtual / override
            Practice1_Employee.Employee1 e5 = new Practice1_Employee.Employee1();
            e5.printName();
            Practice1_Employee.Employee1 e6 = new Practice1_Employee.Worker1();
            e6.printName();
            Console.WriteLine("----------------------------------------------------------------------------------");

            // Practice 2
            Console.WriteLine("Practice 2 Door \n Abstract + Virtual + Interface");
            var dc = new Practice2_Door.DoorController();
            dc.AddDoor(new Practice2_Door.HorizontalDoor());
            dc.AddDoor(new Practice2_Door.VerticalDoor());
            dc.AddDoor(new Practice2_Door.AlarmDoor());
            dc.AddDoor(new Practice2_Door.AutoAlarmDoor());

            dc.OpenDoor();
            Console.WriteLine("----------------------------------------------------------------------------------");

            // Practice 3
            Console.WriteLine("Practice 3 Car (1)");
            var CarA = new Practice3_Car.Car0();
            CarA.model = "Maserati";
            var CarB = new Practice3_Car.Car0() { model = "Gogoro" };
            Console.WriteLine(CarA.run());
            Console.WriteLine(CarB.run());
            Console.WriteLine("--------------------------------------------------");

            Console.WriteLine("Practice 3 Car (2)\n Using Abstract");
            var CarA1 = new Practice3_Car.SportsCar1() { model = "Maserati" };
            var CarB1 = new Practice3_Car.Scooter1() { model = "Gogoro" };
            Console.WriteLine(CarA1.Start() + " " + CarA1.run());
            Console.WriteLine(CarB1.Start() + " " + CarB1.run());
            Console.WriteLine("--------------------------------------------------");

            Console.WriteLine("Practice 3 Car (3)\n Using Abstract + Virtual");
            var CarA2 = new Practice3_Car.SportsCar2() { model = "Maserati" };
            var CarB2 = new Practice3_Car.Scooter2() { model = "Gogoro" };
            var CarC2 = new Practice3_Car.SpecialSportsCar2() { model = "Batmobile" };
            Console.WriteLine(CarA2.Start() + " " + CarA2.run() + " " + "Open door way is " + CarA2.openDoor());
            Console.WriteLine(CarB2.Start() + " " + CarB2.run() + " ");
            Console.WriteLine(CarC2.Start() + " " + CarC2.run() + " " + "Open door way is " + CarC2.openDoor());
            Console.WriteLine("--------------------------------------------------");

            Console.WriteLine("Practice 3 Car (4)add Tax \n Using Abstract + Virtual");
            var CarA3 = new Practice3_Car.SportsCar3();
            CarA3.model = "Maserati";
            CarA3.price = 5000000000;
            CarA3.TaxRate = 0.3M;
            var CarB3 = new Practice3_Car.Scooter3() { model = "Gogoro", price = 30000, TaxRate = 0.1M };
            var CarC3 = new Practice3_Car.SpecialSportsCar3() { model = "Batmobile", price = 3000000000, TaxRate = 0.4M };
            Console.WriteLine($"{CarA3.model} Price：{CarA3.price} Tax：{CarA3.Tax().ToString("N0")}");
            Console.WriteLine($"{CarB3.model} Price：{CarB3.price} Tax：{CarB3.Tax().ToString("N0")}");
            Console.WriteLine($"{CarC3.model} Price：{CarC3.price} Tax：{CarC3.Tax().ToString("N0")}");
            Console.WriteLine("--------------------------------------------------");

            Console.WriteLine("Practice 3 Car (5)add Tax \n Using Abstract + Virtual + Interface");
            var CarA4 = new Practice3_Car.SportsCar4();
            CarA4.model = "Maserati";
            CarA4.price = 5000000000;
            CarA4.TaxRate = 0.3M;
            var CarB4 = new Practice3_Car.Scooter4() { model = "Gogoro", price = 30000, TaxRate = 0.1M };
            var ShoesA = new Practice3_Car.Shoes() { model = "Adidas", price = 3000, TaxRate = 0.07M };
            Console.WriteLine($"{CarA4.model} Price：{CarA4.price} Tax：{CarA4.Tax().ToString("N0")}");
            Console.WriteLine($"{CarB4.model} Price：{CarB4.price} Tax：{CarB4.Tax().ToString("N0")}");
            Console.WriteLine($"{ShoesA.model} Price：{ShoesA.price} Tax：{ShoesA.Tax().ToString("N0")}");
            Console.WriteLine("--------------------------------------------------");

            Console.WriteLine("Practice 3 Car (6)add Tax \n Using Abstract + Virtual + Interface");
            var CarA5 = new Practice3_Car.SportsCar4();
            CarA5.model = "Maserati";
            CarA5.price = 5000000000;
            CarA5.TaxRate = 0.3M;
            var CarB5 = new Practice3_Car.Scooter4() { model = "Gogoro", price = 30000, TaxRate = 0.1M };
            var ShoesA1 = new Practice3_Car.Shoes() { model = "Adidas", price = 3000, TaxRate = 0.07M };

            List<IProduct> proList = new List<IProduct>();
            proList.Add(CarA5);
            proList.Add(CarB5);
            proList.Add(ShoesA1);

            proList.ForEach(o =>
            {
                Console.WriteLine(pritTax(o));
            });
            Console.ReadLine();
        }

        static string pritTax(IProduct product)
        {
            return $"{product.model} Price：{product.price} Tax：{product.Tax().ToString("N0")}";
        }
    }
}
