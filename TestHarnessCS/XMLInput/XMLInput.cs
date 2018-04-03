/////////////////////////////////////////////////////////////////////
// XMLinput.cs - For parsing the input XML File                    //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This file has class XMLProp and XMLinput
 * XMLProp is used for displaying the XML elements.
 * XML input is used for parsing XML document and assigning them to local variables
 * It uses arrays to get separate TestDrivers and Test Libraries present inside.
 * Parsing XML is done with the help of LINQ.
 * 
 * Public Interface:
 * ==================
 * class XMLinput
 *       bool parse(FileStream xml);
 * class XMLProp
 *       void display();
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   XMLInput.cs
 *   
 * 
 *  *   
 * Maintence History:
 * ==================
 * ver 1.0 : 05 Oct 16
 *   - first release
 * 
 */
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TestHarnessCS
{
    public class XMLInput
    {
        public XDocument xdoc;
        public List<XMLProp> testList;
        public string TestDriver { get; set; }
        public XMLInput()
        {
            xdoc = new XDocument();
            testList = new List<XMLProp>();
        }
        public bool parse(FileStream xml)
        {
            xdoc = XDocument.Load(xml);
            XElement[] xlibs = xdoc.Descendants("Test").ToArray();
            int libsLength = xlibs.Count();
            XMLProp test = null;
            for (int i = 0; i < libsLength; i++)
            {
                test = new XMLProp();
                test.TestLibrary = new List<string>();
                test.TestDriver = xlibs[i].Element("testDriver").Value;
                IEnumerable<XElement> xtestcode = xlibs[i].Elements("Library");
                foreach (var xlibrary in xtestcode)
                {
                    test.TestLibrary.Add(xlibrary.Value);
                }
                testList.Add(test);
            }
            return true;

        }
        static void main(string[] args)
        {
                XMLInput xi = new XMLInput();
                FileStream fs = new FileStream("../", System.IO.FileMode.Open);
                xi.parse(fs);
        }
    }
}
