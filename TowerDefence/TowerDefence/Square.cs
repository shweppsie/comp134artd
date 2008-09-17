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
        public bool highlighted;
        public int xcoord;
        public int ycoord;

        public Square(int X, int Y)
        {
            occupied = false;
            highlighted = false;
            xcoord = X;
            ycoord = Y;
        }
    }
}
