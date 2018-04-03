/////////////////////////////////////////////////////////////////////
// BlockingQueue.cs - Solution for queuing test drivers            //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ==================
 * This module uses Queue for Loading Test requests from clients, its functions EnQ and DeQ
 * are used to Queueing Test requests, test drivers from the parsed from the XML files.
 * message sent from clients, results from server
 * 
 * Public Interface:
 * ==================
 *        void enQ(T msg); - Enqueue Test Driver paths taken from XML
 *        public T deQ(); -  Remove Test Driver from queue
 *        int size();     -  Get the size of the BlockingQueue
 *        void Clear();   -  Clear the blocking Queue
 *        
 */
/*
 * Build Process:
 * ==============
 * Files Required:
 *   BlockingQueue.cs
 * 
 *  * Maintence History:
 * ==================
 * ver 1.0 : 20 Nov 16
 *   - first release
 */
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarnessCS
{
    public class BlockingQueue<T>
    {
        private Queue blockingQ;
        object locker = new object();

        public BlockingQueue()
        {
            blockingQ = new Queue();
        }

        public void enQ(T msg)
        {
            lock (locker)
            {
                blockingQ.Enqueue(msg);
                Monitor.Pulse(locker);
            }
        }
        public T deQ()
        {
            T msg = default(T);
            lock (locker)
            {
                while (this.size() == 0)
                {
                    Monitor.Wait(locker);
                }
                msg = (T)blockingQ.Dequeue();
                return msg;
            }
        }
        public int size()
        {
            int count;
            lock (locker)
            {
                count = blockingQ.Count;
            }
            return count;
        }
        public void clear()
        {
            lock (locker)
            {
                blockingQ.Clear();
            }
        }
        static void Main(string[] args)
        {
            BlockingQueue<Queue> block = new BlockingQueue<Queue>();
            Queue Q = null;

            block.enQ(Q);
            block.deQ();
            block.clear();

        }

    }
}
