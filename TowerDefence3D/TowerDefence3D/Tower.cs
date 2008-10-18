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
    class Tower
    {
        public byte dead;
        public Enemy target;
        public float range;
        public Vector3 position;
        public Bullet bullet;
        Stopwatch watch = new Stopwatch();

        public Tower(Vector3 Postion)
        {
            dead = 1;
            position = Postion;
            target = null;
            range = 200;
            bullet = null;
        }

        public void FindTarget(Enemy[] enemies)
        {
            Rectangle range = new Rectangle((int)(position.X - 2), (int)(position.Y - 2), 5, 5);
            if (this.dead != 1)
            {
                if (target == null)
                {
                    foreach (Enemy e in enemies)
                    {
                        if (e != null)
                            if (range.Contains(new Point((int)e.PositionCurrent.X/10, (int)e.PositionCurrent.Y/10)))
                            {
                                target = e;
                                break;
                            }
                    }
                }
                else 
                    if (range.Contains(new Point((int)target.PositionCurrent.X / 10, (int)target.PositionCurrent.Y / 10)) == false)
                        target = null;
            }

            
        }

        public void Shoot()
        {
            if (dead != 1)
            {
                if (target != null)
                {
                    if (target.hp == 0)
                    {
                        target.alive = false;
                        target = null;
                    }

                    if (bullet == null)
                    {
                        watch.Start();
                        if (watch.ElapsedMilliseconds > 750)
                        {
                            //shoot
                            Vector3 middle = new Vector3(position.X * 10 + 5, position.Y * 10 + 5, 0);
                            bullet = new Bullet(middle, new Vector3(target.PositionCurrent,0), this);
                            watch.Reset();
                        }
                    }
                }
            }
            
        }
    }
}
