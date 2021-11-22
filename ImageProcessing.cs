
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

class ImageProcessing
{
  static void Main (string[] args)
  {
    Console.Write ("Enter String");
    string input = Console.ReadLine ();
    PrintString(input);
  }

  private static void PrintString (string s)
  {
    Console.WriteLine(s);
  }

}