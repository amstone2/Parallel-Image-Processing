/*
Authors:  Ben Moran and Alex Stone
Project:  Parallel Image Processing in C#
Class:    CSCI 476
Date:     11/22/21
*/


using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

class ImageProcessing
{
  static void Main (string[] args)
  {
    Console.Write ("Enter a filename:\n");
    string input = Console.ReadLine ();

    Stopwatch stopWatch = new Stopwatch();
    stopWatch.Start();
    SerialGreyscale(input);
    stopWatch.Stop();

    Console.WriteLine("\nSerialGreyscale RunTime: " + stopWatch.ElapsedMilliseconds + " ms");
  }

  private static void PrintString (string s)
  {
    Console.WriteLine(s);
  }

  private static void SerialGreyscale (string input)
  {
    Bitmap bmp = new Bitmap(input);
    for (int i = 0; i < bmp.Width; i++)
    {
      for (int j = 0; j < bmp.Height; j++)
      {
          Color c = bmp.GetPixel(i, j);

          //Apply conversion equation
          byte gray = (byte)(.21 * c.R + .71 * c.G + .071 * c.B);

          //Set the color of this pixel
          bmp.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
      }
    }
    bmp.Save("Greyscale.jpg");
  }
}