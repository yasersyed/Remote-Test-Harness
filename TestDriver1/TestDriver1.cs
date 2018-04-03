using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHarnessCS;

namespace TestHarnessCS
{
    [Serializable]
    public class TestDriver1:ITest
    {
        public string getLog()
        {
            throw new NotImplementedException();
        }

        public bool test()
        {
            TestingLibrary1 tl1 = new TestingLibrary1();
            return tl1.testcode();

        }
    }
}
