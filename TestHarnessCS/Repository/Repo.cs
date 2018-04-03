/////////////////////////////////////////////////////////////////////
// Repo.cs - For Getting dll files from clients and                //
//             logs from Server                                    //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module gets dll files uploaded from the client which get
 * downloaded by the server for test execution. 
 * Repository also gets the log files sent from the server and
 * Sends it to the Client.
 * 
 * 
 * Public Interface:
 * ==================
 * void upLoadFile(FileTransferMessage msg);
   Stream downLoadFile(string filename);
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   IRepo.cs, msTimer.cs
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
using System.Threading.Tasks;

namespace TestHarnessCS
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Repo : IRepo
    {
        string filename;
        string savePath = Environment.CurrentDirectory + "\\TestHarnessCS\\Repo_Received";
        string ToSendPath = Environment.CurrentDirectory + "\\TestHarnessCS\\Repo_ToSend";
        int BlockSize = 1024;
        byte[] block;   
        msTimer hrt = new msTimer();
        public Repo()
        {
            block = new byte[BlockSize];
            hrt = new msTimer();
        }
        public void upLoadFile(FileTransferMessage msg)
        {
            int totalBytes = 0;
            hrt.Start();
            filename = msg.filename;
            string rfilename = Path.Combine(savePath, filename);
            
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                while (true)
                {
                    int bytesRead = msg.transferStream.Read(block, 0, BlockSize);
                    totalBytes += bytesRead;
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
            }
            hrt.Stop();
            Console.Write(
              "\n Received file \"{0}\" of {1} bytes in {2} microsec.",
              filename, totalBytes, hrt.ElapsedMicroseconds
            );
        }
        public Stream downLoadFile(string filename)
        {
            hrt.Start();
            string sfilename = Path.Combine(ToSendPath, filename);
            FileStream outStream = null;
            if (File.Exists(sfilename))
            {           
                outStream = new FileStream(sfilename, FileMode.Open);
            }
            else
                Console.WriteLine("\n");
            hrt.Stop();
            Console.Write("\n Sent \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            return outStream;
            
        }

        static ServiceHost CreateServiceChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 50000000;
            Uri baseAddress = new Uri(url);
            Type service = typeof(Repo);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IRepo), binding, baseAddress);
            return host;
        }
        static void Main(string[] args)
        {
            ServiceHost host = CreateServiceChannel("http://localhost:8000/IRepo");
            host.Open();
            Console.WriteLine("\nPress any key to quit\n");
            Console.ReadKey();
            Console.WriteLine("\n");
            host.Close();
        }
    }
}
