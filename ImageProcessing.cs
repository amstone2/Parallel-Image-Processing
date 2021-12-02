/*
   Filename   : ImageProccessing.cs
   Author     : Alex Stone and Ben Moran
   Course     : CSCI 476
   Date       : 12/2/2021
   Assignment : Final Project
   Description: Takes in an image and modifies it or compresses it in parallel.
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
        // Gets command line arguments
        if (args.Length == 4)
        {
            int threads = Int32.Parse(args[0]);
            String filename = args[1];
            String newFilename = args[2];
            String mode = args[3];
            if (mode == "compress")
            {
                compressImageParallel (threads, filename, newFilename);
            }
            else if (mode == "modify")
            {
                ModifyImageInParallel (threads, filename, newFilename);
            }
        }
        else
        {
            // Help messave
            Console.WriteLine("No command line arguments passed.");
            Console.WriteLine("Arg0 = threads");
            Console.WriteLine("Arg1 = filename");
            Console.WriteLine("Arg2 = new fileanme");
            Console.WriteLine("Arg3 = Mode (compress/modify)");
        }
    }

    /************************************************************/
    // Takes a the number of threads (must be an even number), a filename for the original image,
    // and a newfile name for the compressed image
    public static void compressImageParallel(
        int threads,
        String filename,
        String newFilename
    )
    {
        // Image bitmap
        Bitmap bmp = new Bitmap(filename);

        // Original height and width.
        int totalWidth = bmp.Width;
        int totalHeight = bmp.Height;

        // threads passed to partitioning algorithm.
        int numOfColumns = threads / 2;

        // Get the rectangles needed to compress the image.
        Rectangle[] partitionedRectangles = new Rectangle[threads];
        partitionImageAndMakeRectangles(ref bmp,
        threads,
        ref partitionedRectangles);

        // Final bitmap to have lines drawn on it.
        Bitmap finalBitmap = new Bitmap(totalWidth, totalHeight);

        // Set the graphics object to be the bitmap we will draw onto.
        Graphics g = Graphics.FromImage(finalBitmap);

        // Make it all black
        g.Clear(Color.Black);

        // Countdown event so we can tell when the threads are done.
        CountdownEvent cntEvent = new CountdownEvent(threads);

        // Go through all the rectangles, compressing and drawing them back on the bitmap.
        foreach (var rec in partitionedRectangles)
        {
            ThreadPool
                .QueueUserWorkItem(stat =>
                    compressRectangleAndDraw(rec,
                    ref g,
                    ref bmp,
                    ref cntEvent));
        }

        // Wait until all threads are done.
        cntEvent.Wait();

        // Create and save the final bitmap.
        finalBitmap.Save(newFilename, System.Drawing.Imaging.ImageFormat.Jpeg);
    }

    /************************************************************/
    public static void partitionImageAndMakeRectangles(
        ref Bitmap bmp,
        int threads,
        ref Rectangle[] partitionedRectangles
    )
    {
        int numOfColumns = threads / 2;

        // The list of partitioned values.
        List<int>[] partitionIValues = new List<int>[numOfColumns];
        List<int>[] partitionJValues = new List<int>[2];
        partitionImage(ref bmp,
        threads,
        ref partitionIValues,
        ref partitionJValues);

        // Image dimensions.
        int imageWidth = bmp.Width;
        int imageHeight = bmp.Height;

        // Go thro
        int count = 0;
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

                partitionedRectangles[count] = rec;
                ++count;
            }
        }
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

        MemoryStream ms = CompressImage(cloneBitmap, 50);
        var compressedImage = Image.FromStream(ms);

        lock (g)
        {
            // Draw the smaller rectangles on the bitmap
            g.DrawImage(compressedImage, new Point(rec.X, rec.Y));
        }

        cntEvent.Signal();
    }

    /************************************************************/
    // Compressed the bitmap with the qualilty (0 - 100 inclusive).

    // Returns a memerory stream that the image is i so we can use it later.
    public static MemoryStream CompressImage(Bitmap bmp, int quality)
    {
        // Get the encoder using our method.
        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

        // Get the quality of the encoder.
        System.Drawing.Imaging.Encoder QualityEncoder =
            System.Drawing.Imaging.Encoder.Quality;

        // Get an array of encoder objects. The 1 means it is of size one because we only need one encoder.
        EncoderParameters myEncoderParameters = new EncoderParameters(1);

        // Gets the encoder parameter with the specified quality.
        EncoderParameter myEncoderParameter =
            new EncoderParameter(QualityEncoder, quality);

        // Sets the encoder parameter.

        myEncoderParameters.Param[0] = myEncoderParameter;

        // Save the bitmap to the memory stream so it gets compressed.
        var ms = new MemoryStream();
        bmp.Save (ms, jpgEncoder, myEncoderParameters);
        return ms;
    }

    /************************************************************/
    // Get the encoder of the specified format.

    // Returns an ImageCodeInfo object to be ussed in compression.
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
    // Takes an image and partions it using our partitioning formula using rectangles.

    // J/Height/Y will always be two to make it more parallelizable

    public static void partitionImage(
        ref Bitmap bmp,
        int threads,
        ref List<int>[] partitionIValues,
        ref List<int>[] partitionJValues
    )
    {
        int numOfColumns = threads / 2;

        // Image dimensions.
        int imageWidth = bmp.Width;
        int imageHeight = bmp.Height;

        for (int i = 0; i < numOfColumns; ++i)
        {
            // Calculate the I values using our algorithm.
            int myFirstI = (i * imageWidth) / (numOfColumns);
            int myLastI = ((i + 1) * imageWidth) / (numOfColumns);

            // Put the i values in the list.
            List<int> iValues = new List<int>();
            iValues.Add (myFirstI);
            iValues.Add (myLastI);
            partitionIValues[i] = iValues;
        }
        for (int j = 0; j < 2; ++j)
        {
            // Calculate the j values using our algorithm.
            int myFirstJ = (j * imageHeight) / (2);
            int myLastJ = ((j + 1) * imageHeight) / (2);

            // Place the j values in the list.
            List<int> jValues = new List<int>();
            jValues.Add (myFirstJ);
            jValues.Add (myLastJ);
            partitionJValues[j] = jValues;
        }
    }

    /************************************************************/
    // Gets the image and modifies it based on the method called
    private static void ModifyImageInParallel(
        int threads,
        string filename,
        String newFilename
    )
    {
        // New bitmap of the input image.
        Bitmap bmp = new Bitmap(filename);

        // threads passed to partitioning algorithm.
        int numOfColumns = threads / 2;

        // The list of partitioned values.
        List<int>[] partitionIValues = new List<int>[numOfColumns];
        List<int>[] partitionJValues = new List<int>[2];

        partitionImage(ref bmp,
        threads,
        ref partitionIValues,
        ref partitionJValues);

        // Count down event so we can see how many threads have been completed.
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

                // Place the methods in the threadpool.
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
        // Wiat for all threads to finish.
        cntEvent.Wait();
        // Save the bitmap to the new file name.
        bmp.Save (newFilename);
    }

    /************************************************************/
    // Changes a pixel color on the bitmap to black and write.
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
