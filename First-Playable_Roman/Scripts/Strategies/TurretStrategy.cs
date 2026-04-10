using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using First_Playable_Roman.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

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

        private Sprite _projectileSprite;

        public List<TurretProjectile> Projectiles { get; private set; }

        public TurretStrategy(int xPos, int yPos, Room room) : base(9999, xPos, yPos, 0, room)
        {
            _currentDirection = 0;
            _shootTimer = ShootInterval;
            Projectiles = new List<TurretProjectile>();
            IsShooting = false;
        }

        public void SetProjectileSprite(Sprite sprite)
        {
            _projectileSprite = sprite;
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
            Projectiles.Add(new TurretProjectile(fireOrigin, direction, _projectileSprite));

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
        public float Rotation { get; private set; }

        private const float Speed = 4f;
        private const int Radius = 10;
        private const float RotationOffset = MathHelper.PiOver2;

        private Sprite _sprite;

        public TurretProjectile(Vector2 startPosition, Vector2 direction, Sprite sprite = null)
        {
            Position = startPosition;
            Velocity = direction * Speed;
            Rotation = MathF.Atan2(direction.Y, direction.X);
            _sprite = sprite;
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
            if (_sprite != null)
            {
                spriteBatch.Draw(
                    _sprite.Region.Texture,
                    Position,
                    _sprite.Region.SourceRectangle,
                    Color.White,
                    Rotation + RotationOffset,
                    new Vector2(_sprite.Region.Width * 0.5f, _sprite.Region.Height * 0.5f),
                    _sprite.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                Core.DrawRectangleOutline(GetBounds(), Color.OrangeRed);
            }
        }
    }
}
