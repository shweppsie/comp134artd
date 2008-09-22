using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using ARTKPManagedWrapper;

namespace ImageRecognise
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SingleIdSimple();
        }

        private void SingleIdSimple()
        {
            try
            {
                //image being tracked
                pictureBox1.Image = Image.FromFile("data/image_320_240_8_marker_id_simple_nr031.jpg");
                //get the raw sample image bits the same way the sample does
                //this will be done differently when using a webcam feed
                string imagePath = "data/image_320_240_8_marker_id_simple_nr031.raw";
                int imageWidth = 320;
                int imageHeight = 240;
                int bytesPerPixel = 1;
                byte[] imageBytes = new byte[imageWidth * imageHeight * bytesPerPixel];
                int retVal = -1;
                int numberOfBytesRead = ArManWrap.ARTKPLoadImagePath(imagePath, imageWidth, imageHeight, bytesPerPixel, imageBytes);
                if (numberOfBytesRead <= 0)
                {
                    throw new Exception("image not loaded");
                }
                //create the AR Tracker for finding a single marker
                IntPtr tracker = ArManWrap.ARTKPConstructTrackerSingle(-1, imageWidth, imageHeight);
                if (tracker == IntPtr.Zero)
                {
                    throw new Exception("tracker construction failed");
                }
                //get the Tracker description
                IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
                string desc = Marshal.PtrToStringAnsi(ipDesc);
                //set pixel format of sample image
                int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_LUM);
                //init tracker with camera calibration file, near plane, and far plane
                string cameraCalibrationPath = "data/LogitechPro4000.dat";
                retVal = ArManWrap.ARTKPInit(tracker, cameraCalibrationPath, 1.0f, 1000.0f);
                if (retVal != 0)
                {
                    throw new Exception("tracker not initialized");
                }
                //set pattern width of markers (millimeters)
                ArManWrap.ARTKPSetPatternWidth(tracker, 80);
                //set border width percentage of marker (.25 is a huge border)
                ArManWrap.ARTKPSetBorderWidth(tracker, 0.250f);
                //set lighting threshold. this could be automatic
                ArManWrap.ARTKPSetThreshold(tracker, 150);
                //set undistortion mode
                ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
                //set tracker to look for simple ID-based markers
                ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_SIMPLE);
                //now that tracker is finally setup ... find the marker
                //in a video based app, setup will happen once and then marker detection will happen in a loop
                int pattern = -1;
                bool updateMatrix = true;
                IntPtr markerInfos = IntPtr.Zero;
                int numMarkers;
                int markerId = ArManWrap.ARTKPCalc(tracker, imageBytes, pattern, updateMatrix, out markerInfos, out numMarkers);
                //clear any markers that already exist in Viewport3D
                //modelMarkers.Children.Clear();
                if (numMarkers == 1)
                {
                    //add rectangle marker to 3D scene at the origin
                    //AddMarker(Brushes.Cyan);
                    //marshal the MarkerInfo from native to managed
                    ArManWrap.ARMarkerInfo markerInfo = (ArManWrap.ARMarkerInfo)Marshal.PtrToStructure(markerInfos, typeof(ArManWrap.ARMarkerInfo));
                    float[] center = new float[] { 0, 0 };
                    float width = 50;
                    float[] markerMatrix = new float[12];
                    //determine how marker is related to camera
                    //just getting the data for kicks here ... not actually using it
                    //in this sample, the transformations are only applied to the camera and the marker stays at the origin
                    //alternately, the camera could be left at the origin and the marker(s) could be transformed
                    float retTransMat = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, markerMatrix);
                    Marshal.DestroyStructure(markerInfos, typeof(ArManWrap.ARMarkerInfo));
                }
                //how confident is the marker tracking?
                float conf = ArManWrap.ARTKPGetConfidence(tracker);
                //get model view matrix
                float[] modelViewMatrix = new float[16];
                ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
                //get projection matrix
                float[] projMatrix = new float[16];
                ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);
                //dispose of tracker
                ArManWrap.ARTKPCleanup(tracker, IntPtr.Zero);

                string text = "Confidence: " + conf + "\r\n";

                text += "Location";
                foreach (float i in modelViewMatrix)
                {
                    text += ": " + i + " ";
                }
                text += "\r\n";

                text += "Projection";
                foreach (float i in projMatrix)
                    text += ": " + i + " ";
                text += "\r\n";

                textBox1.Text = text;

                //##OLD WPF STUFF
                //apply model view matrix to MatrixCamera
                //Matrix3D wpfModelViewMatrix = ArManWrap.GetWpfMatrixFromOpenGl(modelViewMatrix);
                //matrixCamera.ViewMatrix = wpfModelViewMatrix;
                //apply projection matrix to MatrixCamera
                //Matrix3D wpfProjMatrix = ArManWrap.GetWpfMatrixFromOpenGl(projMatrix);
                //matrixCamera.ProjectionMatrix = wpfProjMatrix;
            }
            catch (Exception ex)
            {
                int lastError = Marshal.GetLastWin32Error();
                MessageBox.Show("lastError : " + lastError.ToString() + "\r\n" + ex.ToString());
            }
        }
    }
}
