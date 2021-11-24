using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.OOP
{
    class FunctionOverloading
    {
        public string name;

        //overloaded functions
        public void setName(string last)
        {
            name = last;
        }

        public void setName(string first, string last)
        {
            name = first + "" + last;
        }

        public void setName(string first, string middle, string last)
        {
            name = first + "" + middle + "" + last;
        }

        public void Main()
        {
            Console.WriteLine("Function Overloading \n");
            Console.WriteLine("Function overloading allows multiple implementations of the same function in a class. " +
                "Overloaded methods share the same name but have a unique signature. The number of parameters, types of parameters or both must be different. " +
                "A function can't be overloaded on the basis of a different return type alone. ");

            FunctionOverloading obj = new FunctionOverloading();

            obj.setName("barack");
            obj.setName("barack ", " obama ");
            obj.setName("barack ", "hussian", "obama");
            Console.WriteLine("------------------------------------------------------------------------");
        }
    }
}
