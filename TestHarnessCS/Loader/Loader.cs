/////////////////////////////////////////////////////////////////////
// Loader.cs - For Loading the Test Driver assemblies and          //
//             Executing the test cases                            //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module creates child AppDomain for the Server.
 * By creating the child domain for the Test Exec and utilizing the ITestInterface we are calling their functions,
 * such as Test and getLog from the interface.
 * 
 * Public Interface:
 * ==================
 *        void setpath(string path); 
 *        void loadtests(XMLProp xp);
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   Loader.cs, ILoader.cs, Logger.cs, TestExec.cs, ITestInterface.cs, XMLinput.cs
 *   
 * 
 *  *   
 * Maintence History:
 * ==================
 * ver 1.0 : 20 Nov 16
 *   - first release
 * 
 */
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TestHarnessCS;

namespace TestHarnessCS
{
    [Serializable]
    public class Loader
    {
        string testlibpath;
        public bool result;
        msTimer time = new msTimer();
        public Loader()
        { }
        public void setpath(string path)
        {
            testlibpath = path;
        }
        void showAssemblies(AppDomain ad)
        {
            Assembly[] arrayOfAssems = ad.GetAssemblies();
            foreach (Assembly assem in arrayOfAssems)
                Console.Write("\n  " + assem.ToString());
        }
        public void loadTests(XMLProp testRequest)
        {
            Logger lg = new Logger();
            lg.add("Received Test Request : " + testRequest.TestDriver).displaycurrent();
            XMLProp lib = testRequest;
            lg.add("\nSetting Up Domain for test request").displaycurrent();
            AppDomainSetup domainsetup = new AppDomainSetup();
            Assembly assem = null;
            domainsetup.ApplicationBase = System.Environment.CurrentDirectory;
            //domainsetup.ApplicationBase=System.Environment.CurrentDirectory;
            string driverPath = System.Environment.CurrentDirectory + "\\TestHarnessCS\\Server_Received\\" + lib.TestDriver ;

            //Creating Evidence
            Evidence appEvidence = AppDomain.CurrentDomain.Evidence;
            lg.add("\nCreating child App Domain: ").displaycurrent();

            //Using Domain setup and evidence to create the Child application Domain
            AppDomain domain = AppDomain.CreateDomain(testRequest.TestDriver.Replace(".dll", "-AppDomain"));//, appEvidence, domainsetup);
            assem = Assembly.LoadFrom(driverPath);
            lg.add("\nApp Domain created: " + domain.FriendlyName);
            domain.Load(Path.GetFileNameWithoutExtension(lib.TestDriver));

            //domain.Load(assem.FullName + ".dll");
            Console.WriteLine("\nDisplaying Assemblies: ");
            showAssemblies(domain);
            Console.WriteLine("\n------------------------");
            string name = assem.GetName().Name;
            Console.WriteLine("\nRequirement 5 - Checking if Derived from ITest Interface");
            ITest tdr = null;
            Type[] types = assem.GetExportedTypes();
            foreach(Type t in types)
            {
                if(t.IsClass && typeof(ITest).IsAssignableFrom(t))
                {
                    try
                    {
                        tdr = (ITest)Activator.CreateInstance(t);
                        Console.WriteLine("Passed, This Test driver is derived from ITest Interface \n");
                    }
                    catch
                    {
                        Console.WriteLine("\n This test driver is not derived from ITest Interface \n");
                        continue;
                    }

                }
            }

            Console.Write("\n Child Domain name :" + domain.FriendlyName + "\n");
            Console.WriteLine("---------------------------------------------------------");
            time.Start();
            //ObjectHandle ohandle = domain.CreateInstance(lib.TestDriver.Replace(".dll", ""), "" + lib.TestDriver.Replace(".dll", ""));
            ObjectHandle ohandle = domain.CreateInstance(lib.TestDriver.Replace(".dll", ""), "TestHarnessCS."+name);
            Object obj = ohandle.Unwrap();
            ITest TI = (ITest)obj;
            bool result = TI.test();
            time.Stop();
            lg.add("\nThe result of the test is " + result + "\n  Executed in " + time.ElapsedMicroseconds + " ms");
            Console.Write("\n The result of the test is {0} , \n Executed in {1}  ms", result, time.ElapsedMicroseconds);
            AppDomain.Unload(domain);
            lg.add("\n");
            lg.add("Unloading Domain");
            lg.savetofile(driverPath + ".log");
            Console.WriteLine("\n ---------------------------Ending Test-------------------------------------------");
        }
        static void Main(string[] args)
        {
            Loader load = new Loader();
            load.setpath(Directory.GetCurrentDirectory());
            XMLProp xl = null;
            load.loadTests(xl);
        }
    }
}
