using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarnessCS
{
    public class TestingLibrary2
    {
        bool test4()
        {
            string text = "test4: Creating new directory, throwing exception if any";

            try
            {
                Directory.CreateDirectory("../../../Logs/testDirectory");
                Console.WriteLine(text + " : Passed");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(text + ": Failed because - " + ex.Message);

                return false;
            }
        }
        public bool testcode()
        {
            bool result;
            result = test4();
            if (result == true)
                return true;
            else
                return false;
        }
    }
}
