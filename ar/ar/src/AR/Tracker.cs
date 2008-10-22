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
        Dictionary<int, MyMarkerInfo> dicMarkerInfos = new Dictionary<int, MyMarkerInfo>();

        /// <summary>
        /// Starts Camera and Initializes AR Variables
        /// </summary>
        /// <param name="control">Control to draw to</param>
        public Tracker(int _width, int _height, int _bytesperpixel, Guid _sampleGrabberSubType, ArManWrap.PIXEL_FORMAT _arPixelFormat)
        {
            //set up general AR stuff
            width = _width;
            height = _height;
            bytesPerPixel = _bytesperpixel;
            sampleGrabberSubType = _sampleGrabberSubType;
            arPixelFormat = _arPixelFormat;

            //get current directory
            arDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);

            //make a short for bits per pixel
            bitsPerPixel = (short)(bytesPerPixel * 8);
            
            //make the tracker
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
            //make a tracker to find multiple markers
            IntPtr tracker = ArManWrap.ARTKPConstructTrackerMulti(-1, width, height);
            if (tracker == IntPtr.Zero)
                throw new Exception("ARTKPConstructTracker failed");

            //get information about the AR
            IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
            string desc = Marshal.PtrToStringAnsi(ipDesc);
            System.Diagnostics.Debug.WriteLine(desc);
            
            //get the pixelformat
            int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)arPixelFormat);

            //camera calibration datafile
            string cameraCalibrationPath = arDir + "/data/no_distortion.cal";
            
            //file containing a list of markers
            string multipath = arDir + "/data/markerboard_480-499.cfg";

            //initialise the tracker
            int retInit = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, multipath, 1.0f, 8000f);
            if (retInit != 0)
                throw new Exception("ARTKPInitMulti failed");

            //border we're looking for?
            ArManWrap.ARTKPSetBorderWidth(tracker, 0.125f);
            
            //set lighting threshold. this is set to automatic
            bool autoThresh = ArManWrap.ARTKPIsAutoThresholdActivated(tracker);
            ArManWrap.ARTKPActivateAutoThreshold(tracker, true);
            
            //set undistortion mode
            ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
            
            //set tracker to look for simple ID-based markers
            ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_SIMPLE);
            
            //dont use lite detection
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
        public Dictionary<int, MyMarkerInfo> Track(byte[] video, out Matrix finalprojMatrix)
        {
            try
            {
                //reset all markers to not found
                foreach (MyMarkerInfo mmi in dicMarkerInfos.Values)
                {
                    mmi.found = false;
                }

                //get the number of visible markers
                int numMarkers = ArManWrap.ARTKPCalcMulti(tracker, video); //uses ArDetectMarker internally (unless set to Lite)

                //float array for the projetion matrix
                float[] projMatrix = new float[16];

                //get the projection matrix
                ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);

                //convert the projection matrix from OpenGL to XNA
                finalprojMatrix = FloatToMatrix(projMatrix);

                //if we can see markers then check them
                if (numMarkers > 0)
                    check_markers(numMarkers);

                return dicMarkerInfos;
            }
            catch (Exception e)
            {
                int lastError = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine("Error: " + lastError.ToString() + "\r\n" + e.ToString());
                finalprojMatrix = new Matrix();
                return null;
            }
        }

        private void check_markers(int numMarkers)
        {
            //System.Diagnostics.Debug.WriteLine(numMarkers.ToString() + " markers detected!");
            for (int i = 0; i < numMarkers; i++)
            {
                //get information about the marker
                ArManWrap.ARMarkerInfo armi = ArManWrap.ARTKPGetDetectedMarkerStruct(tracker, i);
                IntPtr markerInfos = ArManWrap.ARTKPGetDetectedMarker(tracker, i);
                
                //various variables
                float[] center = new float[2];
                float width = 50;
                float[] matrix = new float[16];
                float Translation = 0;

                //new marker object
                MyMarkerInfo mmi = null;
                
                //have we seen this marker before?
                if (dicMarkerInfos.ContainsKey(armi.id) == true)
                {
                    //yes, then just update the marker object
                    mmi = dicMarkerInfos[armi.id];
                    Translation = ArManWrap.ARTKPGetTransMatCont(tracker, markerInfos, mmi.prevMatrix, center, width, matrix);
                }
                else
                {
                    //no, make a new marker object
                    mmi = new MyMarkerInfo();
                    dicMarkerInfos.Add(armi.id, mmi);
                    //set Translation and matix
                    Translation = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, matrix);
                }
                //manual deletion of markerinfos (unmanaged)
                Marshal.Release(markerInfos);
                
                //mark the found flag so XNA knows to draw this marker
                mmi.found = true;

                //Reset the notfound count
                mmi.notFoundCount = 0;

                //add information identidying the marker
                mmi.markerInfo = armi;

                //keep old matrix for a frame
                mmi.prevMatrix = matrix;
                
                //Convert matrix for XNA
                Matrix XNAmatrix = OtherFloatToMatix(matrix);
                
                //put the matrix in the marker object
                mmi.transform = XNAmatrix;
            }
        }

        /// <summary>
        /// Converts an OpenGL Matrix to an XNA Matrix
        /// </summary>
        /// <param name="matrix">matrix to convert in float[] form</param>
        /// <returns>converted matrix in XNA Matrix form</returns>
        Matrix FloatToMatrix(float[] matrix)
        {
            int a = 0;
            return new Matrix(matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++],
                    matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++],
                    matrix[a++], matrix[a++], matrix[a++], matrix[a++], matrix[a++]);
        }

        Matrix OtherFloatToMatix(float[] old)
        {
            Matrix m3d = new Matrix();
            unsafe
            {
                m3d.M11 = old[0];
                m3d.M12 = old[4];
                m3d.M13 = old[8];
                m3d.M14 = 0;
                m3d.M21 = old[1];
                m3d.M22 = old[5];
                m3d.M23 = old[9];
                m3d.M24 = 0;
                m3d.M31 = old[2];
                m3d.M32 = old[6];
                m3d.M33 = old[10];
                m3d.M34 = 0;
                m3d.Translation = new Vector3(old[3], old[7], old[11]);
                m3d.M44 = 1;
            }
            return m3d;
        }
    }
}
