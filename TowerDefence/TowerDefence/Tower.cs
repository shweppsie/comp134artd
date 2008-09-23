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

namespace TowerDefence
{
    class Tower
    {
        public float fireRadius_;
        public Enemy target_;
        public Vector2 position_;
        public Bullet bullet_;
        Stopwatch watch = new Stopwatch();

        public Tower(float radius, Vector2 position)
        {
            fireRadius_ = radius;
            target_ = null;
            position_ = position;
        }

        public void Shoot()
        {
            if (target_ != null)
            {
                if (bullet_ == null)
                {
                    watch.Start();
                    //fire a bullet at the targeted enemy
                    if (watch.ElapsedMilliseconds > 750)
                    {
                        bullet_ = new Bullet(new Vector2(position_.X + 25, position_.Y + 25), target_.position_, this);
                        if (target_.hp <= 0)
                            target_ = null;
                        watch.Reset();
                    }
                    
                }
            }
        }

        public void FindTarget(Enemy[] enemies)
        {
            //check the firing radius for an enemy (maybe the closest) then set that as the target   
            Rectangle range = new Rectangle((int)(position_.X + 25 - (0.5 * fireRadius_)),
                    (int)(position_.Y + 25 - (0.5 * fireRadius_)), (int)fireRadius_, (int)fireRadius_);
            if (target_ == null)
            {                
                foreach (Enemy e in enemies)
                {
                    if (e != null)
                    {
                        if (range.Contains((int)e.position_.X, (int)e.position_.Y))
                        {
                            target_ = e;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (range.Contains((int)target_.position_.X, (int)target_.position_.Y) == false)
                {
                    target_ = null;
                }
            }
        }
    }
}
