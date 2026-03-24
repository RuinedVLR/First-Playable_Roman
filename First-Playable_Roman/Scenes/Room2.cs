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
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using System;
using System.Collections.Generic;
using System.Threading;

namespace First_Playable_Roman.Scenes
{
    public class Room2 : Rooms
    {
        public Room2(string tilemapPath) : base(tilemapPath) { }

        public Room2(string tilemapPath, Player player, Vector2 playerPosition, int score = 0) : base(tilemapPath, player, playerPosition, score) {}

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
                new ChaserStrategy(0, 0, 4, 200f, this),
                new TurretStrategy(centerX, centerY, this),
            };
        }

        protected override void CheckRoomTransitions()
        {
            if (_player == null)
                return;

            // Check if player moved to the left edge (transition back to Room1)
            float leftEdgeThreshold = 50; // 50 pixels from left edge
            
            if (_player._position.X < leftEdgeThreshold)
            {
                // Calculate the corresponding position in Room1 (enter from right side)
                Vector2 newPosition = new Vector2(
                    Core.GraphicsDevice.Viewport.Width - 100, // Start from right edge in Room1
                    _player._position.Y // Keep same Y position
                );

                // Switch to Room1
                Core.ChangeScene(new Room1("images/room1-definition.xml", _player, newPosition, _score));
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _tilemap = Tilemap.FromFile(Content, "images/room2-definition.xml");
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
