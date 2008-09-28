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

namespace _dtutorial
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GameObject terrain = new GameObject();
        GameObject missileLauncherBase = new GameObject();
        GameObject missileLauncherHead = new GameObject();
     
       
        Vector3 terrainPosition = Vector3.Zero;

        Vector3 cameraPosition = new Vector3(0.0f, 60.0f, 160.0f);
        Vector3 cameraLookAt = new Vector3(0.0f, 50.0f, 0.0f);
        Matrix cameraProjectionMatrix;
        Matrix cameraViewMatrix;



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
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            cameraViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up);
            cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), graphics.GraphicsDevice.Viewport.AspectRatio, 1.0f, 10000.0f);

            terrain.model = Content.Load<Model>("Models\\terrain");
            missileLauncherBase.scale = 0.2f;
            missileLauncherHead.model = Content.Load<Model>("Models\\launcher_head");
            missileLauncherBase.model = Content.Load<Model>("Models\\launcher_base");
            missileLauncherHead.scale = 0.2f;

            missileLauncherHead.position = missileLauncherBase.position + new Vector3(0.0f, 20.0f, 0.0f);
            
            
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

            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            missileLauncherHead.rotation.Y -= gamePadState.ThumbSticks.Left.X * 0.1f;
            missileLauncherHead.rotation.X -= gamePadState.ThumbSticks.Left.Y * 0.1f;
#if !XBOX
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                missileLauncherHead.rotation.Y +=0.05f;
               
            }
             if (keyboardState.IsKeyDown(Keys.Right))
            {
                missileLauncherHead.rotation.Y -=0.05f;
                
            }
             if (keyboardState.IsKeyDown(Keys.Up))
            {
                missileLauncherHead.rotation.X +=0.05f;
                
            }

             if (keyboardState.IsKeyDown(Keys.Down))
            {
                missileLauncherHead.rotation.X -=0.05f;
                
            }


            if (keyboardState.IsKeyDown(Keys.W))
            {
                missileLauncherBase.position.Z -= 1;
                missileLauncherHead.position.Z -= 1;
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
                missileLauncherBase.position.Z += 1;
                missileLauncherHead.position.Z += 1;
            }


            if (keyboardState.IsKeyDown(Keys.A))
            {
                missileLauncherBase.position.X -= 1;
                missileLauncherHead.position.X -= 1;
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                missileLauncherBase.position.X += 1;
                missileLauncherHead.position.X += 1;
            }


#endif
            missileLauncherHead.rotation.Y = MathHelper.Clamp(missileLauncherHead.rotation.Y, -MathHelper.PiOver4, MathHelper.PiOver4);
            missileLauncherHead.rotation.X = MathHelper.Clamp(missileLauncherHead.rotation.X, 0, MathHelper.PiOver4);


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

          // DrawModel(terrain.model, terrain.position);

           DrawGameObject(terrain);
           DrawGameObject(missileLauncherBase);
           DrawGameObject(missileLauncherHead);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
      
         }

        private void DrawGameObject(GameObject terrain)
        {
          DrawModel(terrain.model, terrain.position, terrain.rotation, terrain.scale)  ;
        }

        void DrawModel(Model model, Vector3 modelPosition, Vector3 modelRotation, float scale)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.DiffuseColor = Color.Blue.ToVector3();
                    
                   //effect.World = Matrix.CreateScale(scale)* 
                   //    Matrix.CreateFromYawPitchRoll(modelRotation.Y, modelRotation.X, modelRotation.Z)*
                   //    Matrix.CreateTranslation(modelPosition);

                    effect.World = Matrix.CreateScale(scale) * Matrix.CreateTranslation(modelPosition) * Matrix.CreateFromYawPitchRoll(modelRotation.Y, modelRotation.X, modelRotation.Z);
                   
                    //effect.World =  Matrix.CreateFromYawPitchRoll(modelRotation.Y, modelRotation.X, modelRotation.Z) * Matrix.CreateTranslation(modelPosition)*Matrix.CreateScale(scale);
                   
                    effect.Projection = cameraProjectionMatrix;
                    effect.View = cameraViewMatrix;
                }
                mesh.Draw();
            }
        }
    }

}
