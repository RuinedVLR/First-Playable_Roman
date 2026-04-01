using First_Playable_Roman.Scripts;
using First_Playable_Roman.Scripts.Movements;
using First_Playable_Roman.Scripts.Strategies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman.Scenes
{
    public class Room4 : Room
    {
        public Room4(string tilemapPath) : base(tilemapPath) { }

        public Room4(string tilemapPath, Player player, Vector2 playerPosition, int score = 0) : base(tilemapPath, player, playerPosition, score) { }

        // Defines the tilemap to draw.
        private Tilemap _tilemap;

        private Song _themeSong;

        // The SpriteFont Description used to draw text.
        private SpriteFont _font;

        protected override void InitializeItems()
        {
            // Items now drop from enemies
        }

        protected override void InitializeEnemies()
        {
            // Calculate center of the room for turret placement
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
            if (_player._position.Y < upThreshold)
            {
                Vector2 newPosition = new Vector2(
                    _player._position.X,
                    Core.GraphicsDevice.Viewport.Height - 100
                );

                // Switch to Room2
                Core.ChangeScene(new Room2("images/room2-definition.xml", _player, newPosition, _score));
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _tilemap = Tilemap.FromFile(Content, "images/room4-definition.xml");
            _tilemap.Scale = new Vector2(4.0f, 4.0f);

            // Load the background music
            _themeSong = Content.Load<Song>("audio/backgroundMusic");

            // Load the font
            _font = Content.Load<SpriteFont>("fonts/04B_30");
        }

        public override void Initialize()
        {
            // TODO: Add your initialization logic here

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
