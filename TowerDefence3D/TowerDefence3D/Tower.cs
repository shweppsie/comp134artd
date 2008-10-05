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
    class Tower
    {
        public byte dead;
        public Enemy target;
        public float range;


        public Tower()
        {
            dead = 1;
            target = null;
            range = 200;
        }

        public void FindTarget(Enemy[] enemies)
        {

        }
    }
}
