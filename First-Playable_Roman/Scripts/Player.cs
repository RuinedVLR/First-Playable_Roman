using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGameLibrary.Input;
using MonoGameLibrary;
using First_Playable_Roman.Scripts;
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

        public bool _isShowHitboxes = false;

        public Player(string name, int hp, int xPos, int yPos, int speed, AnimatedSprite playerSprite) : base(xPos, yPos)
        {
            Name = name;
            Health = new Health(hp);
            _speed = speed;

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

        public void PlayerInput(Rectangle roomBounds)
        {
            // Skip player input when game over or player missing.
            if (Game1._state == Game1.GameState.GameOver || this == null || Sprite == null)
                return;

            KeyboardInfo keyboard = Core.Input.Keyboard;

            

            int playerInputX = 0;
            int playerInputY = 0;

            if (keyboard.IsKeyDown(Keys.Space))
            {
                _speed = 4;
            }
            else
            {
                _speed = 2;
            }

            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) playerInputY--;
            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) playerInputY++;
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) playerInputX--;
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) playerInputX++;

            // Apply input to position

            _position = new Vector2(playerInputX * _speed, playerInputY * _speed);

            // Clamp using playable room bounds so continuous input won't cause jitter.
            int minX = roomBounds.Left;
            int minY = roomBounds.Top;
            int maxX = roomBounds.Right - (int)Sprite.Width;
            int maxY = roomBounds.Bottom - (int)Sprite.Height;

            _position.X = Math.Clamp(_position.X, minX, Math.Max(minX, maxX));
            _position.Y = Math.Clamp(_position.Y, minY, Math.Max(minY, maxY));

            if(keyboard.WasKeyJustPressed(Keys.T))
            {
                if(_isShowHitboxes)
                {
                    _isShowHitboxes = false;
                }
                else
                {
                    _isShowHitboxes = true;
                }
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