/////////////////////////////////////////////////////////////////////
// CommChannel.cs - Solution for creating channel between          //
//                  Server and Client for sending test requests    //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module uses WSHttpBinding for creating channel between two endpoints
 * defined by the Client and Server respectively
 * 
 * Public Interface:
 * ==================
        void postMessage(Message msg); - For posting messages constructed with class - Messages
        Message GetMessage(); - For getting messages from other endpoints
 *        
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   BlockingQueue.cs, Messages.cs
 *   
 * 
 *  * Maintence History:
 * ==================
 * ver 1.0 : 20 Nov 16
 *   - first release
 */
//

using CommChannel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHarnessCS;

namespace TestHarnessCS
{
    public class Receiver<T> : IComm
    {
        static BlockingQueue<Message> receiverQueue = null;
        ServiceHost service = null;
        public string name { get; set; }
        public Receiver()
        {
            if (receiverQueue == null)
                receiverQueue = new BlockingQueue<Message>();
        }
        public Thread start(ThreadStart rcvThreadProc)
        {
            Thread rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            return rcvThread;
        }
        public void close()
        {
            service.Close();
        }
        public Message GetMessage()
        {
            Message msg = receiverQueue.deQ();
            return msg;
        }
        public void CreateRcvChannel(string address)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(address);
            service = new ServiceHost(typeof(Receiver<T>), baseAddress);
            service.AddServiceEndpoint(typeof(IComm), binding, baseAddress);
            service.Open();
            Console.Write("\n  Service is open listening on {0}", address);
        }
        public void postMessage(Message msg)
        {
            receiverQueue.enQ(msg);
        }
        public static void Main()
        {
            Receiver<T> tc = new Receiver<T>();
            tc.GetMessage();
            Message stub = new Message();
            tc.postMessage(stub);
            tc.close();
        }
    }
    public class Comm<T>
    {
        public string name { get; set; } = typeof(T).Name;
        public Receiver<T> rcv { get; set; } = new Receiver<T>();
        public Sender snd { get; set; } = new Sender();
        public Comm()
        {
            rcv.name = name;
            snd.name = name;
        }
        public static string makeEndPoint(string url, int port)
        {
            string endpoint = url + ":" + port.ToString() + "/IComm";
            return endpoint;
        }
        public void thrdProc()
        {
            while (true)
            {
                Message msg = rcv.GetMessage();
                msg.showMsg();
                if (msg.body == "quit")
                    break;
            }
        }
    }
    public class Sender
    {
        public string name { get; set; }
        IComm channel;
        BlockingQueue<Message> senderQueue = null;
        Thread sndThread = null;
        int tryCount = 0, maxCount = 10;
        string currEndpoint = "";
        string lastError = "";
        public Sender()
        {
            senderQueue = new BlockingQueue<Message>();
            sndThread = new Thread(ThreadProc);
            sndThread.IsBackground = true;
            sndThread.Start();
        }
        void ThreadProc()
        {
            tryCount = 0;
            while (true)
            {
                Message msg = senderQueue.deQ();
                if (msg.to != currEndpoint)
                {
                    currEndpoint = msg.to;
                    CreateSendChannel(currEndpoint);
                }
                while (true)
                {
                    try
                    {
                        channel.postMessage(msg);
                        tryCount = 0;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\n Connection Failed :" + ex.Message);
                        if (++tryCount < maxCount)
                        {
                            Thread.Sleep(100);
                        }
                        else
                        {
                            currEndpoint = "";
                            tryCount = 0;
                            break;
                        }
                    }
                }
                if (msg.body == "quit")
                {
                    break;
                }
            }
        }
        public void CreateSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<IComm> factory = new ChannelFactory<IComm>(binding, address);
            channel = factory.CreateChannel();
            Console.Write("\n  service proxy created for {0}", address);
        }
        public void PostMessage(Message msg)
        {
            senderQueue.enQ(msg);
        }
        public string GetLastError()
        {
            string t = lastError;
            lastError = "";
            return t;
        }
        public void Close()
        {
            ChannelFactory<IComm> temp = (ChannelFactory<IComm>)channel;
            temp.Close();
        }
    }
    public class CommChannel
    {
    }
}
