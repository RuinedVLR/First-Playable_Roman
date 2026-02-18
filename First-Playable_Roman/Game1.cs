using First_Playable_Roman.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using MonoGameLibrary;

namespace First_Playable_Roman
{
    public class Game1 : Core
    {
        private Song _themeSong;

        public Game1() : base("TestForNow", 1280, 720, false)
        {

        }

        protected override void Initialize()
        {

            base.Initialize();

            // Start playing background music
            if (_themeSong != null)
            {
                Audio.PlaySong(_themeSong);
                Audio.SongVolume = 0.3f;
            }

            ChangeScene(new TitleScene());
        }

        protected override void LoadContent()
        {
            // Load the background music
            _themeSong = Content.Load<Song>("audio/backgroundMusic");

        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameScene._state == GameScene.GameState.Playing)
            {
                Window.Title = "Test for now";
            }
            else if (GameScene._state == GameScene.GameState.GameOver)
            {
                Window.Title = "Press R to Restart";
            }
        }
    }
}
