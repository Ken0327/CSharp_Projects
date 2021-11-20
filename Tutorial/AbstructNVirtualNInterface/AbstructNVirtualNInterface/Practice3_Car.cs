using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AbstructNVirtualNInterface
{
    class Practice3_Car
    {
        public class Car0
        {
            public string model; //型號
            public int CC;  //CC數
            public decimal price; //

            public string run()
            {
                return $"{model} run fast.";
            }
        }


        public abstract class Car1
        {
            public string model; //型號
            public int CC;  //CC數
            public decimal price; //

            public string Start()
            {
                return $"Engine start.";
            }

            abstract public string run();
        }

        public class SportsCar1 : Car1
        {
            public int passengers = 4;
            public int Door = 4;
            public string opneDoor()
            {
                return "Side open.";
            }

            public override string run()
            {
                return $"{model} run fast.";
            }
        }

        public class Scooter1 : Car1
        {
            public int passengers = 1;

            public override string run()
            {
                return $"{model} run slow.";
            }
        }


        public abstract class Car2
        {
            public string model; //型號
            public int CC;  //CC數
            public decimal price; //

            public string Start()
            {
                return $"Engine start.";
            }

            abstract public string run();
        }

        public class SportsCar2 : Car2
        {
            public int passengers = 4;
            public int Door = 4;
            public virtual string openDoor()
            {
                return "Side open.";
            }

            public override string run()
            {
                return $"{model} run fast.";
            }
        }

        public class SpecialSportsCar2 : SportsCar2
        {
            public override string openDoor()
            {
                return "Open up.";
            }
        }

        public class Scooter2 : Car2
        {
            public int passengers = 1;

            public override string run()
            {
                return $"{model} run slow.";
            }
        }

        public abstract class Car3
        {
            public string model; //型號
            public int CC;  //CC數
            public decimal price; //售價
            public decimal TaxRate; //稅率
            public string Start()
            {
                return $"Engine start";
            }

            abstract public string run();

            //要繳給國家的稅
            public decimal Tax()
            {
                return price * TaxRate;
            }
        }

        public class SportsCar3 : Car3
        {
            public int passengers = 4;
            public int Door = 4;
            public virtual string openDoor()
            {
                return "Side open.";
            }

            public override string run()
            {
                return $"{model} run fast.";
            }
        }

        public class SpecialSportsCar3 : SportsCar3
        {
            public override string openDoor()
            {
                return "Open up.";
            }
        }

        public class Scooter3 : Car3
        {
            public int passengers = 1;

            public override string run()
            {
                return $"{model} run slow.";
            }
        }

        public interface IProduct
        {
            //string model; --錯誤的寫法。介面不能實作，只能宣告
            //型號
            string model { get; set; }//宣告 model這個屬性需有 get 與 set 兩個方法
            decimal price { get; set; }
            //要繳給國家的稅
            decimal Tax();
        }

        //鞋子
        public class Shoes : IProduct
        {
            //string model; --錯誤的寫法。必須明確實作介面屬性的方法
            /*屬性宣告的完整寫法
            private string _model = "";
            string IProduct.model {
                get {
                    return _model;
                }
                set {
                    _model = value;
                }
            }
            */
            public string model { get; set; } //語法糖。與介面的 string model { get; set; } 是不一樣的意思 
            public decimal price { get; set; } //售價
            public decimal TaxRate; //稅率
            public int Size;
            public decimal Tax()
            {
                return price * TaxRate;
            }
        }

        public abstract class Car4 : IProduct
        {
            public string model { get; set; }
            public int CC;  //CC數
            public decimal price { get; set; }
            public decimal TaxRate; //稅率
            public string Start()
            {
                return $"Engine start";
            }

            abstract public string run();

            //要繳給國家的稅
            public decimal Tax()
            {
                return price * TaxRate;
            }
        }

        public class SportsCar4 : Car4
        {
            public int passengers = 4;
            public int Door = 4;
            public virtual string openDoor()
            {
                return "Side open.";
            }

            public override string run()
            {
                return $"{model} run fast.";
            }
        }

        public class SpecialSportsCar4 : SportsCar4
        {
            public override string openDoor()
            {
                return "Open up.";
            }
        }

        public class Scooter4 : Car4
        {
            public int passengers = 1;

            public override string run()
            {
                return $"{model} run slow.";
            }
        }

    }
}