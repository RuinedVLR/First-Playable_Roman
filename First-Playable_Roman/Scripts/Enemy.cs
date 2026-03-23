using First_Playable_Roman.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace First_Playable_Roman.Scripts
{
    public abstract class Enemy : Entity
    {
        public Health Health { get; set; }
        public int Speed { get; set; }
        public bool IsActive { get; set; }
        private int _maxHealth;

        public void TakeDamage(int damage)
        {
            Health.TakeDamage(damage);
            
            // Check if enemy died
            if (Health.CurrentHealth <= 0)
            {
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

        public Vector2 PositionAndCollision(Vector2 velocity, List<Rectangle> obstacles, float width, float height)
        {
            Vector2 newPosition = _position + velocity;

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            Vector2 normal = Vector2.Zero;

            // Check collision with obstacles only
            Rectangle enemyRect = new Rectangle(
                (int)newPosition.X,
                (int)newPosition.Y,
                (int)width,
                (int)height
            );

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

        public Enemy(int maxHp, int xPos, int yPos, int speed) : base(xPos, yPos)
        {
            _maxHealth = maxHp;
            Health = new Health(maxHp);           
            Speed = speed;
            IsActive = true;
        }

        public abstract Vector2 Move();
    }
}
