/*
   Filename   : count.cc
   Author     : Alex M. Stone
   Course     : CSCI 476
   Date       : 11/4/2021
   Assignment : Arithomania, ah, ah, ah
   Description: Sorts a vector using parallel counting sort, 
   serial counting sort, and standard sort.
*/
/************************************************************/
// Using declaration
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/************************************************************/
class ImageProcessing
{
    /************************************************************/
    static void Main(string[] args)
    {
        // To check the length of Command line arguments

        if (args.Length == 4)
        {
            int threads = Int32.Parse(args[0]);
            String filename = args[1];
            String newFilename = args[2];
            String mode = args[3];
            if(mode == "compress")
            {
              compressImageParallel(threads, filename, newFilename);
            }
            else if(mode == "modify")
            {
              ChangeImage(threads, filename, newFilename);
            }
        }
        else
        {
          Console.WriteLine("No command line arguments passed.");
          Console.WriteLine("Arg0 = threads");
          Console.WriteLine("Arg1 = filename");
          Console.WriteLine("Arg2 = new fileanme");
          Console.WriteLine("Arg3 = Mode (compress/modify)");

        }
    }

    /************************************************************/
    public static void compressImageParallel(int threads, String filename, String newFilename)
    {
        // Image bitmap
        Bitmap bmp = new Bitmap(filename);

        // Original height and width.
        int totalWidth = bmp.Width;
        int totalHeight = bmp.Height;

        // threads passed to partitioning algorithm.
        int numThreads = (int) Math.Ceiling(Math.Sqrt(threads));

        Rectangle[] partitionedRectangles = new Rectangle[threads];
        partitionImageAndMakeRectangles(ref bmp,
        numThreads,
        ref partitionedRectangles);

        // Final bitmap to have lines drawn on it.
        Bitmap finalBitmap = new Bitmap(totalWidth, totalHeight);

        Graphics g = Graphics.FromImage(finalBitmap);

        Image blankBitmap = Image.FromFile(filename);

        // Make it all black
        g.Clear(Color.Black);

        CountdownEvent cntEvent = new CountdownEvent(threads);

        foreach (var rec in partitionedRectangles)
        {
            ThreadPool
                .QueueUserWorkItem(stat =>
                    compressRectangleAndDraw(rec,
                    ref g,
                    ref bmp,
                    ref cntEvent));
        }
        cntEvent.Wait();

        // Create and save the final bitmap.
        finalBitmap
            .Save(newFilename,
            System.Drawing.Imaging.ImageFormat.Jpeg);
        finalBitmap.Dispose();
    }

    /************************************************************/
    public static void compressRectangleAndDraw(
        Rectangle rec,
        ref Graphics g,
        ref Bitmap bmp,
        ref CountdownEvent cntEvent
    )
    {
        Bitmap cloneBitmap = bmp.Clone(rec, bmp.PixelFormat);

        MemoryStream ms = CompressImage(cloneBitmap, 3);
        var compressedImage = Image.FromStream(ms);

        lock (g)
        {
            g.DrawImage(compressedImage, new Point(rec.X, rec.Y));
        }

        // Draw the smaller rectangles on the bitmap
        cntEvent.Signal();
    }

    /************************************************************/
    public static void partitionImageAndMakeRectangles(
        ref Bitmap bmp,
        int numThreads,
        ref Rectangle[] partitionedRectangles
    )
    {
        // The list of partitioned values.
        List<int>[] partitionIValues = new List<int>[numThreads];
        List<int>[] partitionJValues = new List<int>[numThreads];
        partitionImage(ref bmp,
        numThreads,
        ref partitionIValues,
        ref partitionJValues);

        // Image dimensions.
        int imageWidth = bmp.Width;
        int imageHeight = bmp.Height;

        int hello = 0;
        foreach (var iList in partitionIValues)
        {
            foreach (var jlist in partitionJValues)
            {
                List<int> recValues = new List<int>();
                int iStart = iList[0];
                int iEnd = iList[1];
                int jStart = jlist[0];
                int jEnd = jlist[1];

                int x = iStart;
                int y = jStart;
                int width = iEnd - iStart;
                int height = jEnd - jStart;

                Rectangle rec = new Rectangle(x, y, width, height);

                partitionedRectangles[hello] = rec;
                ++hello;
            }
        }
    }

    /************************************************************/
    public static MemoryStream CompressImage(Bitmap bmp, int quality)
    {
        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

        System.Drawing.Imaging.Encoder QualityEncoder =
            System.Drawing.Imaging.Encoder.Quality;

        EncoderParameters myEncoderParameters = new EncoderParameters(1);

        EncoderParameter myEncoderParameter =
            new EncoderParameter(QualityEncoder, quality);

        myEncoderParameters.Param[0] = myEncoderParameter;

        // bmp1.Save (DestPath, jpgEncoder, myEncoderParameters);
        var ms = new MemoryStream();
        bmp.Save (ms, jpgEncoder, myEncoderParameters);
        return ms;
    }

    /************************************************************/
    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }

    /************************************************************/
    private static void ChangeImage(int threads, string filename, String newFilename)
    {
        // New bitmap of the input image.
        Bitmap bmp = new Bitmap(filename);


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
        bmp.Save(newFilename);
    }

    /************************************************************/
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
        }
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

    /************************************************************/
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
/************************************************************/
