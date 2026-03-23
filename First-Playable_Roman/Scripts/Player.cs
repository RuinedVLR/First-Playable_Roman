using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using MonoGameLibrary.Input;
using MonoGameLibrary;
using First_Playable_Roman.Scenes;
using MonoGameLibrary.Graphics;

namespace First_Playable_Roman.Scripts
{
    public class Player : Entity
    {
        public Health Health { get; set; }
        public string Name { get; set; }
        public int _speed;
        public AnimatedSprite Sprite { get; private set; }
        public Rectangle Bounds { get; private set; }

        public int HitboxWidth { get; private set; }
        public int HitboxHeight { get; private set; }

        private Vector2 _hitboxOffset;

        public bool _isShowHitboxes = false;

        public int Score { get; set; }

        public BowSystem Bow { get; private set; }
        public bool HasBow { get; set; }
        public bool HasKnife { get; set; }

        public Player(string name, int hp, int xPos, int yPos, int speed, AnimatedSprite playerSprite) : base(xPos, yPos)
        {
            Name = name;
            Health = new Health(hp);
            _speed = speed;
            Sprite = playerSprite;
            HasBow = false;
            HasKnife = false;
            
            HitboxWidth = 64;
            HitboxHeight = 64;

            if (Sprite != null)
            {
                _hitboxOffset = new Vector2(
                    (Sprite.Width - HitboxWidth) * 0.5f,
                    (Sprite.Height - HitboxHeight) * 0.5f
                );
            }
            else
            {
                _hitboxOffset = Vector2.Zero;
            }
        }

        public void EquipBow(Sprite bowSprite)
        {
            Bow = new BowSystem(bowSprite);
            HasBow = true;
        }

        public void TakeDamage(int damage)
        {
            Health.TakeDamage(damage);
        }

        public void Attack(Enemy enemy)
        {
            enemy.TakeDamage(Health.CurrentHealth);
        }

        private void Draw()
        {
            if (Sprite != null)
            {
                Sprite.Draw(Core.SpriteBatch, _position);
            }
            if (_isShowHitboxes)
            {
                Core.DrawRectangleOutline(Bounds, Color.Red);
            }
        }

        // Returns Player hitbox centered on sprite
        public Rectangle GetHitbox()
        {
            return new Rectangle(
                (int)(_position.X + _hitboxOffset.X),
                (int)(_position.Y + _hitboxOffset.Y),
                HitboxWidth,
                HitboxHeight
            );
        }

        public void PlayerInput(Rectangle roomBounds, List<Rectangle> obstacles, List<Enemy> enemies)
        {
            // Skip player input when game over or player missing.
            if (Rooms._state == Rooms.GameState.GameOver || Sprite == null)
                return;

            KeyboardInfo keyboard = Core.Input.Keyboard;

            int playerInputX = 0;
            int playerInputY = 0;

            // Bow aiming and shooting logic
            if (HasBow && Bow != null)
            {
                if (keyboard.IsKeyDown(Keys.Space))
                {
                    _speed = 1; // Speed down when aiming
                }
                else
                {
                    _speed = 2;
                }
            }

            if (keyboard.IsKeyDown(Keys.LeftShift) && !keyboard.IsKeyDown(Keys.Space))
            {
                _speed = 4;
            }
            else if(!keyboard.IsKeyDown(Keys.Space))
            {
                _speed = 2;
            }

            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) playerInputY--;
            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) playerInputY++;
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) playerInputX--;
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) playerInputX++;

            Vector2 oldPosition = _position;

            _position.X += playerInputX * _speed;

            // X collision check
            Rectangle playerRectX = new Rectangle(
                (int)(_position.X + _hitboxOffset.X),
                (int)(_position.Y + _hitboxOffset.Y),
                HitboxWidth,
                HitboxHeight
            );

            bool hasCollisionX = false;
            if (obstacles != null)
            {
                foreach (Rectangle obstacle in obstacles)
                {
                    if (playerRectX.Intersects(obstacle))
                    {
                        hasCollisionX = true;
                        break;
                    }
                }
            }

            // X axis collision response
            if (hasCollisionX)
            {
                _position.X = oldPosition.X;
            }

            _position.Y += playerInputY * _speed;

            // Y collision check
            Rectangle playerRectY = new Rectangle(
                (int)(_position.X + _hitboxOffset.X),
                (int)(_position.Y + _hitboxOffset.Y),
                HitboxWidth,
                HitboxHeight
            );

            bool hasCollisionY = false;
            if (obstacles != null)
            {
                foreach (Rectangle obstacle in obstacles)
                {
                    if (playerRectY.Intersects(obstacle))
                    {
                        hasCollisionY = true;
                        break;
                    }
                }
            }

            // Y axis collision response
            if (hasCollisionY)
            {
                _position.Y = oldPosition.Y;
            }

            // Hitbox update
            Bounds = new Rectangle(
                (int)(_position.X + _hitboxOffset.X),
                (int)(_position.Y + _hitboxOffset.Y),
                HitboxWidth,
                HitboxHeight
            );

            if(keyboard.WasKeyJustPressed(Keys.T))
            {
                _isShowHitboxes = !_isShowHitboxes;
            }

            // If the M key is pressed, toggle mute state for audio.
            if (keyboard.WasKeyJustPressed(Keys.M))
            {
                Core.Audio.ToggleMute();
            }

            // If the + button is pressed, increase the volume.
            if (keyboard.WasKeyJustPressed(Keys.OemPlus))
            {
                Core.Audio.SongVolume += 0.1f;
                Core.Audio.SoundEffectVolume += 0.1f;
            }

            // If the - button was pressed, decrease the volume.
            if (keyboard.WasKeyJustPressed(Keys.OemMinus))
            {
                Core.Audio.SongVolume -= 0.1f;
                Core.Audio.SoundEffectVolume -= 0.1f;
            }
        }
    }
}