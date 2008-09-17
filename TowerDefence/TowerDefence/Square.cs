using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefence
{
    class Square
    {
        public bool occupied;
        public Vector2 position;

        public Square(float X, float Y)
        {
            occupied = false;
            position = new Vector2(X, Y);
        }

        
    }
}
