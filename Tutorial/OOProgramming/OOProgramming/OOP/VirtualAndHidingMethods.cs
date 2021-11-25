using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.OOP
{
    class VirtualAndHidingMethods
    {
        public void Main()
        {
            Console.WriteLine("Virtual Methods \n");
            Console.WriteLine(
                "By declaring a base class function as virtual, you allow the function to be overridden in any derived class. " +
                "The idea behind a virtual function is to redefine the implementation of the base class method in the derived class as required. " +
                "If a method is virtual in the base class then we have to provide the override keyword in the derived class. Neither member fields nor static functions can be declared as virtual.");

            new myBase().VirtualMethod();
            new myDerived().VirtualMethod();
            new myBase().VirtualNewMethod();
            new myDerived().VirtualNewMethod();

            Console.WriteLine("------------------------------------------------------------------------");
        }

        class myBase
        {
            //virtual function
            public virtual void VirtualMethod()
            {
                Console.WriteLine("virtual method defined in the base class");
            }

            public virtual void VirtualNewMethod()
            {
                Console.WriteLine("virtual new method defined in the base class");
            }
        }

        class myDerived : myBase
        {
            // redifing the implementation of base class method
            public override void VirtualMethod()
            {
                Console.WriteLine("virtual method defined in the Derive class");

                //base.VirtualMethod();
            }

            // hiding the implementation of base class method
            public new void VirtualNewMethod()
            {
                Console.WriteLine("virtual new method defined in the Derive class");
                
                //still access the base class method
                base.VirtualNewMethod();
            }
        }
    }
}
