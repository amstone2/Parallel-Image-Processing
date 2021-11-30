
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

class ImageProcessing
{
  static void Main(string[] args)
  {
    // Console.Write("Enter file name: ");
    // string input = Console.ReadLine();
    BlackAndWhite("i.jpg");
  }

  private static void PrintString(string s)
  {
    Console.WriteLine(s);
  }


  private static void BlackAndWhite(string input)
  {
    Bitmap bmp = new Bitmap(input);

    int threads = 4;
    int numThreads = threads / 2;

    int imageWidth = bmp.Width;
    int imageHeight = bmp.Height;



    List<int>[] partitionValues = new List<int>[4];
    List<int> hello = new List<int>();
    hello.Add(-1);
    hello.Add(-1);
    hello.Add(-1);
    hello.Add(-1);

    partitionValues[0] = hello;
    partitionValues[1] = new List<int> { -1, -1, -1, -1 };
    partitionValues[2] = new List<int> { -1, -1, -1, -1 };
    partitionValues[3] = new List<int> { -1, -1, -1, -1 };


    foreach (var list in partitionValues)
    {
      foreach (var element in list)
      {
        Console.Write(element + " ");
      }
      Console.WriteLine();
    }

    // int[,] array = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
    // int[] newArray = new int[] { 10, 11, 12 };
    // array.SetValue(newArray, 0);


    // Collection of string  
    // int[] first = { 0, 1, 2 };
    // int[] second = { 3, 4, 5 };
    // int[] third = {6, 7, 8};

    // Create a List and add a collection  
    // List<int[]> partitionValues = new List<int>();
    // partitionValues.AddRange(first);
    // partitionValues.AddRange(second);
    // partitionValues.AddRange(third);

    // for(int i = 0; i < partitionValues.Count; ++i)
    // {
    //   Console.Write(partitionValues[i].ToString() + ", ");
    // }

    // foreach (int a in animalsList)
    // {
    //   Console.Write(a);
    // }




    Console.Write("\n********************************************************************\n");
    Console.Write("Image width: " + imageWidth + " Image Height: " + imageHeight + " Num threads: " + numThreads + "\n");
    for (int i = 0; i < numThreads; ++i)
    {
      int myFirstI = (i * imageWidth) / (numThreads);
      int myLastI = ((i + 1) * imageWidth) / (numThreads);

      Console.Write("\n********************************************************************\n");
      Console.Write("I: " + i + "\n");
      Console.Write("myFirstI: " + myFirstI + " myLastI: " + myLastI + "\n\n");

      List<int> partitionedValues = new List<int>();
      partitionedValues.Add(myFirstI);
      partitionedValues.Add(myLastI);


      for (int j = 0; j < numThreads; ++j)
      {


        int myFirstJ = (j * imageHeight) / (numThreads);
        int myLastJ = ((j + 1) * imageHeight) / (numThreads);


        partitionedValues.Add(myFirstJ);
        partitionedValues.Add(myLastJ);

        partitionValues[i] = partitionedValues;

        Console.Write("j: " + j + "\n");
        Console.WriteLine("myFirstJ: " + myFirstJ + " myLastJ: " + myLastJ + "\n");

      }
    }
    Console.Write("\n\n");
    foreach (var list in partitionValues)
    {
      foreach (var element in list)
      {
        Console.Write(element + " ");
      }
      Console.WriteLine();
    }


    // for (int i = 0; i < partitionValues.GetLength(0); ++i)
    // {
    //   for (int j = 0; j < partitionValues.GetLongLength(1); ++j)
    //   {
    //     Console.Write(partitionValues[i, j] + " ");
    //   }
    //   Console.WriteLine();

    // }


    // for (int i = 0; i < bmp.Width; i++)
    // {
    //   for (int j = 0; j < bmp.Height; j++)
    //   {
    //       Color c = bmp.GetPixel(i, j);

    //       //Apply conversion equation
    //       byte gray = (byte)(.21 * c.R + .71 * c.G + .071 * c.B);

    //       //Set the color of this pixel
    //       bmp.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
    //   }
    // }
    // bmp.Save("Greyscale.jpg");
  }
}