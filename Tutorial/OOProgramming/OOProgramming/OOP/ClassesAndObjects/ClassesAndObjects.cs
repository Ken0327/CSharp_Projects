using System;

namespace OOProgramming.OOP
{
    class ClassesAndObjects
    {
        //Classes are special kinds of templates from which you can create objects.
        //Each object contains data and methods to manipulate and access that data.
        //The class defines the data and the functionality that each object of that class can contain.

        //A class declaration consists of a class header and body.The class header includes attributes, modifiers, and the class keyword. 
        //The class body encapsulates the members of the class, that are the data members and member functions.The syntax of a class declaration is as follows:

        //    Attributes accessibility modifiers class identifier : baselist { body }

        //Attributes provide additional context to a class, like adjectives; 
        //for example the Serializable attribute.Accessibility is the visibility of the class. The default accessibility of a class is internal. 
        //Private is the default accessibility of class members. The following table lists the accessibility keywords;
        //---------------------------------------------------------------------------------------------------------
        //Keyword                 Description
        //public                  Public class is visible in the current and referencing assembly.
        //private                 Visible inside current class.
        //protected               Visible inside current and derived class.
        //Internal                Visible inside containing assembly.
        //Internal                protected Visible inside containing assembly and descendent of thecurrent class.
        //---------------------------------------------------------------------------------------------------------
        //Modifiers refine the declaration of a class. The list of all modifiers defined in the table are as follows;

        //Modifier                Description
        //sealed                  Class can't be inherited by a derived class.
        //static                  Class contains only static members.
        //unsafe                  The class that has some unsafe construct likes pointers.
        //Abstract                The instance of the class is not created if the Class is abstract.

        //The baselist is the inherited class. By default, classes inherit from the System.Object type.
        //A class can inherit and implement multiple interfaces but doesn't support multiple inheritances.        

        class customer
        {
            // Member Variables
            public int CustID;
            public string Name;
            public string Address;

            //constuctor for initializing fields
            public customer()
            {
                CustID = 1101;
                Name = "Tom";
                Address = "USA";
            }

            //method for displaying customer records (functionality)
            public void displayData()
            {
                Console.WriteLine("Customer=" + CustID);
                Console.WriteLine("Name=" + Name);
                Console.WriteLine("Address=" + Address);
            }
            // Code for entry point
        }

        static class staticDemo
        {
            //static fields
            static int x = 10, y;

            //static method
            static void calcute()
            {
                y = x * x;
                Console.WriteLine(y);
            }
            static void Start(string[] args)
            {
                //function calling directly
                staticDemo.calcute();
            }
        }

        //Entry point
        public void Main()
        {
            Console.WriteLine(
                "Classes are special kinds of templates from which you can create objects. Each object contains data and methods to manipulate and access that data. " +
                "The class defines the data and the functionality that each object of that class can contain. " +
                "A class declaration consists of a class header and body.The class header includes attributes, modifiers, and the class keyword. " +
                "The class body encapsulates the members of the class, that are the data members and member functions.The syntax of a class declaration is as follows:"
                );
            Console.WriteLine("------------------------------------------------------------------------");
            // object instantiation
            customer obj = new customer();

            //Method calling
            obj.displayData();

            //fields calling
            Console.WriteLine(obj.CustID);
            Console.WriteLine(obj.Name);
            Console.WriteLine(obj.Address);
            Console.WriteLine("------------------------------------------------------------------------");

            Console.WriteLine(
                "Partial classes" + "\n" + 
                "Typically, a class will reside entirely in a single file." +
                "However, in situations where multiple developers need access to the same class, then having the class in multiple files can be beneficial." +
                "The partial keywords allow a class to span multiple source files.When compiled, the elements of the partial types are combined into a single assembly. " +
                "There are some rules for defining a partial class as in the following; \n" +
                "1) A partial type must have the same accessibility. \n" +
                "2) Each partial type is preceded with the 'partial' keyword. \n" +
                "3) If the partial type is sealed or abstract then the entire class will be sealed and abstract. \n" +
                "In the following example we are adding two files, partialPart1.cs and partialPart2.cs, and declare a partial class, partialclassDemo, in both classes.");

            //partial class instance
            var obj1 = new PartialClassDemo();
            obj1.method1();
            obj1.method2();
            Console.WriteLine("------------------------------------------------------------------------");

            Console.WriteLine(
                "Static classes " + "\n" +
                "A static class is declared using the 'static' keyword." +
                "If the class is declared as static then the compiler never creates an instance of the class. " +
                "All the member fields, properties and functions must be declared as static and they are accessed by the class name directly not by a class instance object.");

            Console.WriteLine("------------------------------------------------------------------------");
        }
    }
}
