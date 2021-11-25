using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.OOP
{
    class Interface
    {
        public void Main()
        {
            Console.WriteLine("Interface \n");
            Console.WriteLine(
                "An interface is a set of related functions that must be implemented in a derived class. " +
                "Members of an interface are implicitly public and abstract. Interfaces are similar to abstract classes. " +
                "First, both types must be inherited; second, you cannot create an instance of either. " +
                "Although there are several differences as in the following; \n" +
                
                "1) An Abstract class can contain some implementations but an interface can't.\n" +
                "2) An Interface can only inherit other interfaces but abstract classes can inherit from other classes and interfaces.\n" +
                "3) An Abstract class can contain constructors and destructors but an interface can't.\n" +
                "4) An Abstract class contains fields but interfaces don't.\n" +

                "So the question is, which of these to choose? Select interfaces because with an interface, " +
                "the derived type still can inherit from another type and interfaces are more straightforward than abstract classes.");

            test obj = new test();
            obj.methodA();
            obj.methodB();
            obj.methodC();

            Console.WriteLine("------------------------------------------------------------------------");
        }

        // interface
        public interface xyz
        {
            void methodA();
            void methodB();
        }

        // An interface can be inherited from other interfaces as in the following:
        public interface abc : xyz
        {
            void methodC();
        }

        // interface method implementation
        class test : abc
        {
            public void methodA()
            {
                Console.WriteLine("methodA");
            }
            public void methodB()
            {
                Console.WriteLine("methodB");
            }
            public void methodC()
            {
                Console.WriteLine("methodC");
            }
        }


    }
}
