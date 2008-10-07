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

namespace TowerDefence3D
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {

        #region VARIABLE DECLARATIONS
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont CourierNew;

        //Width of "playfield"
        private const int PlayfieldWidth = 10;

        //Width of one playfield tile
        private const float TileWidth = 10.0f;

        //Our movement restriction information array - 0 - cant move there - Any other positive value - Move penality
        private byte[,] Towers;
        private Tower[,] Towerz;

        //Keyboard states
        private KeyboardState KBState_Current;
        private KeyboardState KBState_Prev;
        //Mouse states
        private MouseState MSState_Current;
        private MouseState MSState_Prev;

        //Elapsed game time
        float elapsedTime;

        //Matrix array, containing positions, where to place walls - you coudl also calculate them on the fly and save some memory, but i think, that
        //it works fine in this way (256kb isnt to much nowdays :))
        private Matrix[,] TowerMatrixs;

        //There is need to scale ground plane matrix, acording to PlayfieldWidth
        private Matrix PlaneMatrix;

        //Models used in this sample
        private Model Model_Tower;
        private Model Model_Sphere;
        private Model Model_Plane;

        //Box texture
        private Texture2D Texture_Tower;
        private Texture2D Texture_WhiteQuad;

        //Our "character" location in terms of current tile
        //private Point CharacterLocation;
        //Character model world matrix
        private Matrix CharacterMatrix;

        //Is we adding or removing boxes?
        private bool Tower_Add = false;

        //Effect
        private Effect effect;

        //Our Free-Fly Camera
        Camera camera;

        //Location in 3D where a mouse click has ocurred
        Vector3 Click;

        //Pathfinding object
        PathFinder myPathFinder;
        //List of found nodes:
        List<PathReturnNode> foundPath;
        Enemy Character;

        bool CanMove = false;

        //TD STUFF
        Enemy[] enemies;
        #endregion

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;

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

            Towers = new byte[PlayfieldWidth, PlayfieldWidth];
            Towerz = new Tower[PlayfieldWidth, PlayfieldWidth];
            TowerMatrixs = new Matrix[PlayfieldWidth, PlayfieldWidth];
            enemies = new Enemy[20];
            
            //Init walls array, so its emty at the start
            //Also inti our Matrix array, so it contains wall positions used to draw them
            for (int y = 0; y < PlayfieldWidth; y++)
            {
                for (int x = 0; x < PlayfieldWidth; x++)
                {
                    Towers[x, y] = 1;
                    Towerz[x, y] = new Tower(Vector3.Zero);
                    TowerMatrixs[x, y] = Matrix.CreateTranslation(new Vector3(x * TileWidth + 5.0f, y * TileWidth + 5.0f, 0.0f));
                }
            }

            PlaneMatrix = new Matrix();
            int Scale = PlayfieldWidth * (int)TileWidth;
            PlaneMatrix = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(new Vector3(Scale/2, Scale/2, 0));

            camera = new Camera(new Vector3(50, 50, 100), new Vector3(5.15f, 0, 2.35f));

            IsMouseVisible = true;

            //CharacterLocation = new Point(0, 0);

            myPathFinder = new PathFinder(Towers);
            myPathFinder.HeuristicEstimate = 8;

            Enemy.Initalize(24, (int)TileWidth);

            enemies[0] = new Enemy(new Vector2(0, 0));
            //Character = new Enemy(new Vector2(0, 0));

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
            CourierNew = Content.Load<SpriteFont>("CourierNew");

            //Load all models used in sample
            Model_Tower = Content.Load<Model>("Models\\box");
            Model_Sphere = Content.Load<Model>("Models\\SphereHighPoly");
            Model_Plane = Content.Load<Model>("Models\\plane");

            //Load textures
            Texture_Tower = Content.Load<Texture2D>("Models\\crate");
            Texture_WhiteQuad = Content.Load<Texture2D>("Models\\whiteTex");

            //We need to modify models texturing data, to create tiled texture, regarding
            //of the size of our playfield - here i get vertex data, modify it, set it back to model:
            VertexPositionNormalTexture[] Vertexs = new VertexPositionNormalTexture[4];
            Model_Plane.Meshes[0].VertexBuffer.GetData<VertexPositionNormalTexture>(Vertexs);
            for (int n = 0; n < Vertexs.Length; n += 1)
            {
                Vertexs[n].TextureCoordinate *= PlayfieldWidth;
            }
            Model_Plane.Meshes[0].VertexBuffer.SetData<VertexPositionNormalTexture>(Vertexs);

            //Load the effect
            effect = Content.Load<Effect>("Textured_Lit");

            //I set effect parameters, that will not change here
            effect.Parameters["lightPosition"].SetValue(new Vector3(80, 80, 80));
            effect.Parameters["ambientLightColor"].SetValue(Color.Black.ToVector4());
            effect.Parameters["diffuseLightColor"].SetValue(Color.White.ToVector4() * 0.2f );
            effect.Parameters["specularLightColor"].SetValue(Color.White.ToVector4()*0.2f);

            effect.Parameters["specularPower"].SetValue(32);
            effect.Parameters["specularIntensity"].SetValue(1.340f);

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
            elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KBState_Prev = KBState_Current;
            KBState_Current = Keyboard.GetState();

            MSState_Prev = MSState_Current;
            MSState_Current = Mouse.GetState();

            if (IsKeyPush(Keys.B))
            {
                Tower_Add = !Tower_Add;
            }

            if (IsKeyPush(Keys.O))
            {
                CanMove = !CanMove;
            }

            if (IsKeyPush(Keys.Subtract))
            {
                myPathFinder.HeuristicEstimate -= 1;
            }
            if (IsKeyPush(Keys.Add))
            {
                myPathFinder.HeuristicEstimate += 1;
            }

            camera.Update(elapsedTime, KBState_Current, MSState_Current, MSState_Prev, graphics.GraphicsDevice);

            //This is where you move character:
            if (MSState_Current.RightButton == ButtonState.Pressed && MSState_Prev.RightButton == ButtonState.Released)
            {
                if (CanMove)
                {
                    Click = GetCollision();

                    if (Click.X > 0 && Click.Y > 0)
                    {

                        //Point Start = new Point(((int)(Character.PositionCurrent.X / (int)TileWidth)), ((int)(Character.PositionCurrent.Y / (int)TileWidth)));
                        Point Start = new Point(((int)(enemies[0].PositionCurrent.X / (int)TileWidth)), ((int)(enemies[0].PositionCurrent.Y / (int)TileWidth)));
                        Point End = new Point(((int)Click.X) / (int)TileWidth, ((int)Click.Y) / (int)TileWidth);

                        if (End.X >= 0 && End.Y >= 0 && End.X < PlayfieldWidth && End.Y < PlayfieldWidth)
                        {
                            if (Start == End)
                                enemies[0].LinearMove(enemies[0].PositionCurrent, new Vector2(Click.X, Click.Y));
                            else
                            {
                                foundPath = myPathFinder.FindPath(Start, End);
                                if (foundPath != null)
                                    enemies[0].PathMove(ref foundPath, enemies[0].PositionCurrent, new Vector2(Click.X, Click.Y));
                            }
                        }
                    }

                }
            }

            //Add or Remove walls
            if (MSState_Current.LeftButton == ButtonState.Pressed)
            {
                Click = GetCollision();

                Point point = new Point(((int)Click.X) / (int)TileWidth, ((int)Click.Y) / (int)TileWidth);

                if (point.X >= 0 && point.Y >= 0 && point.X < PlayfieldWidth && point.Y < PlayfieldWidth)
                {
                    if (Tower_Add)
                    {
                        Towerz[point.X, point.Y].dead = 1;
                        Towers[point.X, point.Y] = 1;
                    }
                    else
                    {
                        Towerz[point.X, point.Y].dead = 0;
                        Towerz[point.X, point.Y].position = new Vector3((float)point.X, (float)point.Y, 0.0f);
                        Towers[point.X, point.Y] = 0;
                    }
                }
            }

            //Update character and it's mesh position
            enemies[0].Update(elapsedTime);
            CharacterMatrix = Matrix.CreateScale(4) * Matrix.CreateTranslation(enemies[0].PositionCurrent.X, enemies[0].PositionCurrent.Y, 2.5f);

            //Run the tower methods
            foreach (Tower t in Towerz)
            {
                t.FindTarget(enemies);
                t.Shoot();
                if (t.bullet != null)
                {
                    t.bullet.Move();
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Gets if key is pusshed down for moment
        /// </summary>
        /// <param name="key">Key pusshed</param>
        /// <returns></returns>
        private bool IsKeyPush(Keys key)
        {
            if (KBState_Current.IsKeyDown(key) && KBState_Prev.IsKeyUp(key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get collision from ray cast at mouse position to ground plane 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetCollision()
        {
            Vector3 startC = new Vector3(MSState_Current.X, MSState_Current.Y, 0.0f);
            Vector3 endC = new Vector3(MSState_Current.X, MSState_Current.Y, 4096.0f);

            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(startC,
                camera.mProjection, camera.mView, Matrix.Identity);

            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(endC,
                camera.mProjection, camera.mView, Matrix.Identity);

            Vector3 rayDirection = Vector3.Normalize(farPoint - nearPoint);
            float cosAlpha = Vector3.Dot(Vector3.UnitZ, rayDirection);
            float deltaD = Vector3.Dot(Vector3.UnitZ, nearPoint);

            float distance = deltaD / cosAlpha;

            return nearPoint - (rayDirection * distance);
        }

        float start = 0;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;


            //Set effect parameters
            effect.Parameters["view"].SetValue(camera.mView);
            effect.Parameters["projection"].SetValue(camera.mProjection);
            effect.Parameters["cameraPosition"].SetValue(camera.vecPosition);

            //Draw ground plane
            effect.Parameters["xTexture0"].SetValue(Texture_Tower);
            effect.Parameters["world"].SetValue(PlaneMatrix);

            effect.Parameters["emmissive"].SetValue(Color.White.ToVector4()*0.4f);
            DrawSampleMesh(Model_Plane);

            //Draw walls
            effect.Parameters["emmissive"].SetValue(Color.White.ToVector4() * 0.8f);
            effect.Parameters["xTexture0"].SetValue(Texture_Tower);
            DrawBoxArray(Model_Tower);

            //Draw character
            if (CharacterMatrix != null)
            {
                effect.Parameters["xTexture0"].SetValue(Texture_WhiteQuad);
                effect.Parameters["world"].SetValue(CharacterMatrix);

                if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    start = gameTime.ElapsedRealTime.Seconds;
                }

                Vector3 green = Color.Green.ToVector3();
                Vector3 red = Color.Red.ToVector3();
                Vector3 color = Vector3.Lerp(green, red, 1 - enemies[0].hp/5);
                //color.Normalize();

                if (start > 150)
                    start = 0;
                start+=0.1f;
                effect.Parameters["emmissive"].SetValue(new Color(color).ToVector4());
                DrawSampleMesh(Model_Sphere);
            }

            //Draw bullets
            foreach (Tower t in Towerz)
            {
                if (t.bullet != null)
                {
                    effect.Parameters["xTexture0"].SetValue(Texture_WhiteQuad);
                    effect.Parameters["world"].SetValue(t.bullet.matrix);
                    effect.Parameters["emmissive"].SetValue(Color.Red.ToVector4());

                    DrawSampleMesh(Model_Sphere);
                }
            }

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            spriteBatch.DrawString(CourierNew, "Press WASD to move", new Vector2(10, 0), Color.White);

            spriteBatch.DrawString(CourierNew, "Press 'o' to enable movement mode, click MB_Right to move = " + CanMove.ToString(), new Vector2(10, 14), Color.White);
            spriteBatch.DrawString(CourierNew, "Press 'B' to enable/disable wall drawing/clearig, click on map to draw = " + Tower_Add.ToString(), new Vector2(10, 30), Color.White);
            spriteBatch.DrawString(CourierNew, "Herusitic estimate is '" + myPathFinder.HeuristicEstimate.ToString() + "' Press +/- to change", new Vector2(10, 46), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawBoxArray(Model sampleMesh)
        {
            if (sampleMesh == null)
                return;

            //our sample meshes only contain a single part, so we don't need to bother
            //looping over the ModelMesh and ModelMeshPart collections. If the meshes
            //were more complex, we would repeat all the following code for each part
            ModelMesh mesh = sampleMesh.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            //set the vertex source to the mesh's vertex buffer
            graphics.GraphicsDevice.Vertices[0].SetSource(
                mesh.VertexBuffer, meshPart.StreamOffset, meshPart.VertexStride);

            //set the vertex delclaration
            graphics.GraphicsDevice.VertexDeclaration = meshPart.VertexDeclaration;

            //set the current index buffer to the sample mesh's index buffer
            graphics.GraphicsDevice.Indices = mesh.IndexBuffer;

            // set the current technique based on the user selection
            effect.CurrentTechnique = effect.Techniques["Pixel_Diffuse_Pixel_Phong"];

            //at this point' we're ready to begin drawing
            //To start using any effect, you must call Effect.Begin
            //to start using the current technique (set in LoadGraphicsContent)


            for (int y = 0; y < PlayfieldWidth; y += 1)
            {
                for (int x = 0; x < PlayfieldWidth; x += 1)
                {
                    if (Towerz[x, y].dead == 0)
                    {
                        effect.Parameters["world"].SetValue(TowerMatrixs[x, y]);

                        effect.Begin(SaveStateMode.None);

                        //now we loop through the passes in the teqnique, drawing each
                        //one in order
                        for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
                        {
                            //EffectPass.Begin will update the device to
                            //begin using the state information defined in the current pass
                            effect.CurrentTechnique.Passes[i].Begin();

                            //sampleMesh contains all of the information required to draw
                            //the current mesh       

                            graphics.GraphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList, meshPart.BaseVertex, 0,
                                meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);

                            //EffectPass.End must be called when the effect is no longer needed
                            effect.CurrentTechnique.Passes[i].End();
                        }

                        //Likewise, Effect.End will end the current technique
                        effect.End();
                    }

                }
            }
        }
        public void DrawSampleMesh(Model sampleMesh)
        {
            if (sampleMesh == null)
                return;

            //our sample meshes only contain a single part, so we don't need to bother
            //looping over the ModelMesh and ModelMeshPart collections. If the meshes
            //were more complex, we would repeat all the following code for each part
            ModelMesh mesh = sampleMesh.Meshes[0];
            ModelMeshPart meshPart = mesh.MeshParts[0];

            //set the vertex source to the mesh's vertex buffer
            graphics.GraphicsDevice.Vertices[0].SetSource(
                mesh.VertexBuffer, meshPart.StreamOffset, meshPart.VertexStride);

            //set the vertex delclaration
            graphics.GraphicsDevice.VertexDeclaration = meshPart.VertexDeclaration;

            //set the current index buffer to the sample mesh's index buffer
            graphics.GraphicsDevice.Indices = mesh.IndexBuffer;

            // set the current technique based on the user selection
            effect.CurrentTechnique = effect.Techniques["Pixel_Diffuse_Pixel_Phong"];

            //at this point' we're ready to begin drawing
            //To start using any effect, you must call Effect.Begin
            //to start using the current technique (set in LoadGraphicsContent)

            effect.Begin(SaveStateMode.None);

            //now we loop through the passes in the teqnique, drawing each
            //one in order
            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            {
                //EffectPass.Begin will update the device to
                //begin using the state information defined in the current pass
                effect.CurrentTechnique.Passes[i].Begin();

                //sampleMesh contains all of the information required to draw
                //the current mesh       

                graphics.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, meshPart.BaseVertex, 0,
                    meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);

                //EffectPass.End must be called when the effect is no longer needed
                effect.CurrentTechnique.Passes[i].End();
            }

            //Likewise, Effect.End will end the current technique
            effect.End();
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Main game = new Main())
            {
                game.Run();
            }
        }
    }
}
