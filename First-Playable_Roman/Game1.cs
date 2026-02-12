using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace First_Playable_Roman
{
    public class Game1 : Core
    {
        private AnimatedSprite _playerSprite;
        private AnimatedSprite _slime;

        private Player _player;
        private Vector2 _playerPosition;

        public Game1() : base("TestForNow", 1280, 720, false)
        {

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            string nameInput = Console.ReadLine();

            _player = new Player(nameInput, 100, 588, 4);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            TextureAtlas atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");

            _playerSprite = atlas.CreateAnimatedSprite("Player-animation");
            _slime = atlas.CreateAnimatedSprite("Slime-animation");
        }
        //588, 4
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            _playerSprite.Update(gameTime);
            _slime.Update(gameTime);

            PlayerInput();
            _playerPosition = new Vector2(_player._position._xPos, _player._position._yPos);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _playerSprite.Draw(SpriteBatch, _playerPosition);
            _slime.Draw(SpriteBatch, new Vector2(200, 50));

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public void PlayerInput()
        {
            int playerInputX = 0;
            int playerInputY = 0;

            KeyboardState keyboardState = Keyboard.GetState();

            if (Input.Keyboard.IsKeyDown(Keys.Space))
            {
                _player._speed = 4;
            }
            else
            {
                _player._speed = 2;
            }

            if (Input.Keyboard.IsKeyDown(Keys.W))
            {
                playerInputY--;
            }
            if (Input.Keyboard.IsKeyDown(Keys.S))
            {
                playerInputY++;
            }
            if (Input.Keyboard.IsKeyDown(Keys.A))
            {
                playerInputX--;
            }
            if (Input.Keyboard.IsKeyDown(Keys.D))
            {
                playerInputX++;
            }

            _player._position._xPos += playerInputX * _player._speed;
            _player._position._yPos += playerInputY * _player._speed;

            if (_player._position._xPos < 0)
            {
                _player._position._xPos = 0;
            }
            if (_player._position._yPos < 0)
            {
                _player._position._yPos = 0;
            }
        }
    }
}
