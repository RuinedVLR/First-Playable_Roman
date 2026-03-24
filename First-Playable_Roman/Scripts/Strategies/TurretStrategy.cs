using MonoGameLibrary;
using First_Playable_Roman.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace First_Playable_Roman.Scripts.Movements
{
    internal class TurretStrategy : Enemy
    {
        public bool IsShooting { get; private set; }

        // Directions: Right, Down, Left, Up
        private static readonly Vector2[] ShootDirections =
        [
            new Vector2(1, 0),   // Right
            new Vector2(0, 1),   // Down
            new Vector2(-1, 0),  // Left
            new Vector2(0, -1)   // Up
        ];

        private int _currentDirection;
        private float _shootTimer;
        private const float ShootInterval = 1.0f; // 1 second between shots

        public List<TurretProjectile> Projectiles { get; private set; }

        public TurretStrategy(int xPos, int yPos, Rooms room) : base(9999, xPos, yPos, 0, room)
        {
            _currentDirection = 0;
            _shootTimer = ShootInterval;
            Projectiles = new List<TurretProjectile>();
            IsShooting = false;
        }

        public override Vector2 Move()
        {
            // Turret does not move
            return Vector2.Zero;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsActive)
                return;

            // Update shoot timer
            _shootTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_shootTimer <= 0f)
            {
                Shoot();
                _shootTimer = ShootInterval;
            }

            // Update all projectiles
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                Projectiles[i].Update();
            }
        }

        private void Shoot()
        {
            IsShooting = true;

            // Calculate fire origin from center of enemy sprite
            Vector2 fireOrigin = new Vector2(
                _position.X + SpriteWidth * 0.5f,
                _position.Y + SpriteHeight * 0.5f
            );

            // Create projectile in the current direction
            Vector2 direction = ShootDirections[_currentDirection];
            Projectiles.Add(new TurretProjectile(fireOrigin, direction));

            // Cycle to the next direction
            _currentDirection = (_currentDirection + 1) % ShootDirections.Length;
        }

        // Removes projectiles that are out of room bounds.
        public void CleanupProjectiles(Rectangle roomBounds)
        {
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                if (Projectiles[i].IsOutOfBounds(roomBounds))
                {
                    Projectiles.RemoveAt(i);
                }
            }
        }

        // Checks if any projectile hits the given player circle bounds.
        public bool CheckProjectileHit(Circle playerBounds)
        {
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                if (Projectiles[i].CheckCollision(playerBounds))
                {
                    Projectiles.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the turret sprite
            base.Draw(spriteBatch);

            // Draw projectiles
            foreach (TurretProjectile projectile in Projectiles)
            {
                projectile.Draw(spriteBatch);
            }
        }
    }

    public class TurretProjectile
    {
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }

        private const float Speed = 4f;
        private const int Radius = 6;

        public TurretProjectile(Vector2 startPosition, Vector2 direction)
        {
            Position = startPosition;
            Velocity = direction * Speed;
        }

        public void Update()
        {
            Position += Velocity;
        }

        public bool CheckCollision(Circle target)
        {
            Circle projectileBounds = new Circle((int)Position.X, (int)Position.Y, Radius);
            return projectileBounds.Intersects(target);
        }

        public bool IsOutOfBounds(Rectangle roomBounds)
        {
            return Position.X < roomBounds.Left || Position.X > roomBounds.Right ||
                   Position.Y < roomBounds.Top || Position.Y > roomBounds.Bottom;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(
                (int)(Position.X - Radius),
                (int)(Position.Y - Radius),
                Radius * 2,
                Radius * 2
            );
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Core.DrawRectangleOutline(GetBounds(), Color.OrangeRed);
        }
    }
}
