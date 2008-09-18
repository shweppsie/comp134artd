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

namespace game1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private Model gameShip;
        private Vector3 Position = Vector3.One;
        private float Zoom = 50;
        private float RotationY = 0.0f;
        private float RotationX = 0.0f;
        private Matrix gameWorldRotation;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

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

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            gameShip = Content.Load<Model>("BaseTower");
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

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
        /// 
        
        protected override void Update(GameTime gameTime)
        {
            UpdateGamePad();
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
        /// 


        private void DrawModel(Model m)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            float aspectRatio = graphics.GraphicsDevice.Viewport.Width / graphics.GraphicsDevice.Viewport.Height;
            m.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
            aspectRatio, 1.0f, 10000.0f);
            Matrix view = Matrix.CreateLookAt(new Vector3(0.0f, 50.0f, Zoom), Vector3.Zero, Vector3.Up);

            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = gameWorldRotation * transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(Position);
                }
                mesh.Draw();
            }
        }
        
        private void UpdateGamePad()
        {
            KeyboardState state = Keyboard.GetState();
            //GamePadState state = GamePad.GetState(PlayerIndex.One);

            if (state.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            if (state.IsKeyDown(Keys.Left))
            {
                Position.X += 1;
            }

            if (state.IsKeyDown(Keys.Right))
            {
                Position.X += -1;
            }
            if (state.IsKeyDown(Keys.Up))
            {
                Zoom++; 

            }

            
            
            if (state.IsKeyDown(Keys.A))
            {
                RotationX += 1.0f;
            }
            else if (state.IsKeyDown(Keys.S))
            {
                RotationX -= 1.0f;
            }
            gameWorldRotation = Matrix.CreateRotationX(MathHelper.ToRadians(RotationX)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(RotationY));
        }


        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawModel(gameShip);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
