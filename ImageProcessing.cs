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

    public class ThreadDataForCompression {
        public Rectangle rec;
        public readonly Graphics g;
        public readonly Bitmap bmp;
        public int compressionValue;
        public readonly CountdownEvent cntEvent;

        public ThreadDataForCompression(Rectangle r, ref Graphics g, in Bitmap bitmap, int cv, ref CountdownEvent ce) {
          this.rec = r;
          this.g = g;
          this.bmp = bitmap;
          this.compressionValue = cv;
          this.cntEvent = ce;
        }
    }


        public class ThreadDataForModification {
        public Rectangle rec;
        public readonly Bitmap bmp;
        public int modificationValue;
        public readonly CountdownEvent cntEvent;

        public ThreadDataForModification(Rectangle r, ref Bitmap bitmap, int modificationValue, ref CountdownEvent cntEvent) {
          this.rec = r;
          this.bmp = bitmap;
          this.modificationValue = modificationValue;
          this.cntEvent = cntEvent;
        }
    }
/************************************************************/

class CodeOverhall
{
    /************************************************************/
    static void Main(string[] args)
    {
        // Gets command line arguments
        if (args.Length >= 4)
        {
            int threads = Int32.Parse(args[0]);
            String filename = args[1];

            String newFilename = args[2];
            int index = newFilename.IndexOf(".");
            String serialNewFileName = newFilename.Substring(0, index);
            serialNewFileName += "Serial.jpeg";

            String mode = args[3];

            // For compression
            if (mode == "compress")
            {
                int compressionValue = Int32.Parse(args[4]);

                compressImageParallel (threads, filename, newFilename, compressionValue);
                compressImageSerial (filename, serialNewFileName, compressionValue);

            }
            else if(mode == "baw")
            {
              blackAndWhiteImageInParallel(threads, filename, newFilename);
              blackAndWhiteImageSerial(filename, serialNewFileName);
            }
            else if(mode == "bri")
            {
               int brightness = Int32.Parse(args[4]);

              brightnessInParallel(threads, filename, newFilename, brightness);
              brightnessSerial(filename, serialNewFileName, brightness);
            }
            else if(mode == "border")
            {
              setBorderInParallel(threads, filename, newFilename);
            }
        }
        else
        {
            // Help message.
            Console.WriteLine("No command line arguments passed.");
            Console.WriteLine("Arg0 = threads");
            Console.WriteLine("Arg1 = filename");
            Console.WriteLine("Arg2 = new fileanme");
            Console.WriteLine("Arg3 = Mode (compress)");
        }
      
    }

    /************************************************************/
    // Takes a the number of threads (must be an even number), a filename for the original image,
    // and a newfile name for the compressed image
    public static void compressImageParallel(
        int threads,
        String filename,
        String newFilename,
        int compressionValue
    )
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        // Image bitmap
        Bitmap bmp = new Bitmap(filename);

        // Original height and width.
        int totalWidth = bmp.Width;
        int totalHeight = bmp.Height;

        // threads passed to partitioning algorithm.
        int numOfColumns = threads / 2;

        // Get the rectangles needed to compress the image.
        Rectangle[] partitionedRectangles = new Rectangle[threads];
        getRectangles(ref bmp,
        threads,
        ref partitionedRectangles);

        // Final bitmap to have lines drawn on it.
        Bitmap finalBitmap = new Bitmap(totalWidth, totalHeight);

        // Set the graphics object to be the bitmap we will draw onto.
        Graphics g = Graphics.FromImage(finalBitmap);

        // Countdown event so we can tell when the threads are done.
        CountdownEvent cntEvent = new CountdownEvent(threads);




        // Go through all the rectangles, compressing and drawing them back on the bitmap.
        foreach (var rec in partitionedRectangles)
        {
            var data = new ThreadDataForCompression(rec, ref g, in bmp, compressionValue, ref cntEvent);
            ThreadPool.QueueUserWorkItem(s => compressRectangleAndDraw(s), data);
        }
        

        // Wait until all threads are done.
        cntEvent.Wait();

        // Create and save the final bitmap.
        watch.Stop();
        long parTime = watch.ElapsedMilliseconds;
        Console.WriteLine("New Parallel time Time time for compression: " + parTime + " ms\n\n");
        finalBitmap.Save(newFilename, System.Drawing.Imaging.ImageFormat.Jpeg);
    }


    /************************************************************/
    public static void getRectangles(
        ref Bitmap bmp,
        int threads,
        ref Rectangle[] partitionedRectangles
    )
    {
        int numOfColumns = threads / 2;

        // The list of partitioned values.
        List<int>[] partitionIValues = new List<int>[2];
        List<int>[] partitionJValues = new List<int>[numOfColumns];
        partitionImage(ref bmp,
        threads,
        ref partitionIValues,
        ref partitionJValues);

        // Image dimensions.
        int imageWidth = bmp.Width;
        int imageHeight = bmp.Height;

        // Go through the i and j values to create the rectangles.
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

        for (int i = 0; i < 2; ++i)
        {
            // Calculate the I values using our algorithm.
            int myFirstI = (i * imageWidth) / (2);
            int myLastI = ((i + 1) * imageWidth) / (2);

            // Put the i values in the list.
            List<int> iValues = new List<int>();
            iValues.Add (myFirstI);
            iValues.Add (myLastI);
            partitionIValues[i] = iValues;
        }
        for (int j = 0; j < numOfColumns; ++j)
        {
            // Calculate the j values using our algorithm.
            int myFirstJ = (j * imageHeight) / (numOfColumns);
            int myLastJ = ((j + 1) * imageHeight) / (numOfColumns);

            // Place the j values in the list.
            List<int> jValues = new List<int>();
            jValues.Add (myFirstJ);
            jValues.Add (myLastJ);
            partitionJValues[j] = jValues;
        }
    }

    /************************************************************/
    public static void compressRectangleAndDraw(object d)
    {
        ThreadDataForCompression data = (ThreadDataForCompression)d;
        var rec = data.rec;
        var g = data.g;
        var bmp = data.bmp;
        var compressionValue = data.compressionValue;
        var cntEvent = data.cntEvent;

        Bitmap cloneBitmap = bmp.Clone(rec, bmp.PixelFormat);

        MemoryStream ms = CompressImage(cloneBitmap, compressionValue);
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
    public static MemoryStream CompressImage(Bitmap bmp, int compressionValue)
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
            new EncoderParameter(QualityEncoder, compressionValue);

        // Sets the encoder parameter.
        myEncoderParameters.Param[0] = myEncoderParameter;

        // Save the bitmap to the memory stream so it gets compressed.
        var ms = new MemoryStream();
        bmp.Save (ms, jpgEncoder, myEncoderParameters);
        return ms;
    }

    /************************************************************/
    // Get the encoder of the specified format.
    // Returns an ImageCodeInfo object to be used in compression.
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
    public static void compressImageSerial(
        String filename,
        String newFilename,
        int compressionValue
    )
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        Bitmap bmp = new Bitmap(filename);
        MemoryStream ms = CompressImage (bmp, compressionValue);
        var compressedImage = Image.FromStream(ms);

        // compressedImage.Save(newFilename);

        watch.Stop();
        long serialTime = watch.ElapsedMilliseconds;
        Console.WriteLine("Serial Time time for compression: " + serialTime + " ms\n");
    }
    /************************************************************/


      public static void blackAndWhiteImageInParallel(
        int threads,
        String filename,
        String newFilename
    )
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        // New bitmap of the input image.
        Bitmap bmp = new Bitmap(filename);

        // threads passed to partitioning algorithm.
        int numOfColumns = threads / 2;

        // Get the rectangles needed to modify the image in.
        Rectangle[] partitionedRectangles = new Rectangle[threads];
        getRectangles(ref bmp,
        threads,
        ref partitionedRectangles);

        // Count down event so we can see how many threads have been completed.
        CountdownEvent cntEvent = new CountdownEvent(threads);

        int modificationValue = 0;
        // Go through all the elements in the partioned list and apply the filter.
        foreach (var rec in partitionedRectangles)
        {
            var data = new ThreadDataForModification(rec, ref bmp, modificationValue, ref cntEvent);
            ThreadPool.QueueUserWorkItem(s => setPixelBlackAndWhite(s), data);
        }

        // Wiat for all threads to finish.
        cntEvent.Wait();

        watch.Stop();
        long parallelTime = watch.ElapsedMilliseconds;
        Console.WriteLine("Parallel time for baw: " + parallelTime + " ms\n");
        // Save the bitmap to the new file name.
        bmp.Save (newFilename);
    }

    public static void blackAndWhiteImageSerial(String filename, String newFilename)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();

        // New bitmap of the input image.
        Bitmap bmp = new Bitmap(filename);
        Rectangle rec = new Rectangle(0, 0, bmp.Width, bmp.Height);
        int modificationValue = 0;
        CountdownEvent cntEvent = new CountdownEvent(1);

        var data = new ThreadDataForModification(rec, ref bmp, modificationValue, ref cntEvent);

        setPixelBlackAndWhite(data);

        watch.Stop();
        long parallelTime = watch.ElapsedMilliseconds;
        Console.WriteLine("Serial time for baw: " + parallelTime + " ms\n");
        // bmp.Save (newFilename);
    }


    public static void setPixelBlackAndWhite(object d)
    {
        ThreadDataForModification data = (ThreadDataForModification)d;
        var rec = data.rec;
        var bmp = data.bmp;
        var modificationValue = data.modificationValue;
        var cntEvent = data.cntEvent;

        int iStart = rec.X;
        int iEnd = rec.Width + rec.X;
        int jStart = rec.Y;
        int jEnd = rec.Height + rec.Y;
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


      public static void brightnessInParallel(
        int threads,
        String filename,
        String newFilename,
        int brightness
    )
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        // New bitmap of the input image.
        Bitmap bmp = new Bitmap(filename);

        // threads passed to partitioning algorithm.
        int numOfColumns = threads / 2;

        // Get the rectangles needed to modify the image in.
        Rectangle[] partitionedRectangles = new Rectangle[threads];
        getRectangles(ref bmp,
        threads,
        ref partitionedRectangles);

        // Count down event so we can see how many threads have been completed.
        CountdownEvent cntEvent = new CountdownEvent(threads);

        int modificationValue = brightness;
        // Go through all the elements in the partioned list and apply the filter.
        foreach (var rec in partitionedRectangles)
        {
            var data = new ThreadDataForModification(rec, ref bmp, modificationValue, ref cntEvent);
            ThreadPool.QueueUserWorkItem(s => setBrightness(s), data);
        }

        // Wiat for all threads to finish.
        cntEvent.Wait();

        watch.Stop();
        long parallelTime = watch.ElapsedMilliseconds;
        Console.WriteLine("Parallel time for baw: " + parallelTime + " ms\n");
        // Save the bitmap to the new file name.
        bmp.Save (newFilename);
    }

    public static void brightnessSerial(String filename, String newFilename, int brightness)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();

        // New bitmap of the input image.
        Bitmap bmp = new Bitmap(filename);
        Rectangle rec = new Rectangle(0, 0, bmp.Width, bmp.Height);
        int modificationValue = brightness;
        CountdownEvent cntEvent = new CountdownEvent(1);

        var data = new ThreadDataForModification(rec, ref bmp, modificationValue, ref cntEvent);

        setBrightness(data);

        watch.Stop();
        long parallelTime = watch.ElapsedMilliseconds;
        Console.WriteLine("Serial time for baw: " + parallelTime + " ms\n");
        bmp.Save (newFilename);
    }



    public static void setBrightness(object d)
    {
        ThreadDataForModification data = (ThreadDataForModification)d;
        var rec = data.rec;
        var bmp = data.bmp;
        var bri = data.modificationValue;
        var cntEvent = data.cntEvent;

        int iStart = rec.X;
        int iEnd = rec.Width + rec.X;
        int jStart = rec.Y;
        int jEnd = rec.Height + rec.Y;
        // Go through the part of the image and apply the grey image.
        for (int i = iStart; i < iEnd; ++i)
        {
            for (int j = jStart; j < jEnd; ++j)
            {
                Color c = bmp.GetPixel(i, j);
                int red = 0;
                int green = 0;
                int blue = 0;
                int cR = (int) c.R;
                int cG = (int) c.G;
                int cB = (int) c.B;
                // check if values for RGB will be between 0 and 255
                if (cR + bri > 255)
                {
                  red = 255;
                }
                else if (cR + bri < 0)
                {
                  red = 0;
                }
                else
                {
                  red = cR + bri;
                }
                if (cG + bri > 255)
                {
                  green = 255;
                }
                else if (cG + bri < 0)
                {
                  green = 0;
                }
                else
                {
                  green = cG + bri;
                }
                if (cB + bri > 255)
                {
                  blue = 255;
                }
                else if (cB + bri < 0)
                {
                  blue = 0;
                }
                else
                {
                  blue = cB + bri;
                }
                bmp.SetPixel(i, j, Color.FromArgb(red, green, blue));
            }
        }
        cntEvent.Signal();
    }


      public static void setBorderInParallel(
        int threads,
        String filename,
        String newFilename
    )
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        // New bitmap of the input image.
        Bitmap bmp = new Bitmap(filename);

        // threads passed to partitioning algorithm.
        int numOfColumns = threads / 2;

        // Get the rectangles needed to modify the image in.
        Rectangle[] partitionedRectangles = new Rectangle[threads];
        getRectangles(ref bmp,
        threads,
        ref partitionedRectangles);

        // Count down event so we can see how many threads have been completed.
        CountdownEvent cntEvent = new CountdownEvent(threads);

        int modificationValue = 0;
        // Go through all the elements in the partioned list and apply the filter.
        foreach (var rec in partitionedRectangles)
        {
            var data = new ThreadDataForModification(rec, ref bmp, modificationValue, ref cntEvent);
            ThreadPool.QueueUserWorkItem(s => setThreadBorder(s), data);
        }

        // Wiat for all threads to finish.
        cntEvent.Wait();

        watch.Stop();
        long parallelTime = watch.ElapsedMilliseconds;
        Console.WriteLine("Parallel time for border: " + parallelTime + " ms\n");
        // Save the bitmap to the new file name.
        bmp.Save (newFilename);
    }
    public static void setThreadBorder(Object d)
    {
        ThreadDataForModification data = (ThreadDataForModification)d;
        var rec = data.rec;
        var bmp = data.bmp;
        var cntEvent = data.cntEvent;

        int iStart = rec.X;
        int iEnd = rec.Width + rec.X;
        int jStart = rec.Y;
        int jEnd = rec.Height + rec.Y;


        for (int i = iStart; i < iEnd; i += iEnd)
        {
            for (int j = jStart; j < jEnd; ++j)
            {
                Color c = bmp.GetPixel(i, j);

                // Apply conversion equation.
                byte black = (byte)(0 * c.R + 0 * c.G + 0 * c.B);

                // Set the color of this pixel.
                bmp.SetPixel(i, j, Color.FromArgb(black, black, black));
            }
        }
        for (int i = iStart; i < iEnd; ++i)
        {
            for (int j = jStart; j < jEnd; j += jEnd)
            {
                Color c = bmp.GetPixel(i, j);

                // Apply conversion equation.
                byte black = (byte)(0 * c.R + 0 * c.G + 0 * c.B);

                // Set the color of this pixel.
                bmp.SetPixel(i, j, Color.FromArgb(black, black, black));
            }
        }
        cntEvent.Signal();
    }
}
/************************************************************/

