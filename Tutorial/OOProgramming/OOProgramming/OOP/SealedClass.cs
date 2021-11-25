using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.OOP
{
    class SealedClass
    {
        public void Main()
        {
            Console.WriteLine("Sealed Classes \n");
            Console.WriteLine(
                "Sealed classes are the reverse of abstract classes. " +
                "While abstract classes are inherited and are refined in the derived class, sealed classes cannot be inherited. " +
                "You can create an instance of a sealed class. A sealed class is used to prevent further refinement through inheritance. \n" +
                "Why Sealed Classes\n? " +
                "The main purpose of a sealed class is to take away the inheritance feature from the class users so they cannot derive a class from it. " +
                "One of the best usage of sealed classes is when you have a class with static members. " +
                "For example, the Pens and Brushes classes of the System.Drawing namespace. " +
                "The Pens class represents the pens with standard colors.This class has only static members." +
                "For example, Pens.Blue represents a pen with blue color.Similarly, the Brushes class represents standard brushes." +
                "The Brushes.Blue represents a brush with blue color. \n" +
                "So when you're designing a class library and want to restrict your classes not to be derived by developers, you may want to use sealed classes.");

            new X().Main();
            new Y().Main();
            new Z().Main();
            Console.WriteLine("------------------------------------------------------------------------");
        }

        public class X
        {
            protected virtual void F() { Console.WriteLine("X.F"); }
            protected virtual void F2() { Console.WriteLine("X.F2"); }

            public void Main()
            {
                F();
                F2();
            }
        }

        public class Y : X
        {
            sealed protected override void F() { Console.WriteLine("Y.F"); }
            protected override void F2() { Console.WriteLine("Y.F2"); }

            public new void Main()
            {
                F();
                F2();
            }
        }

        public class Z : Y
        {
            // Attempting to override F causes compiler error CS0239.
            // protected override void F() { Console.WriteLine("Z.F");

            // Overriding F2 is allowed.
            protected override void F2() { Console.WriteLine("Z.F2"); }

            public new void Main()
            {
                F2();
            }
        }
    }
}
