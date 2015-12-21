using BaseLogic.DataHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PownedLogic.Test
{
    public class TestDatabaseHandler : DataHandler
    {
        //public static readonly TestDatabaseHandler instance = new TestDatabaseHandler();

        public TestDatabaseHandler() : base()
        {
            base.CreateTable<Test>();
        }

        public void AddTestItem(Test Test)
        {
            base.Insert(Test);
        }

        public List<Test> GetTestItems()
        {
            return base.GetItems<Test>().ToList();
        }
    }
}
