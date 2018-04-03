/////////////////////////////////////////////////////////////////////
// Logger.cs - for displaying workings of the app in console and   //
//              saving logs of Test Exec                           //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides logging facility for the project.
 * We can display the ongoings in the console and have the functionality for saving in log files
 * 
 * Public Interface:
 * ==================
 *      ILogger add(string item);
 *      void displaycurrent();      
 *      void displayall();
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   Logger.cs
 *   
 * Maintence History:
 * ==================
 * ver 1.0 : 20 Nov 16
 *   - first release
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarnessCS
{
    public interface ILogger
    {
        ILogger add(string item);
        void displaycurrent();
        void displayall();
        ILogger savetofile(string filep);
    }
    public class Logger: ILogger
    {
        StringBuilder text = new StringBuilder();
        string currentitem = null;
        public Logger()
        {
            string time = DateTime.Now.ToString();
        }
        void ILogger.displaycurrent()
        {
            Console.WriteLine("" + currentitem);
        }
        public void displayall()
        {
            Console.Write(text + "\n");
        }
        public ILogger add(string item)
        {
            currentitem = item;
            text.Append("\n" + currentitem);
            return this;
        }
        public ILogger savetofile(string filep)
        {
            StreamWriter sw = new StreamWriter(filep);
            sw.WriteLine(text);
            sw.Close();
            return this;
        }
        static void Main(String[] args)
        {
            Logger il = new Logger();
            string text = "test stub";
            il.add(text);
            il.displayall();
        }
    }
}
