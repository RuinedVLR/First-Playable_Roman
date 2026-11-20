using First_Playable_Roman.Scripts;
using First_Playable_Roman.Scripts.Strategies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System.Collections.Generic;

namespace First_Playable_Roman.Scenes
{
    public class Room1 : Room
    {
        public Room1(string tilemapPath) : base(tilemapPath) {}

        public Room1(string tilemapPath, Player player, Vector2 playerPosition, int score = 0, bool isCleared = false)
            : base(tilemapPath, player, playerPosition, score, isCleared) {}

        private Tilemap _tilemap;
        private Song _themeSong;
        private SpriteFont _font;

        // Room1: defeat 5 enemies to clear
        protected override int GetEnemyKillGoal() => 5;

        protected override void InitializeItems()
        {
            // Items now drop from enemies
        }

        protected override void InitializeEnemies()
        {
            _enemies = new List<Enemy>
            {
                new LurkingStrategy(0, 0, GameSettings.Data.Enemies.Lurking.Speed, this),
                new LurkingStrategy(0, 0, GameSettings.Data.Enemies.Lurking.Speed, this),
            };
        }

        protected override void CheckRoomTransitions()
        {
            if (_player == null)
                return;

            float rightThreshold = Core.GraphicsDevice.Viewport.Width - 50;

            if (_player._position.X > rightThreshold)
            {
                Vector2 newPosition = new Vector2(100, _player._position.Y);
                // Pass isCleared = false because Room2 is a new room for the player
                Core.ChangeScene(new Room2("images/room2-definition.xml", _player, newPosition, _score, false));
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _tilemap = Tilemap.FromFile(Content, "images/room1-definition.xml");
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
