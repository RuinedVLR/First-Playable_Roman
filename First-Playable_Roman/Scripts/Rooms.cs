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
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace First_Playable_Roman.Scenes
{
    public class Rooms : Scene
    {
        private AnimatedSprite _playerSprite;
        private AnimatedSprite _slimeSprite;

        private Sprite _knifeSprite;
        private Sprite _heartSprite;
        private Sprite _keySprite;

        private List<Vector2> _knifePositions;

        private List<Vector2> _heartPositions;
        private Vector2 _keyPosition;

        private bool hasKnife;

        private Player _player;
        private Vector2 _playerPosition;

        private List<Enemy> _enemies;

        // Tracks the position of the slime.
        private List<Vector2> _slimePositions;

        // Tracks the velocity of the slime.
        public List<Vector2> _slimeVelocity;

        // Defines the tilemap to draw.
        private Tilemap _tilemap;

        // String to find tilemap xml file
        private string _tilemapPath;

        // List of obstacles
        public List<int> _obstaclesTileIDs;
        public List<Rectangle> _obstacles;

        // Defines the bounds of the room that the slime and bat are contained within.
        private Rectangle _roomBounds;
        private SoundEffect _hitSoundEffect;
        private SoundEffect _bounceSoundEffect;

        private Song _themeSong;

        // The SpriteFont Description used to draw text.
        private SpriteFont _font;

        // Defines the position to draw the health text at.
        private Vector2 _healthTextPosition;

        // Defines the origin used when drawing the health text.
        private Vector2 _healthTextOrigin;

        private int _score;

        private Vector2 _scoreTextPosition;

        private Vector2 _scoreTextOrigin;

        private Vector2 _hasKnifeTextPosition;

        private Vector2 _hasKnifeTextOrigin;

        private Sprite _bowSprite;
        private Sprite _arrowSprite;
        private List<Arrow> _arrows;
        private bool _wasSpacePressed; // Fire keybind detection

        public enum GameState { Playing, GameOver }
        public static GameState _state = GameState.Playing;
        
        public string _currentScene;

        public Rooms(string tilemapPath)
        {
            _tilemapPath = tilemapPath;
        }

        public Rooms(Player player, Vector2 playerPosition)
        {
            _player = player;
            _playerPosition = playerPosition;
        }

        public override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slimeSprite = atlas.CreateAnimatedSprite("Slime-animation");

            _knifeSprite = atlas.CreateSprite("Knife");
            _knifeSprite.Scale = new Vector2(0.2f, 0.15f);
            _heartSprite = atlas.CreateSprite("Heart");
            _heartSprite.Scale = new Vector2(0.2f, 0.2f);
            _keySprite = atlas.CreateSprite("Key");
            _keySprite.Scale = new Vector2(0.2f, 0.2f);

            _bowSprite = atlas.CreateSprite("Bow");
            _bowSprite.Scale = new Vector2(2f, 2f);

            _arrowSprite = atlas.CreateSprite("Arrow");
            _arrowSprite.Scale = new Vector2(2f, 2f);

            // Create the tilemap from the XML configuration file.
            _tilemap = Tilemap.FromFile(Content, _tilemapPath);
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

        public override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Restart();
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

            PlayerIntersections();

            if (_player != null && _obstacles != null)
            {
                _player.PlayerInput(_roomBounds, _obstacles, _enemies);

                if (_player.HasBow && _player.Bow != null && _enemies != null && _enemies.Count > 0)
                {
                    // Update bow reload time
                    _player.Bow.Update(gameTime);
                    
                    _player.Bow.UpdateAim(_player._position, _enemies);
                }

                // Keybind check for shooting arrow (space)
                if (_player.HasBow && _player.Bow != null)
                {
                    bool isSpaceDown = Core.Input.Keyboard.IsKeyDown(Keys.Space);

                    if (isSpaceDown)
                    {
                        _player.Bow.StartAiming();
                    }
                    else
                    {
                        if (_player.Bow.IsAiming)
                        {
                            _player.Bow.StopAiming();
                        }
                    }

                    // Check space go up (release) to shoot - только если можно стрелять
                    if (_wasSpacePressed && !isSpaceDown && _player.Bow.CanShoot)
                    {
                        // Create arrow
                        Arrow newArrow = _player.Bow.ShootArrow(_player._position);
                        if (newArrow != null)
                        {
                            newArrow.SetSprite(_arrowSprite);
                            _arrows.Add(newArrow);
                        }
                    }

                    _wasSpacePressed = isSpaceDown;
                }
            }

            for (int i = _arrows.Count - 1; i >= 0; i--)
            {
                _arrows[i].Update();

                // Arrow collision check with enemies
                bool hitEnemy = false;
                for (int j = 0; j < _enemies.Count; j++)
                {
                    if (_arrows[i].CheckCollision(_enemies[j]))
                    {
                        _enemies[j].TakeDamage(25);
                        _score += 50;
                        hitEnemy = true;
                        break;
                    }
                }

                // Delete arrow if hit bounds
                if (hitEnemy || _arrows[i].IsOutOfBounds(_roomBounds))
                {
                    _arrows.RemoveAt(i);
                }
            }

            if (_player != null)
                _playerPosition = new Vector2(_player._position.X, _player._position.Y);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw the tilemap.
            _tilemap?.Draw(Core.SpriteBatch);

            // Draw Enemies
            for (int i = 0; i < _slimePositions.Count; i++)
                _slimeSprite?.Draw(Core.SpriteBatch, _slimePositions[i]);

            // Draw Knifes
            for (int i = 0; i < _knifePositions.Count; i++)
                _knifeSprite?.Draw(Core.SpriteBatch, _knifePositions[i]);

            // Draw Hearts
            for (int i = 0; i < _heartPositions.Count; i++)
                _heartSprite?.Draw(Core.SpriteBatch, _heartPositions[i]);

            // Draw Key
            _keySprite?.Draw(Core.SpriteBatch, _keyPosition);

            // Draw player (before bow and arrows)
            if (_state == GameState.Playing && _playerSprite != null)
            {
                _playerSprite.Draw(Core.SpriteBatch, _playerPosition);
            }

            // Draw Bow
            if (_player != null && _player.HasBow && _player.Bow != null)
            {
                _player.Bow.Draw(Core.SpriteBatch, _playerPosition);
            }

            // Draw Arrows
            foreach (Arrow arrow in _arrows)
            {
                arrow.Draw(Core.SpriteBatch);
            }

            if (_player != null && _player._isShowHitboxes)
            {
                Debug.Print("Pressed T to show Hitboxes");

                Core.DrawRectangleOutline(_player.GetHitbox(), Color.Red);
                for (int i = 0; i < _obstacles.Count; i++)
                    Core.DrawRectangleOutline(_obstacles[i], Color.Red);
                for (int i = 0; i < _slimePositions.Count; i++)
                    Core.DrawRectangleOutline(new Rectangle(
                        (int)_slimePositions[i].X,
                        (int)_slimePositions[i].Y,
                        (int)_slimeSprite.Width,
                        (int)_slimeSprite.Height
                    ), Color.Red
                );
            }

            // Draw UI text (always on top)
            if (_state == GameState.Playing && _player != null)
            {
                // Draw the health
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

                Core.SpriteBatch.DrawString(
                    _font,
                    $"Score: {_score}",
                    _scoreTextPosition,
                    Color.White,
                    0.0f,
                    _scoreTextOrigin,
                    1.5f,
                    SpriteEffects.None,
                    0.0f
                );

                Core.SpriteBatch.DrawString(
                    _font,
                    $"Has Knife: {(hasKnife ? "Yes" : "No")}",
                    new Vector2(_healthTextPosition.X, _healthTextPosition.Y + 40), // Position below health text
                    Color.White,
                    0.0f,
                    new Vector2(0, _healthTextOrigin.Y), // Left align, same Y origin as health text
                    1.5f,
                    SpriteEffects.None,
                    0.0f
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

        //private void AllTurretShoot(object state)
        //{
        //    List<TurretStrategy> turrets = new List<TurretStrategy>();

        //    foreach (Enemy enemy in _enemies)
        //        if (enemy is TurretStrategy)
        //            turrets.Add((TurretStrategy)enemy);

        //    foreach (TurretStrategy turret in turrets)
        //    {
        //        Circle projectile = turret.Shoot();
        //        _projectiles.Add(projectile);
        //    }
        //}

        private void PlayerIntersections()
        {
            Circle playerBounds = new Circle(
                (int)(_playerPosition.X + (_player.HitboxWidth * 0.5f)),
                (int)(_playerPosition.Y + (_player.HitboxHeight * 0.5f)),
                (int)(_player.HitboxWidth * 0.5f)
            );

            if (!hasKnife) // check if already has a knife
            {
                for (int i = 0; i < _knifePositions.Count; i++)
                {
                    Vector2 knifePos = _knifePositions[i];

                    Circle knifeBounds = new Circle(
                        (int)(knifePos.X + _knifeSprite.Width * 0.5f),
                        (int)(knifePos.Y + _knifeSprite.Height * 0.5f),
                        (int)(_knifeSprite.Width * 0.5f)
                    );

                    if (playerBounds.Intersects(knifeBounds))
                    {
                        hasKnife = true;

                        // get rid of the knife
                        _knifePositions[i] = new Vector2(-9999, -9999);

                        // sound effect
                        if (_hitSoundEffect != null)
                            Core.Audio.PlaySoundEffect(_hitSoundEffect);
                    }
                }
            }

            for (int i = 0; i < _heartPositions.Count; i++)
            {
                Vector2 heartPos = _heartPositions[i];

                Circle heartBounds = new Circle(
                    (int)(heartPos.X + _heartSprite.Width * 0.5f),
                    (int)(heartPos.Y + _heartSprite.Height * 0.5f),
                    (int)(_heartSprite.Width * 0.5f)
                );

                if (playerBounds.Intersects(heartBounds))
                {
                    _player.Health.Heal(30);

                    _heartPositions[i] = new Vector2(-9999, -9999);

                    if (_hitSoundEffect != null)
                        Core.Audio.PlaySoundEffect(_hitSoundEffect);
                }
            }

            Vector2 keyPos = _keyPosition;

            Circle keyBounds = new Circle(
                (int)(keyPos.X + _keySprite.Width * 0.5f),
                (int)(keyPos.Y + _keySprite.Height * 0.5f),
                (int)(_keySprite.Width * 0.5f)
            );

            if (playerBounds.Intersects(keyBounds))
            {
                _score += 500;
                _keyPosition = new Vector2(-9999, -9999);
                if (_hitSoundEffect != null)
                    Core.Audio.PlaySoundEffect(_hitSoundEffect);
            }

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

                if (i < _enemies.Count)
                {
                    _enemies[i]._position = newSlimePosition;
                }

                // Rebuild slime bounding circle for overlap/interaction checks
                Circle slimeBounds = new Circle(
                    (int)centerX,
                    (int)centerY,
                    (int)radius
                );

                if (playerBounds.Intersects(slimeBounds))
                {
                    if (!hasKnife)
                        _player.TakeDamage(10);
                    else
                    {
                        _score += 100;
                        hasKnife = false;
                    }

                    // Respawn slime inside playable area leaving exactly one tile margin on each edge.
                    int tileWScaled = (int)Math.Max(1, Math.Round(_tilemap.TileWidth));
                    int tileHScaled = (int)Math.Max(1, Math.Round(_tilemap.TileHeight));

                    int innerCols = Math.Max(1, _tilemap.Columns - 2);
                    int innerRows = Math.Max(1, _tilemap.Rows - 2);

                    int column = Random.Shared.Next(0, innerCols);
                    int row = Random.Shared.Next(0, innerRows);

                    // Position on tile grid inside playable area (roomX + column * tileWScaled)
                    newSlimePosition = new Vector2(_roomBounds.Left + column * tileWScaled, _roomBounds.Top + row * tileHScaled);
                    _slimePositions[i] = newSlimePosition;

                    if (i < _enemies.Count)
                    {
                        _enemies[i]._position = newSlimePosition;
                    }

                    _slimeVelocity.Add(_enemies[i].Move());

                    // Play hit sound effect on player damage
                    if (_hitSoundEffect != null)
                        Core.Audio.PlaySoundEffect(_hitSoundEffect);
                }
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
            Core.ExitOnEscape = false;

            _score = 0;
            hasKnife = false;

            _roomBounds = new Rectangle(
                (int)_tilemap.TileWidth,
                (int)_tilemap.TileHeight,
                (int)(_tilemap.TileWidth * _tilemap.Columns - _tilemap.TileWidth * 2),
                (int)(_tilemap.TileHeight * _tilemap.Rows - _tilemap.TileHeight * 2)
             );

            // Recreate basic player and sprites from content (simple restart).
            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slimeSprite = atlas.CreateAnimatedSprite("Slime-animation");

            _bowSprite = atlas.CreateSprite("Bow");
            _bowSprite.Scale = new Vector2(2f, 2f);

            _arrowSprite = atlas.CreateSprite("Arrow");
            _arrowSprite.Scale = new Vector2(2f, 2f);

            _arrows = new List<Arrow>();
            _wasSpacePressed = false;

            Core.Audio.PlaySong(_themeSong);
            Core.Audio.SongVolume = 0.3f;

            int[] tilesInts = _tilemap.GetTilesIDs();

            _obstaclesTileIDs = new List<int>
            {
                03,
                04,
                07,
                08,
                11,
                59,
                63,
                64
            };

            _obstacles = new List<Rectangle>();

            for (int i = 0; i < tilesInts.Length; i++)
            {
                if (_obstaclesTileIDs.Contains(tilesInts[i]))
                {
                    int x = i % _tilemap.Columns;
                    int y = (int)Math.Floor((double)(i / _tilemap.Columns));

                    _obstacles.Add(new Rectangle(
                                (int)(x * _tilemap.TileWidth),
                                (int)(y * _tilemap.TileHeight),
                                (int)_tilemap.TileWidth,
                                (int)_tilemap.TileHeight
                    ));
                }
            }

            Vector2 safePlayerPosition = FindSafePosition(_roomBounds, _obstacles, (int)_playerSprite.Width, (int)_playerSprite.Height);

            _player = new Player("Player", 100, (int)safePlayerPosition.X, (int)safePlayerPosition.Y, 1, _playerSprite);

            _playerPosition = new Vector2(_player._position.X, _player._position.Y);

            _player.EquipBow(_bowSprite);

            _enemies = new List<Enemy>
    {
        new LurkingStrategy(100, 100, 5, 5),
        new LurkingStrategy(100, 100, 5, 5),
    };
            _slimePositions = new List<Vector2>();
            _slimeVelocity = new List<Vector2>();

            for (int i = 0; i < _enemies.Count; i++)
            {
                _slimePositions.Add(new Vector2(_enemies[i]._position.X, _enemies[i]._position.Y));
                _slimeVelocity.Add(_enemies[i].Move());
            }

            _knifePositions = new List<Vector2>
            {
                new Vector2(200, 100),
                new Vector2(300, 100)
            };
            _heartPositions = new List<Vector2>
            {
                new Vector2(500, 100),
                new Vector2(500, 500)
            };
            _keyPosition = new Vector2(800, 400);

            // Set the position of the score text to align to the left edge of the
            // room bounds, and to vertically be at the center of the first tile.
            _healthTextPosition = new Vector2(_roomBounds.Left, _tilemap.TileHeight * 0.5f);

            // Set the origin of the text so it is left-centered.
            float healthTextYOrigin = _font.MeasureString("Health").Y * 0.5f;
            _healthTextOrigin = new Vector2(0, healthTextYOrigin);

            _scoreTextPosition = new Vector2(_roomBounds.Right - 300, _tilemap.TileHeight * 0.5f);

            float scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
            _scoreTextOrigin = new Vector2(0, scoreTextYOrigin);

            _hasKnifeTextPosition = new Vector2(_healthTextPosition.X, _healthTextPosition.Y + 40);

            float hasKnifeTextYOrigin = _font.MeasureString("Has Knife").Y * 0.5f;
            _hasKnifeTextOrigin = new Vector2(0, hasKnifeTextYOrigin);

            _state = GameState.Playing;
        }

        private Vector2 FindSafePosition(Rectangle roomBounds, List<Rectangle> obstacles, int entityWidth, int entityHeight)
        {
            int centerX = roomBounds.Left + (roomBounds.Width / 2) - (entityWidth / 2);
            int centerY = roomBounds.Top + (roomBounds.Height / 2) - (entityHeight / 2);

            Rectangle testRect = new Rectangle(centerX, centerY, entityWidth, entityHeight);
    
            // Check if center is safe to spawn
            bool isSafe = true;
            foreach (Rectangle obstacle in obstacles)
            {
                if (testRect.Intersects(obstacle))
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe)
            {
                return new Vector2(centerX, centerY);
            }

            return new Vector2(roomBounds.Left + 10, roomBounds.Top + 10);
        }
    }
}
