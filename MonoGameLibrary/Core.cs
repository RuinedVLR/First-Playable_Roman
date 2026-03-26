using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Input;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Scenes;

namespace MonoGameLibrary
{
    public class Core : Game
    {
        internal static Core s_instance;

        /// <summary>
        /// Gets reference to Core instance
        /// </summary>
        public static Core Instance => s_instance;

        // The scene that is currently active.
        private static Scene s_activeScene;

        // The next scene to switch to, if there is one.
        private static Scene s_nextScene;

        /// <summary>
        /// Gets graphics manager to control presentation of graphics
        /// </summary>
        public static GraphicsDeviceManager Graphics { get; private set; }

        /// <summary>
        /// Gets graphics device used to create graphical resourses and simple renders
        /// </summary>
        public static new GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets sprite batch used to render all 2D graphics
        /// </summary>
        public static SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// Gets content manager used to load global assets
        /// </summary>
        public static new ContentManager Content { get; private set; }

        /// <summary>
        /// Gets a reference to the input management system.
        /// </summary>
        public static InputManager Input { get; private set; }

        /// <summary>
        /// Gets a reference to the window bounds.
        /// </summary>
        public static Rectangle Bounds;

        /// <summary>
        /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
        /// </summary>
        public static bool ExitOnEscape { get; set; }

        public static AudioController Audio { get; private set; }

        /// <summary>
        /// New Core Instance
        /// </summary>
        /// <param name="title">The title to display in the title bar of the game window.</param>
        /// <param name="width">The initial width, in pixels, of the game window.</param>
        /// <param name="height">The initial height, in pixels, of the game window.</param>
        /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
        public Core(string title, int width, int height, bool fullScreen)
        {
            if (s_instance != null)
            {
                throw new InvalidOperationException($"Only a single Core instance can be created");
            }

            // Store reference
            s_instance = this;

            // New Graphics Device Manager
            Graphics = new GraphicsDeviceManager(this);

            // Set Graphics
            Graphics.PreferredBackBufferWidth = width;
            Graphics.PreferredBackBufferHeight = height;
            Graphics.IsFullScreen = fullScreen;

            // Apply Graphics
            Graphics.ApplyChanges();

            //Set Bounds Size
            Bounds = new Rectangle(0, 0, width, height);

            // Set Window name
            Window.Title = title;

            // Set core's content manager to game's content manager
            Content = base.Content;

            // Set the root directory for Content
            Content.RootDirectory = "Content";

            // Mouse Visible by default
            IsMouseVisible = true;

            // Exit on escape is true by default
            ExitOnEscape = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Set core's graphics device to reference of base game's graphics device
            GraphicsDevice = base.GraphicsDevice;

            // Create Sprite Batch instance
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a new input manager.
            Input = new InputManager();

            // Create a new audio controller.
            Audio = new AudioController();
        }

        protected override void UnloadContent()
        {
            // Dispose of the audio controller.
            Audio.Dispose();

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // Update the input manager.
            Input.Update(gameTime);

            // Update the audio controller.
            Audio.Update();

            if (ExitOnEscape && Input.Keyboard.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // if there is a next scene waiting to be switch to, then transition
            // to that scene.
            if (s_nextScene != null)
            {
                TransitionScene();
            }

            // If there is an active scene, update it.
            if (s_activeScene != null)
            {
                s_activeScene.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public static void ChangeScene(Scene next)
        {
            // Only set the next scene value if it is not the same
            // instance as the currently active scene.
            if (s_activeScene != next)
            {
                s_nextScene = next;
            }
        }

        private static void TransitionScene()
        {
            // If there is an active scene, dispose of it.
            if (s_activeScene != null)
            {
                s_activeScene.Dispose();
            }

            // Force the garbage collector to collect to ensure memory is cleared.
            GC.Collect();

            // Change the currently active scene to the new scene.
            s_activeScene = s_nextScene;

            // Null out the next scene value so it does not trigger a change over and over.
            s_nextScene = null;

            // If the active scene now is not null, initialize it.
            // Remember, just like with Game, the Initialize call also calls the
            // Scene.LoadContent
            if (s_activeScene != null)
            {
                s_activeScene.Initialize();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (s_activeScene != null)
            {
                s_activeScene.Draw(gameTime);
            }
            
            base.Draw(gameTime);
        }

        public static void DrawRectangleOutline(Rectangle rect, Color color)
        {
            int borderWidth = 3; // Desired thickness of the outline
            Color outlineColor = color; // Desired color of the outline
            Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // Draw the top line
            SpriteBatch.Draw(pixel,
                            new Rectangle(rect.X, rect.Y, rect.Width, borderWidth),
                            outlineColor);

            // Draw the bottom line
            SpriteBatch.Draw(pixel,
                            new Rectangle(rect.X, rect.Y + rect.Height - borderWidth, rect.Width, borderWidth),
                            outlineColor);

            // Draw the left line
            SpriteBatch.Draw(pixel,
                            new Rectangle(rect.X, rect.Y, borderWidth, rect.Height),
                            outlineColor);

            // Draw the right line
            SpriteBatch.Draw(pixel,
                            new Rectangle(rect.X + rect.Width - borderWidth, rect.Y, borderWidth, rect.Height),
                            outlineColor);
        }
    }
}
