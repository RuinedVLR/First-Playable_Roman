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
    public class Room2 : Room
    {
        public Room2(string tilemapPath) : base(tilemapPath) { }

        public Room2(string tilemapPath, Player player, Vector2 playerPosition, int score = 0, bool isCleared = false)
            : base(tilemapPath, player, playerPosition, score, isCleared) {}

        private Tilemap _tilemap;
        private Song _themeSong;
        private SpriteFont _font;

        protected override int GetEnemyKillGoal() => 8;

        protected override List<int> GetObstacleTileIDs()
        {
            return new List<int> { 03, 04, 07, 08, 11, 49, 50, 53, 54, 59, 63, 64 };
        }

        protected override void InitializeItems() { }

        protected override void InitializeEnemies()
        {
            _enemies = new List<Enemy>
            {
                new LurkingStrategy(0, 0, 5, this),
                new LurkingStrategy(0, 0, 5, this),
                new ChaserStrategy(0, 0, 3, 200f, this),
            };
        }

        protected override void CheckRoomTransitions()
        {
            if (_player == null)
                return;

            float leftThreshold = 50;
            float upThreshold = 50;
            float downThreshold = Core.GraphicsDevice.Viewport.Height - 50;

            // Left → Room1 (pass Room1's cleared state: Room1 was already cleared to get here)
            if (_player._position.X < leftThreshold)
            {
                Vector2 newPosition = new Vector2(Core.GraphicsDevice.Viewport.Width - 100, _player._position.Y);
                Core.ChangeScene(new Room1("images/room1-definition.xml", _player, newPosition, _score, true));
            }

            // Top → Room3
            if (_player._position.Y < upThreshold)
            {
                Vector2 newPosition = new Vector2(_player._position.X, Core.GraphicsDevice.Viewport.Height - 100);
                Core.ChangeScene(new Room3("images/room3-definition.xml", _player, newPosition, _score, false));
            }

            // Bottom → Room4
            if (_player._position.Y > downThreshold)
            {
                Vector2 newPosition = new Vector2(_player._position.X, 100);
                Core.ChangeScene(new Room4("images/room4-definition.xml", _player, newPosition, _score, false));
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _tilemap = Tilemap.FromFile(Content, "images/room2-definition.xml");
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
