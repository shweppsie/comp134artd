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
    class Bullet
    {
        public Vector3 position;
        public Vector3 destination;
        public Vector3 velocity;
        public Tower origin;
        public Matrix matrix;
        public const float speed = 50;
        private int i;

        public Bullet(Vector3 startPosition, Vector3 Destination, Tower tower)
        {
            origin = tower;
            position = startPosition;
            destination = Destination;
            velocity = (destination - position) / speed;
            matrix = Matrix.CreateScale(1) * Matrix.CreateTranslation(position.X, position.Y, 4.0f);
            i = 0;
        }

        public void Move()
        {
            position += velocity;//(destination - position) / 400;
            matrix = Matrix.CreateScale(1) * Matrix.CreateTranslation(position.X, position.Y, 4.0f);
            i++;
            if (i == speed)
            {
                origin.bullet = null;
                if (origin.target != null)
                    if (origin.target.hp > 0)
                        origin.target.hp--;
            }
        }
    }
}
