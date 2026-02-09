using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input;

namespace MonoGameLibrary
{
    public class Core : Game
    {
        internal static Core s_instance;

        /// <summary>
        /// Gets reference to Core instance
        /// </summary>
        public static Core Instance => s_instance;

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

            // Set Window name
            Window.Title = title;

            // Set core's content manager to game's content manager
            Content = base.Content;

            // Set the root directory for Content
            Content.RootDirectory = "Content";

            // Mouse Visible by default
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Set core's graphics device to reference of base game's graphics device
            GraphicsDevice = base.GraphicsDevice;

            // Create Sprite Batch instance
            SpriteBatch = new SpriteBatch(GraphicsDevice);
        }
    }
}
