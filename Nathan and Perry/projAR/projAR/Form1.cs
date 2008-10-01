using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

using ARTKPManagedWrapper;
using DirectShowLib;
using WPFUtil;

namespace projAR
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DsDevice[] vidCapDev = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            foreach (DsDevice dd in vidCapDev)
            {
                cbDevices.Items.Add(dd.Name);
            }
            if (vidCapDev.Length >= 1)
            {
                cbDevices.SelectedIndex = 0;
            }
        }

        int _width = 640;
        int _height = 480;
        int _bytesPerPixel = 4;
        int deviceNum = 0;
        Guid _sampleGrabberSubType = MediaSubType.ARGB32; //.RGB24;
        ArManWrap.PIXEL_FORMAT _arPixelFormat = ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_ABGR;

        #region webcam
        SnapShot.Capture cam = null;

        private void button1_Click(object sender, EventArgs e)
        {
            //make a short for bits per pixel
            short bitsPerPixel = (short)(_bytesPerPixel * 8);

            //start camera
            cam = new SnapShot.Capture(deviceNum, _width, _height, bitsPerPixel, panel1, _sampleGrabberSubType);
            
            //pointer to memory 
            IntPtr test = cam.Click();
            Bitmap b = new Bitmap(cam.Width, cam.Height, cam.Stride, PixelFormat.Format24bppRgb, test);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cam.Dispose();
        }
        #endregion

        #region ARStuff
        public void StartTracking()
        {
            //clear out any data from previous tracking
            StopTracking();

            //add a tracker
            TrackDelegate trackDelegate = new TrackDelegate(Track);
            //start the tracker
            trackDelegate.BeginInvoke(null, null);
        }


        public void StopTracking()
        {
            _runTracking = false;
            //modelMarkers.Children.Clear();
        }

        private int GetPixelOffset(int row, int col, int width, int height, int bytesPerPixel)
        {
            return ((row * width) + col) * bytesPerPixel;
        }

        private bool _runTracking = true;

        private delegate void TrackDelegate();
        private void Track()
        {
            try
            {
                //LoadImageBytes with raw image only
                //TODO be able to pass parameters into Tracker template
                IntPtr tracker = ArManWrap.ARTKPConstructTrackerMulti(-1, _width, _height);
                if (tracker == IntPtr.Zero)
                {
                    throw new Exception("ARTKPConstructTrackerMulti failed");
                }

                IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
                string desc = Marshal.PtrToStringAnsi(ipDesc);
                System.Console.WriteLine(desc);
                int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)_arPixelFormat); //MOD

                //string cameraCalibrationPath = "data/LogitechPro4000.dat";
                string cameraCalibrationPath = "data/no_distortion.cal";
                //ARToolKitPlus_CamCal_Rev02
                //640 480 320 240 1500.0 1500.0 0.0 0.0 0.0 0.0 0.0 0.0 0

                string multiPath = "data/markerboard_480-499.cfg";
                /*
                # multimarker definition file for ARToolKit (format defined by ARToolKit)
                # dataset for test Painting application

                # number of markers
                20

                # marker 0
                480 (this is the id)
                40.0 (probably the width)
                0.0 0.0 (probably the center)
                 1.0000  0.0000 0.0000 -100.0
                 0.0000  1.0000 0.0000   75.0
                 0.0000  0.0000 1.0000    0.0

                # marker 1
                ...
                */

                int retInit = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, multiPath, 1.0f, 2000.0f);
                //retVal = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, null, 1.0f, 1000.0f); //crashes
                if (retInit != 0)
                {
                    throw new Exception("ARTKPInitMulti failed");
                }
                //11mm border (total), 5.5m each side, pattern width is 43mm
                //11 / 43 = .25
                //5.5 / 43 = .125
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

                byte[] imageBytes = new byte[_width * _height * _bytesPerPixel];
                byte[] flipY = new byte[imageBytes.Length];

                float[] modelViewMatrix = new float[16];
                float[] projMatrix = new float[16];

                object[] args = new object[2];
                //UpdateViewportDelegate updateViewDel = new UpdateViewportDelegate(UpdateViewport);

                Dictionary<int, MyMarkerInfo> dicMarkerInfos = new Dictionary<int, MyMarkerInfo>();

                _runTracking = true;
                while (_runTracking) //TODO clean this up
                {
                    try
                    {
                        //reset all markers to not found
                        foreach (MyMarkerInfo mmi in dicMarkerInfos.Values)
                        {
                            mmi.found = false;
                        }

                        IntPtr ipImage = cam.Click();
                        Marshal.Copy(ipImage, imageBytes, 0, imageBytes.Length);
                        Marshal.FreeCoTaskMem(ipImage); //NOTE Marshal.Release was no good here
                        //NOTE flip the image along Y - this has to be done
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

                        ////save test System.Drawing bitmap
                        //System.Drawing.Bitmap b = new System.Drawing.Bitmap(cam.Width, cam.Height, cam.Stride,
                        //    _bmpSavePixelFormat, ipImage);
                        //b.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
                        ////b.Save("c:\\test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                        ////System.Drawing.Bitmap b = new System.Drawing.Bitmap(imagePath);
                        //System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, b.Width, b.Height);
                        //System.Drawing.Imaging.PixelFormat pf = b.PixelFormat;
                        //System.Drawing.Imaging.BitmapData bmpData = b.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, pf);
                        //IntPtr ptr = bmpData.Scan0;
                        //int numBytes = b.Width * b.Height * _bytesPerPixel;
                        //imageBytes = new byte[numBytes];
                        //System.Runtime.InteropServices.Marshal.Copy(ptr, imageBytes, 0, imageBytes.Length);
                        //b.UnlockBits(bmpData);

                        ////get test WPF bitmap
                        //BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(b.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                        //    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                        //MemoryStream ms = new MemoryStream();

                        int numMarkers = ArManWrap.ARTKPCalcMulti(tracker, flipY); //uses ArDetectMarker internally (unless set to Lite)
                        ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
                        ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);

                        //this will change if AutoThreshold is on
                        //int threshold = ArManWrap.ARTKPGetThreshold(tracker);
                        //System.Console.WriteLine("threshold " + threshold.ToString());

                        Matrix wpfModelViewMatrix = convert(modelViewMatrix);
                        Matrix wpfProjMatrix = convert(projMatrix);

                        //ArManWrap.DumpMatrix("projection matrix", wpfProjMatrix);
                        //ArManWrap.DumpMatrix("modelView matrix", wpfModelViewMatrix);

                        //markers
                        if (numMarkers > 0)
                        {
                            //MessageBox.Show(numMarkers.ToString() + " markers detected!");
                            for (int i = 0; i < numMarkers; i++)
                            {
                                ArManWrap.ARMarkerInfo armi = ArManWrap.ARTKPGetDetectedMarkerStruct(tracker, i);
                                IntPtr markerInfos = ArManWrap.ARTKPGetDetectedMarker(tracker, i); //armi.id);
                                float[] center = new float[2];
                                float width = 50;
                                float[] matrix = new float[12];
                                float retTransMat = 0;

                                MyMarkerInfo mmi = null;
                                if (dicMarkerInfos.ContainsKey(armi.id) == true)
                                {
                                    mmi = dicMarkerInfos[armi.id];
                                    //make sure the matrix i'm passing in is ordered correctly
                                    retTransMat = ArManWrap.ARTKPGetTransMatCont(tracker, markerInfos, mmi.prevMatrix, center, width, matrix);
                                }
                                else
                                {
                                    mmi = new MyMarkerInfo();
                                    dicMarkerInfos.Add(armi.id, mmi);
                                    retTransMat = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, matrix);
                                }
                                Marshal.Release(markerInfos);
                                mmi.found = true;
                                mmi.notFoundCount = 0;
                                mmi.markerInfo = armi;
                                mmi.prevMatrix = matrix;
                                Matrix m3d = convert(matrix);
                                mmi.transform = m3d;
                            }
                        }

                        args[0] = wpfModelViewMatrix;
                        args[1] = dicMarkerInfos;
                        //get back on UI thread
                        //this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, updateViewDel, wpfProjMatrix, args);
                    }
                    catch (Exception ex)
                    {
                        int lastError = Marshal.GetLastWin32Error();
                        MessageBox.Show("lastError : " + lastError.ToString() + "\r\n" + ex.ToString());
                    }
                }

                ArManWrap.ARTKPCleanup(tracker, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public class MyMarkerInfo
        {
            public Matrix transform;
            public ArManWrap.ARMarkerInfo markerInfo;
            public float[] prevMatrix = new float[0];
            public bool found = false;
            //public ModelVisual3D modelVisual3D = null;
            public int notFoundCount = 0;
        }

        private bool cameraUpdated = false;

        //private delegate void UpdateViewportDelegate(Matrix projMatrix, Matrix modelViewMatrix, Dictionary<int, MyMarkerInfo> markerInfos);
        //private void UpdateViewport(Matrix projMatrix, Matrix modelViewMatrix, Dictionary<int, MyMarkerInfo> markerInfos)
        //{
        //    if (cameraUpdated == false)
        //    {
        //        //TODO fix the far plane distance?
        //        //ArManWrap.DumpMatrix("projection ", projMatrix);
        //        //matrixCamera.ProjectionMatrix = projMatrix;
        //        //matrixCamera.ViewMatrix = Matrix.Identity;
        //        //cameraUpdated = true;
        //    }
        //    //modelMarkers.Children.Clear();
        //    List<int> delKeys = new List<int>();
        //    foreach (MyMarkerInfo mmi in markerInfos.Values)
        //    {
        //        if (mmi.found == false)
        //        {
        //            //to help with Viewport3D flicker when model is removed and then added again
        //            //If the virtual image does not appear, or it flickers in and out of view 
        //            //it may be because of lighting conditions. 
        //            //This can often be fixed by changing the lighting threshold value used by the image processing routines.
        //            if (mmi.notFoundCount >= 10) //TODO hardcoded value is a WAG
        //            {
        //                if (mmi.modelVisual3D is IModelCode)
        //                {
        //                    IModelCode imc = (IModelCode)mmi.modelVisual3D;
        //                    imc.Stop();
        //                }
        //                modelMarkers.Children.Remove(mmi.modelVisual3D);
        //                delKeys.Add(mmi.markerInfo.id);
        //                //System.Console.WriteLine("removing " + mmi.markerInfo.id.ToString());
        //            }
        //            else
        //            {
        //                mmi.notFoundCount = mmi.notFoundCount + 1;
        //                //System.Console.WriteLine("not found " + mmi.markerInfo.id.ToString());
        //            }
        //        }
        //        else
        //        {
        //            if (mmi.modelVisual3D == null)
        //            {
        //                //ModelVisual3D mv3d = AddMarker(Brushes.Cyan);
        //                ModelVisual3D mv3d = AddMarkerModel(mmi.markerInfo.id);
        //                mmi.modelVisual3D = mv3d;
        //                MatrixTransform3D mt3d = new MatrixTransform3D(mmi.transform);
        //                mv3d.Transform = mt3d;
        //                System.Console.WriteLine("adding " + mmi.markerInfo.id.ToString());
        //            }
        //            else
        //            {
        //                MatrixTransform3D mt3d = (MatrixTransform3D)mmi.modelVisual3D.Transform;
        //                mt3d.Matrix = mmi.transform;
        //                //System.Console.WriteLine("updating " + mmi.markerInfo.id.ToString());
        //            }
        //            //MatrixTransform3D mt3d = new MatrixTransform3D(mmi.transform);
        //            //mmi.modelVisual3D.Transform = mt3d;
        //        }
        //    }
        //    foreach (int delKey in delKeys)
        //    {
        //        markerInfos.Remove(delKey);
        //    }
        //}

        public Matrix convert(float[] mat)
        {
            Matrix m3d = new Matrix();
            m3d.M11 = mat[0];
            m3d.M12 = mat[1];
            m3d.M13 = mat[2];
            m3d.M14 = mat[3];
            m3d.M21 = mat[4];
            m3d.M22 = mat[5];
            m3d.M23 = mat[6];
            m3d.M24 = mat[7];
            m3d.M31 = mat[8];
            m3d.M32 = mat[9];
            m3d.M33 = mat[10];
            m3d.M34 = mat[11];
            m3d.M44 = mat[15];
            return m3d;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartTracking();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StopTracking();
        }
        #endregion
    }
}