using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using First_Playable_Roman.Scenes;
using MonoGameLibrary.Graphics;
using First_Playable_Roman.Scripts.Movements;

namespace First_Playable_Roman.Scripts
{
    public abstract class Enemy : Entity
    {
        public Health Health { get; set; }
        public int Speed { get; set; }
        public bool IsActive { get; set; }
        private int _maxHealth;

        private Room _rooms;

        // Sprite for this enemy (animated or static)
        protected AnimatedSprite _animatedSprite;
        protected Sprite _staticSprite;

        // Width of the enemy sprite
        public float SpriteWidth =>
            _animatedSprite?.Width ?? _staticSprite?.Width ?? 64f;

        // Height of the enemy sprite
        public float SpriteHeight =>
            _animatedSprite?.Height ?? _staticSprite?.Height ?? 64f;

        public void SetAnimatedSprite(AnimatedSprite sprite)
        {
            _animatedSprite = sprite;
            _staticSprite = null;
        }

        public void SetStaticSprite(Sprite sprite)
        {
            _staticSprite = sprite;
            _animatedSprite = null;
        }

        public virtual void UpdateSprite(GameTime gameTime)
        {
            _animatedSprite?.Update(gameTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive)
                return;

            if (_animatedSprite != null)
            {
                _animatedSprite.Draw(spriteBatch, _position);
            }
            else if (_staticSprite != null)
            {
                _staticSprite.Draw(spriteBatch, _position);
            }
        }

        public void TakeDamage(int damage)
        {
            Health.TakeDamage(damage);
            
            // Check if enemy died
            if (Health.CurrentHealth <= 0)
            {
                _rooms.AddScore(100);
                IsActive = false;
            }
        }

        public void Attack(Player player)
        {
            player.TakeDamage(Health.CurrentHealth);
        }

        public void Respawn(Rectangle roomBounds, float tileWidth, float tileHeight, int columns, int rows)
        {
            // Respawn enemy inside playable area leaving exactly one tile margin on each edge.
            int tileWScaled = (int)Math.Max(1, Math.Round(tileWidth));
            int tileHScaled = (int)Math.Max(1, Math.Round(tileHeight));

            int innerCols = Math.Max(1, columns - 2);
            int innerRows = Math.Max(1, rows - 2);

            int column = Random.Shared.Next(0, innerCols);
            int row = Random.Shared.Next(0, innerRows);

            // Position on tile grid inside playable area (roomX + column * tileWScaled)
            Vector2 newPosition = new Vector2(roomBounds.Left + column * tileWScaled, roomBounds.Top + row * tileHScaled);
            _position = newPosition;
            
            // Restore health and reactivate
            Health.Heal(_maxHealth);
            IsActive = true;
        }

        public Vector2 PositionAndCollision(Vector2 velocity, List<Rectangle> obstacles, float width, float height, Rectangle roomBounds)
        {
            Vector2 newPosition = _position + velocity;

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            Vector2 normal = Vector2.Zero;

            // Clamp enemy position within room bounds
            if (newPosition.X < roomBounds.Left)
            {
                newPosition.X = roomBounds.Left;
                normal.X = 1f;
            }
            else if (newPosition.X + width > roomBounds.Right)
            {
                newPosition.X = roomBounds.Right - width;
                normal.X = -1f;
            }

            if (newPosition.Y < roomBounds.Top)
            {
                newPosition.Y = roomBounds.Top;
                normal.Y = 1f;
            }
            else if (newPosition.Y + height > roomBounds.Bottom)
            {
                newPosition.Y = roomBounds.Bottom - height;
                normal.Y = -1f;
            }

            // Check collision with obstacles
            Rectangle enemyRect = new Rectangle(
                (int)newPosition.X,
                (int)newPosition.Y,
                (int)width,
                (int)height
            );

            if (obstacles != null)
            {
                foreach (Rectangle obstacle in obstacles)
                {
                    if (enemyRect.Intersects(obstacle))
                    {
                        // Calculate collision normal based on overlap
                        int overlapLeft = enemyRect.Right - obstacle.Left;
                        int overlapRight = obstacle.Right - enemyRect.Left;
                        int overlapTop = enemyRect.Bottom - obstacle.Top;
                        int overlapBottom = obstacle.Bottom - enemyRect.Top;

                        // Find the smallest overlap to determine the collision direction
                        int minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

                        if (minOverlap == overlapLeft)
                        {
                            normal.X = -1f; // Push left
                            newPosition.X = obstacle.Left - width;
                        }
                        else if (minOverlap == overlapRight)
                        {
                            normal.X = 1f; // Push right
                            newPosition.X = obstacle.Right;
                        }
                        else if (minOverlap == overlapTop)
                        {
                            normal.Y = -1f; // Push up
                            newPosition.Y = obstacle.Top - height;
                        }
                        else if (minOverlap == overlapBottom)
                        {
                            normal.Y = 1f; // Push down
                            newPosition.Y = obstacle.Bottom;
                        }

                        break; // Handle one collision at a time
                    }
                }
            }

            // Update position
            _position = newPosition;

            // Return the reflected velocity if there was a collision
            if (normal != Vector2.Zero)
            {
                normal.Normalize();
                return Vector2.Reflect(velocity, normal);
            }

            return velocity;
        }

        public Enemy(int maxHp, int xPos, int yPos, int speed, Room room) : base(xPos, yPos)
        {
            _maxHealth = maxHp;
            Health = new Health(maxHp);
            Speed = speed;
            IsActive = true;
            _rooms = room;
        }

        public abstract Vector2 Move();

        public Room GetRoom()
        {
            return _rooms;
        }
    }
}
