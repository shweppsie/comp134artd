//----------------------------------------------
// (c) 2007 by casey chesnut, brains-N-brawn LLC
//----------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;

using ARTKPManagedWrapper;


namespace wpfTest
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class Window1 : System.Windows.Window
    {

        public Window1()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Window1_Loaded);

            this.btnMultBmp.Click += new RoutedEventHandler(btn_Click);
            this.btnMultConfig.Click += new RoutedEventHandler(btn_Click);
            this.btnMultDisctinct.Click += new RoutedEventHandler(btn_Click);
            this.btnSingIdBch.Click += new RoutedEventHandler(btn_Click);
            this.btnSingIdSimple.Click += new RoutedEventHandler(btn_Click);
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            SingleIdSimple();
            //SingleIdBch();
            //MultiConfig();
            //MultiDistinct();
            //MultiDistinctBmp(); //test with bmp instead of raw image
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == this.btnMultBmp)
            {
                this.MultiDistinctBmp();
            }
            else if (sender == this.btnMultConfig)
            {
                this.MultiConfig();
            }
            else if (sender == this.btnMultDisctinct)
            {
                this.MultiDistinct();
            }
            else if (sender == this.btnSingIdBch)
            {
                this.SingleIdBch();
            }
            else if (sender == this.btnSingIdSimple)
            {
                this.SingleIdSimple();
            }
        }

        private ModelVisual3D AddMarker(System.Windows.Media.Brush brush)
        {
            ModelVisual3D mv3d = new ModelVisual3D();
            Model3DGroup m3dg = new Model3DGroup();
            GeometryModel3D gm3d = new GeometryModel3D();
            gm3d.Material = new DiffuseMaterial(brush);
            gm3d.BackMaterial = new DiffuseMaterial(Brushes.Orange);
            MeshGeometry3D mg3d = new MeshGeometry3D();
            mg3d.Positions.Add(new Point3D(-20, 20, 0));
            mg3d.Positions.Add(new Point3D(-20, -20, 0));
            mg3d.Positions.Add(new Point3D(20, 20, 0));
            mg3d.Positions.Add(new Point3D(20, -20, 0));
            mg3d.TextureCoordinates.Add(new Point(0, 0));
            mg3d.TextureCoordinates.Add(new Point(0, 1));
            mg3d.TextureCoordinates.Add(new Point(1, 0));
            mg3d.TextureCoordinates.Add(new Point(1, 1));
            mg3d.TriangleIndices.Add(0);
            mg3d.TriangleIndices.Add(1);
            mg3d.TriangleIndices.Add(2);
            mg3d.TriangleIndices.Add(1);
            mg3d.TriangleIndices.Add(3);
            mg3d.TriangleIndices.Add(2);
            gm3d.Geometry = mg3d;
            m3dg.Children.Add(gm3d);
            mv3d.Content = m3dg;
            modelMarkers.Children.Add(mv3d);
            return mv3d;
        }
       
        private void MultiDistinctBmp()
        {
            try
            {
                //NOTE saved as 24bpp RGB images using MS Paint
                //NOTE had to flip vertical, or it would not work
                //NOTE converted image down to 320x240

                //Uri uriImage = new Uri("pack://siteoforigin:,,,/data/markerboard_480-499.bmp");
                Uri uriImage = new Uri("pack://siteoforigin:,,,/data/webcam_test.bmp");
                backImage.Source = new BitmapImage(uriImage);

                //string imagePath = "data/markerboard_480-499.bmp";
                string imagePath = "data/webcam_test.bmp";
                int imageWidth = 320;
                int imageHeight = 240;
                int bytesPerPixel = 3;
                byte[] imageBytes = new byte[imageWidth * imageHeight * bytesPerPixel];
                int retVal = -1;

                /*
                //imageBytes = new byte[imageWidth * imageHeight * bytesPerPixel];
                //IntPtr cameraBufferPointer = IntPtr.Zero;
                int numberOfBytesRead = ArManWrap.ARTKPLoadImagePath(imagePath, imageWidth, imageHeight, bytesPerPixel, imageBytes);
                //Array.Reverse(imageBytes);
                if (numberOfBytesRead <= 0)
                {
                    int one = 1;
                }
                */

                System.Drawing.Bitmap b = new System.Drawing.Bitmap(imagePath);
                //NOTE this is important, will not be recognized if flipped
                //b.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY); 
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, b.Width, b.Height);
                System.Drawing.Imaging.PixelFormat pf = b.PixelFormat;
                System.Drawing.Imaging.BitmapData bmpData = b.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, pf);
                IntPtr ptr = bmpData.Scan0;
                int numBytes = b.Width * b.Height * bytesPerPixel;
                imageBytes = new byte[numBytes];
                System.Runtime.InteropServices.Marshal.Copy(ptr, imageBytes, 0, imageBytes.Length);
                b.UnlockBits(bmpData);

                //TODO LoadImageBytes with raw image only
                //TODO be able to pass parameters into Tracker template
                IntPtr tracker = ArManWrap.ARTKPConstructTrackerMulti(-1, imageWidth, imageHeight);
                if (tracker == IntPtr.Zero)
                {
                    int one = 1;
                }

                //IntPtr markerInfos2;
                //int numMarkers2;
                //int retDetMar = ArManWrap.ARTKPArDetectMarkerLite(tracker, imageBytes, 160, out markerInfos2, out numMarkers2);

                //  ARTKPCleanup(tracker, cameraBufferPointer);
                //StringBuilder description = new StringBuilder();
                IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
                string desc = Marshal.PtrToStringAnsi(ipDesc);
                //string lastLog = ARTKPGetLastLog(tracker);
                //NOTE PIXEL_FORMAT_RGB and PIXEL_FORMAT_BGR both work
                int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_BGR); //.PIXEL_FORMAT_LUM);
                string cameraCalibrationPath = "data/no_distortion.cal"; // "data/LogitechPro4000.dat";
                string multiPath = "data/markerboard_480-499.cfg";
                retVal = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, multiPath, 1.0f, 1000.0f);
                //retVal = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, null, 1.0f, 1000.0f);
                if (retVal != 0)
                {
                    int one = 1;
                }
                //ArManWrap.ARTKPSetPatternWidth(tracker, 80);
                ArManWrap.ARTKPSetBorderWidth(tracker, 0.125f);
                ArManWrap.ARTKPSetThreshold(tracker, 160);
                ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
                //ARTKPSetPoseEstimator(tracker, (int)POSE_ESTIMATOR.POSE_ESTIMATOR_RPP);
                ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_SIMPLE);
                int numMarkers = ArManWrap.ARTKPCalcMulti(tracker, imageBytes);
                //float conf = ArManWrap.ARTKPGetConfidence(tracker);
                float[] modelViewMatrix = new float[16];
                ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
                float[] projMatrix = new float[16];
                ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);

                Matrix3D wpfModelViewMatrix = ArManWrap.GetWpfMatrixFromOpenGl(modelViewMatrix);
                matrixCamera.ViewMatrix = Matrix3D.Identity; // wpfModelViewMatrix;
                Matrix3D wpfProjMatrix = ArManWrap.GetWpfMatrixFromOpenGl(projMatrix);
                matrixCamera.ProjectionMatrix = wpfProjMatrix;

                ArManWrap.DumpMatrix("projection matrix", wpfProjMatrix);
                ArManWrap.DumpMatrix("modelView matrix", wpfModelViewMatrix);

                //markers
                modelMarkers.Children.Clear();
                if (numMarkers > 0)
                {
                    //int[] markerIDs = new int[numMarkers];
                    //ArManWrap.ARTKPGetDetectedMarkers(tracker, markerIDs);
                    for (int i = 0; i < numMarkers; i++)
                    {
                        ArManWrap.ARMarkerInfo armi = ArManWrap.ARTKPGetDetectedMarkerStruct(tracker, i);
                        IntPtr markerInfos = ArManWrap.ARTKPGetDetectedMarker(tracker, i); //armi.id);
                        float[] center = new float[2];
                        float width = 50;
                        float[] matrix = new float[12];
                        //float retGetTransMat = ArManWrap.ARTKPArMultiGetTransMat(tracker, out ipMarkerInfo, armi.id, config);
                        float retTransMat = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, matrix);

                        //add rectangle at each spot
                        ModelVisual3D mv3d = AddMarker(Brushes.Cyan);

                        MatrixTransform3D mt3d = new MatrixTransform3D();
                        //http://artoolkit.sourceforge.net/apidoc/ar_8h.html#01eddf593ac98e4547e7131263e0d8c6
                        //the transformation matrix from the marker coordinates to camera coordinate frame, 
                        //that is the relative position of real camera to the real marker 
                        Matrix3D m3d = ArManWrap.GetWpfMatrixFromOpenGl12(matrix);

                        //http://www.hitl.washington.edu/artoolkit/documentation/tutorialcamera.htm
                        //m3d.Invert();

                        //wpfProjMatrix
                        //wpfProjMatrix.Invert();
                        //wpfModelViewMatrix
                        //wpfModelViewMatrix.Invert();
                        mt3d.Matrix = m3d;
                        //mt3d.Matrix = wpfModelViewMatrix * m3d;
                        //mt3d.Matrix.Invert();
                        //Transform3DGroup t3dg = new Transform3DGroup();
                        //t3dg.Children.Add(new MatrixTransform3D(wpfProjMatrix));
                        //t3dg.Children.Add(mt3d);
                        //t3dg.Children.Add(new MatrixTransform3D(wpfModelViewMatrix));
                        mv3d.Transform = mt3d; // t3dg;

                        ArManWrap.DumpMatrix("marker " + i.ToString(), mt3d.Matrix);

                    }
                }
                ArManWrap.ARTKPCleanup(tracker, IntPtr.Zero);

            }
            catch (Exception ex)
            {
                int lastError = Marshal.GetLastWin32Error();
                MessageBox.Show("lastError : " + lastError.ToString() + "\r\n" + ex.ToString());
            }
        }

        private void MultiDistinct()
        {
            try
            {
                string sample = "markerboard_480-499"; //works
                Uri uriImage = new Uri("pack://siteoforigin:,,,/data/" + sample + ".jpg");
                backImage.Source = new BitmapImage(uriImage);

                string imagePath = "data/markerboard_480-499.raw";
                int imageWidth = 320;
                int imageHeight = 240;
                int bytesPerPixel = 1;
                byte[] imageBytes = new byte[imageWidth * imageHeight * bytesPerPixel];
                int retVal = -1;

                //imageBytes = new byte[imageWidth * imageHeight * bytesPerPixel];
                //IntPtr cameraBufferPointer = IntPtr.Zero;
                int numberOfBytesRead = ArManWrap.ARTKPLoadImagePath(imagePath, imageWidth, imageHeight, bytesPerPixel, imageBytes);
                //Array.Reverse(imageBytes);
                if (numberOfBytesRead <= 0)
                {
                    int one = 1;
                }
                
                //LoadImageBytes with raw image only
                //TODO be able to pass parameters into Tracker template
                IntPtr tracker = ArManWrap.ARTKPConstructTrackerMulti(-1, imageWidth, imageHeight);
                if (tracker == IntPtr.Zero)
                {
                    int one = 1;
                }

                //IntPtr markerInfos2;
                //int numMarkers2;
                //int retDetMar = ArManWrap.ARTKPArDetectMarkerLite(tracker, imageBytes, 160, out markerInfos2, out numMarkers2);

                //  ARTKPCleanup(tracker, cameraBufferPointer);
                //StringBuilder description = new StringBuilder();
                IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
                string desc = Marshal.PtrToStringAnsi(ipDesc);
                //string lastLog = ARTKPGetLastLog(tracker);
                int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_LUM);
                string cameraCalibrationPath = "data/no_distortion.cal"; // "data/LogitechPro4000.dat";
                string multiPath = "data/markerboard_480-499.cfg";
                retVal = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, multiPath, 1.0f, 1000.0f);
                //retVal = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, null, 1.0f, 1000.0f);
                if (retVal != 0)
                {
                    int one = 1;
                }
                //ArManWrap.ARTKPSetPatternWidth(tracker, 80);
                ArManWrap.ARTKPSetBorderWidth(tracker, 0.125f);
                ArManWrap.ARTKPSetThreshold(tracker, 160);
                ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
                //ARTKPSetPoseEstimator(tracker, (int)POSE_ESTIMATOR.POSE_ESTIMATOR_RPP);
                ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_SIMPLE);
                int numMarkers = ArManWrap.ARTKPCalcMulti(tracker, imageBytes);
                //float conf = ArManWrap.ARTKPGetConfidence(tracker);
                float[] modelViewMatrix = new float[16];
                ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
                float[] projMatrix = new float[16];
                ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);

                Matrix3D wpfModelViewMatrix = ArManWrap.GetWpfMatrixFromOpenGl(modelViewMatrix);
                matrixCamera.ViewMatrix = Matrix3D.Identity; // wpfModelViewMatrix;
                Matrix3D wpfProjMatrix = ArManWrap.GetWpfMatrixFromOpenGl(projMatrix);
                matrixCamera.ProjectionMatrix = wpfProjMatrix;

                ArManWrap.DumpMatrix("projection matrix", wpfProjMatrix);
                ArManWrap.DumpMatrix("modelView matrix", wpfModelViewMatrix);

                //markers
                modelMarkers.Children.Clear();
                if (numMarkers > 0)
                {
                    //int[] markerIDs = new int[numMarkers];
                    //ArManWrap.ARTKPGetDetectedMarkers(tracker, markerIDs);
                    for (int i = 0; i < numMarkers; i++)
                    {
                        ArManWrap.ARMarkerInfo armi = ArManWrap.ARTKPGetDetectedMarkerStruct(tracker, i);
                        IntPtr markerInfos = ArManWrap.ARTKPGetDetectedMarker(tracker, i); //armi.id);
                        float[] center = new float[2];
                        float width = 50;
                        float[] matrix = new float[12];
                        //float retGetTransMat = ArManWrap.ARTKPArMultiGetTransMat(tracker, out ipMarkerInfo, armi.id, config);
                        float retTransMat = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, matrix);

                        //add rectangle at each spot
                        ModelVisual3D mv3d = AddMarker(Brushes.Cyan);

                        MatrixTransform3D mt3d = new MatrixTransform3D();
                        //http://artoolkit.sourceforge.net/apidoc/ar_8h.html#01eddf593ac98e4547e7131263e0d8c6
                        //the transformation matrix from the marker coordinates to camera coordinate frame, 
                        //that is the relative position of real camera to the real marker 
                        Matrix3D m3d = ArManWrap.GetWpfMatrixFromOpenGl12(matrix); 

                        //http://www.hitl.washington.edu/artoolkit/documentation/tutorialcamera.htm
                        //m3d.Invert();
                        
                        //wpfProjMatrix
                        //wpfProjMatrix.Invert();
                        //wpfModelViewMatrix
                        //wpfModelViewMatrix.Invert();
                        mt3d.Matrix = m3d;
                        //mt3d.Matrix = wpfModelViewMatrix * m3d;
                        //mt3d.Matrix.Invert();
                        Transform3DGroup t3dg = new Transform3DGroup();
                        //t3dg.Children.Add(new MatrixTransform3D(wpfProjMatrix));
                        t3dg.Children.Add(mt3d);
                        //t3dg.Children.Add(new MatrixTransform3D(wpfModelViewMatrix));
                        mv3d.Transform = mt3d; // t3dg;

                        ArManWrap.DumpMatrix("marker " + i.ToString(), mt3d.Matrix);
                        
                    }
                }
                ArManWrap.ARTKPCleanup(tracker, IntPtr.Zero);

            }
            catch (Exception ex)
            {
                int lastError = Marshal.GetLastWin32Error();
                MessageBox.Show("lastError : " + lastError.ToString() + "\r\n" + ex.ToString());
            }
        }

        private void MultiConfig()
        {
            try
            {
                string sample = "markerboard_480-499"; //works
                Uri uriImage = new Uri("pack://siteoforigin:,,,/data/" + sample + ".jpg");
                backImage.Source = new BitmapImage(uriImage);                

                string imagePath = "data/markerboard_480-499.raw";
                int imageWidth = 320;
                int imageHeight = 240;
                int bytesPerPixel = 1;
                byte[] imageBytes = new byte[imageWidth * imageHeight * bytesPerPixel];
                int retVal = -1;

                //imageBytes = new byte[imageWidth * imageHeight * bytesPerPixel];
                //IntPtr cameraBufferPointer = IntPtr.Zero;
                int numberOfBytesRead = ArManWrap.ARTKPLoadImagePath(imagePath, imageWidth, imageHeight, bytesPerPixel, imageBytes);
                if (numberOfBytesRead <= 0)
                {
                    int one = 1;
                }
                //LoadImageBytes with raw image only
                //TODO be able to pass parameters into Tracker template
                IntPtr tracker = ArManWrap.ARTKPConstructTrackerMulti(-1, imageWidth, imageHeight);
                if (tracker == IntPtr.Zero)
                {
                    int one = 1;
                }
                //  ARTKPCleanup(tracker, cameraBufferPointer);
                //StringBuilder description = new StringBuilder();
                IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
                string desc = Marshal.PtrToStringAnsi(ipDesc);
                //string lastLog = ARTKPGetLastLog(tracker);
                int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_LUM);
                string cameraCalibrationPath = "data/LogitechPro4000.dat";
                string multiPath = "data/markerboard_480-499.cfg";
                retVal = ArManWrap.ARTKPInitMulti(tracker, cameraCalibrationPath, multiPath, 1.0f, 1000.0f);
                if (retVal != 0)
                {
                    int one = 1;
                }
                //ArManWrap.ARTKPSetPatternWidth(tracker, 80);
                ArManWrap.ARTKPSetBorderWidth(tracker, 0.125f);
                ArManWrap.ARTKPSetThreshold(tracker, 160);
                ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
                //ARTKPSetPoseEstimator(tracker, (int)POSE_ESTIMATOR.POSE_ESTIMATOR_RPP);
                ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_SIMPLE);
                int numMarkers = ArManWrap.ARTKPCalcMulti(tracker, imageBytes);
                //config
                IntPtr config = ArManWrap.ARTKPGetMultiMarkerConfig(tracker);
                ArManWrap.ARMultiMarkerInfoT configStruct = (ArManWrap.ARMultiMarkerInfoT)Marshal.PtrToStructure(config, typeof(ArManWrap.ARMultiMarkerInfoT));
                IntPtr current = configStruct.marker;
                modelMarkers.Children.Clear();
                for (int i = 0; i < configStruct.marker_num; i++)
                {
                    ArManWrap.ARMultiEachMarkerInfoT armemi = (ArManWrap.ARMultiEachMarkerInfoT)Marshal.PtrToStructure(current, typeof(ArManWrap.ARMultiEachMarkerInfoT));
                    //if (armemi.visible != -1)
                    //{
                        //<ModelVisual3D x:Name="modelRectangle">
                        //    <ModelVisual3D.Content>
                        //      <Model3DGroup>
                        //        <GeometryModel3D>
                        //          <GeometryModel3D.Geometry>
                        //            <MeshGeometry3D
                        //                Positions="-25 25 0, -25 -25 0, 25 25 0, 25 -25 0"
                        //            TextureCoordinates="0,0 0,1 1,0, 1,1"
                        //                TriangleIndices=" 0 1 2, 1 3 2" />
                        //          </GeometryModel3D.Geometry>
                        //          <GeometryModel3D.Material>
                        //            <DiffuseMaterial Brush="Cyan" />
                        //          </GeometryModel3D.Material>
                        //          <GeometryModel3D.BackMaterial>
                        //            <DiffuseMaterial Brush="Red" />
                        //          </GeometryModel3D.BackMaterial>             
                        //        </GeometryModel3D>
                        //      </Model3DGroup>
                        //    </ModelVisual3D.Content>
                        //  </ModelVisual3D>

                        //add rectangle at each spot
                        ModelVisual3D mv3d = null;
                        if (armemi.visible != -1)
                        {
                            mv3d = AddMarker(Brushes.Green);
                            //System.Console.WriteLine(armemi.patt_id.ToString());
                        }
                        else
                        {
                            mv3d = AddMarker(Brushes.Red);
                        }

                        MatrixTransform3D mt3d = new MatrixTransform3D();
                        Matrix3D m3d = new Matrix3D();
                        unsafe
                        {
                            m3d = ArManWrap.GetWpfMatrixFromOpenGl12Point(armemi.trans);
                        }
                        mt3d.Matrix = m3d;
                        mv3d.Transform = mt3d;
                        ArManWrap.DumpMatrix("marker " + i.ToString(), mt3d.Matrix);
                    //}
                    current = (IntPtr)((int)current + Marshal.SizeOf(armemi));
                }
                //float conf = ArManWrap.ARTKPGetConfidence(tracker);
                float[] modelViewMatrix = new float[16];
                ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
                float[] projMatrix = new float[16];
                ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);
                ArManWrap.ARTKPCleanup(tracker, IntPtr.Zero);

                Matrix3D wpfModelViewMatrix = ArManWrap.GetWpfMatrixFromOpenGl(modelViewMatrix);
                matrixCamera.ViewMatrix = wpfModelViewMatrix;
                Matrix3D wpfProjMatrix = ArManWrap.GetWpfMatrixFromOpenGl(projMatrix);
                matrixCamera.ProjectionMatrix = wpfProjMatrix;

                ArManWrap.DumpMatrix("projection matrix", wpfProjMatrix);
                ArManWrap.DumpMatrix("modelView matrix", wpfModelViewMatrix);
            }
            catch (Exception ex)
            {
                int lastError = Marshal.GetLastWin32Error();
                MessageBox.Show("lastError : " + lastError.ToString() + "\r\n" + ex.ToString());
            }
        }

        private void SingleIdBch()
        {
            try
            {
                string sample = "image_320_240_8_marker_id_bch_nr0100"; //works
                Uri uriImage = new Uri("pack://siteoforigin:,,,/data/" + sample + ".jpg");
                backImage.Source = new BitmapImage(uriImage);

                string imagePath = "data/image_320_240_8_marker_id_bch_nr0100.raw";
                int imageWidth = 320;
                int imageHeight = 240;
                int bytesPerPixel = 1;
                byte[] imageBytes = new byte[imageWidth * imageHeight * bytesPerPixel];
                int retVal = -1;

                //imageBytes = new byte[imageWidth * imageHeight * bytesPerPixel];
                //IntPtr cameraBufferPointer = IntPtr.Zero;
                int numberOfBytesRead = ArManWrap.ARTKPLoadImagePath(imagePath, imageWidth, imageHeight, bytesPerPixel, imageBytes);
                if (numberOfBytesRead <= 0)
                {
                    int one = 1;
                }
                //LoadImageBytes with raw image only
                //TODO be able to pass parameters into Tracker template
                IntPtr tracker = ArManWrap.ARTKPConstructTrackerSingle(-1, imageWidth, imageHeight);
                if (tracker == IntPtr.Zero)
                {
                    int one = 1;
                }
                //  ARTKPCleanup(tracker, cameraBufferPointer);
                //StringBuilder description = new StringBuilder();
                IntPtr ipDesc = ArManWrap.ARTKPGetDescription(tracker);
                string desc = Marshal.PtrToStringAnsi(ipDesc);
                //string lastLog = ARTKPGetLastLog(tracker);
                int pixelFormat = ArManWrap.ARTKPSetPixelFormat(tracker, (int)ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_LUM);
                string cameraCalibrationPath = "data/LogitechPro4000.dat";
                retVal = ArManWrap.ARTKPInit(tracker, cameraCalibrationPath, 1.0f, 1000.0f);
                if (retVal != 0)
                {
                    int one = 1;
                }
                ArManWrap.ARTKPSetPatternWidth(tracker, 80);
                ArManWrap.ARTKPSetBorderWidth(tracker, 0.125f);
                ArManWrap.ARTKPSetThreshold(tracker, 150);
                ArManWrap.ARTKPSetUndistortionMode(tracker, (int)ArManWrap.UNDIST_MODE.UNDIST_LUT);
                //ARTKPSetPoseEstimator(tracker, (int)POSE_ESTIMATOR.POSE_ESTIMATOR_RPP);
                ArManWrap.ARTKPSetMarkerMode(tracker, (int)ArManWrap.MARKER_MODE.MARKER_ID_BCH);
                //int markerId = ARTKPCalc(tracker, imageBytes);
                int pattern = -1;
                bool updateMatrix = true;
                IntPtr markerInfos = IntPtr.Zero;
                int numMarkers;
                int markerId = ArManWrap.ARTKPCalc(tracker, imageBytes, pattern, updateMatrix, out markerInfos, out numMarkers);
                modelMarkers.Children.Clear();
                if (numMarkers == 1)
                {
                    AddMarker(Brushes.Cyan);
                    ArManWrap.ARMarkerInfo markerInfo = (ArManWrap.ARMarkerInfo)Marshal.PtrToStructure(markerInfos, typeof(ArManWrap.ARMarkerInfo));
                    float[] center = new float[] { 0, 0 };
                    float width = 50;
                    float[] markerMatrix = new float[12];
                    float retTransMat = ArManWrap.ARTKPGetTransMat(tracker, markerInfos, center, width, markerMatrix);
                    Marshal.DestroyStructure(markerInfos, typeof(ArManWrap.ARMarkerInfo));
                }
                float conf = ArManWrap.ARTKPGetConfidence(tracker);
                float[] modelViewMatrix = new float[16];
                ArManWrap.ARTKPGetModelViewMatrix(tracker, modelViewMatrix);
                float[] projMatrix = new float[16];
                ArManWrap.ARTKPGetProjectionMatrix(tracker, projMatrix);
                ArManWrap.ARTKPCleanup(tracker, IntPtr.Zero);

                Matrix3D wpfModelViewMatrix = ArManWrap.GetWpfMatrixFromOpenGl(modelViewMatrix);
                matrixCamera.ViewMatrix = wpfModelViewMatrix;
                Matrix3D wpfProjMatrix = ArManWrap.GetWpfMatrixFromOpenGl(projMatrix);
                matrixCamera.ProjectionMatrix = wpfProjMatrix;
            }
            catch (Exception ex)
            {
                int lastError = Marshal.GetLastWin32Error();
                MessageBox.Show("lastError : " + lastError.ToString() + "\r\n" + ex.ToString());
            }
        }

        private void SingleIdSimple()
        {
            try
            {
                //set the background image that is being tracked against
                //3D rectangles will be rendered over this image in a Viewport3D
                string sample = "image_320_240_8_marker_id_simple_nr031";
                Uri uriImage = new Uri("pack://siteoforigin:,,,/data/" + sample + ".jpg");
                backImage.Source = new BitmapImage(uriImage);
                //get the raw sample image bits the same way the sample does
                //this will be done differently when using a webcam feed
                string imagePath = "data/" + sample + ".raw";
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
                modelMarkers.Children.Clear();
                if (numMarkers == 1)
                {
                    //add rectangle marker to 3D scene at the origin
                    AddMarker(Brushes.Cyan);
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
                //apply model view matrix to MatrixCamera
                Matrix3D wpfModelViewMatrix = ArManWrap.GetWpfMatrixFromOpenGl(modelViewMatrix);
                matrixCamera.ViewMatrix = wpfModelViewMatrix;
                //apply projection matrix to MatrixCamera
                Matrix3D wpfProjMatrix = ArManWrap.GetWpfMatrixFromOpenGl(projMatrix);
                matrixCamera.ProjectionMatrix = wpfProjMatrix;
            }
            catch (Exception ex)
            {
                int lastError = Marshal.GetLastWin32Error();
                MessageBox.Show("lastError : " + lastError.ToString() + "\r\n" + ex.ToString());
            }
        }

    }
}