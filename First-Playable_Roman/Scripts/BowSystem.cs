using First_Playable_Roman.Scripts.Movements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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

        // True while Space is held and charging
        public bool IsAiming { get; private set; }

        // Charge progress from 0.0 to 1.0
        public float ChargeProgress => Math.Min(_chargeTimer / ChargeTime, 1f);

        // True when fully charged
        public bool IsFullyCharged => _chargeTimer >= ChargeTime;

        // True if an auto-shot was triggered this frame
        public bool HasPendingShot => _pendingShot;

        private Sprite _bowSprite;
        private SoundEffect _shootEffect;
        private Vector2 _offset;

        private float _chargeTimer;
        private const float ChargeTime = 1.0f;
        private const float RotationOffset = MathHelper.PiOver2;

        private bool _pendingShot;

        // True after a shot fires — blocks charging until Space is released
        private bool _waitingForRelease;

        public BowSystem(Sprite bowSprite, SoundEffect shootEffect)
        {
            _bowSprite = bowSprite;
            _shootEffect = shootEffect;
            _offset = new Vector2(32, 32);
            Rotation = 0f;
            IsAiming = false;
            _chargeTimer = 0f;
            _pendingShot = false;
            _waitingForRelease = false;
        }

        public void Update(GameTime gameTime)
        {
            _pendingShot = false;

            if (IsAiming)
            {
                _chargeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_chargeTimer >= ChargeTime)
                {
                    _pendingShot = true;
                    _chargeTimer = 0f;
                    IsAiming = false;
                    _waitingForRelease = true;
                }
            }
        }

        // Called every frame when Space is held down
        public void StartAiming()
        {
            // Don't start charging until Space was released after last shot
            if (_waitingForRelease)
                return;

            IsAiming = true;
        }

        // Called every frame when Space is NOT held — always call this when Space is up
        public void StopAiming()
        {
            IsAiming = false;
            _chargeTimer = 0f;
            _waitingForRelease = false;  // Space released — ready for next charge
        }

        // Update aim to point towards the closest active enemy
        public void UpdateAim(Vector2 playerPosition, List<Enemy> enemies)
        {
            if (enemies == null || enemies.Count == 0)
                return;

            Enemy closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                if (enemy is TurretStrategy || !enemy.IsActive)
                    continue;

                float distance = Vector2.Distance(playerPosition, enemy._position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy != null)
            {
                Vector2 direction = closestEnemy._position - playerPosition;
                Rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        // Creates an arrow in the aiming direction
        public Arrow ShootArrow(Vector2 playerPosition)
        {
            if (_shootEffect != null)
                Core.Audio.PlaySoundEffect(_shootEffect);

            Vector2 arrowStartPosition = playerPosition + _offset;
            Vector2 direction = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));

            return new Arrow(arrowStartPosition, direction, Rotation);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerPosition)
        {
            // Show bow only while actively charging
            if (!IsAiming || _bowSprite == null)
                return;

            Position = playerPosition + _offset;

            // Interpolate color White → Yellow as charge fills up
            Color bowColor = Color.Lerp(Color.White, Color.Yellow, ChargeProgress);

            spriteBatch.Draw(
                _bowSprite.Region.Texture,
                Position,
                _bowSprite.Region.SourceRectangle,
                bowColor,
                Rotation + RotationOffset,
                new Vector2(_bowSprite.Region.Width * 0.5f, _bowSprite.Region.Height * 0.5f),
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

        private const float Speed = 16f;
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

        public Rectangle GetBounds()
        {
            if (_arrowSprite == null)
                return new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

            int width = 32;
            int height = 32;

            return new Rectangle(
                (int)(Position.X - width * 0.5f),
                (int)(Position.Y - height * 0.5f),
                width,
                height
            );
        }

        public bool CheckCollision(Enemy enemy)
        {
            if (!IsActive || enemy is TurretStrategy)
                return false;

            Rectangle arrowRect = GetBounds();

            Rectangle enemyRect = new Rectangle(
                (int)enemy._position.X,
                (int)enemy._position.Y,
                64,
                64
            );

            return arrowRect.Intersects(enemyRect);
        }

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
