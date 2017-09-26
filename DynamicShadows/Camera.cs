using System;
using OpenTK;
using OpenTK.Input;
using Minalear.Engine;

namespace DynamicShadows
{
    public class Camera
    {
        public Matrix4 View { get; private set; }
        public Matrix4 Projection { get; private set; }
        public Vector3 Position { get { return position; } }
        public float X { get { return position.X; } set { position.X = value; } }
        public float Y { get { return position.Y; } set { position.Y = value; } }
        public float Z { get { return position.Z; } set { position.Z = value; } }

        public Camera(Vector3 position, Vector3 rotation)
        {
            this.position = position;
            this.rotation = rotation;

            up = new Vector3(0f, 1f, 0f);

            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, 1.7778f, 0.1f, 50f);
            View = Matrix4.LookAt(position, position + forward, new Vector3(0f, 1f, 0f));
        }

        public void Update(GameTime gameTime)
        {
            if (!locked)
                freeCameraLook(gameTime);

            forward = new Vector3(
                (float)Math.Cos(rotation.X) * (float)Math.Cos(rotation.Y),
                (float)Math.Sin(rotation.X),
                (float)Math.Cos(rotation.X) * (float)Math.Sin(rotation.Y));
            if (forward.LengthSquared > 0f)
                forward.Normalize();

            right = Vector3.Cross(forward, up);

            View = Matrix4.LookAt(position, position + forward, up);

            if (Keyboard.GetState().IsKeyDown(Key.Up))
                locked = false;
            else if (Keyboard.GetState().IsKeyDown(Key.Down))
                locked = true;
        }

        private void freeCameraLook(GameTime gameTime)
        {
            var mState = Mouse.GetState();
            var kState = Keyboard.GetState();

            Vector3 moveMod = Vector3.Zero;
            float moveSpeed = (kState.IsKeyDown(Key.ShiftLeft)) ? 0.1f : 10f;
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
                    rotation.Y -= mouseDelta.X * 0.003f;

                    rotation.X = MathHelper.Clamp(rotation.X, -1.5f, 1.5f);
                }

                if (kState.IsKeyDown(Key.W))
                    moveMod.Z = moveSpeed;
                if (kState.IsKeyDown(Key.S))
                    moveMod.Z = -moveSpeed;

                if (kState.IsKeyDown(Key.E))
                    moveMod.Y = moveSpeed;
                if (kState.IsKeyDown(Key.Q))
                    moveMod.Y = -moveSpeed;
            }

            position += forward * moveMod.Z;
            position += right * moveMod.X;
            position += new Vector3(0f, 1f, 0f) * moveMod.Y;
            
            lastMouseState = mState;
            lastKeyState = kState;
        }

        public bool locked = true;

        private Vector3 position, rotation;
        private Vector3 forward, right, up;

        //Input
        private KeyboardState lastKeyState;
        private MouseState lastMouseState;
    }
}
