using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.OOP
{
    class AbstractClass
    {
        public void Main()
        {
            Console.WriteLine("Abstract Classes \n");
            Console.WriteLine(
                "C# allows both classes and functions to be declared abstract using the abstract keyword. " +
                "You can't create an instance of an abstract class. An abstract member has a signature but no function body and they must be overridden in any non-abstract derived class. " +
                "Abstract classes exist primarily for inheritance. Member functions, properties and indexers can be abstract. " +
                "A class with one or more abstract members must be abstract as well. Static members can't be abstract. " +
                "In this example, we are declaring an abstract class Employess with a method displayData() that does not have an implementation." +
                "Then we are implementing the displayData() body in the derived class. " +
                "One point to be noted here is that we have to prefixe the abstract method with the override keyword in the derived class.");

            // class instance
            new test().displayData();

            Console.WriteLine("------------------------------------------------------------------------");
        }

        abstract class Employee
        {
            public abstract void displayData();
        }

        class test: Employee
        {
            public override void displayData()
            {
                Console.WriteLine("Abstract class method");
            }
        }
    }
}
