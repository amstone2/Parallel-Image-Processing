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
        Bitmap myBitmap = new Bitmap("j.jpg");


        Rectangle firstRect = new Rectangle(0, 0, 256, 512);
        Rectangle secondRect = new Rectangle(256, 0, 256, 512);

        Bitmap cloneFirstBitmap =
            myBitmap.Clone(firstRect, myBitmap.PixelFormat);
        Bitmap cloneSecondBitmap =
            myBitmap.Clone(secondRect, myBitmap.PixelFormat);


        MemoryStream ms = CompressImage(cloneFirstBitmap, 2);
        var compressedFirstImage = Image.FromStream(ms);


        ms = CompressImage(cloneSecondBitmap, 2);
        var compressedSecondImage = Image.FromStream(ms);

        int width = myBitmap.Width;
        int height = myBitmap.Height;

        Bitmap img3 = new Bitmap(512, 512);

        Graphics g = Graphics.FromImage(img3);

        Image img1 = Image.FromFile("j.jpg");

        g.Clear(Color.Black);
        g.DrawImage(compressedFirstImage, new Point(0, 0));
        g.DrawImage(compressedSecondImage, new Point(256, 0));

        // g.DrawImage(cloneSecondBitmap, new Point(0, 0));
        g.Dispose();

        img3.Save("CompressedImage.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        img3.Dispose();
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
