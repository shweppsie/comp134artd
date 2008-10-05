using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

using ARTKPManagedWrapper;
using DirectShowLib;
using SnapShot;
using System.Runtime.InteropServices;

namespace projAR
{
    class AR
    {
        //camera object
        Capture cam;

        //wraps AR tracking methods
        IntPtr tracker;

        //stores myMarkerInfos
        public Dictionary<int, MyMarkerInfo> dicMarkerInfos;

        //various variables
        int _width;
        int _height;
        int _bytesPerPixel;
        Guid _sampleGrabberSubType = MediaSubType.ARGB32;
        ArManWrap.PIXEL_FORMAT _arPixelFormat = ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_ABGR;

        //bool for tracking loop and camera update
        private bool cameraUpdated = false;

        /// <summary>
        /// Starts Camera and Initializes AR Variables
        /// </summary>
        /// <param name="control">Control to render webcam to</param>
        /// <param name="width">width of webcam bitmap</param>
        /// <param name="height">height of webcam bitmap</param>
        /// <param name="bytesPerPixel">bytes Per Pixel</param>
        public AR(Control control, int width, int height, int bytesPerPixel)
        {
            //set class globals
            _width = width;
            _height = height;
            _bytesPerPixel = bytesPerPixel;

            //set to the first webcam device
            int deviceNum = 0;

            //make a short for bits per pixel
            short bitsPerPixel = (short)(_bytesPerPixel * 8);

            //start camera
            cam = new SnapShot.Capture(deviceNum, _width, _height, bitsPerPixel, control, _sampleGrabberSubType);

            //pointer to memory 
            IntPtr test = cam.Click();

            //make a bitmap object for the frames and start rendering

            Bitmap b = new Bitmap(cam.Width, cam.Height, cam.Stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, test);

            dicMarkerInfos = new Dictionary<int, MyMarkerInfo>();

            tracker = make_tracker();
        }

        /// <summary>
        /// Clean Up Unsafe Code. This method MUST be called before the program closes.
        /// </summary>
        public void Dispose()
        {
            ArManWrap.ARTKPCleanup(tracker, IntPtr.Zero);
            cam.Dispose();
        }

        /// <summary>
        /// Creates a Tracker
        /// </summary>
        /// <returns>An instance of a tracker</returns>
        private IntPtr make_tracker()
        {
            //LoadImageBytes with raw image only
            IntPtr tracker = ArManWrap.ARTKPConstructTrackerMulti(-1, _width, _height);
            if (tracker == IntPtr.Zero)
            {
                throw new Exception("ARTKPConstructTrackerMulti failed");
            }

            IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
            string desc = Marshal.PtrToStringAnsi(ipDesc);
            System.Console.WriteLine(desc);
            int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)_arPixelFormat);

            //camera clibration datafile
            string cameraCalibrationPath = "data/no_distortion.cal";

            //marker to look for
            string multiPath = "data/markerboard_480-499.cfg";

            int retInit = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, multiPath, 1.0f, 2000.0f);
            if (retInit != 0)
            {
                throw new Exception("ARTKPInitMulti failed");
            }

            bool use_id_bch = false;
            if (use_id_bch == true)
            {
                ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_BCH);
                ArManWrap.ARTKPSetBorderWidth(tracker, 0.250f);
            }
            else //id_simple (supposed to be robust)
            {
                ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_SIMPLE);
                ArManWrap.ARTKPSetBorderWidth(tracker, 0.125f);
            }

            //ArManWrap.ARTKPSetThreshold(tracker, 160);
            bool autoThresh = ArManWrap.ARTKPIsAutoThresholdActivated(tracker);
            ArManWrap.ARTKPActivateAutoThreshold(tracker, true);

            ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
            ArManWrap.ARTKPSetUseDetectLite(tracker, false);
            //ArManWrap.ARTKPSetPoseEstimator(tracker, (int)ArManWrap.POSE_ESTIMATOR.POSE_ESTIMATOR_ORIGINAL_CONT); //POSE_ESTIMATOR_RPP

            return tracker;
        }

        public void Track(out Matrix finalprojMatrix)
        {
            float[] modelViewMatrix = new float[16];
            float[] projMatrix = new float[16];

            dicMarkerInfos.Clear();

            finalprojMatrix = new Matrix();

            try
            {
                //reset all markers to not found
                foreach (MyMarkerInfo mmi in dicMarkerInfos.Values)
                {
                    //should probably only be done after 10 frames or so
                    mmi.found = false;
                }

                //get number of markers and matrices
                int numMarkers = ArManWrap.ARTKPCalcMulti(tracker, flipimage());
                ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
                ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);

                //set up matrices
                Matrix wpfModelViewMatrix = ArManWrap.GetXNAMatrixFromOpenGl(modelViewMatrix);
                float[] matrix = projMatrix;
                int a = 0;
                finalprojMatrix = new Matrix(matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++],
                    matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++],
                    matrix[a++], matrix[a++], matrix[a++], -matrix[a++], matrix[a++]);
                //ArManWrap.GetXNAMatrixFromOpenGl(projMatrix);

                //markers to check
                if (numMarkers > 0)
                {
                    check_markers(numMarkers, dicMarkerInfos);
                }
            }
            catch (Exception ex)
            {
                int lastError = Marshal.GetLastWin32Error();
                MessageBox.Show("lastError : " + lastError.ToString() + "\r\n" + ex.ToString());
            }
        }

        /// <summary>
        /// We have to flip the image for it to be useable in AR
        /// </summary>
        /// <returns>bytes of flipped image</returns>
        private byte[] flipimage()
        {
            byte[] imageBytes = new byte[_width * _height * _bytesPerPixel];
            byte[] flipY = new byte[imageBytes.Length];

            //grab image from camera
            IntPtr ipImage = cam.Click();
            Marshal.Copy(ipImage, imageBytes, 0, imageBytes.Length);
            Marshal.FreeCoTaskMem(ipImage);

            //flip the image along Y - this has to be done
            int srcPixOffset;
            int tarPixOffset;

            for (int col = 0; col < cam.Width; col++)
            {
                for (int row = 0; row < cam.Height; row++)
                {
                    srcPixOffset = GetPixelOffset(row, col, cam.Width, cam.Height, _bytesPerPixel);
                    tarPixOffset = GetPixelOffset(cam.Height - row - 1, col, cam.Width, cam.Height, _bytesPerPixel); ;
                    for (int j = 0; j < _bytesPerPixel; j++)
                    {
                        flipY[tarPixOffset + j] = imageBytes[srcPixOffset + j];
                    }
                }
            }

            return flipY;
        }

        private int GetPixelOffset(int row, int col, int width, int height, int bytesPerPixel)
        {
            return ((row * width) + col) * bytesPerPixel;
        }

        private void check_markers(int numMarkers, Dictionary<int, MyMarkerInfo> dicMarkerInfos)
        {
            Console.WriteLine(numMarkers.ToString() + " markers detected!");
            for (int i = 0; i < numMarkers; i++)
            {
                ArManWrap.ARMarkerInfo armi = ArManWrap.ARTKPGetDetectedMarkerStruct(tracker, i);
                IntPtr markerInfos = ArManWrap.ARTKPGetDetectedMarker(tracker, i); //armi.id);
                float[] center = new float[2];
                float width = 50;
                //float[] matrix = new float[12];
                float[] matrix = new float[16];
                float retTransMat = 0;

                MyMarkerInfo mmi = null;
                //if (dicMarkerInfos.ContainsKey(armi.id) == true)
                //{
                //    mmi = dicMarkerInfos[armi.id];
                //    //make sure the matrix i'm passing in is ordered correctly
                //    retTransMat = ArManWrap.ARTKPGetTransMatCont(tracker, markerInfos, mmi.prevMatrix, center, width, matrix);
                //}
                //else
                {
                    mmi = new MyMarkerInfo();
                    dicMarkerInfos.Add(armi.id, mmi);
                    //retTransMat = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, matrix);
                    ArManWrap.ARTKPGetModelViewMatrix(tracker, matrix);
                }
                Marshal.Release(markerInfos);
                mmi.found = true;
                mmi.notFoundCount = 0;
                mmi.markerInfo = armi;
                mmi.prevMatrix = matrix;
                //mmi.transform = ArManWrap.GetXNAMatrixFromOpenGl12(matrix);
                int a = 0;
                Matrix m = new Matrix(matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++], 
                    matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++], 
                    matrix[a++], matrix[a++], matrix[a++], -matrix[a++], matrix[a++]);
                mmi.transform = m;
            }
        }
    }
}
