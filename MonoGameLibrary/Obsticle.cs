using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using System;

namespace MonoGameLibrary
{
    internal class Obsticle
    {
        Vector2 _position;
        public Vector2 Position { get { return _position; } set { _position = value; } }
        public Sprite Sprite { get; private set; }
        public Rectangle Bounds { get; private set; }

        public Obsticle(Vector2 position, Sprite sprite)
        {
            Position = position;
            Sprite = sprite;
            Bounds = new Rectangle((int)position.X, (int)position.Y, (int)sprite.Width, (int)sprite.Height);
        }

        public void Draw()
        {
            Sprite.Draw(Core.SpriteBatch, Position);
        }

        private void UpdateBounds()
        {
            Bounds = new Rectangle
            (
                (int)Position.X,
                (int)Position.Y,
                (int)Sprite.Width,
                (int)Sprite.Height
            );
        }
    }
}
