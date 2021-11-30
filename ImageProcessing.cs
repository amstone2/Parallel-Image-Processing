using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

class ImageProcessing
{
    static void Main(string[] args)
    {
        // Console.Write("Enter file name: ");
        // string input = Console.ReadLine();
        ProcessImage("i.jpg");
    }

    private static void ProcessImage(string input)
    {
        // New bitmap of the input image.
        Bitmap bmp = new Bitmap(input);

        // Threads to run on.
        int threads = 4;

        // threads passed to partitioning algorithm.
        int numThreads = threads / 2;

        // The list of partitioned values.
        List<int>[] partitionIValues = new List<int>[numThreads];
        List<int>[] partitionJValues = new List<int>[numThreads];

        partitionImage(ref bmp,
        numThreads,
        ref partitionIValues,
        ref partitionJValues);

        var tasks = new Task[numThreads];

        CountdownEvent cntEvent = new CountdownEvent(threads);

        // Go through all the elements in the partioned list and apply the filter.
        foreach (var iList in partitionIValues)
        {
            foreach (var jlist in partitionJValues)
            {
                // Get all the values for the bitmap.
                int iStart = iList[0];
                int iEnd = iList[1];
                int jStart = jlist[0];
                int jEnd = jlist[1];

                ThreadPool
                    .QueueUserWorkItem(state =>
                        setPixelBlackAndWhite(iStart,
                        iEnd,
                        jStart,
                        jEnd,
                        ref bmp,
                        ref cntEvent));
            }
        }

        cntEvent.Wait();
        bmp.Save("Greyscale.jpg");
    }

    public static void partitionImage(
        ref Bitmap bmp,
        int numThreads,
        ref List<int>[] partitionIValues,
        ref List<int>[] partitionJValues
    )
    {
        // Image dimensions.
        int imageWidth = bmp.Width;
        int imageHeight = bmp.Height;

        for (int i = 0; i < numThreads; ++i)
        {
            // Calculate the I values using our algorithm.
            int myFirstI = (i * imageWidth) / (numThreads);
            int myLastI = ((i + 1) * imageWidth) / (numThreads);

            // Put the i values in the list.
            List<int> iValues = new List<int>();
            iValues.Add (myFirstI);
            iValues.Add (myLastI);
            partitionIValues[i] = iValues;

            for (int j = 0; j < numThreads; ++j)
            {
                // Calculate the j values using our algorithm.
                int myFirstJ = (j * imageHeight) / (numThreads);
                int myLastJ = ((j + 1) * imageHeight) / (numThreads);

                // Place the j values in the list.
                List<int> jValues = new List<int>();
                jValues.Add (myFirstJ);
                jValues.Add (myLastJ);
                partitionJValues[j] = jValues;
            }
        }
    }

    public static void setPixelBlackAndWhite(
        int iStart,
        int iEnd,
        int jStart,
        int jEnd,
        ref Bitmap bmp,
        ref CountdownEvent cntEvent
    )
    {
      // Go through the part of the image and apply the grey image.
        for (int i = iStart; i < iEnd; ++i)
        {
            for (int j = jStart; j < jEnd; ++j)
            {
                Color c = bmp.GetPixel(i, j);

                //Apply conversion equation
                byte gray = (byte)(.21 * c.R + .71 * c.G + .071 * c.B);

                //Set the color of this pixel
                bmp.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
            }
        }
        cntEvent.Signal();
    }
}
