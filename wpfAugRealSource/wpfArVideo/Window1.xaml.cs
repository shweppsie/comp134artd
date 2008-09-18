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
using System.Threading;
using System.IO;
using System.Windows.Markup;
using System.Reflection;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;

using DirectShowLib;
using WPFUtil;
using ARTKPManagedWrapper;
using System.Xml;

namespace wpfArVideo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class Window1 : System.Windows.Window
    {

        public Window1()
        {
            try
            {
                InitializeComponent();

                this.Loaded += new RoutedEventHandler(Window1_Loaded);
                this.Closing += new System.ComponentModel.CancelEventHandler(Window1_Closing);

                LoadCaptureDevices();

                this.btnVideoStart.Click += new RoutedEventHandler(btn_Click);
                this.btnTrackStart.Click += new RoutedEventHandler(btn_Click);
                this.btnTrackStop.Click += new RoutedEventHandler(btn_Click);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Window1 " + ex.ToString());
            }
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == this.btnVideoStart)
                {
                    WebcamVideo();
                }
                else if (sender == this.btnTrackStart)
                {
                    StartTracking();
                }
                else if (sender == this.btnTrackStop)
                {
                    StopTracking();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("btn_Click " + ex.ToString());
            }
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitModels();

                //WebcamVideo();
                //StartTracking();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Window1_Loaded " + ex.ToString());
            }
        }

        void Window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cam != null)
            {
                cam.Dispose();
            }
        }

        int _width = 640;
        int _height = 480;
        int _bytesPerPixel = 4;
        PixelFormat _mediaBitmapSourcePixelFormat = PixelFormats.Bgra32; //.Rgb24;
        Guid _sampleGrabberSubType = MediaSubType.ARGB32; //.RGB24;
        ArManWrap.PIXEL_FORMAT _arPixelFormat = ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_ABGR; //.PIXEL_FORMAT_RGB;
        //System.Drawing.Imaging.PixelFormat _bmpSavePixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb; //.Format24bppRgb;

        #region WPF_WEBCAM
        SnapShot.Capture cam = null;
        WPFUtil.BitmapBuffer buf = null;
        System.Windows.Forms.Panel panel; //putting Panel in HwndHost didnt help with frame rate loss

        private void LoadCaptureDevices()
        {
            //TODO load, autoselect first one, then fix deviceNum below
            DsDevice[] vidCapDev = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            foreach (DsDevice dd in vidCapDev)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = dd.Name;
                cbi.Tag = dd;
                cbDevices.Items.Add(cbi);
            }
            if (vidCapDev.Length >= 1)
            {
                cbDevices.SelectedIndex = 0;
            }
        }

        private void WebcamVideo()
        {
            if (cam != null)
            {
                MessageBox.Show("webcam already started");
                return;
            }

            //These are 'dummy' pixels
            byte[] pixels = new byte[_width * _height * _bytesPerPixel];
            //Create a new bitmap source
            BitmapSource bmpsrc = BitmapSource.Create(_width, _height, 96, 96,
                _mediaBitmapSourcePixelFormat, null, pixels, _width * _bytesPerPixel);
            //Set our Image in our Xaml to our new bitmap
            videoImage.Source = bmpsrc;

            //Create our helper class
            buf = new WPFUtil.BitmapBuffer(bmpsrc);
            IntPtr buffPointer = buf.BufferPointer;

            panel = new System.Windows.Forms.Panel();
            short bitsPerPixel = (short)(_bytesPerPixel * 8);
            int deviceNum = 0;
            deviceNum = cbDevices.SelectedIndex;
            cam = new SnapShot.Capture(deviceNum, _width, _height, bitsPerPixel, panel, _sampleGrabberSubType);
            cam.Dispatcher = this.Dispatcher;
            cam.SampleEvent += new SnapShot.Capture.SampleDelegate(cam_SampleEvent);
        }

        object syncObject = new object();

        void cam_SampleEvent(IntPtr pBuffer, int BufferLen)
        {
            if (buf != null && cam != null)
            {
                try
                {
                    if (Monitor.TryEnter(syncObject) == true)
                    {
                        try
                        {
                            BitmapBuffer.CopyMemory(buf.BufferPointer, pBuffer, BufferLen);
                            videoImage.InvalidateVisual();
                            //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new InvalidateImageDelegate(InvalidateImage));
                            //System.Threading.Thread.Sleep(55);
                            //Marshal.Release(pBuffer);
                        }
                        finally
                        {
                            Monitor.Exit(syncObject);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private delegate void InvalidateImageDelegate();
        private void InvalidateImage()
        {
            videoImage.InvalidateVisual();
        }
        #endregion

        #region AR_TRACKING
        public void StartTracking()
        {
            StopTracking();

            TrackDelegate trackDelegate = new TrackDelegate(Track);
            trackDelegate.BeginInvoke(null, null);
        }

        public void StopTracking()
        {
            _runTracking = false;
            modelMarkers.Children.Clear();
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
                UpdateViewportDelegate updateViewDel = new UpdateViewportDelegate(UpdateViewport);

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

                        Matrix3D wpfModelViewMatrix = ArManWrap.GetWpfMatrixFromOpenGl(modelViewMatrix);
                        Matrix3D wpfProjMatrix = ArManWrap.GetWpfMatrixFromOpenGl(projMatrix);

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
                                Matrix3D m3d = ArManWrap.GetWpfMatrixFromOpenGl12(matrix);
                                mmi.transform = m3d;
                            }
                        }

                        args[0] = wpfModelViewMatrix;
                        args[1] = dicMarkerInfos;
                        //get back on UI thread
                        this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render,
                            updateViewDel, wpfProjMatrix, args);
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
            public Matrix3D transform;
            public ArManWrap.ARMarkerInfo markerInfo;
            public float[] prevMatrix = new float[0];
            public bool found = false;
            public ModelVisual3D modelVisual3D = null;
            public int notFoundCount = 0;
        }

        private bool cameraUpdated = false;

        private delegate void UpdateViewportDelegate(Matrix3D projMatrix, Matrix3D modelViewMatrix, Dictionary<int, MyMarkerInfo> markerInfos);
        private void UpdateViewport(Matrix3D projMatrix, Matrix3D modelViewMatrix, Dictionary<int, MyMarkerInfo> markerInfos)
        {
            if (cameraUpdated == false)
            {
                //TODO fix the far plane distance?
                ArManWrap.DumpMatrix("projection ", projMatrix);
                matrixCamera.ProjectionMatrix = projMatrix;
                matrixCamera.ViewMatrix = Matrix3D.Identity;
                cameraUpdated = true;
            }
            //modelMarkers.Children.Clear();
            List<int> delKeys = new List<int>();
            foreach (MyMarkerInfo mmi in markerInfos.Values)
            {
                if (mmi.found == false)
                {
                    //to help with Viewport3D flicker when model is removed and then added again
                    //If the virtual image does not appear, or it flickers in and out of view 
                    //it may be because of lighting conditions. 
                    //This can often be fixed by changing the lighting threshold value used by the image processing routines.
                    if (mmi.notFoundCount >= 10) //TODO hardcoded value is a WAG
                    {
                        if (mmi.modelVisual3D is IModelCode)
                        {
                            IModelCode imc = (IModelCode)mmi.modelVisual3D;
                            imc.Stop();
                        }
                        modelMarkers.Children.Remove(mmi.modelVisual3D);
                        delKeys.Add(mmi.markerInfo.id);
                        //System.Console.WriteLine("removing " + mmi.markerInfo.id.ToString());
                    }
                    else
                    {
                        mmi.notFoundCount = mmi.notFoundCount + 1;
                        //System.Console.WriteLine("not found " + mmi.markerInfo.id.ToString());
                    }
                }
                else
                {
                    if (mmi.modelVisual3D == null)
                    {
                        //ModelVisual3D mv3d = AddMarker(Brushes.Cyan);
                        ModelVisual3D mv3d = AddMarkerModel(mmi.markerInfo.id);
                        mmi.modelVisual3D = mv3d;
                        MatrixTransform3D mt3d = new MatrixTransform3D(mmi.transform);
                        mv3d.Transform = mt3d;
                        System.Console.WriteLine("adding " + mmi.markerInfo.id.ToString());
                    }
                    else
                    {
                        MatrixTransform3D mt3d = (MatrixTransform3D)mmi.modelVisual3D.Transform;
                        mt3d.Matrix = mmi.transform;
                        //System.Console.WriteLine("updating " + mmi.markerInfo.id.ToString());
                    }
                    //MatrixTransform3D mt3d = new MatrixTransform3D(mmi.transform);
                    //mmi.modelVisual3D.Transform = mt3d;
                }
            }
            foreach (int delKey in delKeys)
            {
                markerInfos.Remove(delKey);
            }
        }
        #endregion

        #region 3D_MODELS
        XmlDocument xdModels = new XmlDocument();
        Dictionary<int, MyModel> dicModels = new Dictionary<int, MyModel>();

        public class MyModel
        {
            public int id;
            public bool trans = false;
            public string path;
            public string code;
            public Viewbox root; 
            public Model3DGroup m3dg;
            public int rotX;
            public int rotY;
            public int rotZ;
            public int sizeX;
            public int sizeY;
            public int sizeZ;
            public int offX;
            public int offY;        
            public int offZ;
        }

        private void InitModels()
        {
            try
            {
                xdModels.Load("models.xml");
                foreach (XmlNode xnNode in xdModels.DocumentElement.ChildNodes)
                {
                    if (xnNode.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    XmlElement xeNode = (XmlElement)xnNode;
                    MyModel mm = new MyModel();
                    mm.id = Int32.Parse(xeNode.Attributes["id"].Value);
                    mm.trans = bool.Parse(xeNode.Attributes["trans"].Value);
                    mm.path = xeNode.Attributes["path"].Value;
                    mm.code = xeNode.Attributes["code"].Value;
                    mm.rotX = Int32.Parse(xeNode.Attributes["rotX"].Value);
                    mm.rotY = Int32.Parse(xeNode.Attributes["rotY"].Value);
                    mm.rotZ = Int32.Parse(xeNode.Attributes["rotZ"].Value);
                    mm.sizeX = Int32.Parse(xeNode.Attributes["sizeX"].Value);
                    mm.sizeY = Int32.Parse(xeNode.Attributes["sizeY"].Value);
                    mm.sizeZ = Int32.Parse(xeNode.Attributes["sizeZ"].Value);
                    mm.offX = Int32.Parse(xeNode.Attributes["offX"].Value);
                    mm.offY = Int32.Parse(xeNode.Attributes["offY"].Value);
                    mm.offZ = Int32.Parse(xeNode.Attributes["offZ"].Value);

                    FileStream fs = new FileStream(mm.path, FileMode.Open, FileAccess.Read); //
                    Viewbox vb = (Viewbox)XamlReader.Load(fs);
                    fs.Close();
                    mm.root = vb; //for INameScope

                    Viewport3D v3d = (Viewport3D)vb.Child;
                    ModelVisual3D mv3d = (ModelVisual3D)v3d.Children[0];
                    Model3DGroup m3dgScene = (Model3DGroup)mv3d.Content;
                    Model3DGroup m3dg = (Model3DGroup)m3dgScene.Children[m3dgScene.Children.Count - 1];
                    mm.m3dg = m3dg;

                    if (mm.id == 482)
                    {
                        int one = 1; //debug
                    }

                    //m3dg.Transform = null;
                    Transform3DGroup t3dg = new Transform3DGroup();
                    m3dg.Transform = t3dg;
                    //change size
                    double scaleX = mm.sizeX / m3dg.Bounds.SizeX;
                    double scaleY = mm.sizeY / m3dg.Bounds.SizeY;
                    double scaleZ = mm.sizeZ / m3dg.Bounds.SizeZ;
                    scaleX = FixDouble(scaleX);
                    scaleY = FixDouble(scaleY);
                    scaleZ = FixDouble(scaleZ);
                    t3dg.Children.Add(new ScaleTransform3D(scaleX, scaleY, scaleZ));
                    //move to origin
                    double offX = (-1 * m3dg.Bounds.X) - (m3dg.Bounds.SizeX / 2);
                    double offY = (-1 * m3dg.Bounds.Y) - (m3dg.Bounds.SizeY / 2);
                    double offZ = (-1 * m3dg.Bounds.Z) - (m3dg.Bounds.SizeZ / 2);
                    offX = FixDouble(offX);
                    offY = FixDouble(offY);
                    offZ = FixDouble(offZ);
                    t3dg.Children.Add(new TranslateTransform3D(offX, offY, offZ));
                    //rotate
                    t3dg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), mm.rotX)));
                    t3dg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), mm.rotY)));
                    t3dg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), mm.rotZ)));
                    //move to top of Z
                    t3dg.Children.Add(new TranslateTransform3D(0, 0, m3dg.Bounds.SizeZ / 2));
                    //t3dg.Children.Add(new TranslateTransform3D(0, m3dg.Bounds.SizeY / 2, 0));
                    //move to offset
                    t3dg.Children.Add(new TranslateTransform3D(mm.offX, mm.offY, mm.offZ)); //0,0,0

                    dicModels.Add(mm.id, mm);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private double FixDouble(double d)
        {
            if (double.IsNaN(d) == true || double.IsPositiveInfinity(d) == true || double.IsNegativeInfinity(d) == true)
            {
                return 0;
            }
            return d;
        }

        Random rand = new Random();
        private Brush GetRandomBrush()
        {
            Type t = typeof(Brushes);
            MemberInfo[] mia = t.GetMembers(BindingFlags.Public | BindingFlags.Static);
            List<string> lstBrushes = new List<string>();
            foreach (MemberInfo mi in mia)
            {
                if (mi.Name.StartsWith("get_") == true)
                    continue;
                lstBrushes.Add(mi.Name);
            }
            int val = rand.Next(lstBrushes.Count);
            string colorName = lstBrushes[val];
            BrushConverter bc = new BrushConverter();
            Brush b = (Brush)bc.ConvertFromString(colorName);
            return b; // Brushes.Cyan;
        }

        private ModelVisual3D AddMarker(System.Windows.Media.Brush brush)
        {
            ModelVisual3D mv3d = new ModelVisual3D();
            Model3DGroup m3dg = new Model3DGroup();
            GeometryModel3D gm3d = new GeometryModel3D();
            gm3d.Material = new DiffuseMaterial(brush);
            gm3d.BackMaterial = new DiffuseMaterial(Brushes.Orange);
            MeshGeometry3D mg3d = new MeshGeometry3D();
            mg3d.Positions.Add(new Point3D(-10, 10, 0));
            mg3d.Positions.Add(new Point3D(-10, -10, 0));
            mg3d.Positions.Add(new Point3D(10, 10, 0));
            mg3d.Positions.Add(new Point3D(10, -10, 0));
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

        private ModelVisual3D AddMarkerModel(int markerId)
        {
            ModelVisual3D mv3d;
            if (dicModels.ContainsKey(markerId) == true)
            {
                MyModel mm = dicModels[markerId];
                Model3DGroup m3dg = mm.m3dg;

                if (mm.code == null || mm.code == String.Empty)
                {
                    mv3d = new ModelVisual3D();
                    mv3d.Content = m3dg;
                    AddModelBasedOnTransparency(mv3d, mm.trans);

                    //change MaterialProperty randomly
                    //TODO this will screw up model with transparency
                    GeometryModel3D gm3d = (GeometryModel3D)m3dg.Children[0];
                    MaterialGroup mg = (MaterialGroup)gm3d.Material;
                    DiffuseMaterial dm = (DiffuseMaterial)mg.Children[0];
                    dm.Brush = GetRandomBrush();
                }
                else
                {
                    string strType = mm.code;
                    Type type = Type.GetType(strType);
                    object modelCode = Activator.CreateInstance(type);
                    mv3d = (ModelVisual3D)modelCode;
                    mv3d.Content = m3dg;
                    AddModelBasedOnTransparency(mv3d, mm.trans);

                    IModelCode imc = (IModelCode)modelCode;
                    imc.Init(mm);
                    imc.Start();
                }
            }
            else
            {
                System.Console.WriteLine("markerID : " + markerId.ToString());
                mv3d = AddMarker(Brushes.Red);
            }
            
            return mv3d;
        }

        private void AddModelBasedOnTransparency(ModelVisual3D mv3d, bool trans)
        {
            if (trans == true)
            {
                //transparent items must come last because of z-buffer
                modelMarkers.Children.Add(mv3d);
            }
            else //not transparent, so add before transparent items
            {
                modelMarkers.Children.Insert(0, mv3d);
            }
        }
        #endregion   
    }
}