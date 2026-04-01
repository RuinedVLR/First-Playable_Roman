using First_Playable_Roman.Scripts;
using First_Playable_Roman.Scripts.Items;
using First_Playable_Roman.Scripts.Movements;
using First_Playable_Roman.Scripts.Strategies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace First_Playable_Roman.Scenes
{
    public abstract class Room : Scene
    {
        private AnimatedSprite _playerSprite;

        private AnimatedSprite _slimeSprite;
        private AnimatedSprite _chaserSprite;
        private Sprite _turretSprite;

        protected Sprite _knifeSprite;
        protected Sprite _heartSprite;
        protected Sprite _keySprite;

        protected List<KnifeItem> _knives;
        protected List<HeartItem> _hearts;
        protected KeyItem _key;

        protected Player _player;
        protected Vector2 _playerPosition;

        protected List<Enemy> _enemies;

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

        protected int _score;

        private Vector2 _scoreTextPosition;

        private Vector2 _scoreTextOrigin;

        private Vector2 _hasKnifeTextPosition;

        private Vector2 _hasKnifeTextOrigin;

        private Sprite _bowSprite;
        private Sprite _arrowSprite;
        private List<Arrow> _arrows;
        private bool _wasSpacePressed; // Fire keybind detection

        public enum GameState { Playing, GameOver, GameWin }
        public static GameState _state = GameState.Playing;
        
        public string _currentScene;

        // Indicates whether an existing player was passed in
        private bool _hasExistingPlayer;

        // Tracks whether a key has already been dropped this session
        private bool _keyHasDropped;

        // Score passed from the previous room.
        private int _initialScore;

        public Room(string tilemapPath)
        {
            _tilemapPath = tilemapPath;
            _hasExistingPlayer = false;
        }

        public Room(string tilemapPath, Player player, Vector2 playerPosition, int score = 0)
        {
            _tilemapPath = tilemapPath;
            _player = player;
            _playerPosition = playerPosition;
            _hasExistingPlayer = true;
            _initialScore = score;
        }

        // Abstract method to be implemented by child classes
        protected abstract void InitializeItems();

        protected abstract void InitializeEnemies();

        
        public void SpawnEnemyDrop(Vector2 dropPosition)
        {
            // Key drop: 100% if score >= 1000 and key hasn't been dropped yet
            if (_score >= 1000 && !_keyHasDropped)
            {
                _key = new KeyItem(dropPosition, _keySprite);
                _keyHasDropped = true;
            }

            // Knife drop: 20% chance
            if (Random.Shared.NextDouble() < 0.30 && Random.Shared.NextDouble() > 0.10)
            {
                _knives.Add(new KnifeItem(dropPosition, _knifeSprite));
            }

            // Heart drop: 10% chance
            if (Random.Shared.NextDouble() < 0.10)
            {
                _hearts.Add(new HeartItem(dropPosition, _heartSprite, 30));
            }
        }

        public override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slimeSprite = atlas.CreateAnimatedSprite("Slime-animation");

            _chaserSprite = atlas.CreateAnimatedSprite("Chaser-animation");

            // Create a static sprite for turret enemies (uses first slime frame as base)
            _turretSprite = atlas.CreateSprite("Turret");
            _turretSprite.Scale = new Vector2(2f, 2f);

            _knifeSprite = atlas.CreateSprite("Knife");
            _heartSprite = atlas.CreateSprite("Heart");
            _keySprite = atlas.CreateSprite("Key");

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

            Core.ExitOnEscape = false;

            _roomBounds = new Rectangle(
                (int)_tilemap.TileWidth,
                (int)_tilemap.TileHeight,
                (int)(_tilemap.TileWidth * _tilemap.Columns - _tilemap.TileWidth * 2),
                (int)(_tilemap.TileHeight * _tilemap.Rows - _tilemap.TileHeight * 2)
             );

            // Recreate sprites from content (needed for both new and existing players).
            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slimeSprite = atlas.CreateAnimatedSprite("Slime-animation");

            _chaserSprite = atlas.CreateAnimatedSprite("Chaser-animation");

            _turretSprite = atlas.CreateSprite("Turret");
            _turretSprite.Scale = new Vector2(2f, 2f);

            _knifeSprite = atlas.CreateSprite("Knife");
            _heartSprite = atlas.CreateSprite("Heart");
            _keySprite = atlas.CreateSprite("Key");

            _bowSprite = atlas.CreateSprite("Bow");
            _bowSprite.Scale = new Vector2(2f, 2f);

            _arrowSprite = atlas.CreateSprite("Arrow");
            _arrowSprite.Scale = new Vector2(2f, 2f);

            _arrows = new List<Arrow>();
            _wasSpacePressed = false;

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

            if (_hasExistingPlayer && _player != null)
            {
                // Reuse the existing player
                // but keep health, knife, bow, and other state intact.
                _player.Sprite = _playerSprite;

                // Apply the saved spawn position to the player object.
                _player._position = _playerPosition;

                // Re-equip bow with the new sprite if the player already has one.
                if (_player.HasBow)
                {
                    _player.EquipBow(_bowSprite);
                }

                // Restore the score from the previous room.
                _score = _initialScore;

                // After the first Restart call, clear the flag so that pressing R
                // to restart after Game Over creates a fresh player.
                _hasExistingPlayer = false;
            }
            else
            {
                // No existing player — create a brand-new one (first room or Game Over restart).
                _score = 0;

                Vector2 safePlayerPosition = FindSafePosition(_roomBounds, _obstacles, (int)_playerSprite.Width, (int)_playerSprite.Height);

                _player = new Player("Player", 100, (int)safePlayerPosition.X, (int)safePlayerPosition.Y, 1, _playerSprite);

                _playerPosition = new Vector2(_player._position.X, _player._position.Y);

                _player.EquipBow(_bowSprite);
            }

            _slimePositions = new List<Vector2>();
            _slimeVelocity = new List<Vector2>();

            // Initialize empty item lists (items drop from enemies)
            _knives = new List<KnifeItem>();
            _hearts = new List<HeartItem>();
            _key = null;
            _keyHasDropped = false;

            // Call the abstract method to initialize room-specific settings
            InitializeItems();
            InitializeEnemies();

            // Properly position and initialize enemies, assign sprites per strategy
            for (int i = 0; i < _enemies.Count; i++)
            {
                // Assign sprite based on enemy type
                if (_enemies[i] is TurretStrategy)
                {
                    _enemies[i].SetStaticSprite(_turretSprite);

                    // Center turret position so the sprite is visually centered on its coordinates
                    _enemies[i]._position.X -= _enemies[i].SpriteWidth * 0.5f;
                    _enemies[i]._position.Y -= _enemies[i].SpriteHeight * 0.5f;
                }
                else if (_enemies[i] is ChaserStrategy)
                {
                    _enemies[i].SetAnimatedSprite(_chaserSprite);
                    // Use Respawn to set enemy position in a safe random location
                    _enemies[i].Respawn(_roomBounds, _tilemap.TileWidth, _tilemap.TileHeight, _tilemap.Columns, _tilemap.Rows);
                }
                else
                {
                    _enemies[i].SetAnimatedSprite(_slimeSprite);

                    // Use Respawn to set enemy position in a safe random location
                    _enemies[i].Respawn(_roomBounds, _tilemap.TileWidth, _tilemap.TileHeight, _tilemap.Columns, _tilemap.Rows);
                }

                _slimePositions.Add(new Vector2(_enemies[i]._position.X, _enemies[i]._position.Y));
                _slimeVelocity.Add(_enemies[i].Move());
            }

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

            //Core.ChangeScene(new Room1("images/room1-definition.xml"));

            _state = GameState.Playing;
        }

        public override void Update(GameTime gameTime)
        {
            // Ensure core / input state is updated before we inspect input.
            base.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Core.ChangeScene(new TitleScene());

            if (_state == GameState.Playing)
            {
                Core.Instance.Window.Title = "Slime Slasher";
            }
            else if (_state == GameState.GameOver)
            {
                Core.Instance.Window.Title = "Press R to Restart";
            }
            else if (_state == GameState.GameWin)
            {
                Core.Instance.Window.Title = "You win!";
            }

            _playerSprite?.Update(gameTime);

            // Update all enemy sprites
            if (_enemies != null)
            {
                foreach (Enemy enemy in _enemies)
                {
                    enemy.UpdateSprite(gameTime);
                }
            }

            // If game over, allow restart and skip gameplay updates
            if (_state == GameState.GameOver || _state == GameState.GameWin)
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

            if (_player != null && _player.HasKey)
            {
                GameWin();
                return;
            }

            // Check room transitions
            CheckRoomTransitions();

            // Update chaser enemies with player position
            if (_enemies != null && _player != null)
            {
                Vector2 playerCenter = new Vector2(
                    _playerPosition.X + _player.HitboxWidth * 0.5f,
                    _playerPosition.Y + _player.HitboxHeight * 0.5f
                );

                for (int i = 0; i < _enemies.Count; i++)
                {
                    if (_enemies[i] is ChaserStrategy chaser && chaser.IsActive)
                    {
                        chaser.UpdateTarget(playerCenter);

                        // When chasing, override velocity every frame to track the player
                        if (chaser.IsChasing)
                        {
                            _slimeVelocity[i] = chaser.Move();
                        }
                    }
                }
            }

            // Delegate intersection checks to the player and accumulate score.
            _score += _player.CheckIntersections(
                _playerPosition,
                _knives,
                _hearts,
                _key,
                _enemies,
                _slimePositions,
                _slimeVelocity,
                _slimeSprite,
                _roomBounds,
                _tilemap,
                _hitSoundEffect,
                _obstacles);

            if (_player != null && _obstacles != null)
            {
                _player.PlayerInput(_obstacles);

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

                    // Check space go up (release) to shoot
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
                    if (_enemies[j].IsActive && _arrows[i].CheckCollision(_enemies[j]))
                    {
                        // Save position before damage (enemy may deactivate)
                        Vector2 enemyDeathPos = _slimePositions[j];

                        _enemies[j].TakeDamage(25);

                        // If enemy died, spawn drops and respawn it
                        if (!_enemies[j].IsActive)
                        {
                            SpawnEnemyDrop(enemyDeathPos);

                            _enemies[j].Respawn(_roomBounds, _tilemap.TileWidth, _tilemap.TileHeight, _tilemap.Columns, _tilemap.Rows);
                            _slimePositions[j] = _enemies[j]._position;
                            _slimeVelocity[j] = _enemies[j].Move();
                        }

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

            // Update turret enemies and check projectile collisions with player
            if (_enemies != null && _player != null)
            {
                Circle playerBounds = new Circle(
                    (int)(_playerPosition.X + (_player.HitboxWidth * 0.5f)),
                    (int)(_playerPosition.Y + (_player.HitboxHeight * 0.5f)),
                    (int)(_player.HitboxWidth * 0.5f)
                );

                foreach (Enemy enemy in _enemies)
                {
                    if (enemy is TurretStrategy turret && turret.IsActive)
                    {
                        turret.Update(gameTime);
                        turret.CleanupProjectiles(_roomBounds);

                        if (turret.CheckProjectileHit(playerBounds))
                        {
                            _player.TakeDamage(15);

                            if (_hitSoundEffect != null)
                                Core.Audio.PlaySoundEffect(_hitSoundEffect);
                        }
                    }
                }
            }

            if (_player != null)
                _playerPosition = new Vector2(_player._position.X, _player._position.Y);
        }

        // Abstract method for room-specific transitions
        protected abstract void CheckRoomTransitions();

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw the tilemap.
            _tilemap?.Draw(Core.SpriteBatch);

            // Draw Enemies (each enemy draws its own sprite)
            if (_enemies != null)
            {
                foreach (Enemy enemy in _enemies)
                {
                    enemy.Draw(Core.SpriteBatch);
                }
            }

            // Draw Items
            if (_knives != null)
            {
                foreach (KnifeItem knife in _knives)
                    knife.Draw();
            }

            if (_hearts != null)
            {
                foreach (HeartItem heart in _hearts)
                    heart.Draw();
            }

            _key?.Draw();

            // Draw player
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

                // Draw player hitbox
                Core.DrawRectangleOutline(_player.GetHitbox(), Color.Red);

                // Draw obstacles hitboxes
                for (int i = 0; i < _obstacles.Count; i++)
                    Core.DrawRectangleOutline(_obstacles[i], Color.Red);

                // Draw enemy hitboxes
                if (_enemies != null)
                {
                    foreach (Enemy enemy in _enemies)
                    {
                        if (enemy.IsActive)
                        {
                            Core.DrawRectangleOutline(new Rectangle(
                                (int)enemy._position.X,
                                (int)enemy._position.Y,
                                (int)enemy.SpriteWidth,
                                (int)enemy.SpriteHeight
                            ), Color.Red);

                            // Draw turret projectile hitboxes
                            if (enemy is TurretStrategy turret)
                            {
                                foreach (TurretProjectile proj in turret.Projectiles)
                                {
                                    Core.DrawRectangleOutline(proj.GetBounds(), Color.OrangeRed);
                                }
                            }

                            // Draw chaser detection radius
                            if (enemy is ChaserStrategy chaser)
                            {
                                Vector2 center = new Vector2(
                                    enemy._position.X + enemy.SpriteWidth * 0.5f,
                                    enemy._position.Y + enemy.SpriteHeight * 0.5f
                                );
                                int radius = (int)chaser.DetectionRadius;
                                Core.DrawRectangleOutline(new Rectangle(
                                    (int)(center.X - radius),
                                    (int)(center.Y - radius),
                                    radius * 2,
                                    radius * 2
                                ), chaser.IsChasing ? Color.Red : Color.Cyan);
                            }
                        }
                    }
                }

                // Draw knife hitboxes
                if (_knives != null)
                {
                    foreach (KnifeItem knife in _knives)
                    {
                        if (!knife.IsCollected)
                        {
                            Core.DrawRectangleOutline(knife.GetBounds(), Color.Yellow);
                        }
                    }
                }

                // Draw heart hitboxes
                if (_hearts != null)
                {
                    foreach (HeartItem heart in _hearts)
                    {
                        if (!heart.IsCollected)
                        {
                            Core.DrawRectangleOutline(heart.GetBounds(), Color.LightGreen);
                        }
                    }
                }

                // Draw key hitbox
                if (_key != null && !_key.IsCollected)
                {
                    Core.DrawRectangleOutline(_key.GetBounds(), Color.Blue);
                }

                // Draw arrow hitboxes
                foreach (Arrow arrow in _arrows)
                {
                    if (arrow.IsActive)
                    {
                        Core.DrawRectangleOutline(arrow.GetBounds(), Color.Orange);
                    }
                }
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
                    $"Has Knife: {(_player.HasKnife ? "Yes" : "No")}",
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
                    "Game Over - press ESC to Exit",
                    new Vector2(Core.GraphicsDevice.Viewport.Width * 0.5f, Core.GraphicsDevice.Viewport.Height * 0.3f),
                    Color.Gold,
                    0.0f,
                    _font.MeasureString("Game Over - press ESC to Exit") * 0.5f,
                    1.0f,
                    SpriteEffects.None,
                    0.0f
                );
            }
            else if (_state == GameState.GameWin)
            {
                Core.SpriteBatch.DrawString(
                    _font,
                    "You won! Press ESC to Exit",
                    new Vector2(Core.GraphicsDevice.Viewport.Width * 0.5f, Core.GraphicsDevice.Viewport.Height * 0.3f),
                    Color.Aqua,
                    0.0f,
                    _font.MeasureString("You won! Press ESC to Exit") * 0.5f,
                    1.0f,
                    SpriteEffects.None,
                    0.0f
                );
            }

            Core.SpriteBatch.End();
            base.Draw(gameTime);
        }

        public void AddScore(int points)
        {
            _score += points;
        }

        private void GameOver()
        {
            if (_state == GameState.GameOver) return;

            // Switch state
            _state = GameState.GameOver;

            // Stop music
            Core.Audio.PauseAudio();

            // Clear player references
            _player = null;
            _playerSprite = null;

            // Freeze slime
            for(int i = 0; i < _slimeVelocity.Count; i++)
                _slimeVelocity[i] = Vector2.Zero;

            // Play hit sound
            if (_hitSoundEffect != null)
                Core.Audio.PlaySoundEffect(_hitSoundEffect);
        }

        private void GameWin()
        {
            if(_state == GameState.GameWin) return;

            _state = GameState.GameWin;

            Core.Audio.PauseAudio();

            _player = null;
            _playerSprite = null;

            for (int i = 0; i < _slimeVelocity.Count; i++)
                _slimeVelocity[i] = Vector2.Zero;
        }

        private void Restart()
        {
            
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
