using System;
using OpenTK;
using OpenTK.Input;
using Minalear.Engine;

namespace DeferredRendering
{
    public class Camera
    {
        public Matrix4 View { get; private set; }
        public Matrix4 Projection { get; private set; }
        public Vector3 Position { get { return position; } }

        public Camera(Vector3 position, Vector3 rotation)
        {
            this.position = position;
            this.rotation = rotation;

            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, 1.7778f, 0.1f, 5000f);
            View = Matrix4.LookAt(position, position + forward, new Vector3(0f, 1f, 0f));
        }

        public void Update(GameTime gameTime)
        {
            var mState = Mouse.GetState();
            var kState = Keyboard.GetState();

            Vector3 moveMod = Vector3.Zero;
            float moveSpeed = (kState.IsKeyDown(Key.ShiftLeft)) ? 1f : 10f;
            moveSpeed *= gameTime.Delta; 

            if (kState.IsKeyDown(Key.A))
                moveMod.X = -moveSpeed;
            if (kState.IsKeyDown(Key.D))
                moveMod.X = moveSpeed;
            
            if (!locked)
            {
                if (mState.RightButton == ButtonState.Pressed)
                {
                    Vector2 mouseDelta = new Vector2(
                        mState.X - lastMouseState.X,
                        mState.Y - lastMouseState.Y);

                    rotation.X += mouseDelta.Y * 0.003f;
                    rotation.Y += mouseDelta.X * 0.003f;
                }
                
                if (kState.IsKeyDown(Key.W))
                    moveMod.Z = moveSpeed;
                if (kState.IsKeyDown(Key.S))
                    moveMod.Z = -moveSpeed;
                
                if (kState.IsKeyDown(Key.Q))
                    moveMod.Y = moveSpeed;
                if (kState.IsKeyDown(Key.E))
                    moveMod.Y = -moveSpeed;
            }

            position += forward * moveMod.Z;
            position += right * moveMod.X;
            position += new Vector3(0f, 1f, 0f) * moveMod.Y;

            forward = new Vector3(
                (float)Math.Cos(rotation.X) * (float)Math.Sin(rotation.Y),
                (float)Math.Sin(rotation.X),
                (float)Math.Cos(rotation.X) * (float)Math.Cos(rotation.Y));
            right = new Vector3(
                (float)Math.Sin(rotation.Y - MathHelper.PiOver2),
                0, (float)Math.Cos(rotation.Y - MathHelper.PiOver2));

            View = Matrix4.LookAt(position, position + forward, new Vector3(0f, 1f, 0f));

            lastMouseState = mState;
            lastKeyState = kState;
        }

        private bool locked = false;

        private Vector3 position, rotation;
        private Vector3 forward, right;

        //Input
        private KeyboardState lastKeyState;
        private MouseState lastMouseState;
    }
}
