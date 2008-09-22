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
    class Bullet
    {
        public Vector2 position_;
        public Vector2 destination_;
        private Tower origin;

        public Bullet(Vector2 startPos, Vector2 destination, Tower tower)
        {
            position_ = startPos;
            destination_ = destination;
            origin = tower;
        }

        public void Move()
        {
            
        }
    }
}
