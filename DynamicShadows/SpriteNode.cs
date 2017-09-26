using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace DynamicShadows
{
    public class SpriteNode : SceneNode
    {
        public Texture2D Sprite;
        public int AnimIndex;

        private float timer = 0f;

        public SpriteNode(Texture2D sprite, Vector3 position) 
            : base(position)
        {
            Sprite = sprite;
            AnimIndex = 0;
            
            Scale = new Vector3(1f, 1.3334f, 1f);
            Rotation = Vector3.Zero;

            setTextureFilters();
        }

        public override void Update(GameTime gameTime)
        {
            handleTimer(gameTime);
            handleInput(gameTime);
        }

        private void handleTimer(GameTime gameTime)
        {
            timer += gameTime.Delta;
            if (timer >= 0.25f)
            {
                timer = 0f;
                AnimIndex++;

                if (AnimIndex == 7)
                    AnimIndex = 1;
            }
        }
        private void handleInput(GameTime gameTime)
        {
            KeyboardState kState = Keyboard.GetState();

            Vector2 velocity = Vector2.Zero;
            float moveSpeed = 2f;

            if (kState.IsKeyDown(Key.A))
                velocity.X -= moveSpeed;
            if (kState.IsKeyDown(Key.D))
                velocity.X += moveSpeed;

            Position.Xy += (velocity * gameTime.Delta);
        }

        private void setTextureFilters()
        {
            Sprite.Bind();
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
