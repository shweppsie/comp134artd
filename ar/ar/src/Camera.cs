using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using SnapShot;
using System.Drawing;

namespace projAR
{
    class Camera
    {
        //used for flipping the image
        byte[] imageBytes;
        byte[] flipY;
        short bitsPerPixel;
        short bytesPerPixel;

        //camera object
        Capture cam;

        public Camera(int deviceNum, int _width, int _height, short _bytesPerPixel, Guid _sampleGrabberSubType)
        {
            bytesPerPixel = _bytesPerPixel;

            imageBytes = new byte[_width * _height * _bytesPerPixel];
            flipY = new byte[_width * _height * _bytesPerPixel];

            //start camera
            bitsPerPixel = (short)(_bytesPerPixel * 8);
            cam = new SnapShot.Capture(deviceNum, _width, _height, bitsPerPixel, new System.Windows.Forms.Panel(), _sampleGrabberSubType);

            //pointer to memory 
            IntPtr test = cam.Click();

            //make a bitmap object for the frames and start rendering
            Bitmap b = new Bitmap(cam.Width, cam.Height, cam.Stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, test);

        }

        /// <summary>
        /// We have to flip the image for it to be useable in AR
        /// </summary>
        /// <returns>bytes of flipped image</returns>
        public byte[] GetFlippedImage()
        {
            //grab image from camera
            IntPtr ipImage = cam.Click();
            if (ipImage == IntPtr.Zero)
                return null;

            Marshal.Copy(ipImage, imageBytes, 0, imageBytes.Length);
            Marshal.FreeCoTaskMem(ipImage);

            //flip the image along Y - this has to be done
            int srcPixOffset;
            int tarPixOffset;

            for (int col = 0; col < cam.Width; col++)
            {
                for (int row = 0; row < cam.Height; row++)
                {
                    srcPixOffset = GetPixelOffset(row, col, cam.Width, cam.Height, bytesPerPixel);
                    tarPixOffset = GetPixelOffset(cam.Height - row - 1, col, cam.Width, cam.Height, bytesPerPixel); ;
                    for (int j = 0; j < bytesPerPixel; j++)
                    {
                        flipY[tarPixOffset + j] = imageBytes[srcPixOffset + j];
                    }
                }
            }

            return flipY;
        }

        /// <summary>
        /// helper pixel function
        /// </summary>
        private int GetPixelOffset(int row, int col, int width, int height, int bytesPerPixel)
        {
            return ((row * width) + col) * bytesPerPixel;
        }

        public void Dispose()
        {
            cam.Dispose();
        }
    }
}
