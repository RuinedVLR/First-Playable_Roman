using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

namespace First_Playable_Roman
{
    public class Game1 : Core
    {
        private AnimatedSprite _playerSprite;
        private AnimatedSprite _slime;

        private Player _player;
        private Vector2 _playerPosition;

        private const float MOVEMENT_SPEED = 5.0f;

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

        public Game1() : base("TestForNow", 1280, 720, false)
        {
            _player = new Player("Player", 100, 588, 4);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            // Do not compute room bounds here: tilemap and scale are created in LoadContent.
            // Read player name and set default positions that don't depend on tilemap.
            string nameInput = Console.ReadLine();
            _player = new Player(nameInput, 100, 588, 4);

            // Default slime position until LoadContent sets the room and initial placement.
            _slimePosition = new Vector2(100, 100);

            AssignRandomSlimeVelocity();

            // Removed direct RecalculateRoomBoundsAndPlaceSlime() call from Initialize.
            // It must run after LoadContent has created and scaled the tilemap.
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slime = atlas.CreateAnimatedSprite("Slime-animation");

            // Create the tilemap from the XML configuration file.
            _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");

            // Apply scale used by the game visuals.
            _tilemap.Scale = new Vector2(4.0f, 4.0f);

            // Try to perform the recalculation now; if GraphicsDevice is not ready, defer it.
            var gd = GraphicsDevice ?? Core.GraphicsDevice;
            if (gd != null)
            {
                RecalculateRoomBoundsAndPlaceSlime();
                _needsRoomRecalc = false;
            }
            else
            {
                _needsRoomRecalc = true;
            }
        }

        private void RecalculateRoomBoundsAndPlaceSlime()
        {
            if (_tilemap == null)
                throw new InvalidOperationException("_tilemap must be initialized before recalculating room bounds.");

            var gd = GraphicsDevice ?? Core.GraphicsDevice;
            if (gd == null)
                throw new InvalidOperationException("GraphicsDevice not initialized yet.");

            // Scaled tile size in pixels (ensure at least 1)
            int tileWScaled = (int)Math.Max(1, _tilemap.TileWidth * _tilemap.Scale.X);
            int tileHScaled = (int)Math.Max(1, _tilemap.TileHeight * _tilemap.Scale.Y);

            int screenW = gd.PresentationParameters.BackBufferWidth;
            int screenH = gd.PresentationParameters.BackBufferHeight;

            // Diagnostic output - paste these lines if you still see issues
            Console.WriteLine($"[RoomRecalc] tileWScaled={tileWScaled}, tileHScaled={tileHScaled}, screenW={screenW}, screenH={screenH}");

            // Guarantee exactly one scaled-tile margin on each side.
            int roomX = tileWScaled;
            int roomY = tileHScaled;
            int roomWidth = Math.Max(0, screenW - tileWScaled * 2);
            int roomHeight = Math.Max(0, screenH - tileHScaled * 2);

            Console.WriteLine($"[RoomRecalc] roomX={roomX}, roomY={roomY}, roomWidth={roomWidth}, roomHeight={roomHeight}");

            _roomBounds = new Rectangle(roomX, roomY, roomWidth, roomHeight);

            // Place slime in the visual center of the room (pixel-center).
            // Uses the slime sprite size to center its top-left position so the sprite is centered visually.
            float slimeCx = roomX + roomWidth * 0.5f;
            float slimeCy = roomY + roomHeight * 0.5f;
            float slimeHalfW = _slime?.Width * 0.5f ?? 0f;
            float slimeHalfH = _slime?.Height * 0.5f ?? 0f;
            _slimePosition = new Vector2(slimeCx - slimeHalfW, slimeCy - slimeHalfH);
        }

        protected override void Update(GameTime gameTime)
        {
            // If we deferred room recalculation because GraphicsDevice wasn't ready, do it now.
            if (_needsRoomRecalc)
            {
                var gd = GraphicsDevice ?? Core.GraphicsDevice;
                if (gd != null && _tilemap != null)
                {
                    RecalculateRoomBoundsAndPlaceSlime();
                    _needsRoomRecalc = false;
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _playerSprite.Update(gameTime);
            _slime.Update(gameTime);

            PlayerInput();
            _playerPosition = new Vector2(_player._position._xPos, _player._position._yPos);

            // Calculate the new position of the slime based on the velocity.
            Vector2 newSlimePosition = _slimePosition + _slimeVelocity;

            // Use float centers / radius to avoid truncation-induced repeated collisions
            float slimeHalfW2 = _slime.Width * 0.5f;
            float slimeHalfH2 = _slime.Height * 0.5f;

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
                int totalColumns = GraphicsDevice.PresentationParameters.BackBufferWidth / (int)_slime.Width;
                int totalRows = GraphicsDevice.PresentationParameters.BackBufferHeight / (int)_slime.Height;

                int column = Random.Shared.Next(0, totalColumns);
                int row = Random.Shared.Next(0, totalRows);

                _slimePosition = new Vector2(column * _slime.Width, row * _slime.Height);

                AssignRandomSlimeVelocity();
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
            _slimeVelocity = direction * MOVEMENT_SPEED;
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
            _slime.Draw(SpriteBatch, _slimePosition);

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

            // Clamp using room bounds so continuous input won't cause jitter
            int minX = _roomBounds.Left;
            int minY = _roomBounds.Top;
            int maxX = _roomBounds.Right - (int)_playerSprite.Width;
            int maxY = _roomBounds.Bottom - (int)_playerSprite.Height;

            _player._position._xPos = Math.Clamp(_player._position._xPos, minX, Math.Max(minX, maxX));
            _player._position._yPos = Math.Clamp(_player._position._yPos, minY, Math.Max(minY, maxY));
        }
    }
}
