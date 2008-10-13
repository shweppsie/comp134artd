using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ARTKPManagedWrapper;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using DirectShowLib;
using System.IO;
using System.Reflection;

namespace projAR
{
    class Tracker
    {
        //wraps AR tracking methods
        IntPtr tracker;

        //directory AR is running from
        string arDir;

        //various variables
        short bitsPerPixel;
        int width;
        int height;
        int bytesPerPixel;
        Guid sampleGrabberSubType;
        ArManWrap.PIXEL_FORMAT arPixelFormat;

        /// <summary>
        /// Starts Camera and Initializes AR Variables
        /// </summary>
        /// <param name="control">Control to draw to</param>
        public Tracker(int _width, int _height, int _bytesperpixel, Guid _sampleGrabberSubType, ArManWrap.PIXEL_FORMAT _arPixelFormat)
        {
            width = _width;
            height = _height;
            bytesPerPixel = _bytesperpixel;
            sampleGrabberSubType = _sampleGrabberSubType;
            arPixelFormat = _arPixelFormat;

            arDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);

            //make a short for bits per pixel
            bitsPerPixel = (short)(bytesPerPixel * 8);
            
            tracker = make_tracker();
        }

        /// <summary>
        /// Clean Up Unsafe Code. This method MUST be called before the program closes.
        /// </summary>
        public void Dispose()
        {
            ArManWrap.ARTKPCleanup(tracker, IntPtr.Zero);
        }

        /// <summary>
        /// Creates a Tracker
        /// </summary>
        /// <returns>An instance of a tracker</returns>
        private IntPtr make_tracker()
        {
            //LoadImageBytes with raw image only
            IntPtr tracker = ArManWrap.ARTKPConstructTrackerSingle(-1, width, height);

            if (tracker == IntPtr.Zero)
            {
                throw new Exception("ARTKPConstructTracker failed");
            }

            IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
            string desc = Marshal.PtrToStringAnsi(ipDesc);
            int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)arPixelFormat);

            //camera calibration datafile
            string cameraCalibrationPath = arDir + "/data/no_distortion.cal";

            int retInit = ArManWrap.ARTKPInit(tracker, cameraCalibrationPath, 1.0f, 3000f);
            if (retInit != 0)
            {
                throw new Exception("ARTKPInitMulti failed");
            }

            ArManWrap.ARTKPSetPatternWidth(tracker, 80);
            //set border width percentage of marker (.25 is a huge border)
            ArManWrap.ARTKPSetBorderWidth(tracker, 0.250f);
            //set lighting threshold. this could be automatic
            ArManWrap.ARTKPSetThreshold(tracker, 150);
            //set undistortion mode
            ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
            //set tracker to look for simple ID-based markers
            ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_SIMPLE);

            return tracker;
        }

        /// <summary>
        /// Triggers a single track of all markers
        /// </summary>
        /// <param name="video">a video frame in byte[] form</param>
        /// <param name="finalprojMatrix">Projection Matrix</param>
        /// <param name="finalmodelMatrix">Model Matrix</param>
        /// <param name="fi">bytes from webcam in bitmap form</param>
        /// <returns>boolean value representing if the tracker has been detected</returns>
        public bool Track(byte[] video, out Matrix finalprojMatrix, out Matrix finalmodelMatrix)
        {
            //return true if we found the marker
            bool found = false;

            int pattern = 480;
            bool updateMatrix = true;
            IntPtr markerInfos = IntPtr.Zero;
            int numMarkers;

            int markerId = ArManWrap.ARTKPCalc(tracker, video, pattern, updateMatrix, out markerInfos, out numMarkers);

            Console.WriteLine(numMarkers);

            if (numMarkers >= 1)
            {
                found = true;
                ArManWrap.ARMarkerInfo markerInfo = (ArManWrap.ARMarkerInfo)Marshal.PtrToStructure(markerInfos, typeof(ArManWrap.ARMarkerInfo));
                float[] center = new float[] { 0, 0 };
                float width = 50;
                float[] markerMatrix = new float[16];
                float retTransMat = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, markerMatrix);
                Marshal.DestroyStructure(markerInfos, typeof(ArManWrap.ARMarkerInfo));

                //matrix conversion????
                Matrix m3d = new Matrix();
                m3d.M11 = markerMatrix[0];
                m3d.M12 = markerMatrix[1];
                m3d.M13 = markerMatrix[2];
                m3d.M14 = markerMatrix[3];
                m3d.M21 = markerMatrix[4];
                m3d.M22 = markerMatrix[5];
                m3d.M23 = markerMatrix[6];
                m3d.M24 = markerMatrix[7];
                m3d.M31 = markerMatrix[8];
                m3d.M32 = markerMatrix[9];
                m3d.M33 = markerMatrix[10];
                m3d.M34 = markerMatrix[11];
                m3d.M44 = markerMatrix[15];

                m3d.Translation = new Vector3(markerMatrix[12], markerMatrix[13], markerMatrix[14]);
            }

            //how confident is the marker tracking?
            float conf = ArManWrap.ARTKPGetConfidence(tracker);

            float[] modelViewMatrix = new float[16];
            float[] projmatrix = new float[16];

            finalprojMatrix = new Matrix();
            finalmodelMatrix = new Matrix();

            ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
            ArManWrap.ARTKPGetProjectionMatrix(tracker, projmatrix);

            finalprojMatrix = convert(projmatrix);
            finalmodelMatrix = convert(modelViewMatrix);

            return found;
        }

        private void check_markers(int numMarkers, Dictionary<int, MyMarkerInfo> dicMarkerInfos)
        {
            Console.WriteLine(numMarkers.ToString() + " markers detected!");
            for (int i = 0; i < numMarkers - 1; i++)
            {
                //ArManWrap.ARMarkerInfo armi = ArManWrap.ARTKPGetDetectedMarkerStruct(tracker, i);
                IntPtr markerInfos = ArManWrap.ARTKPGetDetectedMarker(tracker, i);
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
                //{
                //    mmi = new MyMarkerInfo();
                //    if(!dicMarkerInfos.ContainsKey(armi.id))
                //        dicMarkerInfos.Add(armi.id, mmi);
                //    retTransMat = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, matrix);
                //    //ArManWrap.ARTKPGetModelViewMatrix(tracker, matrix);
                //}
                retTransMat = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, matrix);
                Marshal.Release(markerInfos);
                mmi.found = true;
                mmi.notFoundCount = 0;
                //mmi.markerInfo = armi;
                mmi.prevMatrix = matrix;
                Matrix tmp = ArManWrap.GetXNAMatrixFromOpenGl12(matrix);
                tmp.M43 = -tmp.M43;
                mmi.transform = Matrix.Identity;
            }
        }

        /// <summary>
        /// Converts an OpenGL Matrix to an XNA Matrix
        /// </summary>
        /// <param name="matrix">matrix to convert in float[] form</param>
        /// <returns>converted matrix in XNA Matrix form</returns>
        Matrix convert(float[] matrix)
        {
            int a = 0;
            return new Matrix(matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++],
                    matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++],
                    matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++]);
        }
    }
}
