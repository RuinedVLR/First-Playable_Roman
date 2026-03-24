using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman.Scripts.Items
{
    public abstract class Items
    {
        public Vector2 Position { get; set; }
        protected Sprite Sprite { get; set; }
        public bool IsCollected { get; protected set; }

        // When true, the item cannot be picked up until the player leaves its hitbox first
        private bool _pickupGuard;

        protected Items(Vector2 position, Sprite sprite)
        {
            Position = position;
            Sprite = sprite;
            IsCollected = false;
            _pickupGuard = true; // Enabled on spawn — player must not be overlapping to pick up
        }

        public virtual void Draw()
        {
            if (!IsCollected)
            {
                Sprite?.Draw(Core.SpriteBatch, Position);
            }
        }

        public Rectangle GetBounds()
        {
            if (Sprite == null)
                return new Rectangle((int)Position.X, (int)Position.Y, 1, 1);

            return new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)Sprite.Width,
                (int)Sprite.Height
            );
        }

        public bool CheckCollision(Circle playerBounds)
        {
            if (IsCollected)
                return false;

            // Convert rectangle to circle for collision check
            Rectangle itemBounds = GetBounds();
            Circle itemCircle = new Circle(
                itemBounds.Center.X,
                itemBounds.Center.Y,
                (int)(itemBounds.Width * 0.5f)
            );

            bool isOverlapping = playerBounds.Intersects(itemCircle);

            // If pickup guard is active, wait until the player leaves the hitbox
            if (_pickupGuard)
            {
                if (!isOverlapping)
                {
                    // Player has left the hitbox — disable guard
                    _pickupGuard = false;
                }

                // Block pickup while guard is active
                return false;
            }

            return isOverlapping;
        }

        public virtual void Collect(Player player)
        {
            IsCollected = true;
            Position = new Vector2(-9999, -9999);
        }
    }
}
