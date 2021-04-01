using System;
using System.Collections.Generic;
using System.Text;

namespace OOProgramming
{
    class Property
    {
        // 屬性是方法的變形
        // 屬性本身變形，變成本身具有方法的功能，以下class1就等於class2，變得更簡潔了。
        public class Class1
        {
            private int _a = 0;

            public int Get()
            {
                return _a;
            }
            public void Set(int value)
            {
                _a = value;
            }
        }

        public class Class2
        {
            private int _a = 0;

            public int A
            {
                //int X = class2.A; 取值時執行get大括號內的方法
                get { return _a; }
                // class.A = X; 寫入X值時執行set大括號內的方法
                set { _a = value; }
            }
        }
    }
}
