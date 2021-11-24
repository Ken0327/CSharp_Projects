using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming.OOP
{
    class Encapsulation
    {
        // Encapsulation Begin
        int x;

        //class constructor
        public Encapsulation()
        {
        }

        public Encapsulation(int iX)
        {
            this.x = iX;
        }

        //calculating the square
        public void MySquare()
        {
            int Calc = x * x;
            Console.WriteLine(Calc);
        }

        // End of Encapsulation

        public void Main()
        {
            Console.WriteLine("Encapsulation \n");
            Console.WriteLine(
                "Encapsulation is the mechanism that binds together the code and the data it manipulates, and keeps both safe from outside interference and misuse. " +
                "In OOP, code and data may be combined in such a way that a self-contained box is created. When code and data are linked together in this way, an object is created and encapsulation exists. \n" +
                "Within an object, code, data or both may be private or public to that object. Private code or data may not be accessible by a piece of the program that exists outside the object. " +
                "When the code and data is public, other portions of your program may access it even though it is defined within an object. ");

            //instance created
            Encapsulation obj = new Encapsulation(20);

            obj.MySquare();

            Console.WriteLine("------------------------------------------------------------------------");
        }
    }
}
