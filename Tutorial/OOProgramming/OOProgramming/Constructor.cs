using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming
{
    class Constructor
    {
        public class A
        {
            protected string _str;
            public A()
            {
                _str = "嗨我是建構式";
                Console.WriteLine("我被A初始化了" + _str);
            }
        }
        public class B : A
        {
            public B()
            {
                Console.WriteLine("我被B初始化了" + _str);
            }
        }
        public class C : B
        {
            public C()
            {
                Console.WriteLine("我被C初始化了" + _str);
            }
        }

        public class D
        {
            protected string _str;
            public D(string talk)
            {
                _str = talk;
                Console.WriteLine(_str + "呼叫天龍D");
            }
        }
        public class E : D
        {
            public E() : base("天龍E") //多了base
            {
                Console.WriteLine(_str + "呼叫長江一號");
            }
        }

        public class F
        {
            private string x;
            private string y;
            public F()
            {
                x = "無參數X初始化";
                y = "無參數Y初始化";
            }
            public F(string str) : this()
            {
                x = str;
            }
            public string GetX()
            {
                return x;
            }
            public string GetY()
            {
                return y;
            }
        }
    }
}
