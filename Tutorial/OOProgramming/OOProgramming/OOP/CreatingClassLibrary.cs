using LibraryUtil;
using System;

namespace OOProgramming.OOP
{
    class CreatingClassLibrary
    {
        public void Main()
        {
            Console.WriteLine("Creating and accessing Class Component Library");
            Console.WriteLine("Build this code and you will notice that a DLL file was created, not an executable, in the root directory of the application (path = D:\\Github\\DotNETC#\\CSharp_Projects\\Tutorial\\LibraryUtil\\LibraryUtil\\bin\\Debug\\LibraryUtil.dll).");
            Console.WriteLine(" Then add the class library dll file reference to access the declared class in the library dll. ");
            //library class instance
            MathLib obj2 = new MathLib();

            //method populate
            obj2.calculareSum(2, 5);
            obj2.calculareSqrt(25);
            Console.WriteLine("------------------------------------------------------------------------");
        }
    }
}
