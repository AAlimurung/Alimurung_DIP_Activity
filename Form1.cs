using ImageProcess2;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WebCamLib;
using System.Windows.Forms;
using System.Collections.Generic;
using AForge.Video;
using System.Threading.Tasks;
using AForge.Video.DirectShow;
using System;
using System.Data;
using System.ComponentModel;

namespace DIP_Activity
{
    public partial class Form1 : Form
    {
        Bitmap? loaded;
        Bitmap? processed;
        Bitmap? subtracted;

        System.Windows.Forms.Timer currentTimer;
        Device[] devices;

        public Form1()
        {
            InitializeComponent();
        }

        //fetch image from system
        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pictureBox1.Image = loaded = new Bitmap(openFileDialog1.FileName);
        }

        //dialogue box
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        //pixel copy
        private void pixelCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            processed = new Bitmap(loaded.Width, loaded.Height);

            for (int i = 0; i < loaded.Width; i++)
                for (int j = 0; j < loaded.Height; j++)
                    processed.SetPixel(i, j, loaded.GetPixel(i, j));

            pictureBox2.Image = processed;
        }

        //dialogue box
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        //save image 
        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (processed == null)
                return;

            processed.Save(saveFileDialog1.FileName);
        }

        //greyscales of image
        private void grayscalingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            Bitmap copy = (Bitmap)loaded.Clone();
            BitmapFilter.GrayScale(copy);
            pictureBox2.Image = copy;
        }

        //inversion of colors (may not be correct)
        private void inversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            Bitmap copy = (Bitmap)loaded.Clone();
            BitmapFilter.Invert(copy);
            pictureBox2.Image = copy;
        }

        //mirror image horizontal
        private void mirrorHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            processed = new Bitmap(loaded.Width, loaded.Height);

            int width = loaded.Width;
            int height = loaded.Height;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    processed.SetPixel(width - i - 1, j, loaded.GetPixel(i, j));

            pictureBox2.Image = processed;
        }

        //mirror image vertically
        private void mirrorVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            processed = new Bitmap(loaded.Width, loaded.Height);

            int width = loaded.Width;
            int height = loaded.Height;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    processed.SetPixel(i, height - j - 1, loaded.GetPixel(i, j));

            pictureBox2.Image = processed;
        }

        //grayscale to uploaded image
        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            int[] histData = new int[256];
            Color pixel;
            int maxFreq = 420;

            for (int i = 0; i < loaded.Width; i++)
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    int ave = (pixel.R + pixel.G + pixel.B) / 3;
                    histData[ave]++;

                    if (histData[ave] > maxFreq)
                        maxFreq = histData[ave];
                }

            processed = new Bitmap(256, 420);
            int mFactor = maxFreq / 420;
            int count;

            for (int i = 0; i < 256; i++)
            {
                count = Math.Min(420, histData[i] / mFactor);

                for (int j = 0; j < count; j++)
                    processed.SetPixel(i, 419 - j, Color.Black);
            }

            pictureBox2.Image = processed;
        }

        //brightness to uploaded image
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            Bitmap copy = (Bitmap)loaded.Clone();

            BitmapFilter.Brightness(copy, trackBar1.Value);

            pictureBox2.Image = copy;

        }

        //contrast to uploaded image
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            Bitmap copy = (Bitmap)loaded.Clone();
            BitmapFilter.Contrast(copy, (SByte)trackBar2.Value);

            pictureBox2.Image = copy;

        }

        //sepia to uploaded image
        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            processed = new Bitmap(loaded.Width, loaded.Height);

            Color pixel;
            for (int i = 0; i < loaded.Width; i++)
                for (int j = 0; j < loaded.Height; j++)
                {
                    pixel = loaded.GetPixel(i, j);
                    processed.SetPixel(i, j,
                        Color.FromArgb(
                            (int)Math.Min(pixel.R * 0.393 + pixel.G * 0.769 + pixel.B * 0.189, 255),
                            (int)Math.Min(pixel.R * 0.349 + pixel.G * 0.686 + pixel.B * 0.168, 255),
                            (int)Math.Min(pixel.R * 0.272 + pixel.G * 0.534 + pixel.B * 0.131, 255)
                            )
                        );
                }

            pictureBox2.Image = processed;
        }

        //dialogue box
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        //dialogue box
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        //subtract bg and uploaded image
        private void button3_Click(object sender, EventArgs e)
        {
            if (loaded == null || processed == null)
                return;

            int greyGreen = 255 / 3;
            int threshold = 5;

            Color pixel;
            subtracted = new Bitmap(loaded.Width, loaded.Height);

            for (int i = 0; i < loaded.Width; i++)
            {
                if (i >= processed.Width)
                    break;

                for (int j = 0; j < loaded.Height; j++)
                {
                    if (j >= processed.Height)
                        break;

                    pixel = loaded.GetPixel(i, j);

                    subtracted.SetPixel(i, j,
                        Math.Abs((pixel.R + pixel.G + pixel.B) / 3 - greyGreen) < threshold ?
                        processed.GetPixel(i, j) : pixel
                        );
                }
            }
            pictureBox3.Image = subtracted;
        }

        //load image as green screen bg
        private void openFileDialog2_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pictureBox2.Image = processed = new Bitmap(openFileDialog2.FileName);
        }

        //rotate the image based on the degree
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            processed = new Bitmap(loaded.Width, loaded.Height);

            float radians = trackBar3.Value * (float)Math.PI / 180f;
            int centerX = loaded.Width / 2;
            int centerY = loaded.Height / 2;
            float cosA = (float)Math.Cos(radians);
            float sinA = (float)Math.Sin(radians);

            for (int i = 0; i < loaded.Width; ++i)
                for (int j = 0; j < loaded.Height; ++j)
                {
                    int translatedX = i - centerX;
                    int translatedY = j - centerY;

                    int newX = (int)(translatedX * cosA - translatedY * sinA) + centerX;
                    int newY = (int)(translatedX * sinA + translatedY * cosA) + centerY;

                    processed.SetPixel(i, j,
                            newX >= 0 && newX < loaded.Width && newY >= 0 && newY < loaded.Height ?
                            loaded.GetPixel(newX, newY) : Color.Transparent
                        );
                }

            pictureBox2.Image = processed;
        }

        //scaling of image
        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            int newWidth = (int)(trackBar4.Value / 50f * loaded.Width);
            int newHeight = (int)(trackBar4.Value / 50f * loaded.Height);

            processed = new Bitmap(newWidth, newHeight);

            for (int i = 0; i < newWidth; ++i)
                for (int j = 0; j < newHeight; ++j)
                    processed.SetPixel(i, j, loaded.GetPixel(
                        i * loaded.Width / newWidth,
                        j * loaded.Height / newHeight
                        )
                     );

            pictureBox2.Image = processed;
        }

        //thresholding of the uploaded image
        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            if (loaded == null)
                return;

            processed = new Bitmap(loaded.Width, loaded.Height);
            int threshold = trackBar5.Value;

            for (int i = 0; i < loaded.Width; ++i)
                for (int j = 0; j < loaded.Height; ++j)
                {
                    Color pixel = loaded.GetPixel(i, j);
                    processed.SetPixel(i, j,
                        (pixel.R + pixel.G + pixel.B) / 3 < threshold ?
                        Color.Black : Color.White
                        );
                }

            pictureBox2.Image = processed;

        }

        //fetch all connected devices
        private void Form1_Load(object sender, EventArgs e)
        {
            devices = DeviceManager.GetAllDevices();
        }

        //render image to box 1
        private void onToolStripMenuItem_Click(object sender, EventArgs e)
        {
            devices[0].ShowWindow(pictureBox1);
        }

        //stop device video processing
        private void offToolStripMenuItem_Click(object sender, EventArgs e)
        {
            devices[0].Stop();
        }

        //timer for subtraction
        private void subtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentTimer != null)
                currentTimer.Enabled = false;

            currentTimer = timer1;
            currentTimer.Enabled = true;
        }

        //device video fetching
        private Image getData()
        {
            IDataObject data;
            devices[0].Sendmessage();
            data = Clipboard.GetDataObject();
            return (Image)data.GetData("System.Drawing.Bitmap", true);
        }

        //subtraction method
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (processed == null)
                return;

            Image bmap = getData();

            if (bmap != null)
            {
                loaded = new Bitmap(bmap);
                subtracted = new Bitmap(loaded.Width, loaded.Height);

                BitmapData bmLoaded = loaded.LockBits(
                    new Rectangle(0, 0, loaded.Width, loaded.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb
                    );

                BitmapData bmProcessed = processed.LockBits(
                    new Rectangle(0, 0, processed.Width, processed.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb
                    );

                BitmapData bmSubtracted = subtracted.LockBits(
                    new Rectangle(0, 0, subtracted.Width, subtracted.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb
                    );

                int limitAve = 255 / 3;
                int threshold = 5;

                unsafe
                {
                    int paddingLoaded = bmLoaded.Stride - loaded.Width * 3;
                    int paddingProcessed = bmProcessed.Stride - processed.Width * 3;
                    int paddingSubtracted = bmSubtracted.Stride - subtracted.Width * 3;

                    byte * pLoaded = (byte *)bmLoaded.Scan0;
                    byte * pProcessed = (byte *)bmProcessed.Scan0;
                    byte * pSubtracted = (byte *)bmSubtracted.Scan0;

                    byte* start_p_processed = (byte*)bmProcessed.Scan0;
                    
                    for (int i = 0; 
                        i < loaded.Height; 
                        i++, pLoaded += paddingLoaded, pSubtracted += paddingSubtracted)
                    {
                        for (int j = 0; 
                            j < loaded.Width; 
                            j++, pLoaded += 3, pSubtracted += 3)
                        {
                            if (Math.Abs(pLoaded[0] + pLoaded[1] + pLoaded[2] - limitAve) < threshold)
                            {
                                pSubtracted[0] = pProcessed[0];
                                pSubtracted[1] = pProcessed[1];
                                pSubtracted[2] = pProcessed[2];
                            }
                            else
                            {
                                pSubtracted[0] = pLoaded[0];
                                pSubtracted[1] = pLoaded[1];
                                pSubtracted[2] = pLoaded[2];
                            }

                            if (j < processed.Width)
                                pProcessed += 3;
                        }

                        if (i < processed.Height)
                            pProcessed = start_p_processed + i * 3;
                    }
                }

                loaded.UnlockBits(bmLoaded);
                processed.UnlockBits(bmProcessed);
                subtracted.UnlockBits(bmSubtracted);

                pictureBox3.Image = subtracted;
            }
        }

        //copy timer 
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentTimer != null)
                currentTimer.Enabled = false;

            currentTimer = timer2;
            currentTimer.Enabled = true;
        }

        //copy from video frames
        private void timer2_Tick(object sender, EventArgs e)
        {
            Image bmap = getData();

            if (bmap != null)
                pictureBox2.Image = bmap;
        }

        //grayscale timers
        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentTimer != null)
                currentTimer.Enabled = false;

            currentTimer = timer3;
            currentTimer.Enabled = true;
        }

        //grayscales
        private void timer3_Tick(object sender, EventArgs e)
        {
            Image image = getData();

            if (image != null)
            {
                processed = new Bitmap(image);
                BitmapFilter.GrayScale(processed);
                pictureBox2.Image = processed;
            }
        }

        //mirror frames (horizontal)
        private void mirrorHorizontalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (currentTimer != null)
                currentTimer.Enabled = false;

            currentTimer = timer4;
            currentTimer.Enabled = true;
        }

        //mirroring
        private void timer4_Tick(object sender, EventArgs e)
        {
            Image image = getData();

            if (image != null)
            {
                processed = new Bitmap(image);

                BitmapFilter.Flip(processed, true, false);

                pictureBox2.Image = processed;
            }
        }

        //mirror frames (vertical)
        private void mirrorVerticalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (currentTimer != null)
                currentTimer.Enabled = false;

            currentTimer = timer5;
            currentTimer.Enabled = true;
        }

        //mirrorings
        private void timer5_Tick(object sender, EventArgs e)
        {
            Image image = getData();

            if (image != null)
            {
                processed = new Bitmap(image);

                BitmapFilter.Flip(processed, false, true);  

                pictureBox2.Image = processed;
            }
        }

        //histogram timer
        private void histogramToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (currentTimer != null)
                currentTimer.Enabled = false;

            currentTimer = timer6;
            currentTimer.Enabled = true;
        }

        //render histogram from frames
        private void timer6_Tick(object sender, EventArgs e)
        {
            Image image = getData();

            if (image != null)
            {
                loaded = new Bitmap(image);
                processed = new Bitmap(256, 420);

                BitmapData bmLoaded = loaded.LockBits(
                    new Rectangle(0, 0, loaded.Width, loaded.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                BitmapData bmProcessed = processed.LockBits(
                    new Rectangle(0, 0, processed.Width, processed.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                unsafe
                {
                    int[] histData = new int[256];
                    int maxFreq = 420;
                    int offSet = bmLoaded.Stride - loaded.Width * 3; // Padding value

                    byte * p = (byte *)(void *)bmLoaded.Scan0;

                    for (int i = 0; i < loaded.Height; i++)
                    {
                        for (int j = 0; j < loaded.Width; j++)
                        {
                            int ave = (int)(.299 * p[2] + .587 * p[1] + .114 * p[0]);
                            p[0] = p[1] = p[2] = (byte)ave;
                            histData[ave]++;

                            if (histData[ave] > maxFreq)
                                maxFreq = histData[ave];

                            p += 3;
                        }

                        p += offSet;
                    }

                    int mFactor = maxFreq / 420;
                    int count;

                    int lastRow = (processed.Height - 1) * bmProcessed.Stride;
                    byte * pointerLast = (byte*)(void*)bmProcessed.Scan0 + lastRow;

                    for (int i = 0; i < 256; i++)
                    {
                        p = pointerLast + i * 3;
                        count = Math.Min(420, histData[i] / mFactor);

                        for (int j = 0; j < count; j++)
                        {
                            p[0] = p[1] = p[2] = 255;

                            p -= bmProcessed.Stride;
                        }
                    }
                }

                loaded.UnlockBits(bmLoaded);
                processed.UnlockBits(bmProcessed);

                pictureBox2.Image = processed;
            }
        }

        //sepia timer 
        private void sepiaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (currentTimer != null)
                currentTimer.Enabled = false;

            currentTimer = timer7;
            currentTimer.Enabled = true;
        }

        //sepia
        private void timer7_Tick(object sender, EventArgs e)
        {
            Image image = getData();

            if (image != null)
            {
                processed = new Bitmap(image);

                BitmapData bmProcessed = processed.LockBits(
                    new Rectangle(0, 0, processed.Width, processed.Height),
                    ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                int offSet = bmProcessed.Stride - processed.Width * 3;

                unsafe
                {
                    byte* p = (byte*)(void*)bmProcessed.Scan0;

                    for (int i = 0; i < processed.Height; i++)
                    {
                        for (int j = 0; j < processed.Width; j++)
                        {
                            p[0] = (byte)Math.Min(255, p[2] * 0.272 + p[1] * 0.534 + p[0] * 0.131);
                            p[1] = (byte)Math.Min(255, p[2] * 0.349 + p[1] * 0.686 + p[0] * 0.168);
                            p[2] = (byte)Math.Min(255, p[2] * 0.393 + p[1] * 0.769 + p[0] * 0.189);

                            p += 3;
                        }

                        p += offSet;
                    }
                }

                processed.UnlockBits(bmProcessed);

                pictureBox2.Image = processed;
            }
        }
    }
        //access webcam through aforge (cannot use the webcamlib due to issues)
    private void onButtonClick(object sender, EventArgs e)
    {
        FilterInfoCollection filter;
        VideoCaptureDevice videv;

        filter = new FilterInfoCollection(FilterCategory.VideoInputDevice);

        foreach (FilterInfo device in filter) 
        {
           
        }
    }
}
