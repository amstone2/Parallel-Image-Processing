
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
  static void Main (string[] args)
  {
    Console.Write ("Enter String\n");
    string input = Console.ReadLine ();
    BlackAndWhite(input);
  }

  private static void PrintString (string s)
  {
    Console.WriteLine(s);
  }

  private static void BlackAndWhite (string input)
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