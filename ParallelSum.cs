// Parallel sum reduction
// Gary M. Zoppetti

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

class ParallelSum
{
  static void Main(string[] args)
  {
    Console.Write("p ==> ");
    string input = Console.ReadLine();
    int numThreads;
    Int32.TryParse(input, out numThreads);
    Console.WriteLine("(hardware threads = "
                       + Environment.ProcessorCount + ")");
    Console.Write("N ==> ");
    input = Console.ReadLine();
    int N;
    Int32.TryParse(input, out N);
    Console.WriteLine();

    int[] A = new int[N];
    Random rand = new Random();
    const int UPPER_BOUND = 4;
    for (int i = 0; i < N; ++i)
      A[i] = rand.Next(0, UPPER_BOUND + 1);

    //** Parallel sum **//
    Stopwatch watch = new Stopwatch();
    watch.Start();
    // Task<> type uses thread pool threads by default
    var tasks = new Task<int>[numThreads];
    for (int i = 0; i < numThreads; ++i)
    {
      // Copy "i" so each closure captures different variables
      // Closures capture variables, not values. 
      int threadId = i;
      tasks[i] = Task.Run(
       () => localSum(threadId, numThreads, A));
    }
    int parallelSum = 0;
    foreach (var task in tasks)
    {
      parallelSum += task.Result;
    }
    watch.Stop();
    long elapsedMs = watch.Elapsed.Milliseconds;
    Console.WriteLine("// sum:       " + parallelSum);
    Console.WriteLine("// time:      " + elapsedMs + " ms\n");

    //** Serial sum **//
    watch.Start();
    int lSerialSum = serialSum(A);
    watch.Stop();
    Console.WriteLine("Serial sum:   " + lSerialSum);
    elapsedMs = watch.Elapsed.Milliseconds;
    Console.WriteLine("Serial time:  " + elapsedMs + " ms");
  }

  private static int localSum(int id, int numThreads, int[] A)
  {
    int lowerBound = id * A.Length / numThreads;
    int upperBound = (id + 1) * A.Length / numThreads;

    int localSum = 0;
    for (int i = lowerBound; i < upperBound; ++i)
      localSum += A[i];
    return localSum;
  }

  private static int serialSum(int[] A)
  {
    int sum = 0;
    foreach (var e in A)
      sum += e;
    return sum;
  }
}