using First_Playable_Roman.Scripts;
using First_Playable_Roman.Scripts.Items;
using First_Playable_Roman.Scripts.Strategies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System.Collections.Generic;
using System.IO;

namespace First_Playable_Roman.Scenes
{
    public class Room1 : Rooms
    {
        public Room1(string tilemapPath) : base(tilemapPath) { }

        public Room1(string tilemapPath, Player player, Vector2 playerPosition, int score) : base(tilemapPath)
        {
            _player = player;
            _playerPosition = playerPosition;
            _score = score;
        }

        // Defines the tilemap to draw.
        private Tilemap _tilemap;

        private Song _themeSong;

        // The SpriteFont Description used to draw text.
        private SpriteFont _font;

        protected override void InitializeItems()
        {
            // Initialize item lists for Room1
            _knives = new List<KnifeItem>
            {
                new KnifeItem(new Vector2(200, 100), _knifeSprite),
                new KnifeItem(new Vector2(300, 100), _knifeSprite)
            };

            _hearts = new List<HeartItem>
            {
                new HeartItem(new Vector2(500, 100), _heartSprite, 30),
                new HeartItem(new Vector2(500, 500), _heartSprite, 30)
            };

            _key = new KeyItem(new Vector2(800, 400), _keySprite);
        }

        protected override void InitializeEnemies()
        {
            // Initialize enemy list for Room1
            _enemies = new List<Enemy>
            {
                new LurkingStrategy(100, 0, 0, 5, this),
                new LurkingStrategy(100, 0, 0, 5, this),
            };
        }

        protected override void CheckRoomTransitions()
        {
            if (_player == null)
                return;

            float rightEdgeThreshold = Core.GraphicsDevice.Viewport.Width - 50;

            if (_player._position.X > rightEdgeThreshold)
            {
                Vector2 newPosition = new Vector2(50, _player._position.Y);

                Core.ChangeScene(new Room2("images/room2-definition.xml", _player, newPosition, _score));
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _tilemap = Tilemap.FromFile(Content, "images/room1-definition.xml");
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
