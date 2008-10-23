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
using DirectShowLib;
using System.Diagnostics;
using TowerDefence3D;

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
        #region AR
        //AR Stuff
        ARCamera cam;
        //list of markers return from AR
        Dictionary<int, MyMarkerInfo> ARMarkers;
        Tracker tracker;
        //array for bytes from webcam
        byte[] w;
        #endregion

        //XNA Stuff
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model mod;
        Matrix world, view, projection;
        Texture2D t;
        Texture2D i;
        Dictionary<int, Tower> towers;
        #region towerdefense

        //Tower defence stuff

        SpriteFont CourierNew;
        SpriteFont CourierNew2;
        SpriteFont CourierNew3;

        //Width of "playfield"
        private const int PlayfieldWidth = 12;

        //Width of one playfield tile
        private const float TileWidth = 20.0f;

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
        bool paused, gameover;
        int pausetime;

        bool added;
        bool removed;

        //TD STUFF
        Enemy[] enemies;
        Point spawnPoint;
        Point endPoint;
        Stopwatch respawnTime;
        private Matrix orientation;
        int score, lives, money;
        const int width = 640;
        const int height = 480;
        const int winHeight = 768;

        int difficulty;

        bool drawgrid = false;
        Vector3 startTxt;
        Vector3 endTxt;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = winHeight;
            graphics.PreferredBackBufferWidth = 1024;
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
            #region AR

            view = Matrix.Identity;
            world = Matrix.Identity;
            projection = Matrix.Identity;

            base.Initialize();
            try
            {
                //various variables
                const int bytesPerPixel = 4;
                Guid sampleGrabberSubType = MediaSubType.ARGB32;
                ArManWrap.PIXEL_FORMAT arPixelFormat = ArManWrap.PIXEL_FORMAT.PIXEL_FORMAT_ABGR;

                cam = new ARCamera(0, width, height, bytesPerPixel, sampleGrabberSubType);
                tracker = new Tracker(width, height, bytesPerPixel, sampleGrabberSubType, arPixelFormat);
            }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine(e.Message); }

            towers = new Dictionary<int, Tower>();

            #endregion

            //Tower defense stuff
            Towers = new byte[PlayfieldWidth, PlayfieldWidth];
            Towerz = new Tower[PlayfieldWidth, PlayfieldWidth];
            TowerMatrixs = new Matrix[PlayfieldWidth, PlayfieldWidth];
            enemies = new Enemy[20];
            //startTxt = Matrix.Identity;

            pauseGame = new Stopwatch();
            pauseGame.Start();
            paused = true;
            gameover = false;

            added = false;
            removed = false;

            score = 0;
            lives = 20;
            money = 100;
            pausetime = 15000;

            difficulty = 0;


            spawnPoint = new Point(PlayfieldWidth / 2, 0);
            endPoint = new Point(PlayfieldWidth / 2, 11);
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
                    TowerMatrixs[x, y] = Matrix.CreateTranslation(new Vector3(x * TileWidth + (TileWidth / 2) - (PlayfieldWidth * TileWidth / 2), y * TileWidth + (TileWidth / 2) - (PlayfieldWidth * TileWidth / 2), 0.0f));
                }
            }

            PlaneMatrix = new Matrix();
            int Scale = PlayfieldWidth * (int)TileWidth;
            PlaneMatrix = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(new Vector3(Scale / 2, Scale / 2, -20));

            camera = new Camera(new Vector3(150, 150, 125), new Vector3(5.45f, 0, -3.95f));

            IsMouseVisible = true;

            myPathFinder = new PathFinder(Towers);
            myPathFinder.HeuristicEstimate = 8;

            Enemy.Initalize(24, (int)TileWidth);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            DepthStencilBuffer b = new DepthStencilBuffer(GraphicsDevice, 1024, 768, DepthFormat.Depth24Stencil8);
            GraphicsDevice.DepthStencilBuffer = b;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            mod = Content.Load<Model>("Models\\box");
            //i = Content.Load<Texture2D>("image");

            foreach (ModelMesh mesh in mod.Meshes)
            {
                foreach (ModelMeshPart p in mesh.MeshParts)
                {
                    t = (p.Effect as BasicEffect).Texture; 
                    //p.Effect = e;
                }
            }
            t = new Texture2D(GraphicsDevice, 640, 480, 1, TextureUsage.None, GraphicsDevice.PresentationParameters.BackBufferFormat);


            //Tower Defense stuff
            CourierNew3 = Content.Load<SpriteFont>("CourierNew3");
            CourierNew2 = Content.Load<SpriteFont>("CourierNew2");
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
            effect.Parameters["diffuseLightColor"].SetValue(Color.White.ToVector4() * 0.2f);
            effect.Parameters["specularLightColor"].SetValue(Color.White.ToVector4() * 0.2f);

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

            #region AR

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

            #endregion

            #region TOWER DEFENSE STUFF

            KBState_Prev = KBState_Current;
            KBState_Current = Keyboard.GetState();

            MSState_Prev = MSState_Current;
            MSState_Current = Mouse.GetState();

            if (pauseGame.ElapsedMilliseconds > pausetime)
            {
                paused = false;
                pauseGame.Reset();
            }

            camera.Update(elapsedTime, KBState_Current, MSState_Current, MSState_Prev, graphics.GraphicsDevice);

            foreach (Tower tower in towers.Values)
            {
                int x = (int)tower.position.X;
                int y = (int)tower.position.Y;
                Point point = new Point((int)(x / TileWidth),(int) (y / TileWidth));

                if (point.X >= -((float)PlayfieldWidth * 0.5) && point.Y >= -((float)PlayfieldWidth / 2) && point.X < (float)PlayfieldWidth / 2 && point.Y < (float)PlayfieldWidth / 2)
                {
                    if (money >= 10)
                    {
                        point.X += (PlayfieldWidth / 2);
                        point.Y += (PlayfieldWidth / 2);
                        if (Towers[point.X, point.Y] == 1)
                        {
                            Towerz[point.X, point.Y].dead = 0;
                            Towerz[point.X, point.Y].position = new Vector3((float)point.X, (float)point.Y, 0.0f);
                            Towers[point.X, point.Y] = 0;
                            money -= 10;

                            foreach (Enemy enemey in enemies)
                            {
                                if (enemey != null)
                                {
                                    MoveEnemy(enemey, new Point((int)(enemey.PositionCurrent.X / TileWidth), (int)(enemey.PositionCurrent.Y / TileWidth)));
                                }
                            }
                        }
                    }
                }
            }

            Click = GetCollision();
            Vector3 Click2 = new Vector3(Click.X + (PlayfieldWidth * TileWidth)/2, Click.Y + (PlayfieldWidth * TileWidth)/2,0);

            //Code to add towers
            if (MSState_Current.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                added = true;
            }
            if (MSState_Current.LeftButton == ButtonState.Released)
            {
                if (added == true && gameover == false)
                {
                    Click = GetCollision();
                    added = false;

                    Point point = new Point((int)(Click2.X / (float)TileWidth), (int)(Click2.Y / (float)TileWidth));

                    //if (point.X >= -((float)PlayfieldWidth * 0.5) && point.Y >= -((float)PlayfieldWidth / 2) && point.X < (float)PlayfieldWidth / 2 && point.Y < (float)PlayfieldWidth / 2)
                    if (point.X >= 0 && point.Y >= 0 && point.X < PlayfieldWidth && point.Y < PlayfieldWidth)
                    {
                        if (money >= 10)
                        {
                            //point.X += (PlayfieldWidth / 2);
                            //point.Y += (PlayfieldWidth / 2);
                            if (Towers[point.X, point.Y] == 1)
                            {
                                Towerz[point.X, point.Y].dead = 0;
                                Towerz[point.X, point.Y].position = new Vector3((float)point.X, (float)point.Y, 0.0f);
                                Towers[point.X, point.Y] = 0;
                                money -= 10;

                                foreach (Enemy enemey in enemies)
                                {
                                    if (enemey != null)
                                    {
                                        MoveEnemy(enemey, new Point((int)(enemey.PositionCurrent.X / TileWidth), (int)(enemey.PositionCurrent.Y / TileWidth)));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Code for the selling of a tower
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

                    Point point = new Point(((int)Click2.X) / (int)TileWidth, ((int)Click2.Y) / (int)TileWidth);

                    //if (point.X >= -((float)PlayfieldWidth * 0.5) && point.Y >= -((float)PlayfieldWidth / 2) && point.X < (float)PlayfieldWidth / 2 && point.Y < (float)PlayfieldWidth / 2)
                    if (point.X >= 0 && point.Y >= 0 && point.X < PlayfieldWidth && point.Y < PlayfieldWidth)
                    {
                        //point.X += (PlayfieldWidth / 2);
                        //point.Y += (PlayfieldWidth / 2);
                        if (Towers[point.X, point.Y] == 0)
                        {
                            Towerz[point.X, point.Y].dead = 1;
                            Towers[point.X, point.Y] = 1;
                            money += 10;
                        }

                    }
                }
            }

            if (KBState_Current.IsKeyDown(Keys.R) && gameover == true)
            {
                gameover = false;
                lives = 20;
                money = 100;
                score = 0;
                enemies = new Enemy[20];
                paused = true;
                pauseGame.Start();
                for (int y = 0; y < PlayfieldWidth; y++)
                {
                    for (int x = 0; x < PlayfieldWidth; x++)
                    {
                        Towers[x, y] = 1;
                        Towerz[x, y] = new Tower(Vector3.Zero);
                        TowerMatrixs[x, y] = Matrix.CreateTranslation(new Vector3(x * TileWidth + (TileWidth / 2) - (PlayfieldWidth * TileWidth / 2), y * TileWidth + (TileWidth / 2) - (PlayfieldWidth * TileWidth / 2), 0.0f));
                    }
                }
            }

            //Respawn enemies !!
            if (paused == false && gameover == false)
            {
                if (respawnTime.ElapsedMilliseconds > 3000)
                {
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        if (enemies[i] == null)
                        {
                            enemies[i] = new Enemy(new Vector2(spawnPoint.X, spawnPoint.Y) * TileWidth, difficulty);
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
                        continue;
                    }
                    enemies[i].Update(elapsedTime);
                    if (enemies[i].finished == true)
                    {
                        enemies[i] = null;
                        lives--;
                    }
                }
            }

            //increase game difficulty
            if (score != 0 && score % 30 == 0)
            {
                difficulty = score / 30;
            }

            //Run the tower methods
            foreach (Tower tower in Towerz)
            {
                tower.FindTarget(enemies);
                tower.Shoot();
                if (tower.bullet != null)
                {
                    tower.bullet.Move();
                }
            }

            //checking the losing condition
            if (lives < 0)
            {
                lives = 0;
                gameover = true;
            }

            #endregion 

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

        private bool IsKeyPush(Microsoft.Xna.Framework.Input.Keys key)
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

        public Vector3 GetCollision()
        {
            Vector3 startC = new Vector3(MSState_Current.X, MSState_Current.Y, 0.0f);
            Vector3 endC = new Vector3(MSState_Current.X, MSState_Current.Y, 4096.0f);

            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(startC,
                projection, view, orientation);

            Vector3 farPoint = graphics.GraphicsDevice.Viewport.Unproject(endC,
                projection, view, orientation);

            Vector3 rayDirection = Vector3.Normalize(farPoint - nearPoint);
            float cosAlpha = Vector3.Dot(Vector3.UnitZ, rayDirection);
            float deltaD = Vector3.Dot(Vector3.UnitZ, nearPoint);

            float distance = deltaD / cosAlpha;

            return nearPoint - (rayDirection * distance);
        }

        /// <summary>
        /// This is called when the game should draw it self.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil | ClearOptions.Target,
                Color.White, 1, 0);
            spriteBatch.Begin();//(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            

            #region AR

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

            GraphicsDevice.RenderState.StencilEnable = true;
            GraphicsDevice.RenderState.StencilPass = StencilOperation.Increment;

            //now to draw the markers
            foreach (MyMarkerInfo mmi in ARMarkers.Values)
            {
                drawgrid = false;
                //is this marker set to be drawn?
                if (mmi.draw)
                {
                    ////System.Diagnostics.Debug.WriteLine(mmi.transform.Translation);
                    world = mmi.transform;//Matrix.Identity;
                    view = Matrix.Identity;
                    //e.DiffuseColor = Color.Purple.ToVector3();
                    //e.Texture = t;
                    //e.TextureEnabled = false;

                    System.Diagnostics.Debug.WriteLine(mmi.markerInfo.id.ToString());
                    //base marker
                    if (mmi.markerInfo.id.ToString() == "499")
                    {
                        camera.mView = view;
                        camera.mProjection = projection;
                        orientation = world;
                        drawgrid = true;
                    }
                    else
                    //towers
                    {
                        //if we have seen this tower before
                        if (towers.ContainsKey(mmi.markerInfo.id))
                        {
                            towers[mmi.markerInfo.id].postitionMatrix = mmi.transform;
                        }
                        else
                        {
                            towers.Add(mmi.markerInfo.id, new Tower(mmi.transform));
                        }
                    }

                    foreach (ModelMesh m in mod.Meshes)
                    {
                        m.Draw();
                    }
                }
            }

            #endregion

            #region TOWER DEFENSE

            //Set effect parameters
            effect.Parameters["view"].SetValue(camera.mView);
            effect.Parameters["projection"].SetValue(camera.mProjection);
            effect.Parameters["cameraPosition"].SetValue(camera.vecPosition);

            //Draw ground plane
            effect.Parameters["xTexture0"].SetValue(Texture_Tower);
            int Scale = PlayfieldWidth * (int)TileWidth;

            //World transformation of the Grid - Will get from the AR marker
            //orientation = Matrix.CreateRotationX((float)gameTime.TotalRealTime.TotalMilliseconds / 2000.0f)*orientation  ;
            //PlaneMatrix = Matrix.CreateScale(Scale) *  Matrix.CreateTranslation(new Vector3(Scale / 2, Scale / 2, 0)) * Matrix.CreateRotationZ(MathHelper.ToRadians(180.0f)) * orientation;
            PlaneMatrix = Matrix.CreateScale(Scale) * orientation;

            effect.Parameters["world"].SetValue(PlaneMatrix);
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            GraphicsDevice.RenderState.DepthBufferEnable = true;


            effect.Parameters["emmissive"].SetValue(Color.White.ToVector4() * 0.4f);
            //GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
            if (drawgrid)
            {
                DrawSampleMesh(Model_Plane);
            }

            //Draw walls
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            effect.Parameters["emmissive"].SetValue(Color.White.ToVector4() * 0.8f);
            effect.Parameters["xTexture0"].SetValue(Texture_Tower);
            DrawBoxArray(Model_Tower);

            //Draw character
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null)
                {
                    effect.Parameters["xTexture0"].SetValue(Texture_WhiteQuad);
                    effect.Parameters["world"].SetValue(Matrix.CreateScale(2)*enemy.matrix * Matrix.CreateTranslation(-(PlayfieldWidth*TileWidth)/2,-(PlayfieldWidth*TileWidth)/2,0) * orientation);

                    Vector3 green = Color.Green.ToVector3();
                    Vector3 red = Color.Red.ToVector3();
                    Vector3 color = Vector3.Lerp(green, red, 1.0f - enemy.hp / Enemy.MAX_HP);
                    color.Normalize();

                    effect.Parameters["emmissive"].SetValue(new Color(color).ToVector4());
                    DrawSampleMesh(Model_Sphere);
                }
            }

            
            //Draw bullets
            foreach (Tower tower in Towerz)
            {
                if (tower.bullet != null)
                {
                    effect.Parameters["xTexture0"].SetValue(Texture_WhiteQuad);
                    effect.Parameters["world"].SetValue(tower.bullet.matrix * Matrix.CreateTranslation(-(PlayfieldWidth * TileWidth) / 2, -(PlayfieldWidth * TileWidth)/2, 0) * orientation);
                    effect.Parameters["emmissive"].SetValue(Color.Red.ToVector4());

                    DrawSampleMesh(Model_Sphere);
                }
            }

           
            startTxt = GraphicsDevice.Viewport.Project(new Vector3(0, -(PlayfieldWidth/2 * TileWidth) - 25, 0), projection, view,  world);
            endTxt = GraphicsDevice.Viewport.Project(new Vector3(0, (PlayfieldWidth/2 * TileWidth) + 10, 0), projection, view, world);
                        
            if (drawgrid)
            {
                spriteBatch.DrawString(CourierNew3, "Start", new Vector2(startTxt.X, startTxt.Y), Color.Green);
                spriteBatch.DrawString(CourierNew3, "End", new Vector2(endTxt.X, endTxt.Y), Color.Red);
            }

            if (paused == true)
            {
                spriteBatch.DrawString(CourierNew, "Time till start:  " + (15 - pauseGame.ElapsedMilliseconds / 1000) + "s", new Vector2(740, 0), Color.White);
            }

            if (gameover == true)
            {
                spriteBatch.DrawString(CourierNew2, "Game Over", new Vector2(400, 350), Color.White);
                spriteBatch.DrawString(CourierNew, "Press 'R' to restart..", new Vector2(380, 400), Color.White);
            }
            else
            {
                spriteBatch.DrawString(CourierNew, "Lives:  " + lives, new Vector2(10, 0), Color.White);
                spriteBatch.DrawString(CourierNew, "Score:  " + score, new Vector2(160, 0), Color.White);
                spriteBatch.DrawString(CourierNew, "Money:  $" + money, new Vector2(310, 0), Color.White);
                spriteBatch.DrawString(CourierNew, "Difficulty:  " + (difficulty +1), new Vector2(10, 30), Color.White);
            }

            if (paused)
            {
                spriteBatch.DrawString(CourierNew, "Left click to make a tower (- $10). Right click to sell a tower (+ $10)", new Vector2(100, winHeight - 25), Color.White);
            }
            spriteBatch.End();

            #endregion
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            graphics.GraphicsDevice.RenderState.StencilFunction = CompareFunction.Equal;
            graphics.GraphicsDevice.RenderState.ReferenceStencil = 0;
            spriteBatch.Draw(t, new Rectangle(
                0,
                0,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height), new Color(255, 255, 255, 255));

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
                        #region render tower stuff
                        effect.Parameters["world"].SetValue(Matrix.CreateScale(2) * TowerMatrixs[x, y] * orientation);

                        effect.Begin(SaveStateMode.SaveState);
                        //now we loop through the passes in the teqnique, drawing each
                        //one in order
                        for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
                        {
                            //EffectPass.Begin will update the device to
                            //begin using the state information defined in the current pass
                            effect.CurrentTechnique.Passes[i].Begin();

                            //sampleMesh contains all of the information required to draw
                            //the current mesh       
                            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
                            //GraphicsDevice.RenderState.AlphaTestEnable = false;
                            //GraphicsDevice.RenderState.DepthBufferEnable = true;
                            graphics.GraphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList, meshPart.BaseVertex, 0,
                                meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);

                            //EffectPass.End must be called when the effect is no longer needed
                            effect.CurrentTechnique.Passes[i].End();
                        }


                        //Likewise, Effect.End will end the current technique
                        effect.End();
                        #endregion
                    }

                }
            }

            //for (int y = 0; y < PlayfieldWidth; y += 1)
            //{
            //    for (int x = 0; x < PlayfieldWidth; x += 1)
            //    {
            //        if (Towerz[x, y].dead == 0)
            //        {
            //            #region render tower stuff
            //            effect.Parameters["world"].SetValue(TowerMatrixs[x, y] *  orientation  );

            //            effect.Begin(SaveStateMode.SaveState);
            //            //now we loop through the passes in the teqnique, drawing each
            //            //one in order
            //            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            //            {
            //                //EffectPass.Begin will update the device to
            //                //begin using the state information defined in the current pass
            //                effect.CurrentTechnique.Passes[i].Begin();

            //                //sampleMesh contains all of the information required to draw
            //                //the current mesh       
            //                //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //                //GraphicsDevice.RenderState.AlphaTestEnable = false;
            //                //GraphicsDevice.RenderState.DepthBufferEnable = true;
            //                graphics.GraphicsDevice.DrawIndexedPrimitives(
            //                    PrimitiveType.TriangleList, meshPart.BaseVertex, 0,
            //                    meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);

            //                //EffectPass.End must be called when the effect is no longer needed
            //                effect.CurrentTechnique.Passes[i].End();
            //            }
                        

            //            //Likewise, Effect.End will end the current technique
            //            effect.End();
            //            #endregion
            //        }

            //    }
            //}
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

        protected override void OnExiting(object sender, EventArgs args)
        {
            tracker.Dispose();
            base.OnExiting(sender, args);
        }
    }
}
