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
using System.Diagnostics;

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
        private const int PlayfieldWidth = 12;

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
        
        //Is we adding or removing boxes??
        private bool Tower_Add = false;

        //Effect
        private Effect effect;

        //Our Free-Fly Camera
        Camera camera;

        //Location in 3D where a mouse click has ocurred
        Vector3 Click;

        //Pathfinding object
        PathFinder myPathFinder;

        //Game pausing stopwatch
        Stopwatch pauseGame;
        bool paused;

        bool added;
        bool removed;

        //TD STUFF
        Enemy[] enemies;
        Point spawnPoint;
        Point endPoint;
        Stopwatch respawnTime;
        private Matrix orientation;
        int score, lives, money;

        #endregion

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;

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

            pauseGame = new Stopwatch();
            pauseGame.Start();
            paused = true;

            added = false;
            removed = false;

            score = 0;
            lives = 10;
            money = 100;

            spawnPoint = new Point((int)(PlayfieldWidth / 2), 0);
            endPoint = new Point((int)(PlayfieldWidth / 2), PlayfieldWidth - 1);
            respawnTime = new Stopwatch();
            respawnTime.Start();
            
            //Init walls array, so its empty at the start
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
            PlaneMatrix = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(new Vector3(Scale/2, Scale/2, -20));

            camera = new Camera(new Vector3(150, 150, 125), new Vector3(5.45f, 0, -3.95f));

            IsMouseVisible = true;

            myPathFinder = new PathFinder(Towers);
            myPathFinder.HeuristicEstimate = 8;

            Enemy.Initalize(24, (int)TileWidth);

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

            if (pauseGame.ElapsedMilliseconds > 100)
            {
                paused = false;
                pauseGame.Reset();
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
                        
            //Add or Remove walls
            if (MSState_Current.LeftButton == ButtonState.Pressed)
            {
                added = true;
            }
            if (MSState_Current.LeftButton == ButtonState.Released)
            {
                if (added == true)
                {
                    Click = GetCollision();
                    added = false;

                    Point point = new Point(((int)Click.X) / (int)TileWidth, ((int)Click.Y) / (int)TileWidth);

                    if (point.X >= 0 && point.Y >= 0 && point.X < PlayfieldWidth && point.Y < PlayfieldWidth)
                    {
                        if (money >= 10)
                        {
                            if (Towers[point.X, point.Y] == 1)
                            {
                                Towerz[point.X, point.Y].dead = 0;
                                //Towerz[point.X, point.Y].position = new Vector3((float)point.X, (float)point.Y, 0.0f);
                                Towers[point.X, point.Y] = 0;
                                money -= 10;

                                foreach (Enemy e in enemies)
                                {
                                    if (e != null)
                                    {
                                        MoveEnemy(e, new Point((int)(e.PositionCurrent.X / TileWidth), (int)(e.PositionCurrent.Y / TileWidth)));
                                    }
                                }
                            }
                        }
                    }
                }
            } 
            if (MSState_Current.RightButton == ButtonState.Pressed)
            {
                removed = true;
            }
            if (MSState_Current.RightButton == ButtonState.Released)
            {
                if (removed == true)
                {
                    Click = GetCollision();
                    removed = false;

                    Point point = new Point(((int)Click.X) / (int)TileWidth, ((int)Click.Y) / (int)TileWidth);

                    if (point.X >= 0 && point.Y >= 0 && point.X < PlayfieldWidth && point.Y < PlayfieldWidth)
                    {
                        if (Towers[point.X,point.Y] == 0)
                        {
                            Towerz[point.X, point.Y].dead = 1;
                            Towers[point.X, point.Y] = 1;
                            money += 10;
                        }
                        
                    }
                }
            }

            //Respawn enemies !!
            if (paused == false)
            {
                if (respawnTime.ElapsedMilliseconds > 3000)
                {
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        if (enemies[i] == null)
                        {
                            enemies[i] = new Enemy(new Vector2(spawnPoint.X, spawnPoint.Y) * 10);
                            MoveEnemy(enemies[i], spawnPoint);
                            respawnTime.Reset();
                            respawnTime.Start();
                            break;
                        }
                    }
                }
            }

            //Update enemies and it's mesh position
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    if (enemies[i].alive == false)
                    {
                        enemies[i] = null;
                        score += 5;
                        money += 5;
                        break;
                    }
                    enemies[i].Update(elapsedTime);
                    if (enemies[i].finished == true)
                    {
                        enemies[i] = null;
                        lives--;
                    }
                }
            }
            

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

        private void MoveEnemy(Enemy e, Point start)
        {
            Point Start = start;  //new Point(((int)(e.PositionCurrent.X / (int)TileWidth)), ((int)(e.PositionCurrent.Y / (int)TileWidth)));
            Point End = endPoint;  // new Point(((int)Click.X) / (int)TileWidth, ((int)Click.Y) / (int)TileWidth);

                if (Start == End)
                    e.LinearMove(e.PositionCurrent / TileWidth, new Vector2(End.X * TileWidth, End.Y * TileWidth));
                else
                {
                    List<PathReturnNode> foundPath1 = myPathFinder.FindPath(Start, End);
                    if (foundPath1 != null)
                        e.PathMove(ref foundPath1, e.PositionCurrent, new Vector2(End.X * TileWidth + 5, End.Y * TileWidth + 9));
                }
            
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

        
        /// Get collision from ray cast at mouse position to ground plane 
        
        public Vector3 GetCollision()
        {
            Vector3 startC = new Vector3(MSState_Current.X, MSState_Current.Y, 0.0f);
            Vector3 endC = new Vector3(MSState_Current.X, MSState_Current.Y, 4096.0f);

            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(startC,
                camera.mProjection, camera.mView, orientation);

            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(endC,
                camera.mProjection, camera.mView, orientation);

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
            int Scale = PlayfieldWidth * (int)TileWidth;

            //World transformation of the Grid - Will get from the AR marker
            orientation =  Matrix.CreateRotationX((float)gameTime.TotalRealTime.TotalMilliseconds / 2000.0f);
            PlaneMatrix = Matrix.CreateScale(Scale) *Matrix.CreateTranslation(new Vector3(Scale / 2, Scale / 2, 0)) * orientation ;
            
            effect.Parameters["world"].SetValue(PlaneMatrix);
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            effect.Parameters["emmissive"].SetValue(Color.White.ToVector4()*0.4f);
            DrawSampleMesh(Model_Plane);

            //Draw walls
            effect.Parameters["emmissive"].SetValue(Color.White.ToVector4() * 0.8f);
            effect.Parameters["xTexture0"].SetValue(Texture_Tower);
            DrawBoxArray(Model_Tower);

            //Draw character
            foreach (Enemy e in enemies)
            {
                if (e != null)
                {
                    effect.Parameters["xTexture0"].SetValue(Texture_WhiteQuad);
                    effect.Parameters["world"].SetValue(e.matrix * orientation);

                    if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Keys.Down))
                    {
                        start = gameTime.ElapsedRealTime.Seconds;
                    }

                    Vector3 green = Color.Green.ToVector3();
                    Vector3 red = Color.Red.ToVector3();
                    Vector3 color = Vector3.Lerp(green, red, 1.0f - e.hp / Enemy.MAX_HP);
                    color.Normalize();

                    if (start > 150)
                        start = 0;
                    start += 0.1f;
                    effect.Parameters["emmissive"].SetValue(new Color(color).ToVector4());
                    DrawSampleMesh(Model_Sphere);
                }
            }

            //Draw bullets
            foreach (Tower t in Towerz)
            {
                if (t.bullet != null)
                {
                    effect.Parameters["xTexture0"].SetValue(Texture_WhiteQuad);
                    effect.Parameters["world"].SetValue( t.bullet.matrix);
                    effect.Parameters["emmissive"].SetValue(Color.Red.ToVector4());

                    DrawSampleMesh(Model_Sphere);
                }
            }

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            spriteBatch.DrawString(CourierNew, "Lives:  " + lives, new Vector2(10, 0), Color.White);
            spriteBatch.DrawString(CourierNew, "Score:  " + score, new Vector2(160, 0), Color.White);
            spriteBatch.DrawString(CourierNew, "Money:  $" + money, new Vector2(310, 0), Color.White);
            if (paused == true)
                spriteBatch.DrawString(CourierNew, "Time till start:  " + (15 - pauseGame.ElapsedMilliseconds/1000) +"s", new Vector2(780, 0), Color.White);
            spriteBatch.DrawString(CourierNew, "Left click to make a tower (- $10). Right click to sell a tower (+ $10)", new Vector2(10, 30), Color.White);
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
                        effect.Parameters["world"].SetValue(TowerMatrixs[x, y] * orientation);

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
