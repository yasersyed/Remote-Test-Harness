/////////////////////////////////////////////////////////////////////
// IServer.cs - Interface for Server                               //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * void sendFile(FileTransferMessage msg);  
 * Stream ReceiveFile(string filename);
 *
 * Required files:
 * ---------------
 * - Server.cs
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
    public interface IServer
    {
        [OperationContract(IsOneWay = true)]
        void sendFile(FileTransferMessage2 msg);

        [OperationContract]
        Stream receiveFile(string filename);
    }
    [MessageContract]
    public class FileTransferMessage2
    {
        [MessageHeader(MustUnderstand = true)]
        public string filename { get; set; }

        [MessageBodyMember(Order = 1)]
        public Stream transferStream { get; set; }
    }
}
