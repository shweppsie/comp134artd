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
    class Follow
    {
        Vector2[] path_;
        Vector2 offset_;
        float speed_;
        float t_;
        float length_;
        int segment_;

        public static Vector2[] DefaultPath = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(0, 100),
                new Vector2(200,100),
                new Vector2(200,300),
                new Vector2(-100,300),
                new Vector2(-100, 500)
            };

        public Follow(Vector2[] path, Vector2 spawnPoint, float speed)
        {
            path_ = path;
            offset_ = spawnPoint;
            speed_ = speed;
            t_ = 0;

            EnterSegment(0);
        }

        private void EnterSegment(int seg)
        {
            segment_ = seg;
            if (segment_ < path_.Length - 1)
            {
                length_ = (path_[segment_ + 1] - path_[segment_]).Length();
                t_ = 0;
            }
        }

        public bool Move(float dt, out Vector2 pos)
        {
            t_ += dt;

            if (t_ >= length_)
            {
                EnterSegment(segment_ + 1);
            }

            pos = (segment_ >= path_.Length - 1) ? offset_ 
                : offset_ + path_[segment_] + (path_[segment_ + 1] - path_[segment_]) * (t_ / length_);

            return segment_ < path_.Length;
        }
    }
}
