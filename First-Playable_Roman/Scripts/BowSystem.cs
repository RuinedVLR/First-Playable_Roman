using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;

namespace First_Playable_Roman.Scripts
{
    public class BowSystem
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; private set; }
        public bool IsAiming { get; private set; }
        public bool CanShoot => _reloadTimer <= 0f;
        
        private Sprite _bowSprite;
        private Vector2 _offset;
        private float _reloadTimer;
        private const float ReloadTime = 1.0f;
        private const float RotationOffset = MathHelper.PiOver2; // offset to align bow sprite

        public BowSystem(Sprite bowSprite)
        {
            _bowSprite = bowSprite;
            _offset = new Vector2(32, 32);
            Rotation = 0f;
            IsAiming = false;
            _reloadTimer = 0f;
        }

        public void Update(GameTime gameTime)
        {
            if (_reloadTimer > 0)
            {
                _reloadTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_reloadTimer < 0)
                    _reloadTimer = 0;
            }
        }

        public void StartAiming()
        {
            IsAiming = true;
        }

        public void StopAiming()
        {
            IsAiming = false;
        }

        // Update aim to point towards the closest enemy
        public void UpdateAim(Vector2 playerPosition, List<Enemy> enemies)
        {
            if (enemies == null || enemies.Count == 0)
                return;

            // Closest enemy detection
            Enemy closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                float distance = Vector2.Distance(playerPosition, enemy._position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy != null)
            {
                // Calculate direction to enemy
                Vector2 direction = closestEnemy._position - playerPosition;
                Rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        // Creates an arrow in the aiming direction
        public Arrow ShootArrow(Vector2 playerPosition)
        {
            if (!CanShoot)
                return null;

            _reloadTimer = ReloadTime;
            
            Vector2 arrowStartPosition = playerPosition + _offset;
            Vector2 direction = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
            
            return new Arrow(arrowStartPosition, direction, Rotation);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerPosition)
        {
            if (!IsAiming || _bowSprite == null)
                return;

            Position = playerPosition + _offset;
            
            System.Diagnostics.Debug.WriteLine($"Drawing bow at: {Position}, Player at: {playerPosition}, Offset: {_offset}");

            // Change bow color based on whether it can shoot
            Color bowColor = CanShoot ? Color.White : Color.Gray;

            // Draw the bow with rotation
            spriteBatch.Draw(
                _bowSprite.Region.Texture,
                Position,
                _bowSprite.Region.SourceRectangle,
                bowColor,
                Rotation + RotationOffset,
                new Vector2(_bowSprite.Region.Width * 0.5f, _bowSprite.Region.Height * 0.5f), // Origin at center
                _bowSprite.Scale,
                SpriteEffects.None,
                0f
            );
        }
    }

    public class Arrow
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; private set; }
        public float Rotation { get; private set; }
        public bool IsActive { get; set; }
        
        private const float Speed = 8f;
        private Sprite _arrowSprite;
        private const float RotationOffset = MathHelper.PiOver2;

        public Arrow(Vector2 startPosition, Vector2 direction, float rotation)
        {
            Position = startPosition;
            Velocity = direction * Speed;
            Rotation = rotation;
            IsActive = true;
        }

        public void SetSprite(Sprite sprite)
        {
            _arrowSprite = sprite;
        }

        public void Update()
        {
            if (!IsActive)
                return;

            Position += Velocity;
        }

        // Enemy collision detection
        public bool CheckCollision(Enemy enemy)
        {
            if (!IsActive)
                return false;

            Rectangle arrowRect = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                16,
                16
            );

            Rectangle enemyRect = new Rectangle(
                (int)enemy._position.X,
                (int)enemy._position.Y,
                64,
                64
            );

            return arrowRect.Intersects(enemyRect);
        }

        // Bounds detection
        public bool IsOutOfBounds(Rectangle roomBounds)
        {
            return Position.X < roomBounds.Left || Position.X > roomBounds.Right ||
                   Position.Y < roomBounds.Top || Position.Y > roomBounds.Bottom;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || _arrowSprite == null)
                return;

            spriteBatch.Draw(
                _arrowSprite.Region.Texture,
                Position,
                _arrowSprite.Region.SourceRectangle,
                Color.White,
                Rotation + RotationOffset,
                new Vector2(_arrowSprite.Region.Width * 0.5f, _arrowSprite.Region.Height * 0.5f),
                _arrowSprite.Scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}
