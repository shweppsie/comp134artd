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

namespace projAR
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        AR ar;

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

            base.Initialize();

            ar = new AR(new forms.Panel(), 640, 480, 4);
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

            ar.Track(out projection);

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
            //spriteBatch.Draw(i, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
            //projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60.0f), 1.33333f, 1.0f, 1000.0f);
            foreach (MyMarkerInfo mmi in ar.dicMarkerInfos.Values)
            {
                Console.WriteLine(mmi.transform);
                //myMarkerInfo.transform
                e.World = Matrix.CreateScale(4.0f) * mmi.transform;
                e.View = Matrix.Identity;// view;
                e.DiffuseColor = Color.Purple.ToVector3();
                e.Texture = t;
                e.TextureEnabled = false;
                e.Projection = projection;

                // TODO: Add your drawing code here
                foreach (ModelMesh m in mod.Meshes)
                {
                    m.Draw();
                }
            }

            

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            ar.Dispose();
            base.OnExiting(sender, args);
        }
    }
}
