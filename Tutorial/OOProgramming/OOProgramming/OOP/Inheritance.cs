using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.OOP
{
    class Inheritance
    {
        public void Main()
        {
            Console.WriteLine("Inheritance \n");
            Console.WriteLine(
                "Inheritance is the process by which one object can acquire the properties of another object. " +
                "Inheritance involves a base class and a derived class. The derived class inherits from the base class and also can override inherited members as well as add new members to extend the base class. " +
                "A base type represents the generalization, whereas a derived type represents a specification of an instance. \n" +
                "Classes can inherit from a single class and one or more interfaces. " +
                "When inheriting from a class, the derived class inherits the members including the code of the base class. " +
                "The important point to remember is that Constructors and Destructors are not inherited from the base class.");

            Father fObj = new Father();
            fObj.FatherMethod();

            //Here Child object can access both class methods
            Child cObj = new Child();
            cObj.FatherMethod();
            cObj.ChildMethod();

            Console.WriteLine("------------------------------------------------------------------------");

            Console.WriteLine("Constructor in Inheritance \n");
            Console.WriteLine(
                "Constructors in a base class are not inherited in a derived class. A derived class has a base portion and derived portion. " +
                "The base portion initializes the base portion, and the constructor of the derived class initializes the derived portion. \n" +
                "Accessibility modifier classname(parameterlist1) : base(parameterlist2) { body } \n" +
                "So the base keyword refers to the base class constructor, while parameterlist2 determines which overloaded base class constructor is called. " +
                "In the following example, the Child class's constructor calls the single-argument constructor of the base Father class;");

            //Here Child object can access both class methods
            Child1 cObj1 = new Child1();
            cObj1.FatherMethod();
            cObj1.ChildMethod();
            Child1 cObj2 = new Child1("Kid");
            cObj2.FatherMethod();
            cObj2.ChildMethod();

            Console.WriteLine("------------------------------------------------------------------------");
        }        
        
        //Base Class
        public class Father
        {
            public void FatherMethod()
            {
                Console.WriteLine("this property belong to Father");
            }
        }

        //Derived class
        public class Child : Father
        {
            public void ChildMethod()
            {
                Console.WriteLine("this property belong to Child");
            }
        }

        //Base Class
        public class Father1
        {
            //constructor
            public Father1()
            {
                Console.WriteLine("Father class constructor");
            }

            public Father1(string name)
            {
                Console.WriteLine(name + " Father class constructor");
            }

            public void FatherMethod()
            {
                Console.WriteLine("this property belong to Father");
            }
        }

        //Derived class
        public class Child1 : Father1
        {
            public Child1() : base()
            {
                Console.WriteLine("child class constructor");
            }

            public Child1(string name) : base(name) // base() or base(name)
            {
                Console.WriteLine(name + " child class constructor");
            }

            public void ChildMethod()
            {
                Console.WriteLine("this property belong to Child");
            }
        }
    }
}
