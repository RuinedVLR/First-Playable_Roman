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
    public class Room3 : Room
    {
        public Room3(string tilemapPath) : base(tilemapPath) { }

        public Room3(string tilemapPath, Player player, Vector2 playerPosition, int score = 0, bool isCleared = false)
            : base(tilemapPath, player, playerPosition, score, isCleared) {}

        private Tilemap _tilemap;
        private Song _themeSong;
        private SpriteFont _font;

        protected override int GetEnemyKillGoal() => 10;

        protected override List<int> GetObstacleTileIDs()
        {
            return new List<int> { 03, 04, 07, 08 };
        }

        protected override void InitializeItems() { }

        protected override void InitializeEnemies()
        {
            int centerX = (int)(_tilemap.TileWidth * _tilemap.Columns * 0.5f);
            int centerY = (int)(_tilemap.TileHeight * _tilemap.Rows * 0.5f);

            _enemies = new List<Enemy>
            {
                new LurkingStrategy(0, 0, 5, this),
                new ChaserStrategy(0, 0, 3, 200f, this),
                new TurretStrategy(centerX, centerY, this),
            };
        }

        protected override void CheckRoomTransitions()
        {
            if (_player == null)
                return;

            // Bottom → Room2 (already cleared to reach Room3)
            float downThreshold = Core.GraphicsDevice.Viewport.Height - 50;

            if (_player._position.Y > downThreshold)
            {
                Vector2 newPosition = new Vector2(_player._position.X, 100);
                Core.ChangeScene(new Room2("images/room2-definition.xml", _player, newPosition, _score, true));
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _tilemap = Tilemap.FromFile(Content, "images/room3-definition.xml");
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
