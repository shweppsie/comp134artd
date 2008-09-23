using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using ARTKPManagedWrapper;
using System.Runtime.InteropServices;

namespace ar
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model mod;
        BasicEffect e;
        Matrix world, view, projection;
        Texture2D t;
        Texture2D i;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
           // projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60.0f), 1.33333f, 1.0f, 1000.0f);
            view = Matrix.Identity;
            world = Matrix.Identity;

            SingleIdSimple();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            mod = Content.Load<Model>("gCrate");
            i = Content.Load<Texture2D>("image");

            e = new BasicEffect(GraphicsDevice, null);
            foreach (ModelMesh mesh in mod.Meshes)
            {
                foreach (ModelMeshPart p in mesh.MeshParts)
                {
                    t = (p.Effect as BasicEffect).Texture; 
                    p.Effect = e;
                }
            }

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(i, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();


            e.World = Matrix.CreateScale(4.0f)* world;
            e.View = view;
            e.DiffuseColor = Color.Purple.ToVector3();
            e.Texture = t;
            e.TextureEnabled = false;
            e.Projection = projection;
            
            // TODO: Add your drawing code here
            foreach (ModelMesh m in mod.Meshes)
            {
                m.Draw();
            }

            base.Draw(gameTime);
        }

        private void SingleIdSimple()
        {
            try
            {
                //image being tracked
                //pictureBox1.Image = Image.FromFile("data/image_320_240_8_marker_id_simple_nr031.jpg");
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
                int i = 0;
                projection = new Matrix(projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++],
                    projMatrix[i++]);
                //dispose of tracker
                ArManWrap.ARTKPCleanup(tracker, IntPtr.Zero);

                Matrix m3d = new Matrix();
                m3d.M11 = modelViewMatrix[0];
                m3d.M12 = modelViewMatrix[1];
                m3d.M13 = modelViewMatrix[2];
                m3d.M14 = modelViewMatrix[3];
                m3d.M21 = modelViewMatrix[4];
                m3d.M22 = modelViewMatrix[5];
                m3d.M23 = modelViewMatrix[6];
                m3d.M24 = modelViewMatrix[7];
                m3d.M31 = modelViewMatrix[8];
                m3d.M32 = modelViewMatrix[9];
                m3d.M33 = modelViewMatrix[10];
                m3d.M34 = modelViewMatrix[11];
                //m3d.OffsetX = modelViewMatrix[12];
                //m3d.OffsetY = modelViewMatrix[13];
                //m3d.OffsetZ = modelViewMatrix[14];
                m3d.M44 = modelViewMatrix[15];

                m3d.Translation = new Vector3(modelViewMatrix[12], modelViewMatrix[13], modelViewMatrix[14]);
                world = m3d;

                

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
            }
        }
    }
}
