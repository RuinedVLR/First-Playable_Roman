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
        private SoundEffect _roomClearEffect;
        private SoundEffect _bowShootEffect;
        private SoundEffect _enemyKillEffect;

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

        // Enemy kill counter: how many non-turret enemies still need to be killed.
        private int _enemyKillsRemaining;
        private Vector2 _killCounterTextPosition;

        // True when all enemies in this room have been defeated.
        // Passed in via constructor when returning to an already-cleared room.
        protected bool _isCleared;

        private Sprite _bowSprite;
        private Sprite _arrowSprite;
        private List<Arrow> _arrows;
        private bool _wasSpacePressed;

        public enum GameState { Playing, GameOver, GameWin }
        public static GameState _state = GameState.Playing;

        public string _currentScene;

        private bool _hasExistingPlayer;
        private bool _keyHasDropped;
        private int _initialScore;

        // Total number of rooms in the game.
        private const int TotalRooms = 5;

        // Global counter of how many rooms have been cleared across the whole run.
        public static int ClearedRoomsCount = 0;

        // True when every room has been cleared and the key should be available.
        public static bool AllRoomsCleared => ClearedRoomsCount >= TotalRooms;

        public Room(string tilemapPath)
        {
            _tilemapPath = tilemapPath;
            _hasExistingPlayer = false;
            _isCleared = false;
        }

        public Room(string tilemapPath, Player player, Vector2 playerPosition, int score = 0, bool isCleared = false)
        {
            _tilemapPath = tilemapPath;
            _player = player;
            _playerPosition = playerPosition;
            _hasExistingPlayer = true;
            _initialScore = score;
            _isCleared = isCleared;
        }

        protected abstract void InitializeItems();
        protected abstract void InitializeEnemies();
        protected abstract void CheckRoomTransitions();

        protected virtual int GetEnemyKillGoal() => 10;

        protected virtual List<int> GetObstacleTileIDs()
        {
            return new List<int> { 03, 04, 07, 08, 11, 59, 63, 64 };
        }

        // True when the room is cleared and transitions are unlocked.
        public bool IsCleared => _isCleared;

        public void OnEnemyKilled()
        {
            if (_enemyKillsRemaining > 0)
            {
                _enemyKillsRemaining--;

                Core.Audio.PlaySoundEffect(_enemyKillEffect);

                if (_enemyKillsRemaining == 0)
                {
                    _isCleared = true;

                    Core.Audio.PlaySoundEffect(_roomClearEffect);

                    // Track how many unique rooms have been cleared globally
                    ClearedRoomsCount++;

                    if (_enemies != null)
                    {
                        foreach (Enemy enemy in _enemies)
                            enemy.IsActive = false;
                    }

                    for (int i = 0; i < _slimeVelocity.Count; i++)
                        _slimeVelocity[i] = Vector2.Zero;

                    // Spawn key in the center of the room when all rooms are cleared
                    if (AllRoomsCleared && !_keyHasDropped)
                    {
                        Vector2 center = new Vector2(
                            _roomBounds.Left + _roomBounds.Width * 0.5f,
                            _roomBounds.Top + _roomBounds.Height * 0.5f
                        );
                        _key = new KeyItem(center, _keySprite);
                        _keyHasDropped = true;
                    }
                }
            }
        }

        public void SpawnEnemyDrop(Vector2 dropPosition)
        {
            double roll = Random.Shared.NextDouble();

            // 10% knife, 10% heart, mutually exclusive
            if (roll < 0.10)
                _knives.Add(new KnifeItem(dropPosition, _knifeSprite));
            else if (roll < 0.20)
                _hearts.Add(new HeartItem(dropPosition, _heartSprite, 30));
        }

        public override void LoadContent()
        {
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

            _tilemap = Tilemap.FromFile(Content, _tilemapPath);
            _tilemap.Scale = new Vector2(4.0f, 4.0f);

            _bounceSoundEffect = Content.Load<SoundEffect>("audio/bounceSoundEffect");
            _hitSoundEffect = Content.Load<SoundEffect>("audio/pixelhitsound");
            _roomClearEffect = Content.Load<SoundEffect>("audio/roomClearSoundEffect");
            _bowShootEffect = Content.Load<SoundEffect>("audio/bowShoot");
            _enemyKillEffect = Content.Load<SoundEffect>("audio/deathSoundEffect");

            _themeSong = Content.Load<Song>("audio/backgroundMusic");
            _font = Content.Load<SpriteFont>("fonts/04B_30");
        }

        public override void Initialize()
        {
            base.Initialize();

            Core.ExitOnEscape = false;

            _roomBounds = new Rectangle(
                (int)_tilemap.TileWidth,
                (int)_tilemap.TileHeight,
                (int)(_tilemap.TileWidth * _tilemap.Columns - _tilemap.TileWidth * 2),
                (int)(_tilemap.TileHeight * _tilemap.Rows - _tilemap.TileHeight * 2)
             );

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

            Core.Audio.SongVolume = 0.1f;

            int[] tilesInts = _tilemap.GetTilesIDs();

            _obstaclesTileIDs = GetObstacleTileIDs();

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
                _player.Sprite = _playerSprite;
                _player._position = _playerPosition;

                if (_player.HasBow)
                    _player.EquipBow(_bowSprite, _bowShootEffect);

                _score = _initialScore;
                _hasExistingPlayer = false;
            }
            else
            {
                _score = 0;

                // New game: reset the global cleared rooms counter
                ClearedRoomsCount = 0;

                Vector2 safePlayerPosition = FindSafePosition(_roomBounds, _obstacles, (int)_playerSprite.Width, (int)_playerSprite.Height);

                _player = new Player("Player", 100, (int)safePlayerPosition.X, (int)safePlayerPosition.Y, 1, _playerSprite);
                _playerPosition = new Vector2(_player._position.X, _player._position.Y);
                _player.EquipBow(_bowSprite, _bowShootEffect);
            }

            _slimePositions = new List<Vector2>();
            _slimeVelocity = new List<Vector2>();

            _knives = new List<KnifeItem>();
            _hearts = new List<HeartItem>();
            _key = null;
            _keyHasDropped = false;

            if (_isCleared)
            {
                // Room already cleared: skip enemies entirely
                _enemyKillsRemaining = 0;
                _enemies = new List<Enemy>();
            }
            else
            {
                // Fresh room: set the kill goal and spawn enemies
                _enemyKillsRemaining = GetEnemyKillGoal();

                InitializeItems();
                InitializeEnemies();

                for (int i = 0; i < _enemies.Count; i++)
                {
                    if (_enemies[i] is TurretStrategy turretEnemy)
                    {
                        _enemies[i].SetStaticSprite(_turretSprite);
                        _enemies[i]._position.X -= _enemies[i].SpriteWidth * 0.5f;
                        _enemies[i]._position.Y -= _enemies[i].SpriteHeight * 0.5f;
                        turretEnemy.SetProjectileSprite(_arrowSprite);
                    }
                    else if (_enemies[i] is ChaserStrategy)
                    {
                        _enemies[i].SetAnimatedSprite(_chaserSprite);
                        _enemies[i].Respawn(_roomBounds, _tilemap.TileWidth, _tilemap.TileHeight, _tilemap.Columns, _tilemap.Rows, _obstacles, _playerPosition);
                    }
                    else
                    {
                        _enemies[i].SetAnimatedSprite(_slimeSprite);
                        _enemies[i].Respawn(_roomBounds, _tilemap.TileWidth, _tilemap.TileHeight, _tilemap.Columns, _tilemap.Rows, _obstacles, _playerPosition);
                    }

                    _slimePositions.Add(new Vector2(_enemies[i]._position.X, _enemies[i]._position.Y));
                    _slimeVelocity.Add(_enemies[i].Move());
                }
            }

            _healthTextPosition = new Vector2(_roomBounds.Left, _tilemap.TileHeight * 0.5f);
            float healthTextYOrigin = _font.MeasureString("Health").Y * 0.5f;
            _healthTextOrigin = new Vector2(0, healthTextYOrigin);

            _scoreTextPosition = new Vector2(_roomBounds.Right - 300, _tilemap.TileHeight * 0.5f);
            float scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
            _scoreTextOrigin = new Vector2(0, scoreTextYOrigin);

            _hasKnifeTextPosition = new Vector2(_healthTextPosition.X, _healthTextPosition.Y + 40);

            _killCounterTextPosition = new Vector2(Core.GraphicsDevice.Viewport.Width * 0.5f, _tilemap.TileHeight * 0.5f);

            _state = GameState.Playing;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Core.ChangeScene(new TitleScene());

            if (_state == GameState.Playing)
                Core.Instance.Window.Title = "Slime Slasher";
            else if (_state == GameState.GameOver)
                Core.Instance.Window.Title = "Press R to Restart";
            else if (_state == GameState.GameWin)
                Core.Instance.Window.Title = "You win!";

            _playerSprite?.Update(gameTime);

            if (_enemies != null)
            {
                foreach (Enemy enemy in _enemies)
                    enemy.UpdateSprite(gameTime);
            }

            if (_state == GameState.GameOver || _state == GameState.GameWin)
            {
                if (Core.Input.Keyboard.WasKeyJustPressed(Keys.R))
                    Restart();
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

            // Tick down invincibility timer every frame
            _player?.UpdateInvincibility(gameTime);

            // Room transitions are only available when the room is cleared
            if (_isCleared)
                CheckRoomTransitions();

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

                        if (chaser.IsChasing)
                            _slimeVelocity[i] = chaser.Move();
                    }
                }
            }

            // Pass _isCleared so knife kills respect the cleared state
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
                _obstacles,
                _isCleared);

            if (_player != null && _obstacles != null)
            {
                // Pass roomBounds and _isCleared for barrier clamping
                _player.PlayerInput(_obstacles, _roomBounds, _isCleared);

                if (_player.HasBow && _player.Bow != null && _enemies != null && _enemies.Count > 0)
                {
                    _player.Bow.Update(gameTime);
                    _player.Bow.UpdateAim(_player._position, _enemies);
                }

                if (_player.HasBow && _player.Bow != null)
                {
                    bool isSpaceDown = Core.Input.Keyboard.IsKeyDown(Keys.Space);

                    if (isSpaceDown)
                        _player.Bow.StartAiming();
                    else
                        _player.Bow.StopAiming();

                    if (_player.Bow.HasPendingShot)
                    {
                        Arrow newArrow = _player.Bow.ShootArrow(_player._position);
                        if (newArrow != null)
                        {
                            newArrow.SetSprite(_arrowSprite);
                            _arrows.Add(newArrow);
                        }
                    }
                }
            }

            for (int i = _arrows.Count - 1; i >= 0; i--)
            {
                _arrows[i].Update();

                bool hitEnemy = false;
                for (int j = 0; j < _enemies.Count; j++)
                {
                    if (_enemies[j].IsActive && _arrows[i].CheckCollision(_enemies[j]))
                    {
                        Vector2 enemyDeathPos = _slimePositions[j];

                        _enemies[j].TakeDamage(25);

                        if (!_enemies[j].IsActive)
                        {
                            SpawnEnemyDrop(enemyDeathPos);

                            // Only respawn if room is not yet cleared
                            if (!_isCleared)
                            {
                                _enemies[j].Respawn(_roomBounds, _tilemap.TileWidth, _tilemap.TileHeight, _tilemap.Columns, _tilemap.Rows, _obstacles, _playerPosition);
                                _slimePositions[j] = _enemies[j]._position;
                                _slimeVelocity[j] = _enemies[j].Move();
                            }
                        }

                        hitEnemy = true;
                        break;
                    }
                }

                if (hitEnemy || _arrows[i].IsOutOfBounds(_roomBounds))
                    _arrows.RemoveAt(i);
            }

            if (_enemies != null && _player != null)
            {
                Circle playerBounds = new Circle(
                    (int)(_playerPosition.X + (_player.HitboxWidth * 0.5f)),
                    (int)(_playerPosition.Y + (_player.HitboxHeight * 0.5f)),
                    (int)(_player.HitboxWidth * 0.5f)
                );

                foreach (Enemy enemy in _enemies)
                {
                    if (enemy is TurretStrategy turret)
                    {
                        // Always update the turret so it can clear projectiles when inactive
                        turret.Update(gameTime);

                        if (turret.IsActive)
                        {
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
            }

            if (_player != null)
                _playerPosition = new Vector2(_player._position.X, _player._position.Y);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _tilemap?.Draw(Core.SpriteBatch);

            if (_enemies != null)
            {
                foreach (Enemy enemy in _enemies)
                    enemy.Draw(Core.SpriteBatch);
            }

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

            // Draw player — skip when blinking during invincibility
            if (_state == GameState.Playing && _playerSprite != null && (_player == null || _player.IsVisible))
                _playerSprite.Draw(Core.SpriteBatch, _playerPosition);

            if (_player != null && _player.HasBow && _player.Bow != null)
                _player.Bow.Draw(Core.SpriteBatch, _playerPosition);

            foreach (Arrow arrow in _arrows)
                arrow.Draw(Core.SpriteBatch);

            if (_player != null && _player._isShowHitboxes)
            {
                Debug.Print("Pressed T to show Hitboxes");

                Core.DrawRectangleOutline(_player.GetHitbox(), Color.Red);

                for (int i = 0; i < _obstacles.Count; i++)
                    Core.DrawRectangleOutline(_obstacles[i], Color.Red);

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

                            if (enemy is TurretStrategy turret)
                            {
                                foreach (TurretProjectile proj in turret.Projectiles)
                                    Core.DrawRectangleOutline(proj.GetBounds(), Color.OrangeRed);
                            }

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

                if (_knives != null)
                {
                    foreach (KnifeItem knife in _knives)
                    {
                        if (!knife.IsCollected)
                            Core.DrawRectangleOutline(knife.GetBounds(), Color.Yellow);
                    }
                }

                if (_hearts != null)
                {
                    foreach (HeartItem heart in _hearts)
                    {
                        if (!heart.IsCollected)
                            Core.DrawRectangleOutline(heart.GetBounds(), Color.LightGreen);
                    }
                }

                if (_key != null && !_key.IsCollected)
                    Core.DrawRectangleOutline(_key.GetBounds(), Color.Blue);

                foreach (Arrow arrow in _arrows)
                {
                    if (arrow.IsActive)
                        Core.DrawRectangleOutline(arrow.GetBounds(), Color.Orange);
                }
            }

            if (_state == GameState.Playing && _player != null)
            {
                Core.SpriteBatch.DrawString(
                    _font,
                    $"Health: {_player.Health.CurrentHealth}",
                    _healthTextPosition,
                    Color.White,
                    0.0f,
                    _healthTextOrigin,
                    1.5f,
                    SpriteEffects.None,
                    0.0f
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
                    new Vector2(_healthTextPosition.X, _healthTextPosition.Y + 40),
                    Color.White,
                    0.0f,
                    new Vector2(0, _healthTextOrigin.Y),
                    1.5f,
                    SpriteEffects.None,
                    0.0f
                );

                string killText = _isCleared ? "Cleared!" : $"Enemies: {_enemyKillsRemaining}";
                Color killColor = _isCleared ? Color.LimeGreen : Color.White;

                Core.SpriteBatch.DrawString(
                    _font,
                    killText,
                    _killCounterTextPosition,
                    killColor,
                    0.0f,
                    _font.MeasureString(killText) * 0.5f,
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

            _state = GameState.GameOver;
            Core.Audio.PauseAudio();
            _player = null;
            _playerSprite = null;

            for (int i = 0; i < _slimeVelocity.Count; i++)
                _slimeVelocity[i] = Vector2.Zero;

            if (_hitSoundEffect != null)
                Core.Audio.PlaySoundEffect(_hitSoundEffect);
        }

        private void GameWin()
        {
            if (_state == GameState.GameWin) return;

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
                return new Vector2(centerX, centerY);

            return new Vector2(roomBounds.Left + 10, roomBounds.Top + 10);
        }
    }
}