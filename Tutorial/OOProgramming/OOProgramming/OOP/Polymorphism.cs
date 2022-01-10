using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.OOP
{
    class Polymorphism
    {
        public void Main()
        {
            Console.WriteLine("Polymorphism \n");
            Console.WriteLine(
                "Polymorphism is the ability to treat the various objects in the same manner. It is one of the significant benefits of inheritance. \n" +
                "Compile-time polymorphism \n" +
                "Compile-time polymorphism can be achieved by using method overloading, and it is also called early binding or static binding." +
                
                
                "We can decide the correct call at runtime based on the derived type of the base reference. This is called late binding. \n" +

                "Run time polymorphism \n" +
                "Run time polymorphism can be achieved by using method overriding, and it is also called late binding or dynamic binding." +

                "In the following example, instead of having a separate routine for the hrDepart, itDepart and financeDepart classes, " +
                "we can write a generic algorithm that uses the base type functions.The method LeaderName() declared in the base abstract class is redefined as per our needs in 2 different classes.");

            Console.WriteLine("\n Compile-time polymorphism = early binding by method overloading");

            var c = new CompileTimePoly();
            c.AddNumbers(1, 2);
            c.AddNumbers(1, 2, 3);

            Console.WriteLine("\n Run-time polymorphism = late binding");

            hrDepart obj1 = new hrDepart();
            itDepart obj2 = new itDepart();
            financeDepart obj3 = new financeDepart();

            obj1.LeaderName();
            obj2.LeaderName();
            obj3.LeaderName();

            Console.WriteLine("------------------------------------------------------------------------");
        }

        public class CompileTimePoly
        {
            public void AddNumbers(int a, int b)
            {
                Console.WriteLine("a + b = {0}", a + b);
            }
            public void AddNumbers(int a, int b, int c)
            {
                Console.WriteLine("a + b + c = {0}", a + b + c);
            }
        }

        public abstract class Employee
        {
            public virtual void LeaderName()
            {
                Console.WriteLine("Mr. K");
            }
        }

        public class hrDepart : Employee
        {
            public override void LeaderName()
            {
                Console.WriteLine("Mr. jone");
            }
        }
        public class itDepart : Employee
        {
            public override void LeaderName()
            {
                Console.WriteLine("Mr. Tom");
            }
        }

        public class financeDepart : Employee
        {
            public override void LeaderName()
            {
                Console.WriteLine("Mr. Linus");
            }
        }
    }
}
