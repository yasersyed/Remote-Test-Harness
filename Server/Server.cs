/////////////////////////////////////////////////////////////////////
// Server.cs - For Getting requests from clients and               //
//             sending back results and log files                  //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module creates gets XML test request and parses it for testing.
 * Server.cs utilizes the interface IServer for functions such as send
 * and receive messages from/to client.
 * It also calls loader for creation of App Domain.
 * 
 * Public Interface:
 * ==================
 * void sendFile(FileTransferMessage2 msg);
 * Stream receiveFile(string filename);
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   IServer.cs,Loader.cs, BlockingQueue.cs, CommChannel.cs, Logger.cs, Repository.cs, IComm.cs, XMLinput.cs
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
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarnessCS;

namespace TestHarnessCS
{
   class Server
    {
        IRepo channel;
        int BlockSize = 1024;
        byte[] block;
        string receivePath = Environment.CurrentDirectory + "\\TestHarnessCS\\Server_Received";
        string sendPath = Environment.CurrentDirectory + "\\TestHarnessCS\\Server_ToSend";
        msTimer time = new msTimer();
        Loader load = new Loader();
        XMLInput xi = new XMLInput();
        List<string> rcvdFile { get; set; } = new List<string>();
        private Thread rcvThread = null;

        BlockingQueue<XMLProp> Queue = new BlockingQueue<XMLProp>();
        public Comm<Server> comm { get; set; } = new Comm<Server>();
        public string endPoint { get; } = Comm<Server>.makeEndPoint("http://localhost", 8080);
        public Stream receiveFile(string filename)
        {
            time.Start();
            string sfilename = Path.Combine(sendPath, filename);
            Console.WriteLine(sfilename);
            FileStream outStream = null;
            if (File.Exists(sfilename))
                outStream = new FileStream(sfilename, FileMode.Open);
            else
                Console.WriteLine("Can't open File: " + Path.GetFullPath(sfilename));
            time.Stop();
            Console.Write("Received file : {0} in {1} ms", sfilename, time.ElapsedMicroseconds);
            return outStream;
        }
        void download(string filename)
        {
            int totalBytes = 0;
            try
            {
                time.Start();
                Stream strm = channel.downLoadFile(filename);
                string rfilename = Path.Combine(receivePath, filename);
                if (!Directory.Exists(receivePath))
                    Directory.CreateDirectory(receivePath);
                using (var outputStream = new FileStream(rfilename, FileMode.Create))
                {
                    while (true)
                    {
                        int bytesRead = strm.Read(block, 0, BlockSize);
                        totalBytes += bytesRead;
                        if (bytesRead > 0)
                            outputStream.Write(block, 0, bytesRead);
                        else
                            break;
                    }
                }
                time.Stop();
                Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} ms", filename, totalBytes, time.ElapsedMicroseconds);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
            }
            try
            {
                CopyFiles(receivePath, sendPath);
            }
            catch
            {
                Console.WriteLine("\n");
            }
        }
        public static void CopyFiles(string sourceDir, string targetDir)
        {
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));
            foreach (var directory in Directory.GetDirectories(sourceDir))
                CopyFiles(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }
        static IRepo CreateServiceChannel(string url)
        {
            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;
            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 500000000;
            EndpointAddress address = new EndpointAddress(url);
            ChannelFactory<IRepo> factory
              = new ChannelFactory<IRepo>(binding, address);
            return factory.CreateChannel();
        }
        public Server()
        {
            rcvdFile = new List<string>();
            block = new byte[BlockSize];
            comm.rcv.CreateRcvChannel(endPoint);
            rcvThread = comm.rcv.start(rcvThreadProc);
        }
        public void wait()
        {
            rcvThread.Join();
        }
        public Message makeMessage(string author, string frompoint, string topoint)
        {
            Message msg = new Message();
            msg.author = author;
            msg.from = frompoint;
            msg.to = topoint;
            return msg;
        }
        void rcvThreadProc()
        {
            while (true)
            {
                Message msg = comm.rcv.GetMessage();
                if (msg.body != "quit")
                {
                    rcvdFile.Clear();
                    string clientAdd = msg.from;
                    msg.time = DateTime.Now;
                    Console.Write("\n  {0} received message:", comm.name);
                    msg.showMsg();
                    string testKey = Path.Combine("..\\", msg.author);
                    string filename = msg.time.ToString().Replace(" ", "_").Replace(":", "_").Replace("/", "_") + ".xml";
                    Directory.CreateDirectory(testKey);
                    string path = Path.Combine(testKey, filename);
                    StreamWriter sw = new StreamWriter(path);
                    sw.WriteLine(msg.body);
                    sw.Close();
                    FileStream xml = new FileStream(path, FileMode.Open);
                    xi.parse(xml);
                    foreach (XMLProp tl in xi.testList)
                    {
                        rcvdFile.Add(tl.TestDriver.ToString());
                        foreach (var tl2 in tl.TestLibrary)
                        {
                            rcvdFile.Add(tl2.ToString());
                        }
                        Queue.enQ(tl);
                    }
                    
                    string request = msg.body.ToString();
                    Console.WriteLine("\n Requirement 4: - DeQueued Message request: " + request +" \n ");
                    channel = CreateServiceChannel("http://localhost:8000/IRepo");
                    try
                    {
                        foreach (string files in rcvdFile)
                        {
                            download(files);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("\n Requirement 3: - Can't find dll file for testing in repository \n");
                        Message failedmsg = new Message();
                        failedmsg = makeMessage("Server",endPoint,"http://localhost:8080");
                        failedmsg.time = DateTime.Now;
                        failedmsg.body = "Requirement 3 - Can't find dll file in repository - terminating request";
                        comm.snd.PostMessage(failedmsg);
                        Message quitmsg = new Message();
                        quitmsg = makeMessage("Server", endPoint, endPoint);
                        quitmsg.time = DateTime.Now;
                        quitmsg.body = "quit";
                        comm.snd.PostMessage(quitmsg);
                    }
                    ((System.ServiceModel.Channels.IChannel)channel).Close();
                    while (Queue.size() != 0)
                    {
                        XMLProp XMLrequest = Queue.deQ();
                        load.loadTests(XMLrequest);
                    }
                    
                    xi.testList.Clear();
                    Console.WriteLine("\n Requirement 6 - Sending Log files to Repository: ");
                    channel = CreateServiceChannel("http://localhost:8000/IRepo");
                    string fqname = receivePath +"\\" +rcvdFile[0] + ".log";
                    using (var inputStream = new FileStream(fqname, FileMode.Open))
                    {
                        FileTransferMessage msg2 = new FileTransferMessage();
                        msg2.filename = rcvdFile[0]+".log";
                        msg2.transferStream = inputStream;
                        channel.upLoadFile(msg2);
                    }
                    ((System.ServiceModel.Channels.IChannel)channel).Close();
                }
                else
                    break;
            }
        }
        static void Main(string[] args)
        {
            Server Server = new Server();
            Server.wait();
            Console.Write("\n  press key to exit: ");
            Console.Write("\n");
            Console.ReadKey();
        }
        public void sendFile(FileTransferMessage2 msg)
        {
            throw new NotImplementedException();
        }
    }
}
