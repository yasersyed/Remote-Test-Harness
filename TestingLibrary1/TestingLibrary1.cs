using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarnessCS
{
    public class TestingLibrary1
    {
        
        bool test1()
        {
            string text = "test1: attempt to create directory on illegal path";
            
            try
            {
                Directory.CreateDirectory("C:/Windows/newfolder");
                Console.WriteLine(text+ ": Passed");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(text + "Failed because: "+ex.Message);
                return false;
            }
        }
        bool test2()
        {
            string text = "test2: attempt to create directory on right path";

            try
            {
                Directory.CreateDirectory("../../NewFolder-testing");
                Console.WriteLine(text + " : Passed ");
                //Console.WriteLine(text);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(text + "Failed because: " + ex.Message);
                return false;
            }
        }
        bool test3()
        {
            string text = "test3: Test that throws exception";

            int i = 1;

            try
            {
                i = i / 0;
                Console.WriteLine("An Exception should be thrown");
                Console.WriteLine(text + ": Passed");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(text +" : Failed, because " +e.Message);
                return false;
            }
            
        }


        public bool testcode()
        {
            bool result1, result2, result3;
                result1 = test1();
                result2 = test2();
                result3 = test3();          
                if (result1 == true && result2 == true && result3 == true)
                    return true;
                else
                    return false;
        }
    }
}
