using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

namespace First_Playable_Roman
{
    public class Game1 : Core
    {
        private AnimatedSprite _playerSprite;
        private AnimatedSprite _slimeSprite;

        private Player _player;
        private Vector2 _playerPosition;

        private Enemy _slime;

        // Tracks the position of the slime.
        private Vector2 _slimePosition;

        // Tracks the velocity of the slime.
        private Vector2 _slimeVelocity;

        // Defines the tilemap to draw.
        private Tilemap _tilemap;

        // Defines the bounds of the room that the slime and bat are contained within.
        private Rectangle _roomBounds;

        // If GraphicsDevice wasn't ready in LoadContent, defer recalc to Update.
        private bool _needsRoomRecalc;

        private SoundEffect _hitSoundEffect;

        private SoundEffect _bounceSoundEffect;

        private Song _themeSong;

        public Game1() : base("TestForNow", 1280, 720, false)
        {
            _player = new Player("Player", 100, 0, 0);
            _slime = new Enemy(0, 0, 0);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            // Do not compute room bounds here: tilemap and scale are created in LoadContent.
            // Read player name and set default positions that don't depend on tilemap.
            string nameInput = Console.ReadLine();
            _player = new Player(nameInput, 100, 565, 0);
            _slime = new Enemy(100, 100, 5);

            // Default slime position until LoadContent sets the room and initial placement.
            _slimePosition = new Vector2(_slime._position._xPos, _slime._position._yPos);

            AssignRandomSlimeVelocity();

            // Start playing background music.
            Audio.PlaySong(_themeSong);
            Audio.SongVolume = 0.3f;
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slimeSprite = atlas.CreateAnimatedSprite("Slime-animation");

            // Create the tilemap from the XML configuration file.
            _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");

            // Apply scale used by the game visuals.
            _tilemap.Scale = new Vector2(4.0f, 4.0f);

            // Try to perform the recalculation now; if GraphicsDevice is not ready, defer it.
            var gd = GraphicsDevice ?? GraphicsDevice;
            if (gd != null)
            {
                RecalculateRoomBoundsAndPlaceSlime();
                _needsRoomRecalc = false;
            }
            else
            {
                _needsRoomRecalc = true;
            }

            // Load the bounce sound effect
            _bounceSoundEffect = Content.Load<SoundEffect>("audio/bounceSoundEffect");

            // Load the collect sound effect
            _hitSoundEffect = Content.Load<SoundEffect>("audio/pixelhitsound");

            // Load the background music
            _themeSong = Content.Load<Song>("audio/backgroundMusic");
        }

        private void RecalculateRoomBoundsAndPlaceSlime()
        {
            if (_tilemap == null)
                throw new InvalidOperationException("_tilemap must be initialized before recalculating room bounds.");

            var gd = GraphicsDevice ?? GraphicsDevice;
            if (gd == null)
                throw new InvalidOperationException("GraphicsDevice not initialized yet.");

            // Scaled tile size in pixels (rounded to avoid truncation errors)
            int tileWScaled = (int)Math.Max(1, Math.Round(_tilemap.TileWidth));
            int tileHScaled = (int)Math.Max(1, Math.Round(_tilemap.TileHeight));

            // Map size in pixels based on tilemap columns/rows and scaled tile size.
            int mapPixelWidth = _tilemap.Columns * tileWScaled;
            int mapPixelHeight = _tilemap.Rows * tileHScaled;

            // Make exactly one tile on each edge non-passable.
            // Playable area is the map inset by one tile from each side.
            int roomX = tileWScaled;
            int roomY = tileHScaled;
            int roomWidth = Math.Max(0, mapPixelWidth - tileWScaled * 2);
            int roomHeight = Math.Max(0, mapPixelHeight - tileHScaled * 2);

            _roomBounds = new Rectangle(roomX, roomY, roomWidth, roomHeight);

            // Place slime in the visual center of the playable area.
            float slimeCx = roomX + roomWidth * 0.5f;
            float slimeCy = roomY + roomHeight * 0.5f;
            float slimeHalfW = _slimeSprite?.Width * 0.5f ?? 0f;
            float slimeHalfH = _slimeSprite?.Height * 0.5f ?? 0f;
            _slimePosition = new Vector2(slimeCx - slimeHalfW, slimeCy - slimeHalfH);

            // Clamp player into the playable area.
            if (_playerSprite != null)
            {
                int maxX = Math.Max(_roomBounds.Left, _roomBounds.Right - (int)_playerSprite.Width);
                int maxY = Math.Max(_roomBounds.Top, _roomBounds.Bottom - (int)_playerSprite.Height);
                _player._position._xPos = Math.Clamp(_player._position._xPos, _roomBounds.Left, maxX);
                _player._position._yPos = Math.Clamp(_player._position._yPos, _roomBounds.Top, maxY);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // If we deferred room recalculation because GraphicsDevice wasn't ready, do it now.
            if (_needsRoomRecalc)
            {
                var gd = GraphicsDevice ?? GraphicsDevice;
                if (gd != null && _tilemap != null)
                {
                    RecalculateRoomBoundsAndPlaceSlime();
                    _needsRoomRecalc = false;
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _playerSprite.Update(gameTime);
            _slimeSprite.Update(gameTime);

            PlayerInput();
            _playerPosition = new Vector2(_player._position._xPos, _player._position._yPos);

            // Calculate the new position of the slime based on the velocity.
            Vector2 newSlimePosition = _slimePosition + _slimeVelocity;

            // Use float centers / radius to avoid truncation-induced repeated collisions
            float slimeHalfW2 = _slimeSprite.Width * 0.5f;
            float slimeHalfH2 = _slimeSprite.Height * 0.5f;

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
                _slimeVelocity = Vector2.Reflect(_slimeVelocity, normal);

                // Play bounce sound effect on collision with room bounds
                Audio.PlaySoundEffect(_bounceSoundEffect);
            }

            _slimePosition = newSlimePosition;

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
                _slimePosition = new Vector2(_roomBounds.Left + column * tileWScaled, _roomBounds.Top + row * tileHScaled);

                AssignRandomSlimeVelocity();

                // Play hit sound effect on player damage
                Audio.PlaySoundEffect(_hitSoundEffect);
            }

            base.Update(gameTime);
        }

        private void AssignRandomSlimeVelocity()
        {
            // Generate a random angle.
            float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

            // Convert angle to a direction vector.
            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);
            Vector2 direction = new Vector2(x, y);

            // Multiply the direction vector by the movement speed.
            _slimeVelocity = direction * _slime._speed;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw the tilemap.
            _tilemap.Draw(SpriteBatch);

            _playerSprite.Draw(SpriteBatch, _playerPosition);
            // Draw the slime sprite.
            _slimeSprite.Draw(SpriteBatch, _slimePosition);

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public void PlayerInput()
        {
            int playerInputX = 0;
            int playerInputY = 0;

            if (Input.Keyboard.IsKeyDown(Keys.Space))
            {
                _player._speed = 4;
            }
            else
            {
                _player._speed = 2;
            }

            if (Input.Keyboard.IsKeyDown(Keys.W)) playerInputY--;
            if (Input.Keyboard.IsKeyDown(Keys.S)) playerInputY++;
            if (Input.Keyboard.IsKeyDown(Keys.A)) playerInputX--;
            if (Input.Keyboard.IsKeyDown(Keys.D)) playerInputX++;

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
            if (Input.Keyboard.WasKeyJustPressed(Keys.M))
            {
                Audio.ToggleMute();
            }

            // If the + button is pressed, increase the volume.
            if (Input.Keyboard.WasKeyJustPressed(Keys.OemPlus))
            {
                Audio.SongVolume += 0.1f;
                Audio.SoundEffectVolume += 0.1f;
            }

            // If the - button was pressed, decrease the volume.
            if (Input.Keyboard.WasKeyJustPressed(Keys.OemMinus))
            {
                Audio.SongVolume -= 0.1f;
                Audio.SoundEffectVolume -= 0.1f;
            }
        }

        public void GameOver()
        {
            
        }
    }
}
