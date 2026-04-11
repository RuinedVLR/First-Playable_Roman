using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace First_Playable_Roman.Scenes
{
    public class TitleScene : Scene
    {
        private const string SLIME_TEXT = "Slime";
        private const string SLASHER_TEXT = "Slasher";
        private const string PRESS_ENTER_TEXT = "Press Enter To Start";

        private static readonly string[] TUTORIAL_LINES =
        [
            "WASD - Move",
            "Shift - Sprint",
            "Space - Hold to Shoot",
            "Enemies drop items - 10% chance for Heart, 10% chance for Knife!",
            "Hearts restore 30 HP, Knives blockes attack and can defeat enemies!",
            "Defeat enemies to progress through the dungeon!"
        ];

        // The font to use to render normal text.
        private SpriteFont _font;

        // The font used to render the title text.
        private SpriteFont _font5x;

        // The position to draw the dungeon text at.
        private Vector2 _slimeTextPos;

        // The origin to set for the dungeon text.
        private Vector2 _slimeTextOrigin;

        // The position to draw the slime text at.
        private Vector2 _slasherTextPos;

        // The origin to set for the slime text.
        private Vector2 _slasherTextOrigin;

        // The position to draw the press enter text at.
        private Vector2 _pressEnterPos;

        // The origin to set for the press enter text when drawing it.
        private Vector2 _pressEnterOrigin;

        // The bottom-left position for the first tutorial line.
        private Vector2 _tutorialStartPos;

        // The texture used for the background pattern.
        private Texture2D _backgroundPattern;

        // The destination rectangle for the background pattern to fill.
        private Rectangle _backgroundDestination;

        // The offset to apply when drawing the background pattern so it appears to
        // be scrolling.
        private Vector2 _backgroundOffset;

        // The speed that the background pattern scrolls.
        private float _scrollSpeed = 150.0f;

        public override void Initialize()
        {
            // LoadContent is called during base.Initialize().
            base.Initialize();

            // While on the title screen, we can enable exit on escape so the player
            // can close the game by pressing the escape key.
            Core.ExitOnEscape = true;

            // Set the position and origin for the Dungeon text.
            Vector2 size = _font5x.MeasureString(SLIME_TEXT);
            _slimeTextPos = new Vector2(550, 100);
            _slimeTextOrigin = size * 0.5f;

            // Set the position and origin for the Slime text.
            size = _font5x.MeasureString(SLASHER_TEXT);
            _slasherTextPos = new Vector2(757, 207);
            _slasherTextOrigin = size * 0.5f;

            // Set the position and origin for the press enter text.
            size = _font.MeasureString(PRESS_ENTER_TEXT);
            _pressEnterPos = new Vector2(640, 500);
            _pressEnterOrigin = size * 0.5f;

            // Position the tutorial block in the bottom-left corner with padding.
            float lineHeight = _font.MeasureString("W").Y + 6;
            float totalHeight = lineHeight * TUTORIAL_LINES.Length;
            int screenHeight = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;
            _tutorialStartPos = new Vector2(20, screenHeight - totalHeight - 20);

            // Initialize the offset of the background pattern at zero.
            _backgroundOffset = Vector2.Zero;

            // Set the background pattern destination rectangle to fill the entire
            // screen background.
            _backgroundDestination = Core.GraphicsDevice.PresentationParameters.Bounds;
        }

        public override void LoadContent()
        {
            // Load the font for the standard text.
            _font = Core.Content.Load<SpriteFont>("fonts/04B_30");

            // Load the font for the title text.
            _font5x = Content.Load<SpriteFont>("fonts/04B_30_5x");

            // Load the background pattern texture.
            _backgroundPattern = Content.Load<Texture2D>("images/background-pattern");
        }

        public override void Update(GameTime gameTime)
        {
            // If the user presses enter, switch to the game scene.
            if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
            {
                Core.ChangeScene(new Room1("images/room1-definition.xml"));
            }

            // Update the offsets for the background pattern wrapping so that it
            // scrolls down and to the right.
            float offset = _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _backgroundOffset.X -= offset;
            _backgroundOffset.Y -= offset;

            // Ensure that the offsets do not go beyond the texture bounds so it is
            // a seamless wrap.
            _backgroundOffset.X %= _backgroundPattern.Width;
            _backgroundOffset.Y %= _backgroundPattern.Height;
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

            // Draw the background pattern first using the PointWrap sampler state.
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
            Core.SpriteBatch.Draw(_backgroundPattern, _backgroundDestination, new Rectangle(_backgroundOffset.ToPoint(), _backgroundDestination.Size), Color.White * 0.5f);
            Core.SpriteBatch.End();

            // Begin the sprite batch to prepare for rendering.
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // The color to use for the drop shadow text.
            Color dropShadowColor = Color.Black * 0.5f;

            // Draw the Dungeon text slightly offset from it is original position and
            // with a transparent color to give it a drop shadow.
            Core.SpriteBatch.DrawString(_font5x, SLIME_TEXT, _slimeTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            // Draw the Dungeon text on top of that at its original position.
            Core.SpriteBatch.DrawString(_font5x, SLIME_TEXT, _slimeTextPos, Color.White, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            // Draw the Slime text slightly offset from it is original position and
            // with a transparent color to give it a drop shadow.
            Core.SpriteBatch.DrawString(_font5x, SLASHER_TEXT, _slasherTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _slasherTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            // Draw the Slime text on top of that at its original position.
            Core.SpriteBatch.DrawString(_font5x, SLASHER_TEXT, _slasherTextPos, Color.White, 0.0f, _slasherTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

            // Draw the press enter text.
            Core.SpriteBatch.DrawString(_font, PRESS_ENTER_TEXT, _pressEnterPos, Color.White, 0.0f, _pressEnterOrigin, 1.0f, SpriteEffects.None, 0.0f);

            // Draw tutorial lines in the bottom-left corner.
            float lineHeight = _font.MeasureString("W").Y + 6;
            for (int i = 0; i < TUTORIAL_LINES.Length; i++)
            {
                Vector2 linePos = _tutorialStartPos + new Vector2(0, i * lineHeight);
                Core.SpriteBatch.DrawString(_font, TUTORIAL_LINES[i], linePos + new Vector2(2, 2), dropShadowColor, 0.0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0.0f);
                Core.SpriteBatch.DrawString(_font, TUTORIAL_LINES[i], linePos, Color.White * 0.85f, 0.0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0.0f);
            }

            // Always end the sprite batch when finished.
            Core.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
