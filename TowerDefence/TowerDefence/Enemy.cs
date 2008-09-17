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
    class Enemy
    {
        public Vector2 position_;

        public Enemy(Vector2 startPostion)
        {
            position_ = startPostion;
        }

        public void Move()
        {
            Follow f = new Follow(Follow.DefaultPath, position_, 20);
            //Console.WriteLine("First pos: {0}", initPos);
            while (f.Move(1, out position_))
            {
                //Console.WriteLine("New pos: {0}", initPos);
                
            }
            //Console.WriteLine("End pos: {0}", initPos);
        }
    }
}
