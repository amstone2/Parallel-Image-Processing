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

class ImageProcessing
{
    static void Main(string[] args)
    {
        // Console.Write("Enter file name: ");
        // string input = Console.ReadLine();
        // ProcessImage("4k.jpeg");
        // CompressImage("fish.jpg", "compressed.jpg", 0);
        compressAndCombineImages();
    }

    public static void compressAndCombineImages()
    {
        // Image bitmap
        Bitmap bmp = new Bitmap("fish.jpg");

        // Original height and width.
        int totalWidth = bmp.Width;
        int totalHeight = bmp.Height;

        // Threads to run on.
        int threads = 16;

        // threads passed to partitioning algorithm.
        int numThreads = (int) Math.Ceiling(Math.Sqrt(threads));
        Console.WriteLine("num threads" + numThreads);

        // int numThreads = threads/2;
        // // The list of partitioned values.
        // List<int>[] partitionIValues = new List<int>[numThreads];
        // List<int>[] partitionJValues = new List<int>[numThreads];
        // partitionImage(ref bmp,
        // numThreads,
        // ref partitionIValues,
        // ref partitionJValues);
        List<int>[] partitionedRectangles = new List<int>[threads];
        partitionImageAndReturnRectangles(ref bmp,
        numThreads,
        ref partitionedRectangles);

        // Final bitmap to have lines drawn on it.
        Bitmap finalBitmap = new Bitmap(totalWidth, totalHeight);

        Graphics g = Graphics.FromImage(finalBitmap);

        Image blankBitmap = Image.FromFile("j.jpg");

        // Make it all black
        g.Clear(Color.Black);

        // /*************************************************************************/
        // int firstRecX = partitionIValues[0][0];
        // int firstRecY = partitionJValues[0][0];
        // int firstRecWidth = partitionIValues[0][1];
        // int firstRecHight = partitionJValues[0][1];

        // // Rectangles to split bitmap.
        // Rectangle firstRect =
        //     new Rectangle(firstRecX, firstRecY, firstRecWidth, firstRecHight);

        // // Split bitmap with rectangles
        // Bitmap cloneFirstBitmap = bmp.Clone(firstRect, bmp.PixelFormat);

        // // Compress the images and get them from the memory stream.
        // MemoryStream ms = CompressImage(cloneFirstBitmap, 2);
        // var compressedFirstImage = Image.FromStream(ms);

        // // Draw
        // g.DrawImage(compressedFirstImage, new Point(firstRecX, firstRecY));

        // /*************************************************************************/
        // /*************************************************************************/
        // int secRecX = partitionIValues[0][1];
        // int secRecY = partitionJValues[0][0];
        // int secRecWidth = partitionIValues[0][1];
        // int secRecHight = partitionJValues[0][1];
        // Rectangle secondRect =
        //     new Rectangle(secRecX, secRecY, secRecWidth, secRecHight);

        // Bitmap cloneSecondBitmap = bmp.Clone(secondRect, bmp.PixelFormat);

        // ms = CompressImage(cloneSecondBitmap, 2);
        // var compressedSecondImage = Image.FromStream(ms);

        // // Draw the smaller rectangles on the bitmap
        // g.DrawImage(compressedSecondImage, new Point(secRecX, secRecY));

        //
        //
        //
        //
        //
        /*************************************************************************/
        /*************************************************************************/
        /*************************************************************************/
        foreach (var list in partitionedRectangles)
        {
            int x = list[0];
            int y = list[1];
            int width = list[2];
            int height = list[3];
            Rectangle secondRect = new Rectangle(x, y, width, height);

            Bitmap cloneBitmap = bmp.Clone(secondRect, bmp.PixelFormat);

            MemoryStream ms = CompressImage(cloneBitmap, 2);
            var compressedImage = Image.FromStream(ms);

            // Draw the smaller rectangles on the bitmap
            g.DrawImage(compressedImage, new Point(x, y));
        }

        /*************************************************************************/
        /*************************************************************************/
        g.Dispose();

        /*************************************************************************/
        // Create and save the final bitmap.
        finalBitmap
            .Save("New.jpg",
            System.Drawing.Imaging.ImageFormat.Jpeg);
        finalBitmap.Dispose();
    }

    public static void partitionImageAndReturnRectangles(
        ref Bitmap bmp,
        int numThreads,
        ref List<int>[] partitionedRectangles
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

                // Console
                //     .WriteLine("istart: " +
                //     iStart +
                //     " iend: " +
                //     iEnd +
                //     " jstart: " +
                //     jStart +
                //     " jEnd: " +
                //     jEnd);
                recValues.Add (iStart);
                recValues.Add (jStart);
                recValues.Add(iEnd - iStart);
                recValues.Add(jEnd - jStart);
                partitionedRectangles[hello] = recValues;
                ++hello;
            }
        }
        Console.WriteLine("\nRectangle\n");
        foreach (var list in partitionedRectangles)
        {
            foreach (var element in list)
            {
                Console.Write(element + " ");
            }
            Console.WriteLine();
        }
    }

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

        Console.WriteLine("\nI\n");
        foreach (var list in partitionIValues)
        {
            foreach (var element in list)
            {
                Console.Write(element + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("\nJ\n");
        foreach (var list in partitionJValues)
        {
            foreach (var element in list)
            {
                Console.Write(element + " ");
            }
            Console.WriteLine();
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
