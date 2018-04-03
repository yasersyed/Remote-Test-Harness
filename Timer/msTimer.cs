/////////////////////////////////////////////////////////////////////
// msTimer.cs - Calculates time taken in high resolution           //
//                                                                 //
// Platform:    Mac Book Pro, Windows 10 Education                 //
// Application: CSE681 - Software Modeling and Analysis            //
// Author:      Syed Yaser Ahmed, Syracuse University Fall 2016    //
//              sysysed@syr.edu, (315) 480-9522                    //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 *  msTimer - Calculates time taken in any given function in ms.
 *
 * Required files:
 * ---------------
 * - msTimer.cs
 * 
 * Maintanence History:
 * --------------------
 * ver 1.0 : 20 Nov 2016
 * - first release
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestHarnessCS
{
    public class msTimer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern
        int QueryPerformanceFrequency(out ulong x);

        [DllImport("kernel32.dll")]
        protected static extern
        int QueryPerformanceCounter(out ulong x);

        protected ulong a, b, f;
        public msTimer()
        {
            a = b = 0UL;
            if (QueryPerformanceFrequency(out f) == 0)
                throw new Win32Exception();
        }
        public ulong ElapsedTicks
        {
            get
            { return (b - a); }
        }
        public ulong ElapsedMicroseconds
        {
            get
            {
                ulong d = (b - a);
                if (d < 0x10c6f7a0b5edUL) // 2^64 / 1e6
                    return (d * 1000000UL) / f;
                else
                    return (d / f) * 1000000UL;
            }
        }
        public TimeSpan ElapsedTimeSpan
        {
            get
            {
                ulong t = 10UL * ElapsedMicroseconds;
                if ((t & 0x8000000000000000UL) == 0UL)
                    return new TimeSpan((long)t);
                else
                    return TimeSpan.MaxValue;
            }
        }
        public ulong Frequency
        {
            get
            { return f; }
        }

        public void Start()
        {
            Thread.Sleep(0);
            QueryPerformanceCounter(out a);
        }

        public ulong Stop()
        {
            QueryPerformanceCounter(out b);
            return ElapsedTicks;
        }
        static int Main()
        {
            msTimer ms = new msTimer();
            ms.Start();
            Console.Write("{0}, {1}, {2} ",ms.ElapsedMicroseconds,ms.ElapsedTicks,ms.ElapsedTimeSpan );
            ms.Stop();
            return 0;
        }
    }
}
