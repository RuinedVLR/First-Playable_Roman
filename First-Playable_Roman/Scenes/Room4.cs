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
    public class Room4 : Room
    {
        public Room4(string tilemapPath) : base(tilemapPath) { }

        public Room4(string tilemapPath, Player player, Vector2 playerPosition, int score = 0, bool isCleared = false)
            : base(tilemapPath, player, playerPosition, score, isCleared) {}

        private Tilemap _tilemap;
        private Song _themeSong;
        private SpriteFont _font;

        protected override int GetEnemyKillGoal() => 12;

        protected override List<int> GetObstacleTileIDs()
        {
            return new List<int> { 03, 07, 08, 09, 16 };
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

            float upThreshold = 50;
            float rightThreshold = Core.GraphicsDevice.Viewport.Width - 50;

            // Top → Room2 (already cleared to reach Room4)
            if (_player._position.Y < upThreshold)
            {
                Vector2 newPosition = new Vector2(_player._position.X, Core.GraphicsDevice.Viewport.Height - 100);
                Core.ChangeScene(new Room2("images/room2-definition.xml", _player, newPosition, _score, true));
            }

            // Right → Room5
            if (_player._position.X > rightThreshold)
            {
                Vector2 newPosition = new Vector2(100, _player._position.Y);
                Core.ChangeScene(new Room5("images/room5-definition.xml", _player, newPosition, _score, false));
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _tilemap = Tilemap.FromFile(Content, "images/room4-definition.xml");
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
