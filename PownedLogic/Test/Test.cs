using BaseLogic.DataHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PownedLogic.Test
{
    public class Test : DataObject
    {
        public int TestInt { get; set; }
        public string TestString { get; set; }
        public DateTime TestDT { get; set; }

        public Test()
            : base()
        {
        }

        public Test(int TestInt, string TestString, DateTime TestDT)
            : base()
        {
            this.TestInt = TestInt;
            this.TestString = TestString;
            this.TestDT = TestDT;
        }

    }
}
