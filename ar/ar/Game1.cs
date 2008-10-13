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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //AR Stuff
        Camera cam;
        Tracker tracker;
        Matrix matrix;

        //XNA Stuff
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model mod;
        BasicEffect e;
        Matrix world, view, projection;
        Texture2D t;
        Texture2D i;
        byte[] w;

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
            //Console.WriteLine("Updating");
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                this.Exit();

            matrix = new Matrix();
            byte[] w = cam.GetFlippedImage();
            if (tracker.Track(w , out view, out matrix))
            {
                //do drawing stuff
            }

            GraphicsDevice.Textures[0] = null; 
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
            //projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60.0f), 1.33333f, 1.0f, 1000.0f);
            //foreach (MyMarkerInfo mmi in ar.dicMarkerInfos.Values)
            //{
                //Console.WriteLine(mmi.transform);
                //myMarkerInfo.transform
                //e.World = Matrix.CreateScale(4.0f) * ;
                e.World = Matrix.Identity;
                e.View = view;
                //e.View = Matrix.Identity;
                e.DiffuseColor = Color.Purple.ToVector3();
                e.Texture = t;
                e.TextureEnabled = false;
                e.Projection = projection;

                // TODO: Add your drawing code here
                foreach (ModelMesh m in mod.Meshes)
                {
                   m.Draw();
                }
            //}

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            tracker.Dispose();
            base.OnExiting(sender, args);
        }
    }
}
