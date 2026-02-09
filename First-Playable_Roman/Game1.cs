using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGameLibrary;

namespace First_Playable_Roman
{
    public class Game1 : Core
    {
        Texture2D playerSprite;
        Texture2D map;
        Texture2D enemySprite;

        Player _player;

        public Game1() : base("TestForNow", 1280, 720, false)
        {

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            string nameInput = Console.ReadLine();

            _player = new Player(name: nameInput, hp: 100, xPos: 5, yPos: 5);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here

            playerSprite = Content.Load<Texture2D>("Player");

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            _player.Input();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            SpriteBatch.Begin();

            SpriteBatch.Draw(
                playerSprite,
                new Vector2(
                    _player._position._xPos,
                    _player._position._yPos),
                null,
                Color.White,
                0.0f,
                Vector2.Zero,
                1.0f,
                SpriteEffects.None,
                0.0f
                );

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
