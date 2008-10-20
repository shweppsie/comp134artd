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

        //AR tracker data structures
        object[] args = new object[2];
        Dictionary<int, MyMarkerInfo> dicMarkerInfos = new Dictionary<int, MyMarkerInfo>();

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
            IntPtr tracker = ArManWrap.ARTKPConstructTrackerMulti(-1, width, height);

            if (tracker == IntPtr.Zero)
            {
                throw new Exception("ARTKPConstructTracker failed");
            }

            IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
            string desc = Marshal.PtrToStringAnsi(ipDesc);
            Console.WriteLine(desc);
            int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)arPixelFormat);

            //camera calibration datafile
            string cameraCalibrationPath = arDir + "/data/no_distortion.cal";
            string multipath = arDir + "/data/markerboard_480-499.cfg";

            int retInit = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, multipath, 1.0f, 3000f);
            if (retInit != 0)
            {
                throw new Exception("ARTKPInitMulti failed");
            }

            ArManWrap.ARTKPSetBorderWidth(tracker, 0.125f);
            //set lighting threshold. this is set to automatic
            bool autoThresh = ArManWrap.ARTKPIsAutoThresholdActivated(tracker);
            ArManWrap.ARTKPActivateAutoThreshold(tracker, true);
            //set undistortion mode
            ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
            //set tracker to look for simple ID-based markers
            ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_SIMPLE);
            ArManWrap.ARTKPSetUseDetectLite(tracker, false);

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
        public Dictionary<int, MyMarkerInfo> Track(byte[] video, out Matrix finalprojMatrix, out Matrix finalmodelViewMatrix)
        {
            try
            {
                //reset all markers to not found
                foreach (MyMarkerInfo mmi in dicMarkerInfos.Values)
                {
                    mmi.found = false;
                }

                int numMarkers = ArManWrap.ARTKPCalcMulti(tracker, video); //uses ArDetectMarker internally (unless set to Lite)

                float[] modelViewMatrix = new float[16];
                float[] projMatrix = new float[16];

                ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
                ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);

                finalmodelViewMatrix = convert(modelViewMatrix);
                finalprojMatrix = convert(projMatrix);

                //we have markers to deal with
                if (numMarkers > 0)
                {
                    check_markers(numMarkers);
                }

                args[0] = finalmodelViewMatrix;
                args[1] = dicMarkerInfos;

                return dicMarkerInfos;
            }
            catch (Exception e)
            {
                int lastError = Marshal.GetLastWin32Error();
                Console.WriteLine("Error: " + lastError.ToString() + "\r\n" + e.ToString());
                finalmodelViewMatrix = new Matrix();
                finalprojMatrix = new Matrix();
                return null;
            }
        }

        private void check_markers(int numMarkers)
        {
            Console.WriteLine(numMarkers.ToString() + " markers detected!");
            for (int i = 0; i < numMarkers; i++)
            {
                ArManWrap.ARMarkerInfo armi = ArManWrap.ARTKPGetDetectedMarkerStruct(tracker, i);
                IntPtr markerInfos = ArManWrap.ARTKPGetDetectedMarker(tracker, i);
                
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
                Matrix m3d = ArManWrap.GetXNAMatrixFromOpenGl12(matrix);
                mmi.transform = m3d;
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
