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

namespace TowerDefence
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Rectangle viewportRect;
        Square[,] grid;
        Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        Enemy[] enemies;
        Follow f;
        Tower[] towers;

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
            viewportRect = new Rectangle(0, 0, 600, 600);
            enemies = new Enemy[1];
            enemies[0] = new Enemy(new Vector2(250, 50));
            f = new Follow(Follow.DefaultPath, enemies[0].position_, 10);
            grid = new Square[10, 10];
            towers = new Tower[10];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    grid[i, j] = new Square(50 * j + 50 , 50 * i + 50);
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
            mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            if (FindSquare() != null)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    bool added = false;
                    int i = 0;
                    if (FindSquare().occupied == false)
                    {
                        while (added == false && i < 10)
                        {
                            if (towers[i] == null)
                            {
                                towers[i] = new Tower(175, FindSquare().position);
                                FindSquare().occupied = true;
                                added = true;
                            }
                            else
                                i++;
                        }
                    }
                }
            }
            f.Move(1, out enemies[0].position_);
            foreach (Tower t in towers)
            {
                if (t != null)
                {
                    t.FindTarget(enemies);
                    t.Shoot();
                    if (t.bullet_ != null)
                        t.bullet_.Move();
                }                
            }
            

            //while (f.Move(1, out enemy.position_))
            //{
            //    spriteBatch.Begin();
            //    spriteBatch.Draw(Content.Load<Texture2D>("Sprites\\enemy"), enemy.position_, Color.White);
            //    spriteBatch.End();
            //}

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
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(Content.Load<Texture2D>("Sprites\\gridpath"), viewportRect, Color.White);
            if (FindSquare() != null)
            {
                Vector2 position = FindSquare().position;
                spriteBatch.Draw(Content.Load<Texture2D>("Sprites\\highlighted"),
                    new Rectangle((int)position.X, (int)position.Y,50,50),
                    Color.White);
            }
            spriteBatch.Draw(Content.Load<Texture2D>("Sprites\\enemy"), enemies[0].position_, Color.White);
            foreach (Tower t in towers)
            {
                if (t != null)
                {
                    spriteBatch.Draw(Content.Load<Texture2D>("Sprites\\tower"), t.position_, Color.White);
                    if (t.bullet_ != null)
                        spriteBatch.Draw(Content.Load<Texture2D>("Sprites\\bullet"), t.bullet_.position_, Color.White);
                }
            }


            spriteBatch.Draw(Content.Load<Texture2D>("Sprites\\cursor"), mousePos, Color.White);
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        // Method to find which square of the grid the mouse is hovering over..
        private Square FindSquare()
        {
            int i = (int)mousePos.Y / 50;
            int j = (int)mousePos.X / 50;
            i--;
            j--;
            if (i >= 0 && i < 10 && j >= 0 && j < 10)
            {
                return grid[i, j];
            }
            else
                return null;
        }
    }
}
