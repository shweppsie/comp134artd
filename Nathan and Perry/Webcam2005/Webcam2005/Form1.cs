using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DirectShowLib;

namespace WebcamFeed
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        //create an instance of the class used to capture an image from cam
        SnapShot.Capture Bob = null;

        private void button2_Click(object sender, EventArgs e)
        {
            //initialise webcam capture class, args are (device number [use 0 for default camera], width, height, control to draw to, MediaSubType.ARGB32 [has to be there])
            Bob = new SnapShot.Capture(0, 320, 240, 4, panel, MediaSubType.ARGB32);

            //pointer to the blocks of memory that the webcam captures an image to
            IntPtr test = Bob.Click();

            //creates a bitmap from the captured image
            //Bob.Stride is always required, replace Bob with the name of your Snapshot.Capture class
            //System.Drawing.Imaging.PixelFormat.Format24bppRgb  is always needed
            //test is the IntPtr returned by Bob.Click(), which is the block of memory to get the bitmap from
            Bitmap b = new Bitmap(Bob.Width, Bob.Height, Bob.Stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb , test);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Bob.Dispose();
        }
    }
}
