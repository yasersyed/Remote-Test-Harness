/////////////////////////////////////////////////////////////////////
// IRepo.cs - Interface for Repository                             //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * void upLoadFile(FileTransferMessage msg);  
 * Stream downLoadFile(string filename);
 *
 * Required files:
 * ---------------
 * - Repo.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 20 Nov 2016
 * - first release
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TestHarnessCS
{
    [ServiceContract(Namespace = "http://TestHarnessCS")]
    public interface IRepo
    {
        [OperationContract(IsOneWay = true)]
        void upLoadFile(FileTransferMessage msg);
        [OperationContract]
        Stream downLoadFile(string filename);
    }

    [MessageContract]
    public class FileTransferMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public string filename { get; set; }

        [MessageBodyMember(Order = 1)]
        public Stream transferStream { get; set; }
    }
}
