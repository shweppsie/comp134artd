//----------------------------------------------
// (c) 2007 by casey chesnut, brains-N-brawn LLC
//----------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;

namespace ARTKPManagedWrapper
{

    public class ArManWrap
    {
        #region METHODS
        public static float[] SwapArray12(float[] matrix)
        {
            float[] prevMatrix = new float[12];
            prevMatrix[0] = matrix[0];
            prevMatrix[1] = matrix[4];
            prevMatrix[2] = matrix[8];
            prevMatrix[3] = matrix[1];
            prevMatrix[4] = matrix[5];
            prevMatrix[5] = matrix[9];
            prevMatrix[6] = matrix[2];
            prevMatrix[7] = matrix[6];
            prevMatrix[8] = matrix[10];
            prevMatrix[9] = matrix[3];
            prevMatrix[10] = matrix[7];
            prevMatrix[11] = matrix[11];
            return prevMatrix;
        }

        public static void DumpMatrix(string line, Matrix3D m3d)
        {
            System.Console.WriteLine("===== " + line + " =====");
            string fmt = "##0.0";
            string space = "   ";
            System.Console.WriteLine(m3d.M11.ToString(fmt) + space + m3d.M12.ToString(fmt) + space + m3d.M13.ToString(fmt) + space + m3d.M14.ToString(fmt));
            System.Console.WriteLine(m3d.M21.ToString(fmt) + space + m3d.M22.ToString(fmt) + space + m3d.M23.ToString(fmt) + space + m3d.M24.ToString(fmt));
            System.Console.WriteLine(m3d.M31.ToString(fmt) + space + m3d.M32.ToString(fmt) + space + m3d.M33.ToString(fmt) + space + m3d.M34.ToString(fmt));
            System.Console.WriteLine(m3d.OffsetX.ToString(fmt) + space + m3d.OffsetY.ToString(fmt) + space + m3d.OffsetZ.ToString(fmt) + space + m3d.M44.ToString(fmt));
        }

        public unsafe static Matrix3D GetWpfMatrixFromOpenGl12Point(float* openGl12)
        {
            Matrix3D m3d = new Matrix3D();
            unsafe
            {
                m3d.M11 = openGl12[0];
                m3d.M12 = openGl12[4];
                m3d.M13 = openGl12[8];
                m3d.M14 = 0;
                m3d.M21 = openGl12[1];
                m3d.M22 = openGl12[5];
                m3d.M23 = openGl12[9];
                m3d.M24 = 0;
                m3d.M31 = openGl12[2];
                m3d.M32 = openGl12[6];
                m3d.M33 = openGl12[10];
                m3d.M34 = 0;
                m3d.OffsetX = openGl12[3];
                m3d.OffsetY = openGl12[7];
                m3d.OffsetZ = openGl12[11];
                m3d.M44 = 1;
            }
            return m3d;
        }

        public static Matrix3D GetWpfMatrixFromOpenGl12(float[] openGl12)
        {
            Matrix3D m3d = new Matrix3D();
            unsafe
            {
                m3d.M11 = openGl12[0];
                m3d.M12 = openGl12[4];
                m3d.M13 = openGl12[8];
                m3d.M14 = 0;
                m3d.M21 = openGl12[1];
                m3d.M22 = openGl12[5];
                m3d.M23 = openGl12[9];
                m3d.M24 = 0;
                m3d.M31 = openGl12[2];
                m3d.M32 = openGl12[6];
                m3d.M33 = openGl12[10];
                m3d.M34 = 0;
                m3d.OffsetX = openGl12[3];
                m3d.OffsetY = openGl12[7];
                m3d.OffsetZ = openGl12[11];
                m3d.M44 = 1;
            }
            return m3d;
        }

        public static Matrix3D GetWpfMatrixFromOpenGl(float[] openGl)
        {
            //OpenGL
            //m0 m4 m8  m12
            //m1 m5 m9  m13
            //m2 m6 m10 m14
            //m3 m7 m11 m15
            //WPF
            //M11   M12   M13   M14
            //M21   M22   M23   M24
            //M31   M32   M33   M34
            //OffX  OffY  OffZ  M44
            
            //NOTES
            //(bottom row) OffX OffY OffZ is translation
            //(diagonal) M11 M22 M33 is scale
            //(right column) M14 M24 M34 M44 is projection (0 0 0 1 if not used)

            /*
            Matrix3D m3d = new Matrix3D();
            m3d.M11 = openGl[0];
            m3d.M12 = openGl[4];
            m3d.M13 = openGl[8];
            m3d.M14 = openGl[12];
            m3d.M21 = openGl[1];
            m3d.M22 = openGl[5];
            m3d.M23 = openGl[9];
            m3d.M24 = openGl[13];
            m3d.M31 = openGl[2];
            m3d.M32 = openGl[6];
            m3d.M33 = openGl[10];
            m3d.M34 = openGl[14];
            m3d.OffsetX = openGl[3];
            m3d.OffsetY = openGl[7];
            m3d.OffsetZ = openGl[11];
            m3d.M44 = openGl[15];
            */

            Matrix3D m3d = new Matrix3D();
            m3d.M11 = openGl[0];
            m3d.M12 = openGl[1];
            m3d.M13 = openGl[2];
            m3d.M14 = openGl[3];
            m3d.M21 = openGl[4];
            m3d.M22 = openGl[5];
            m3d.M23 = openGl[6];
            m3d.M24 = openGl[7];
            m3d.M31 = openGl[8];
            m3d.M32 = openGl[9];
            m3d.M33 = openGl[10];
            m3d.M34 = openGl[11];
            m3d.OffsetX = openGl[12];
            m3d.OffsetY = openGl[13];
            m3d.OffsetZ = openGl[14];
            m3d.M44 = openGl[15];

            return m3d;
        }
        #endregion

        #region PINVOKES
        /// <summary>
        /// test pInvoke
        /// </summary>
        [DllImport("ARToolKitPlus.dll")]
        public static extern int fnARTKPWrapper();

        /// <summary>
        /// ARToolKitPlus simple sample
        /// </summary>
        [DllImport("ARToolKitPlus.dll")]
        public static extern int fnARTKPWrapperSingle(float[] matrix, out int markerId, out float conf);

        /// <summary>
        /// ARToolKitPlus multi sample
        /// </summary>
        [DllImport("ARToolKitPlus.dll")]
        public static extern int fnARTKPWrapperMulti();

        /// <summary>
        /// Defines a simple interface for single-marker tracking with ARToolKitPlus. 
        /// ARToolKitPlus::TrackerSingleMarker provides all methods to access ARToolKit for single marker tracking 
        /// without needing to mess around with it low level methods directly.
        /// A current restriction is that only the best detected marker is returned. 
        /// If you need multi-marker tracking use TrackerMultiMarker. 
        /// </summary>
        /// <param name="patternSizeX"></param>
        /// <param name="patternSizeY"></param>
        /// <param name="patternSampleNum"></param>
        /// <param name="maxLoadPatterns"></param>
        /// <param name="maxImagePatterns"></param>
        /// <param name="imageWidth"></param>
        /// <param name="imageHeight"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ConstructTrackerSingle(int trackerSwitch, int imageWidth, int imageHeight)        
        public static extern IntPtr ARTKPConstructTrackerSingle(int trackerSwitch, int imageWidth, int imageHeight);

        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int LoadImagePath(char * fName, int bpp, unsigned char * outCameraBuffer)
        public static extern int ARTKPLoadImagePath(string fName, int imageWidth, int imageHeight, int bpp, byte[] outCameraBuffer);

        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int LoadImageBytes(unsigned char * cameraBuffer, int bpp, unsigned char * outCameraBuffer)
        public static extern int ARTKPLoadImageBytes(byte[] cameraBuffer, int bpp, byte[] outCameraBuffer);

        /// <summary>
        /// Returns a short description with compiled-in settings.
        /// </summary>
        /// <param name="tracker"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API const char* GetDescription(void)
        public static extern IntPtr ARTKPGetDescription(IntPtr tracker);

        /// <summary>
        /// Logger. TODO Implement
        /// </summary>
        /// <param name="tracker"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API char* GetLastLog(void)
        public static extern string ARTKPGetLastLog(IntPtr tracker);

        /// <summary>
        /// Sets the pixel format of the camera image. 
        /// Default format is RGB888 (PIXEL_FORMAT_RGB) 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="pixelFormat"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void SetPixelFormat(int pixelFormat)
        public static extern int ARTKPSetPixelFormat(IntPtr tracker, int pixelFormat);

        /// <summary>
        /// Set to true to try loading camera undistortion table from a cache file. 
        /// On slow platforms (e.g. Smartphone) creation of the undistortion lookup-table can take quite a while. 
        /// Consequently caching will speedup the start phase. 
        /// If set to true and no cache file could be found a new one will be created. 
        /// The cache file will get the same name as the camera file with the added extension '.LUT' 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="value"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void SetLoadUndistLUT(bool value)
        public static extern void ARTKPSetLoadUndistLUT(IntPtr tracker, bool value);

        /// <summary>
        /// initializes TrackerSingleMarker.
        /// nCamParamFile is the name of the camera parameter file nLogger is an instance which implements the 
        /// ARToolKit::Logger interface
        /// </summary>
        /// <param name="trackerSingle"></param>
        /// <param name="camParamFile">load a camera file. two types of camera files are supported:
        ///  - Std. ARToolKit
        ///  - MATLAB Camera Calibration Toolbox</param>
        /// <param name="nearClip"></param>
        /// <param name="farClip"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int Init(char * camParamFile, float nearClip, float farClip)
        public static extern int ARTKPInit(IntPtr trackerSingle, string camParamFile, float nearClip, float farClip);

        /// <summary>
        /// Sets the width and height of the patterns.
        /// </summary>
        /// <param name="trackerSingle"></param>
        /// <param name="value"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void SetPatternWidth(float value)
        public static extern void ARTKPSetPatternWidth(IntPtr trackerSingle, float value);

        /// <summary>
        /// Sets a new relative border width. ARToolKit's default value is 0.25. 
        /// Take caution that the markers need of course really have thiner borders. 
        /// Values other than 0.25 have not been tested for regular pattern-based matching, but only for id-encoded markers. 
        /// It might be that the pattern creation process needs to be updated too. 
        /// (FROM SAMPLE CODE) ID_BCH = 0.125f / ID_SIMPLE = 0.125f to 0.250f.
        /// to figure it out, measure the width of the marker, the width of the markers, and this value is that percentage.
        /// i'm assuming by border width, it is just one side of the border ... not both sides.
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="value"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void SetBorderWidth(float value)
        public static extern void ARTKPSetBorderWidth(IntPtr tracker, float value);

        /// <summary>
        /// Sets the threshold value that is used for black/white conversion.
        /// alternatively we could also activate automatic thresholding.
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="value"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void SetThreshold(float value)
        public static extern void ARTKPSetThreshold(IntPtr tracker, float value);

        /// <summary>
        /// Returns the current threshold value.
        /// </summary>
        /// <param name="tracker"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int ARTKPGetThreshold(ARToolKitPlus::Tracker* tracker)
        public static extern int ARTKPGetThreshold(IntPtr tracker);

        /// <summary>
        /// Changes the undistortion mode. 
        /// Default value is UNDIST_STD which means that artoolkit's standard undistortion method is used. 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="undistMode"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void SetUndistortionMode(int value)
        public static extern void ARTKPSetUndistortionMode(IntPtr tracker, int undistMode);

        /// <summary>
        /// Changes the Pose Estimation Algorithm. 
        /// POSE_ESTIMATOR_ORIGINAL (default): arGetTransMat() POSE_ESTIMATOR_RPP: "Robust Pose Estimation from a Planar Target"
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="poseEstimator"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void SetPoseEstimator(int value)
        public static extern void ARTKPSetPoseEstimator(IntPtr tracker, int poseEstimator);

        /// <summary>
        /// activate the usage of id-based markers rather than template based markers. 
        /// Template markers are the classic marker type used in ARToolKit. 
        /// Id-based markers directly encode the marker id in the image. 
        /// Simple markers use 3-times redundancy to increase robustness, 
        /// while BCH markers use an advanced CRC algorithm to detect and repair marker damages. 
        /// See arBitFieldPattern.h for more information. 
        /// In order to use id-based markers, the marker size has to be 6x6, 12x12 or 18x18. 
        /// use the tool in tools/IdPatGen to generate markers
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="markerMode"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void SetMarkerMode(int markerId)
        public static extern void ARTKPSetMarkerMode(IntPtr tracker, int markerMode);

        /// <summary>
        /// calculates the transformation matrix.
        /// pass the image as RGBX (32-bits) in 320x240 pixels. if nPattern is not -1 then only this pattern is accepted 
        /// otherwise any found pattern will be used.
        /// </summary>
        /// <param name="trackerSingle"></param>
        /// <param name="cameraBuffer"></param>
        /// <param name="pattern"></param>
        /// <param name="updateMatrix"></param>
        /// <param name="markerInfos"></param>
        /// <param name="numMarkers"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int Calc()
        //public static extern int ARTKPCalc(IntPtr tracker, byte[] cameraBuffer);
        //public static extern int ARTKPCalc(IntPtr tracker, IntPtr cameraBuffer);
        //ARTKPWRAPPER_API int ARTKPCalc(ARToolKitPlus::TrackerSingleMarker* trackerSingle, unsigned char* cameraBuffer, int pattern, bool updateMatrix, ARToolKitPlus::ARMarkerInfo* markerInfos, int numMarkers)
        public static extern int ARTKPCalc(IntPtr trackerSingle, byte[] cameraBuffer, int pattern, bool updateMatrix, out IntPtr markerInfos, out int numMarkers);

        /// <summary>
        /// calculates the transformation matrix between camera and the given marker.
        /// compute camera position in function of detected markers. 
        /// calculate the transformation between a detected marker and the real camera, 
        /// i.e. the position and orientation of the camera relative to the tracking mark. 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="markerInfo"></param>
        /// <param name="center">the physical center of the marker. 
        /// arGetTransMat assumes that the marker is in x-y plane, and z axis is pointing downwards from marker plane. 
        /// So vertex positions can be represented in 2D coordinates by ignoring the z axis information. 
        /// The marker vertices are specified in order of clockwise. </param>
        /// <param name="width">the size of the marker (in mm).</param>
        /// <param name="matrix">the transformation matrix from the marker coordinates to camera coordinate frame, 
        /// that is the relative position of real camera to the real marker </param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API float ARTKPGetTransMat(ARToolKitPlus::TrackerSingleMarker* trackerSingle, ARToolKitPlus::ARMarkerInfo* markerInfo, float* center, float width, float* matrix)
        public static extern float ARTKPGetTransMat(IntPtr tracker, IntPtr markerInfo, float[] center, float width, float[] matrix);

        /// <summary>
        /// calculates the transformation matrix between camera and the given marker (with history).
        /// compute camera position in function of detected marker with an history function. 
        /// calculate the transformation between a detected marker and the real camera, 
        /// i.e. the position and orientation of the camera relative to the tracking mark. 
        /// Since this routine operate on previous values, the result are more stable (less jittering).
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="markerInfo">the structure containing the parameters for the marker 
        /// for which the camera position and orientation is to be found relative to. 
        /// This structure is found using arDetectMarker. </param>
        /// <param name="prevMatrix">the previous transformation matrix obtain. </param>
        /// <param name="center">the physical center of the marker. 
        /// arGetTransMat assumes that the marker is in x-y plane, and z axis is pointing downwards from marker plane. 
        /// So vertex positions can be represented in 2D coordinates by ignoring the z axis information. 
        /// The marker vertices are specified in order of clockwise. </param>
        /// <param name="width">the size of the marker (in mm).</param>
        /// <param name="matrix">the transformation matrix from the marker coordinates to camera coordinate frame, 
        /// that is the relative position of real camera to the real marker </param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API float ARTKPGetTransMatCont(ARToolKitPlus::Tracker* tracker, ARToolKitPlus::ARMarkerInfo* markerInfo, ARFloat preConv[][4], float* center, float width, float* matrix)
        public static extern float ARTKPGetTransMatCont(IntPtr tracker, IntPtr markerInfo, float[] prevMatrix, float[] center, float width, float[] matrix);

        /// <summary>
        /// Returns the confidence value of the currently best detected marker. 
        /// </summary>
        /// <param name="trackerSingle"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API float GetConfidence()
        public static extern float ARTKPGetConfidence(IntPtr trackerSingle);

        /// <summary>
        /// Returns an opengl-style modelview transformation matrix. 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="matrix"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API float* GetModelViewMatrix()
        public static extern void ARTKPGetModelViewMatrix(IntPtr tracker, float[] matrix);

        /// <summary>
        /// Returns an opengl-style projection transformation matrix. 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="matrix"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        public static extern void ARTKPGetProjectionMatrix(IntPtr tracker, float[] matrix);

        /// <summary>
        /// release tracker memory.
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="cameraBuffer"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void Cleanup()
        public static extern void ARTKPCleanup(IntPtr tracker, IntPtr cameraBuffer);

        /// <summary>
        /// Defines a simple interface for multi-marker tracking with ARToolKitPlus. 
        /// ARToolKit::TrackerMultiMarker provides all methods to access ARToolKit for multi marker tracking 
        /// without needing to mess around with it directly.
        /// </summary>
        /// <param name="trackerSwitch"></param>
        /// <param name="imageWidth"></param>
        /// <param name="imageHeight"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API ARToolKitPlus::TrackerMultiMarker* ARTKPConstructTrackerMulti(int trackerSwitch, int imageWidth, int imageHeight)
        public static extern IntPtr ARTKPConstructTrackerMulti(int trackerSwitch, int imageWidth, int imageHeight);

        /// <summary>
        /// nCamParamFile is the name of the camera parameter file nNearClip & nFarClip are near and far clipping values 
        /// for the OpenGL projection matrix nLogger is an instance which implements the ARToolKit::Logger interface 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="camParamFile">load a camera file. two types of camera files are supported: Std. ARToolKit. MATLAB Camera Calibration Toolbox</param>
        /// <param name="multiFile"></param>
        /// <param name="nearClip"></param>
        /// <param name="farClip"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int ARTKPInitMulti(ARToolKitPlus::TrackerMultiMarker* trackerMulti, char * camParamFile, char * multiFile, float nearClip, float farClip)
        public static extern int ARTKPInitMulti(IntPtr tracker, string camParamFile, string multiFile, float nearClip, float farClip);

        /// <summary>
        /// calculates the transformation matrix.
        /// pass the image as RGBX (32-bits) in 320x240 pixels. 
        /// </summary>
        /// <param name="trackerMulti"></param>
        /// <param name="cameraBuffer"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int ARTKPCalcMulti(ARToolKitPlus::TrackerMultiMarker* trackerMulti, unsigned char* cameraBuffer)
        public static extern int ARTKPCalcMulti(IntPtr trackerMulti, byte[] cameraBuffer);

        /// <summary>
        /// Returns the loaded ARMultiMarkerInfoT object. 
        /// If loading the multi-marker config file failed then this method returns NULL. 
        /// </summary>
        /// <param name="trackerMulti"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API const ARToolKitPlus::ARMultiMarkerInfoT* ARTKPGetMultiMarkerConfig(ARToolKitPlus::TrackerMultiMarker* trackerMulti)
        public static extern IntPtr ARTKPGetMultiMarkerConfig(IntPtr trackerMulti);

        /// <summary>
        /// Returns array of detected marker IDs. 
        /// Only access the first getNumDetectedMarkers() markers 
        /// </summary>
        /// <param name="trackerMulti"></param>
        /// <param name="markerIDs"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ARTKPGetDetectedMarkers(ARToolKitPlus::TrackerMultiMarker* trackerMulti, int*& markerIDs)
        public static extern void ARTKPGetDetectedMarkers(IntPtr trackerMulti, int [] markerIDs);

        /// <summary>
        /// Returns the ARMarkerInfo object for a found marker. 
        /// </summary>
        /// <param name="trackerMulti"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API ARToolKitPlus::ARMarkerInfo ARTKPGetDetectedMarker(ARToolKitPlus::TrackerMultiMarker* trackerMulti, int marker)
        public static extern IntPtr ARTKPGetDetectedMarker(IntPtr trackerMulti, int marker);

        /// <summary>
        /// Returns the ARMarkerInfo object for a found marker. 
        /// </summary>
        /// <param name="trackerMulti"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API ARToolKitPlus::ARMarkerInfo ARTKPGetDetectedMarker(ARToolKitPlus::TrackerMultiMarker* trackerMulti, int marker)
        public static extern ARMarkerInfo ARTKPGetDetectedMarkerStruct(IntPtr trackerMulti, int marker);

        /// <summary>
        /// calculates the transformation matrix between camera and the given multi-marker config.
        /// compute camera position in function of the multi-marker patterns (based on detected markers) 
        /// calculate the transformation between the multi-marker patterns and the real camera. 
        /// Based on confident values of detected markers in the multi-markers patterns, a global position is return.
        /// </summary>
        /// <param name="trackerMulti"></param>
        /// <param name="markerInfo"></param>
        /// <param name="markerNum"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API float ARTKPArMultiGetTransMat(ARToolKitPlus::TrackerMultiMarker* trackerMulti, ARToolKitPlus::ARMarkerInfo* markerInfo, int markerNum, ARToolKitPlus::ARMultiMarkerInfoT* config)
        public static extern float ARTKPArMultiGetTransMat(IntPtr trackerMulti, out IntPtr markerInfo, int markerNum, IntPtr config);

        /// <summary>
        /// Returns the number of detected markers used for multi-marker tracking. 
        /// </summary>
        /// <param name="trackerMulti"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int ARTKPGetNumDetectedMarkers(ARToolKitPlus::TrackerMultiMarker* trackerMulti)
        public static extern int ARTKPGetNumDetectedMarkers(IntPtr trackerMulti);

        /// <summary>
        /// Enables usage of arDetectMarkerLite. Otherwise arDetectMarker is used. 
        /// In general arDetectMarker is more powerful since it keeps history about markers. 
        /// In some cases such as very low camera refresh rates it is advantegous to change this. 
        /// Using the non-lite version treats each image independent. 
        /// </summary>
        /// <param name="trackerMulti"></param>
        /// <param name="enable"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ARTKPSetUseDetectLite(ARToolKitPlus::TrackerMultiMarker* trackerMulti, bool enable)
        public static extern void ARTKPSetUseDetectLite(IntPtr trackerMulti, bool enable);

        /// <summary>
        /// Provides access to ARToolKit' patt_trans matrix. 
        /// This method is primarily for compatibility issues with code previously using ARToolKit rather than ARToolKitPlus. patt_trans is the original transformation matrix ARToolKit calculates rather than the OpenGL style version of this matrix that can be retrieved via getModelViewMatrix().
        /// </summary>
        /// <param name="trackerSingle"></param>
        /// <param name="matrix"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ARTKPGetARMatrix(ARToolKitPlus::TrackerSingleMarker* trackerSingle, float* matrix)
        public static extern void ARTKPGetARMatrix(IntPtr trackerSingle, float[] matrix);

        /// <summary>
        /// Provides access to ARToolKit' internal version of the transformation matrix. 
        /// This method is primarily for compatibility issues with code previously using ARToolKit rather than ARToolKitPlus. 
        /// This is the original transformation matrix ARToolKit calculates rather than the OpenGL style version of this matrix 
        /// that can be retrieved via getModelViewMatrix(). 
        /// </summary>
        /// <param name="trackerMulti"></param>
        /// <param name="matrix"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ARTKPGetARMatrixMulti(ARToolKitPlus::TrackerMultiMarker* trackerMulti, float* matrix)
        public static extern void ARTKPGetARMatrixMulti(IntPtr trackerMulti, float[] matrix);

        /// <summary>
        /// adds a pattern to ARToolKit.
        /// pass the patterns filename 
        /// </summary>
        /// <param name="trackerSingle"></param>
        /// <param name="fName"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int ARTKPAddPattern(ARToolKitPlus::TrackerSingleMarker* trackerSingle, char * fName)
        public static extern int ARTKPAddPattern(IntPtr trackerSingle, string fName);

        /// <summary>
        /// marker detection using tracking history.
        /// main function to detect the square markers in the video input frame. 
        /// This function proceeds to thresholding, labeling, contour extraction and line corner estimation 
        /// (and maintains an history). It's one of the main function of the detection routine with arGetTransMat. 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="cameraBuffer"></param>
        /// <param name="thresh"></param>
        /// <param name="markerInfos"></param>
        /// <param name="numMarkers"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int ARTKPArDetectMarker(ARToolKitPlus::Tracker* tracker, unsigned char* cameraBuffer, int thresh, ARToolKitPlus::ARMarkerInfo** markerInfos, int& numMarkers)
        public static extern int ARTKPArDetectMarker(IntPtr tracker, byte[] cameraBuffer, int thresh, out IntPtr markerInfos, out int numMarkers);

        /// <summary>
        /// marker detection without using tracking history.
        /// main function to detect rapidly the square markers in the video input frame. 
        /// this function is a simpler version of arDetectMarker that does not have the same error correction functions 
        /// and so runs a little faster, but is more error prone
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="cameraBuffer"></param>
        /// <param name="thresh"></param>
        /// <param name="markerInfos"></param>
        /// <param name="numMarkers"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int ARTKPArDetectMarkerLite(ARToolKitPlus::Tracker* tracker, unsigned char* cameraBuffer, int thresh, ARToolKitPlus::ARMarkerInfo** markerInfos, int& numMarkers)
        public static extern int ARTKPArDetectMarkerLite(IntPtr tracker, byte[] cameraBuffer, int thresh, out IntPtr markerInfos, out int numMarkers);

        /// <summary>
        /// activates binary markers.
        /// markers are converted to pure black/white during loading
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="threshold"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ARTKPActivateBinaryMarker(ARToolKitPlus::Tracker* tracker, int threshold)
        public static extern void ARTKPActivateBinaryMarker(IntPtr tracker, string threshold);

        /// <summary>
        /// activates the complensation of brightness falloff in the corners of the camera image. 
        /// some cameras have a falloff in brightness at the border of the image, 
        /// which creates problems with thresholding the image. use this function to set a (linear) adapted threshold value. 
        /// the threshold value will stay exactly the same at the center but will deviate near to the border. 
        /// all values specify a difference, not absolute values! nCorners define the falloff a all four corners. 
        /// nLeftRight defines the falloff at the half y-position at the left and right side of the image. 
        /// nTopBottom defines the falloff at the half x-position at the top and bottom side of the image. 
        /// all values between these 9 points (center, 4 corners, left, right, top, bottom) will be interpolated. 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="nEnable"></param>
        /// <param name="nCorners"></param>
        /// <param name="nLeftRight"></param>
        /// <param name="nTopBottom"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ARTKPActivateVignettingCompensation(ARToolKitPlus::Tracker* tracker, bool nEnable, int nCorners, int nLeftRight, int nTopBottom)
        public static extern void ARTKPActivateVignettingCompensation(IntPtr tracker, bool nEnable, int nCorners, int nLeftRight, int nTopBottom);

        /// <summary>
        /// Returns the maximum number of patterns that can be loaded. 
        /// This maximum number of loadable patterns can be set via the __MAX_LOAD_PATTERNS template parameter
        /// </summary>
        /// <param name="tracker"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API int ARTKPGetNumLoadablePatterns(ARToolKitPlus::Tracker* tracker)
        public static extern int ARTKPGetNumLoadablePatterns(IntPtr tracker);

        /// <summary>
        /// Loads a camera calibration file and stores data internally. 
        /// To prevent memory leaks, this method internally deletes an existing camera. 
        /// If you want to use more than one camera, retrieve the existing camera using getCamera() and call setCamera(NULL); 
        /// before loading another camera file. On destruction, ARToolKitPlus will only destroy the currently set camera. 
        /// All other cameras have to be destroyed manually.
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="nCamParamFile"></param>
        /// <param name="nNearClip"></param>
        /// <param name="nFarClip"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API bool ARTKPLoadCameraFile(ARToolKitPlus::Tracker* tracker, const char* nCamParamFile, float nNearClip, float nFarClip)
        public static extern bool ARTKPLoadCameraFile(IntPtr tracker, string nCamParamFile, float nNearClip, float nFarClip);

        /// <summary>
        /// Sets an image processing mode (half or full resolution). 
        /// Half resolution is faster but less accurate. 
        /// When using full resolution smaller markers will be detected at a higher accuracy (or even detected at all). 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="imageProcMode"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ARTKPSetImageProcessingMode(ARToolKitPlus::Tracker* tracker, int imageProcMode)
        public static extern void ARTKPSetImageProcessingMode(IntPtr tracker, int imageProcMode);

        /// <summary>
        /// Sets the number of times the threshold is randomized in case no marker was visible (Default: 2). 
        /// Autothreshold requires a visible marker to estime the optimal thresholding value. 
        /// If no marker is visible ARToolKitPlus randomizes the thresholding value until a marker is found. 
        /// This function sets the number of times ARToolKitPlus will randomize the threshold value 
        /// and research for a marker per calc() invokation until it gives up. 
        /// A value of 2 means that ARToolKitPlus will analyze the image a second time with an other treshold value 
        /// if it does not find a marker the first time. Each unsuccessful try uses less processing power 
        /// than a single full successful position estimation.
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="numRetries"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ARTKPSetNumAutoThresholdRetries(ARToolKitPlus::Tracker* tracker, int numRetries)
        public static extern void ARTKPSetNumAutoThresholdRetries(IntPtr tracker, int numRetries);

        /// <summary>
        /// Returns true if automatic threshold calculation is activated.
        /// </summary>
        /// <param name="tracker"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API bool ARTKPIsAutoThresholdActivated(ARToolKitPlus::Tracker* tracker)
        public static extern bool ARTKPIsAutoThresholdActivated(IntPtr tracker);

        /// <summary>
        /// Enables or disables automatic threshold calculation. 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="enable"></param>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API void ARTKPActivateAutoThreshold(ARToolKitPlus::Tracker* tracker, bool enable)
        public static extern void ARTKPActivateAutoThreshold(IntPtr tracker, bool enable);

        /// <summary>
        /// Calls the pose estimator set with setPoseEstimator() for single marker tracking.
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="markerInfo"></param>
        /// <param name="center"></param>
        /// <param name="width"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API float ARTKPExecuteSingleMarkerPoseEstimator(ARToolKitPlus::Tracker* tracker, ARToolKitPlus::ARMarkerInfo *marker_info, float* center, float width, float* matrix)
        public static extern float ARTKPExecuteSingleMarkerPoseEstimator(IntPtr tracker, IntPtr markerInfo, float[] center, float width, float[] matrix);

        /// <summary>
        /// Calls the pose estimator set with setPoseEstimator() for multi marker tracking.
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="markerInfo"></param>
        /// <param name="markerNum"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [DllImport("ARToolKitPlus.dll", SetLastError = true)]
        //ARTKPWRAPPER_API float ARTKPExecuteMultiMarkerPoseEstimator(ARToolKitPlus::Tracker* tracker, ARToolKitPlus::ARMarkerInfo *marker_info, int marker_num, ARToolKitPlus::ARMultiMarkerInfoT *config)
        public static extern float ARTKPExecuteMultiMarkerPoseEstimator(IntPtr tracker, IntPtr markerInfo, int markerNum, IntPtr config);

        #endregion

        #region STRUCTS

        /// <summary>
        /// multi-marker structure.
        /// Structure for multi-marker tracking really similar to ARMarkerInfo 
        /// </summary>
        public unsafe struct ARMultiEachMarkerInfoT
        {
            /// <summary>
            /// identification of the pattern
            /// </summary>
            public int     patt_id;
            /// <summary>
            /// width of the pattern (in mm) 
            /// </summary>
            public float   width;
            /// <summary>
            /// center of the pattern (in mm) 
            /// </summary>
            public fixed float   center[2];
            /// <summary>
            /// estimated position of the pattern
            /// </summary>
            public fixed float   trans[12];//[3][4];
            /// <summary>
            /// relative position of the pattern
            /// </summary>
            public fixed float   itrans[12];//[3][4];
            /// <summary>
            /// final position of the pattern
            /// </summary>
            public fixed float   pos3d[12];//[4][3];
            /// <summary>
            /// boolean flag for visibility
            /// </summary>
            public int     visible;
            /// <summary>
            /// last state visibility
            /// </summary>
            public int     visibleR;
        };

        /// <summary>
        /// global multi-marker structure.
        /// Main structure for multi-marker tracking.
        /// </summary>
        public unsafe struct ARMultiMarkerInfoT
        {
            /// <summary>
            /// list of markers of the multi-marker pattern
            /// </summary>
            public IntPtr marker; //ARMultiEachMarkerInfoT*
            /// <summary>
            /// number of markers used
            /// </summary>
            public int marker_num;
            /// <summary>
            /// position of the multi-marker pattern (more precisely, the camera position in the multi-marker CS)
            /// </summary>
            public fixed float trans[12]; //[3][4];
            /// <summary>
            /// boolean flag for visibility
            /// </summary>
            public int prevF;
            /// <summary>
            /// last position
            /// </summary>
            public fixed float transR[12]; //[3][4];
        };

        /// <summary>
        /// main structure for detected marker. 
        /// Store information after contour detection (in idea screen coordinate, after distorsion compensated). 
        /// </summary>
        public unsafe struct ARMarkerInfo
        {
            /// <summary>
            /// number of pixels in the labeled region 
            /// </summary>
            public int area;
            /// <summary>
            /// marker identitied number
            /// </summary>
            public int id;
            /// <summary>
            /// Direction that tells about the rotation about the marker (possible values are 0, 1, 2 or 3). 
            /// This parameter makes it possible to tell about the line order of the detected marker 
            /// (so which line is the first one) and so find the first vertex. 
            /// This is important to compute the transformation matrix in arGetTransMat(). 
            /// </summary>
            public int dir;
            /// <summary>
            /// confidence value (probability to be a marker) 
            /// </summary>
            public float cf;
            /// <summary>
            /// center of marker (in ideal screen coordinates) 
            /// </summary>
            public fixed float pos[2];
            /// <summary>
            /// line equations for four side of the marker (in ideal screen coordinates) 
            /// probably 3 points to define a line
            /// Ax + By - D = 0 (?)
            /// OR Ax + By = D
            /// </summary>
            public fixed float line[12];//[4][3];
            /// <summary>
            /// edge points of the marker (in ideal screen coordinates) 
            /// </summary>
            public fixed float vertex[8];//[4][2];
        }
        #endregion

        #region ENUMS
        /// <summary>
        /// 
        /// </summary>
        public enum DEF
        {
            /// <summary>
            /// 
            /// </summary>
            DEF_CAMWIDTH = 320,
            /// <summary>
            /// 
            /// </summary>
            DEF_CAMHEIGHT = 240
        }

        /// <summary>
        /// ARToolKit pixel-format specifiers. 
        /// ARToolKit functions can accept pixel data in a variety of formats. 
        /// This enumerations provides a set of constants you can use to request data in a particular pixel format 
        /// from an ARToolKit function that returns data to you, 
        /// or to specify that data you are providing to an ARToolKit function is in a particular pixel format.
        /// </summary>
        public enum PIXEL_FORMAT
        {
            /// <summary>
            /// Each pixel is represented by 32 bits. Eight bits per each Alpha, Blue, Green, and Red component. 
            /// This is the native 32 bit format for the SGI platform.
            /// </summary>
            PIXEL_FORMAT_ABGR = 1,
            /// <summary>
            /// Each pixel is represented by 32 bits. Eight bits per each Blue, Green, Red, and Alpha component. 
            /// This is the native 32 bit format for the Win32 platform.
            /// </summary>
            PIXEL_FORMAT_BGRA = 2,
            /// <summary>
            /// Each pixel is represented by 24 bits. Eight bits per each Blue, Red, and Green component. 
            /// This is the native 24 bit format for the Win32 platform.
            /// </summary>
            PIXEL_FORMAT_BGR = 3,
            /// <summary>
            /// Each pixel is represented by 32 bits. Eight bits per each Red, Green, Blue, and Alpha component.
            /// </summary>
            PIXEL_FORMAT_RGBA = 4,
            /// <summary>
            ///  Each pixel is represented by 24 bits. Eight bits per each Red, Green, and Blue component. 
            /// This is the native 24 bit format for the Mac platform.
            /// </summary>
            PIXEL_FORMAT_RGB = 5,
            /// <summary>
            /// 
            /// </summary>
            PIXEL_FORMAT_RGB565 = 6,
            /// <summary>
            /// 
            /// </summary>
            PIXEL_FORMAT_LUM = 7
        }

        /// <summary>
        /// Changes the undistortion mode.
        /// </summary>
        public enum UNDIST_MODE
        {
            /// <summary>
            /// no undistortion.
            /// </summary>
            UNDIST_NONE,
            /// <summary>
            /// Default value is UNDIST_STD which means that artoolkit's standard undistortion method is used.
            /// </summary>
            UNDIST_STD,
            /// <summary>
            /// let's use lookup-table undistortion for high-speed
            /// note: LUT only works with images up to 1024x1024
            /// </summary>
            UNDIST_LUT
        }

        /// <summary>
        /// Sets an image processing mode (half or full resolution). 
        /// </summary>
        public enum IMAGE_PROC_MODE
        {
            /// <summary>
            /// Half resolution is faster but less accurate.
            /// </summary>
            IMAGE_HALF_RES,
            /// <summary>
            /// When using full resolution smaller markers will be detected at a higher accuracy
            /// </summary>
            IMAGE_FULL_RES
        }

        /// <summary>
        /// Id-based markers directly encode the marker id in the image. 
        /// In order to use id-based markers, the marker size has to be 6x6, 12x12 or 18x18.
        /// </summary>
        public enum MARKER_MODE
        {
            /// <summary>
            /// Template markers are the classic marker type used in ARToolKit.
            /// </summary>
            MARKER_TEMPLATE,
            /// <summary>
            /// Id-based markers directly encode the marker id in the image.
            /// Simple markers use 3-times redundancy to increase robustness,
            /// </summary>
            MARKER_ID_SIMPLE,
            /// <summary>
            /// Id-based markers directly encode the marker id in the image.
            /// while BCH markers use an advanced CRC algorithm to detect and repair marker damages.
            /// </summary>
            MARKER_ID_BCH,
            //MARKER_ID_BCH2		// upcomming, not implemented yet
        }

        /// <summary>
        /// Changes the Pose Estimation Algorithm.
        /// </summary>
        public enum POSE_ESTIMATOR
        {
            /// <summary>
            /// (default) original "normal" pose estimator
            /// </summary>
            POSE_ESTIMATOR_ORIGINAL,			
            /// <summary>
            /// original "cont" pose estimator
            /// </summary>
            POSE_ESTIMATOR_ORIGINAL_CONT,		
            /// <summary>
            /// new "Robust Planar Pose" estimator.
            /// RPP is more robust than ARToolKit's standard pose estimator.
            /// NOTE I 'think' RPP has to be coplanar.
            /// </summary>
            POSE_ESTIMATOR_RPP					
        }
        #endregion
    }
}
