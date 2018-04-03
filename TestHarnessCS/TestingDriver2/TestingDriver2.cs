using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarnessCS
{
    [Serializable]
    public class TestingDriver2 : ITest
    {
        public string getLog()
        {
            throw new NotImplementedException();
        }

        public bool test()
        {
            TestingLibrary2 tl2 = new TestingLibrary2();
            return tl2.testcode();
        }
    }
}
