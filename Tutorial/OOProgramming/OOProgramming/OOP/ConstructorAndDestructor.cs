using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.OOP
{
    class ConstructorAndDestructor
    {
        public void Main()
        {
            // Constructor
            Console.WriteLine(
                "Constructor and Destructor \n" +
                "A constructor is a specialized function that is used to initialize fields. A constructor has the same name as the class. " +
                "Instance constructors are invoked with the new operator and can't be called in the same manner as other member functions. " +
                "There are some important rules pertaining to constructors as in the following;" +
                "Classes with no constructor have an implicit constructor called the default constructor, that is parameterless. The default constructor assigns default values to fields. \n" +
                
                "1) A public constructor allows an object to be created in the current assembly or referencing assembly.\n" +
                "2) Only the extern modifier is permitted on the constructor.\n" +
                "3) A constructor returns void but does not have an explicitly declared return type.\n" +
                "4) A constructor can have zero or more parameters.\n" +
                "5) Classes can have multiple constructors in the form of default, parameter or both.\n"
                );

            Console.WriteLine("Example shows one constructor for a customer class.");
            // object instantiation
            customer obj = new customer("Barack", "Obama");

            //Method calling
            obj.AppendData();
            Console.WriteLine("------------------------------------------------------------------------");

            // Static Constructor
            Console.WriteLine(
                "Static Constructor \n" +
                "A constructor can be static. You create a static constructor to initialize static fields. " +
                "Static constructors are not called explicitly with the new statement. They are called when the class is first referenced. " +

                "1) There are some limitations of the static constructor as in the following;\n" +
                "2) Static constructors are parameterless. \n" +
                "3) Static constructors can't be overloaded. \n" +
                "4) There is no accessibility specified for Static constructors.\n"
                );

            Console.WriteLine("In the following example the customer class has a static constructor that initializes the static field and this constructor is called when the class is referenced in the Main () at line 26 as in the following: ");

            customer1.getData();
            Console.WriteLine("------------------------------------------------------------------------");

            // Destructors
            Console.WriteLine(
                "Destructors \n" +
                "The purpose of the destructor method is to remove unused objects and resources. " +
                "Destructors are not called directly in the source code but during garbage collection. Garbage collection is nondeterministic. " +
                "A destructor is invoked at an undetermined moment. More precisely a programmer can't control its execution; " +
                "rather it is called by the Finalize () method. Like a constructor, the destructor has the same name as the class except a destructor is prefixed with a tilde (~). " +
                "There are some limitations of destructors as in the following;\n" +

                "1) Destructors are parameterless. \n" +
                "2) A Destructor can't be overloaded. \n" +
                "3) Destructors are not inherited. \n" +
                "4) Destructors can cause performance and efficiency implications. \n"
                );

            Console.WriteLine("The following implements a destructor and dispose method. First of all we are initializing the fields via constructor, " +
                "doing some calculations on that data and displaying it to the console. But at line 9 we are implementing the destructor that is calling a Dispose() method to release all the resources. \n ");

            //instance created
            customer2 obj2 = new customer2();

            obj2.getData();

            Console.WriteLine("------------------------------------------------------------------------");
        }

        class customer
        {
            // Member Variables
            public string Name;

            //constuctor for initializing fields
            public customer(string fname, string lname)
            {
                Name = fname + " " + lname;
            }
            //method for displaying customer records
            public void AppendData()
            {
                Console.WriteLine(Name);
            }
        }

        class customer1
        {
            // Member Variables
            static private int x;

            //constuctor for static initializing fields
            static customer1()
            {
                x = 10;
            }
            //method for get  static field
            static public void getData()
            {
                Console.WriteLine(x);
            }
        }

        class customer2
        {
            // Member Variables
            public int x, y;
            //constuctor for  initializing fields
            public customer2()
            {
                Console.WriteLine("Fields inititalized");
                x = 10;
            }
            //method for get field
            public void getData()
            {
                y = x * x;
                Console.WriteLine(y);
            }
            //method to release resource explicitly
            public void Dispose()
            {
                Console.WriteLine("Fields cleaned");
                x = 0;
                y = 0;
            }
            //destructor
            ~customer2()
            {
                Dispose();
            }
        }
    }
}
