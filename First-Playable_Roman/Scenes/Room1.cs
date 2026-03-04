using First_Playable_Roman.Scripts;
using First_Playable_Roman.Scripts.Movements;
using First_Playable_Roman.Scripts.Strategies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace First_Playable_Roman.Scenes
{
    public class Room1 : Scene
    {
        private AnimatedSprite _playerSprite;
        private AnimatedSprite _slimeSprite;

        private Sprite _knifeSprite;
        private Sprite _heartSprite;
        private Sprite _keySprite;

        private Vector2[] _knifePositions = 
        { 
            new Vector2(100, 100),
            new Vector2(500, 500)
        };
        private Vector2[] _heartPositions =
        {
            new Vector2(500, 100),
            new Vector2(100, 500)
        };
        private Vector2 _keyPosition = new Vector2(800, 800);

        private Player _player;
        private Vector2 _playerPosition;

        private List<Enemy> _enemies;

        // Tracks the position of the slime.
        private List<Vector2> _slimePositions;

        // Tracks the velocity of the slime.
        private List<Vector2> _slimeVelocity;

        // Tracks the number of projectiles shot
        private List<Circle> _projectiles;

        // Defines the tilemap to draw.
        private Tilemap _tilemap;

        // Defines the bounds of the room that the slime and bat are contained within.
        private Rectangle _roomBounds;

        private SoundEffect _hitSoundEffect;
        private SoundEffect _bounceSoundEffect;

        private Song _themeSong;

        // The SpriteFont Description used to draw text.
        private SpriteFont _font;

        // Defines the position to draw the score text at.
        private Vector2 _healthTextPosition;

        // Defines the origin used when drawing the score text.
        private Vector2 _healthTextOrigin;

        private Timer _shootTimer;

        public enum GameState { Playing, GameOver }
        public static GameState _state = GameState.Playing;

        public override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Core.ExitOnEscape = false;

            Rectangle screenBounds = Core.Bounds;

            _roomBounds = new Rectangle(
                (int)_tilemap.TileWidth,
                (int)_tilemap.TileHeight,
                (int)(_tilemap.TileWidth * _tilemap.Columns - _tilemap.TileWidth * 2),
                (int)(_tilemap.TileHeight * _tilemap.Rows - _tilemap.TileHeight * 2)
             );

            // Do not compute room bounds here: tilemap and scale are created in LoadContent.
            // Read player name and set default positions that don't depend on tilemap.
            string nameInput = Console.ReadLine();
            _player = new Player(nameInput, 100, 565, 0, 10);
            _enemies = new List<Enemy>
            {
                new LurkingStrategy(10, 100, 5, 5),
                new LurkingStrategy(10, 100, 5, 5),
                //new TurretStrategy(10, 100, 500, 4)
            };
            _slimePositions = new List<Vector2>();
            _slimeVelocity = new List<Vector2>();

            for (int i = 0; i < _enemies.Count; i++)
            {
                _slimePositions.Add(new Vector2(_enemies[i]._position._xPos, _enemies[i]._position._yPos));
                if(_enemies[i] is LurkingStrategy)
                    AssignRandomSlimeVelocity(i);
            }

            _shootTimer = new Timer(AllTurretShoot, null, 0, 5);

            // Set the position of the score text to align to the left edge of the
            // room bounds, and to vertically be at the center of the first tile.
            _healthTextPosition = new Vector2(_roomBounds.Left, _tilemap.TileHeight * 0.5f);

            // Set the origin of the text so it is left-centered.
            float scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
            _healthTextOrigin = new Vector2(0, scoreTextYOrigin);
        }

        public override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slimeSprite = atlas.CreateAnimatedSprite("Slime-animation");

            _knifeSprite = atlas.CreateSprite("Knife");
            _heartSprite = Content.Load<Sprite>("images/heart");
            _keySprite = Content.Load<Sprite>("images/key");

            // Create the tilemap from the XML configuration file.
            _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
            _tilemap.Scale = new Vector2(4.0f, 4.0f);

            // Load the bounce sound effect
            _bounceSoundEffect = Content.Load<SoundEffect>("audio/bounceSoundEffect");

            // Load the collect sound effect
            _hitSoundEffect = Content.Load<SoundEffect>("audio/pixelhitsound");

            // Load the background music
            _themeSong = Content.Load<Song>("audio/backgroundMusic");

            // Load the font
            _font = Content.Load<SpriteFont>("fonts/04B_30");
        }

        public override void Update(GameTime gameTime)
        {
            // Ensure core / input state is updated before we inspect input.
            base.Update(gameTime);

            if (_state == GameState.Playing)
            {
                Core.Instance.Window.Title = "Test for now";
            }
            else if (_state == GameState.GameOver)
            {
                Core.Instance.Window.Title = "Press R to Restart";
            }

            _playerSprite?.Update(gameTime);
            _slimeSprite?.Update(gameTime);

            // If game over, allow restart and skip gameplay updates
            if (_state == GameState.GameOver)
            {
                if (Core.Input.Keyboard.WasKeyJustPressed(Keys.R))
                {
                    Restart();
                }
                return;
            }

            if (_player != null && _player.Health.CurrentHealth <= 0)
            {
                GameOver();
                return;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Core.ChangeScene(new TitleScene());

            

            PlayerInput();
            if (_player != null)
                _playerPosition = new Vector2(_player._position._xPos, _player._position._yPos);

            // Calculate the new position of the slime based on the velocity.
            for (int i = 0; i < _slimePositions.Count; i++)
            {
                Vector2 newSlimePosition = _slimePositions[i] + _slimeVelocity[i];

                // Use float centers / radius to avoid truncation-induced repeated collisions
                float slimeHalfW2 = _slimeSprite?.Width * 0.5f ?? 0f;
                float slimeHalfH2 = _slimeSprite?.Height * 0.5f ?? 0f;

                // Center coordinates of the slime (float)
                float centerX = newSlimePosition.X + slimeHalfW2;
                float centerY = newSlimePosition.Y + slimeHalfH2;

                // Radius (use half of width/height consistently; pick one if circular)
                float radius = slimeHalfW2;

                Vector2 normal = Vector2.Zero;

                // Top collision -- push to room top + radius (not to absolute 0)
                if (centerY - radius < _roomBounds.Top)
                {
                    centerY = _roomBounds.Top + radius;
                    normal.Y = 1f;
                }
                // Bottom collision
                else if (centerY + radius > _roomBounds.Bottom)
                {
                    centerY = _roomBounds.Bottom - radius;
                    normal.Y = -1f;
                }

                // Left collision -- push to room left + radius
                if (centerX - radius < _roomBounds.Left)
                {
                    centerX = _roomBounds.Left + radius;
                    normal.X = 1f;
                }
                // Right collision
                else if (centerX + radius > _roomBounds.Right)
                {
                    centerX = _roomBounds.Right - radius;
                    normal.X = -1f;
                }

                // Convert center back to top-left position
                newSlimePosition = new Vector2(centerX - slimeHalfW2, centerY - slimeHalfH2);

                // If the normal is anything but Vector2.Zero, reflect the velocity
                if (normal != Vector2.Zero)
                {
                    normal.Normalize();
                    _slimeVelocity[i] = Vector2.Reflect(_slimeVelocity[i], normal);

                    // Play bounce sound effect on collision with room bounds
                    if (_bounceSoundEffect != null)
                        Core.Audio.PlaySoundEffect(_bounceSoundEffect);
                }

                _slimePositions[i] = newSlimePosition;

                // Rebuild slime bounding circle for overlap/interaction checks
                Circle slimeBounds = new Circle(
                    (int)(centerX),
                    (int)(centerY),
                    (int)radius
                );

                // Recompute player bounding circle for collision check (if still using Circle-based checks)
                Circle playerBounds = new Circle(
                    (int)(_playerPosition.X + (_playerSprite.Width * 0.5f)),
                    (int)(_playerPosition.Y + (_playerSprite.Height * 0.5f)),
                    (int)(_playerSprite.Width * 0.5f)
                );

                if (playerBounds.Intersects(slimeBounds))
                {
                    _player.TakeDamage(10);

                    // Respawn slime inside playable area leaving exactly one tile margin on each edge.
                    int tileWScaled = (int)Math.Max(1, Math.Round(_tilemap.TileWidth));
                    int tileHScaled = (int)Math.Max(1, Math.Round(_tilemap.TileHeight));

                    int innerCols = Math.Max(1, _tilemap.Columns - 2);
                    int innerRows = Math.Max(1, _tilemap.Rows - 2);

                    int column = Random.Shared.Next(0, innerCols);
                    int row = Random.Shared.Next(0, innerRows);

                    // Position on tile grid inside playable area (roomX + column * tileWScaled)
                    _slimePositions[i] = new Vector2(_roomBounds.Left + column * tileWScaled, _roomBounds.Top + row * tileHScaled);

                    AssignRandomSlimeVelocity(i);

                    // Play hit sound effect on player damage
                    if (_hitSoundEffect != null)
                        Core.Audio.PlaySoundEffect(_hitSoundEffect);
                }
            }
        }

        private void AssignRandomSlimeVelocity(int index)
        {
            // Generate a random angle.
            float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

            // Convert angle to a direction vector.
            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);
            Vector2 direction = new Vector2(x, y);

            List<LurkingStrategy> lurkingEnemies = new List<LurkingStrategy>();

            lurkingEnemies.Add((LurkingStrategy)_enemies[index]);

            // Multiply the direction vector by the movement speed.
            _slimeVelocity.Add(direction * lurkingEnemies[index].Speed);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw the tilemap.
            _tilemap?.Draw(Core.SpriteBatch);

            for(int i = 0; i < _slimePositions.Count; i++)
                _slimeSprite?.Draw(Core.SpriteBatch, _slimePositions[i]);

            for(int i = 0; i < _knifePositions.Length; i++)
                _knifeSprite?.Draw(Core.SpriteBatch, _knifePositions[i]);

            for (int i = 0; i < _heartPositions.Length; i++)
                _heartSprite?.Draw(Core.SpriteBatch, _heartPositions[i]);

            _keySprite?.Draw(Core.SpriteBatch, _keyPosition);

            // Draw player only when present and playing
            if (_state == GameState.Playing && _playerSprite != null)
            {
                _playerSprite.Draw(Core.SpriteBatch, _playerPosition);

                // Draw the score
                Core.SpriteBatch.DrawString(
                    _font,              // spriteFont
                    $"Health: {_player.Health.CurrentHealth}", // text
                    _healthTextPosition, // position
                    Color.White,        // color
                    0.0f,               // rotation
                    _healthTextOrigin,   // origin
                    1.5f,               // scale
                    SpriteEffects.None, // effects
                    0.0f                // layerDepth
                );
            }
            else if (_state == GameState.GameOver)
            {
                Core.SpriteBatch.DrawString(
                    _font,
                    "Game Over - press R to restart",
                    new Vector2(Core.GraphicsDevice.Viewport.Width * 0.5f, Core.GraphicsDevice.Viewport.Height * 0.3f),
                    Color.Gold,
                    0.0f,
                    _font.MeasureString("Game Over - press R to restart") * 0.5f,
                    1.0f,
                    SpriteEffects.None,
                    0.0f
                );
            }

            Core.SpriteBatch.End();

            base.Draw(gameTime);
        }

        public void PlayerInput()
        {
            KeyboardInfo keyboard = Core.Input.Keyboard;
            
            // Skip player input when game over or player missing.
            if (_state == GameState.GameOver || _player == null || _playerSprite == null)
                return;

            int playerInputX = 0;
            int playerInputY = 0;

            if (keyboard.IsKeyDown(Keys.Space))
            {
                _player._speed = 4;
            }
            else
            {
                _player._speed = 2;
            }

            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) playerInputY--;
            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) playerInputY++;
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) playerInputX--;
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) playerInputX++;

            // Apply input to position
            _player._position._xPos += playerInputX * _player._speed;
            _player._position._yPos += playerInputY * _player._speed;

            // Clamp using playable room bounds so continuous input won't cause jitter.
            int minX = _roomBounds.Left;
            int minY = _roomBounds.Top;
            int maxX = _roomBounds.Right - (int)_playerSprite.Width;
            int maxY = _roomBounds.Bottom - (int)_playerSprite.Height;

            _player._position._xPos = Math.Clamp(_player._position._xPos, minX, Math.Max(minX, maxX));
            _player._position._yPos = Math.Clamp(_player._position._yPos, minY, Math.Max(minY, maxY));

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

        private void AllTurretShoot(object state)
        {
            List<TurretStrategy> turrets = new List<TurretStrategy>();

            foreach (Enemy enemy in _enemies)
                if (enemy is TurretStrategy)
                    turrets.Add((TurretStrategy)enemy);
                    
            foreach (TurretStrategy turret in turrets)
            {
                Circle projectile = turret.Shoot();
                _projectiles.Add(projectile);
            }
        }

        private void GameOver()
        {
            if (_state == GameState.GameOver) return;

            // Switch state
            _state = GameState.GameOver;

            // Stop music
            Core.Audio.PauseAudio();

            // Clear player references so player is effectively "deleted"
            _player = null;
            _playerSprite = null;

            // Optionally freeze slime
            for(int i = 0; i < _slimeVelocity.Count; i++)
                _slimeVelocity[i] = Vector2.Zero;

            // Play hit sound (optional)
            if (_hitSoundEffect != null)
                Core.Audio.PlaySoundEffect(_hitSoundEffect);
        }

        private void Restart()
        {
            // Recreate basic player and sprites from content (simple restart).
            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slimeSprite = atlas.CreateAnimatedSprite("Slime-animation");

            Core.Audio.PlaySong(_themeSong);
            Core.Audio.SongVolume = 0.3f;

            _player = new Player("Player", 100, 565, 0, 10);
            _enemies = new List<Enemy>
            {
                new LurkingStrategy(100, 100, 5, 10),
                new LurkingStrategy(100, 100, 5, 10),
            };
            _slimePositions = new List<Vector2>();
            _slimeVelocity = new List<Vector2>();

            for (int i = 0; i < _enemies.Count; i++)
            {
                _slimePositions.Add(new Vector2(_enemies[i]._position._xPos, _enemies[i]._position._yPos));
                if(_enemies[i] is LurkingStrategy)
                    AssignRandomSlimeVelocity(i);
            }

            _state = GameState.Playing;
        }
    }
}
