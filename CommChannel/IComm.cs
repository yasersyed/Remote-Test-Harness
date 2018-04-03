/////////////////////////////////////////////////////////////////////
// IComm.cs - Interface for CommChannel                            //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * void PostMessage(Message msg);
 * Message getMessage();
 *
 * Required files:
 * ---------------
 * - CommChannel.cs
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
using TestHarnessCS;

namespace CommChannel
{
    [ServiceContract(Namespace = "TestHarnessCS")]
    public interface IComm
    {
        [OperationContract(IsOneWay = true)]
        void postMessage(Message msg);
        Message GetMessage();

    }

    [MessageContract]
    public class FileTransferMessage1
    {
        [MessageHeader(MustUnderstand = true)]
        public string filename { get; set; }

        [MessageBodyMember(Order = 1)]
        public Stream transferStream { get; set; }
    }
}
