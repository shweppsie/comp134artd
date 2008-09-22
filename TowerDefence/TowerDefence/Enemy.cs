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
        public int hp;
        public Follow f;
        public bool kill;

        public Enemy(Vector2 startPostion)
        {
            position_ = startPostion;
            hp = 5;
            f = new Follow(Follow.DefaultPath, position_, 10);
            kill = false;
        }
    }
}
