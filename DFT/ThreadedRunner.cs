using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


/// <summary>
/// Spreads a certain computation across multiple threads.
/// </summary>
public static class ThreadedRunner
{
    /// <summary>
    /// Runs this object's operation across the given number of threads,
    ///   for all values from 0 to (range - 1).
    /// The calling thread will block until this is done.
    /// </summary>
    public static void Run(int nThreads, int range, Action<int, int> doToRangeInclusive)
    {
        //Edge-cases:
        if (range < 1)
            return;
        if (range < nThreads)
            nThreads = range;

  
        //Start up the other threads.
        //Note that we're actually creating one less than the given number of threads,
        //    because this calling thread will also do computations.
        Thread[] threads = new Thread[nThreads - 1];
        int threadSpan = range / nThreads;
        for (int i = 0; i < threads.Length; ++i)
        {
            int start = i * threadSpan;

            threads[i] = new Thread((o) =>
            {
                int startI = (int)o,
                    endI = startI + threadSpan - 1;
                UnityEngine.Assertions.Assert.IsTrue(endI < range);

                doToRangeInclusive(startI, endI);
            });
            threads[i].Start(start);
        }

        //Run the last bit of the computation on this thread if there is any computation left over.
        int myStart = (nThreads - 1) * threadSpan,
            myEnd = range - 1;
        if (myEnd >= myStart)
            doToRangeInclusive(myStart, myEnd);

        //Now wait for the threads to finish.
        for (int i = 0; i < threads.Length; ++i)
                threads[i].Join();
    }
}