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
using System.Runtime.InteropServices;

using ARTKPManagedWrapper;
using forms = System.Windows.Forms;
using System.Windows.Forms;
using DirectShowLib;

namespace projAR
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //AR Stuff
        Camera cam;
        Tracker tracker;

        //XNA Stuff
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model mod;
        BasicEffect e;
        Matrix world, view, projection;
        Texture2D t;
        Texture2D i;
        byte[] w;

        //list of markers return from AR
        Dictionary<int, MyMarkerInfo> ARMarkers;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            this.IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
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
            view = Matrix.Identity;
            world = Matrix.Identity;
            projection = Matrix.Identity;

            base.Initialize();
            try
            {
                //various variables
                const int width = 640;
                const int height = 480;
                const int bytesPerPixel = 4;
                Guid sampleGrabberSubType = MediaSubType.ARGB32;
                ArManWrap.PIXEL_FORMAT arPixelFormat = ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_ABGR;

                cam = new Camera(0, width, height, bytesPerPixel, sampleGrabberSubType);
                tracker = new Tracker(width, height, bytesPerPixel, sampleGrabberSubType, arPixelFormat);
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
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
            t = new Texture2D(GraphicsDevice, 640, 480, 1, TextureUsage.None, GraphicsDevice.PresentationParameters.BackBufferFormat);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                this.Exit();

            //get a frame of the webcam
            w = cam.GetFlippedImage();

            //give the webcam feed to AR to work with
            ARMarkers = tracker.Track(w, out projection);

            //fixes XNA bug with webcam feed
            GraphicsDevice.Textures[0] = null; 
            
            //output the webcam
            t.SetData<byte>(w);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw it self.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(t, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();

            #region AR Ghosting
            if (ARMarkers != null)
            {
                //check each marker in the list returned by AR
                foreach (MyMarkerInfo mmi in ARMarkers.Values)
                {
                    //marker not found
                    if (mmi.found == false)
                    {
                        //leave a ghost for 5 frames
                        if (mmi.notFoundCount < 5)
                        {
                            //count another frame for the ghost
                            mmi.notFoundCount += 1;
                        }
                        else
                        {
                            //marker has been gone too long, stop drawing it
                            if (mmi.draw == true)
                            {
                                mmi.draw = false;
                            }
                        }
                    }
                    else
                    //marker found
                    {
                        //so draw it
                        if (mmi.draw == false)
                            mmi.draw = true;
                    }
                }
            }
            #endregion

            //now to draw the markers
            foreach (MyMarkerInfo mmi in ARMarkers.Values)
            {
                //is this marker set to be drawn?
                if (mmi.draw)
                {
                    //System.Diagnostics.Debug.WriteLine(mmi.transform.Translation);
                    e.World = Matrix.CreateScale(4.0f)*Matrix.CreateTranslation(mmi.transform.Translation);//Matrix.Identity;
                    e.View = Matrix.Identity;
                    e.DiffuseColor = Color.Purple.ToVector3();
                    e.Texture = t;
                    e.TextureEnabled = false;
                    e.Projection = projection;

                    foreach (ModelMesh m in mod.Meshes)
                    {
                        m.Draw();
                    }
                }
            }

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            tracker.Dispose();
            base.OnExiting(sender, args);
        }
    }
}
