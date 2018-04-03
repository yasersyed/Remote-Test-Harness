/////////////////////////////////////////////////////////////////////
// ITestInterface.cs - declares ITest Interface                    //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module provides the ITest Interface, declaring getLog(string path) and  bool test()
 * 
 * Public Interface:
 * ==================
 *        void getLog(string path);
 *        bool test();
 */
/*  *   
 * Maintence History:
 * ==================
 * ver 1.0 : 05 Oct 16
 *   - first release
 * 
 */
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestHarnessCS
{
    public interface ITest
    {
        bool test();
        string getLog();
    }
}
