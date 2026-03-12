using First_Playable_Roman.Scripts;
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
using System.Linq;
using System.Threading;

namespace First_Playable_Roman.Scenes
{
    public class Room1 : Rooms
    {
        public Room1(string tilemapPath) : base(tilemapPath) {}
        
        public Room1(Player player, Vector2 playerPosition) : base(player, playerPosition) {}

        private AnimatedSprite _playerSprite;
        private AnimatedSprite _slimeSprite;

        private List<Vector2> _knifePositions;

        private List<Vector2> _heartPositions;
        private Vector2 _keyPosition;

        private List<Enemy> _enemies;

        // Defines the tilemap to draw.
        private Tilemap _tilemap;

        // Defines the bounds of the room that the slime and bat are contained within.
        private Rectangle _roomBounds;

        private Song _themeSong;

        // The SpriteFont Description used to draw text.
        private SpriteFont _font;

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

            Restart();
        }

        

        public override void Update(GameTime gameTime)
        {
            // Ensure core / input state is updated before we inspect input.
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        private void Restart()
        {
            Core.ExitOnEscape = false;

            _roomBounds = new Rectangle(
                (int)_tilemap.TileWidth,
                (int)_tilemap.TileHeight,
                (int)(_tilemap.TileWidth * _tilemap.Columns - _tilemap.TileWidth * 2),
                (int)(_tilemap.TileHeight * _tilemap.Rows - _tilemap.TileHeight * 2)
             );

            // Recreate basic player and sprites from content (simple restart).
            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slimeSprite = atlas.CreateAnimatedSprite("Slime-animation");

            Core.Audio.PlaySong(_themeSong);
            Core.Audio.SongVolume = 0.3f;

            _enemies = new List<Enemy>
            {
                new LurkingStrategy(100, 100, 5, 5),
                new LurkingStrategy(100, 100, 5, 5),
            };

            int[] tilesInts = _tilemap.GetTilesIDs();

            for (int i = 0; i < tilesInts.Length; i++)
            {
                if (_obstaclesTileIDs.Contains(tilesInts[i]))
                {
                    int x = i % _tilemap.Columns;
                    int y = (int)Math.Floor((double)(i / _tilemap.Columns));

                    _obstacles.Add(new Rectangle(
                        x,
                        y,
                        (int)_tilemap.TileWidth,
                        (int)_tilemap.TileHeight
                    ));
                }
            }

            _knifePositions = new List<Vector2>
            {
                new Vector2(200, 100),
                new Vector2(300, 100)
            };
            _heartPositions = new List<Vector2>
            {
                new Vector2(500, 100),
                new Vector2(500, 500)
            };
            _keyPosition = new Vector2(800, 400);
        }
    }
}
