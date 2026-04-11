using First_Playable_Roman.Scripts;
using First_Playable_Roman.Scripts.Movements;
using First_Playable_Roman.Scripts.Strategies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System.Collections.Generic;

namespace First_Playable_Roman.Scenes
{
    public class Room5 : Room
    {
        public Room5(string tilemapPath) : base(tilemapPath) { }

        public Room5(string tilemapPath, Player player, Vector2 playerPosition, int score = 0, bool isCleared = false)
            : base(tilemapPath, player, playerPosition, score, isCleared) { }

        private Tilemap _tilemap;
        private Song _themeSong;
        private SpriteFont _font;

        // Room5: defeat 15 enemies to clear
        protected override int GetEnemyKillGoal() => 15;

        // Maze theme: only solid walls as obstacles
        protected override List<int> GetObstacleTileIDs()
        {
            return new List<int> { 08, 59, 69, 70, 90 };
        }

        protected override void InitializeItems() { }

        protected override void InitializeEnemies()
        {
            int centerX = (int)(_tilemap.TileWidth * _tilemap.Columns * 0.5f);
            int centerY = (int)(_tilemap.TileHeight * _tilemap.Rows * 0.5f);

            _enemies = new List<Enemy>
            {
                new LurkingStrategy(0, 0, 5, this),
                new LurkingStrategy(0, 0, 5, this),
                new ChaserStrategy(0, 0, 3, 200f, this),
                new ChaserStrategy(0, 0, 3, 200f, this),
                new TurretStrategy(centerX, centerY, this),
            };
        }

        protected override void CheckRoomTransitions()
        {
            if (_player == null)
                return;

            // Left → Room4 (already cleared to reach Room5)
            float leftThreshold = 50;

            if (_player._position.X < leftThreshold)
            {
                Vector2 newPosition = new Vector2(Core.GraphicsDevice.Viewport.Width - 100, _player._position.Y);
                Core.ChangeScene(new Room4("images/room4-definition.xml", _player, newPosition, _score, true));
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _tilemap = Tilemap.FromFile(Content, "images/room5-definition.xml");
            _tilemap.Scale = new Vector2(4.0f, 4.0f);

            _themeSong = Content.Load<Song>("audio/backgroundMusic");
            _font = Content.Load<SpriteFont>("fonts/04B_30");
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
