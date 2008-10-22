using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;


namespace TowerDefence3D
{
    public class Camera
    {
        public Matrix mRotation;
        public Matrix mView;
        public Matrix mProjection;
        public Matrix mWorld;

        public Vector3 vecTarget;
        public Vector3 vecUp;
        public Vector3 vecAngles;
        public Vector3 vecPosition;

        public float moveSpeed = 128.0f;
        private float windowWidth = 1024.0f;
        private float windowHeight = 768.0f;

        Vector3 moveVector = Vector3.Zero;

        private int mice_x;
        private int mice_y;

        public float mult;

        public Camera(Vector3 POSITION, Vector3 ANGLES)
        {
            mWorld = Matrix.Identity;
            vecPosition = POSITION;
            vecAngles = ANGLES;

            mice_x = 1024 / 2;
            mice_y = 768 / 2;

        }

        public void Update(float elapsedTime, KeyboardState CurrentKeyboardState,
                           MouseState currentMouseState, MouseState previousMouseState, GraphicsDevice device)
        {

            moveVector = Vector3.Zero;

            if (CurrentKeyboardState.IsKeyDown(Keys.D))
                moveVector.X += moveSpeed * elapsedTime;
            if (CurrentKeyboardState.IsKeyDown(Keys.A))
                moveVector.X -= moveSpeed * elapsedTime;
            if (CurrentKeyboardState.IsKeyDown(Keys.S))
                moveVector.Y -= moveSpeed * elapsedTime;
            if (CurrentKeyboardState.IsKeyDown(Keys.W))
                moveVector.Y += moveSpeed * elapsedTime;

            if ((currentMouseState.RightButton == ButtonState.Pressed) && (previousMouseState.RightButton == ButtonState.Released))
            {
                mice_x = currentMouseState.X;
                mice_y = currentMouseState.Y;
            }

            if (currentMouseState.RightButton == ButtonState.Pressed)
            {
                if (currentMouseState.X != mice_x)
                {
                    vecAngles.Z -= (currentMouseState.X - mice_x) * 0.006f;
                }
                if (currentMouseState.Y != mice_y)
                {
                    vecAngles.X -= (currentMouseState.Y - mice_y) * 0.006f;
                }

                Mouse.SetPosition(mice_x, mice_y);
            }

            mRotation = Matrix.CreateRotationX(vecAngles.X) * Matrix.CreateRotationZ(vecAngles.Z);
            vecPosition += Vector3.Transform(moveVector, mRotation);

            mRotation = Matrix.CreateRotationX(vecAngles.X) * Matrix.CreateRotationZ(vecAngles.Z);

            vecTarget = vecPosition + Vector3.Transform(new Vector3(0, 1, 0), mRotation);
            vecUp = Vector3.Transform(new Vector3(0, 0, 1), mRotation);

            mView =  Matrix.CreateLookAt(vecPosition, vecTarget, vecUp);

            mProjection =  Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, windowWidth / windowHeight, 0.12f, 8096.0f);
        }

    }
}
