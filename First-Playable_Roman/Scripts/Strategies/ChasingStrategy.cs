using First_Playable_Roman.Scenes;
using Microsoft.Xna.Framework;
using System;

namespace First_Playable_Roman.Scripts.Strategies
{
    internal class ChaserStrategy : Enemy
    {
        // Detection radius when the player is within this distance, the enemy starts chasing
        public float DetectionRadius { get; set; }

        // True when chasing the player.
        public bool IsChasing { get; private set; }

        // Stored reference to the player position for chase calculations
        private Vector2 _targetPosition;
        private bool _hasTarget;

        public ChaserStrategy(int xPos, int yPos, int speed, float detectionRadius, Rooms room)
            : base(75, xPos, yPos, speed, room)
        {
            DetectionRadius = detectionRadius;
            IsChasing = false;
            _hasTarget = false;
        }

        // Call this before Move() to update the player position and determine if the enemy should chase
        public void UpdateTarget(Vector2 playerPosition)
        {
            // Calculate distance from enemy center to player center
            Vector2 enemyCenter = new Vector2(
                _position.X + SpriteWidth * 0.5f,
                _position.Y + SpriteHeight * 0.5f
            );

            float distance = Vector2.Distance(enemyCenter, playerPosition);

            if (distance <= DetectionRadius)
            {
                IsChasing = true;
                _targetPosition = playerPosition;
                _hasTarget = true;
            }
            else
            {
                IsChasing = false;
                _hasTarget = false;
            }
        }

        public override Vector2 Move()
        {
            if (IsChasing && _hasTarget)
            {
                // Move directly toward the player
                Vector2 enemyCenter = new Vector2(
                    _position.X + SpriteWidth * 0.5f,
                    _position.Y + SpriteHeight * 0.5f
                );

                Vector2 direction = _targetPosition - enemyCenter;

                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }

                return direction * Speed;
            }
            else
            {
                // Generate a random angle.
                float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

                // Convert angle to a direction vector.
                float x = (float)Math.Cos(angle);
                float y = (float)Math.Sin(angle);
                Vector2 direction = new Vector2(x, y);

                // Multiply the direction vector by the movement speed.
                Vector2 slimeVelocity = new Vector2(direction.X * Speed, direction.Y * Speed);

                return slimeVelocity;
            }
        }
    }
}
