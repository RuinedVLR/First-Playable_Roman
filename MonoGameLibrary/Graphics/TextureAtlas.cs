using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics
{
    public class TextureAtlas
    {
        private Dictionary<string, TextureRegion> _regions;
        private Dictionary<string, Animation> _animations;

        /// <summary>
        /// Gets or Sets the source texture represented by this texture atlas.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Creates a new texture atlas.
        /// </summary>
        public TextureAtlas()
        {
            _regions = new Dictionary<string, TextureRegion>();
            _animations = new Dictionary<string, Animation>();
        }

        /// <summary>
        /// Creates a new texture atlas instance using the given texture.
        /// </summary>
        /// <param name="texture">The source texture represented by the texture atlas.</param>
        public TextureAtlas(Texture2D texture)
        {
            Texture = texture;
            _regions = new Dictionary<string, TextureRegion>();
            _animations = new Dictionary<string, Animation>();
        }

        /// <summary>
        /// Creates a new region and adds it to this texture atlas using the atlas default texture.
        /// </summary>
        public void AddRegion(string name, int x, int y, int width, int height)
        {
            TextureRegion region = new TextureRegion(Texture, x, y, width, height);
            _regions.Add(name, region);
        }

        /// <summary>
        /// Creates a new region and adds it to this texture atlas using the provided texture.
        /// </summary>
        public void AddRegion(string name, Texture2D texture, int x, int y, int width, int height)
        {
            TextureRegion region = new TextureRegion(texture, x, y, width, height);
            _regions.Add(name, region);
        }

        /// <summary>
        /// Gets the region from this texture atlas with the specified name.
        /// Throws a clearer exception if not found.
        /// </summary>
        public TextureRegion GetRegion(string name)
        {
            if (_regions.TryGetValue(name, out var region))
                return region;

            throw new KeyNotFoundException($"Region '{name}' not found in atlas. Available regions: {string.Join(", ", _regions.Keys)}");
        }

        /// <summary>
        /// Removes the region from this texture atlas with the specified name.
        /// </summary>
        public bool RemoveRegion(string name)
        {
            return _regions.Remove(name);
        }

        /// <summary>
        /// Removes all regions from this texture atlas.
        /// </summary>
        public void Clear()
        {
            _regions.Clear();
        }

        /// <summary>
        /// Adds the given animation to this texture atlas with the specified name.
        /// </summary>
        public void AddAnimation(string animationName, Animation animation)
        {
            _animations.Add(animationName, animation);
        }

        /// <summary>
        /// Gets the animation from this texture atlas with the specified name.
        /// </summary>
        public Animation GetAnimation(string animationName)
        {
            return _animations[animationName];
        }

        /// <summary>
        /// Removes the animation with the specified name from this texture atlas.
        /// </summary>
        public bool RemoveAnimation(string animationName)
        {
            return _animations.Remove(animationName);
        }

        /// <summary>
        /// Creates a new texture atlas based on a texture atlas xml configuration file.
        /// Supports multiple <Texture> + <Regions> pairs in one XML file.
        /// </summary>
        public static TextureAtlas FromFile(ContentManager content, string fileName)
        {
            TextureAtlas atlas = new TextureAtlas();

            string filePath = Path.Combine(content.RootDirectory, fileName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Atlas file not found. Expected at: {filePath}");

            using (Stream stream = TitleContainer.OpenStream(filePath))
            {
                XDocument doc = XDocument.Load(stream);
                XElement root = doc.Root;

                // Load all children so we can iterate pairs like: <Texture> ... </Texture> followed by <Regions> ... </Regions>
                var children = root.Elements().ToList();

                for (int i = 0; i < children.Count; i++)
                {
                    var el = children[i];
                    if (el.Name == "Texture")
                    {
                        string texturePath = el.Value?.Trim();
                        if (string.IsNullOrEmpty(texturePath))
                            continue;

                        Texture2D tex = content.Load<Texture2D>(texturePath);

                        // If the next element is <Regions>, register those regions using this texture
                        if (i + 1 < children.Count && children[i + 1].Name == "Regions")
                        {
                            var regionsElement = children[i + 1];
                            foreach (var regionEl in regionsElement.Elements("Region"))
                            {
                                string name = regionEl.Attribute("name")?.Value;
                                int x = int.Parse(regionEl.Attribute("x")?.Value ?? "0");
                                int y = int.Parse(regionEl.Attribute("y")?.Value ?? "0");
                                int width = int.Parse(regionEl.Attribute("width")?.Value ?? "0");
                                int height = int.Parse(regionEl.Attribute("height")?.Value ?? "0");

                                if (!string.IsNullOrEmpty(name))
                                {
                                    atlas.AddRegion(name, tex, x, y, width, height);
                                }
                            }

                            i++; // skip the Regions element since we've processed it
                        }
                    }
                }

                // Parse animations (if present)
                var animationsEl = root.Element("Animations");
                var animationElements = animationsEl?.Elements("Animation");

                if (animationElements != null)
                {
                    foreach (var animationElement in animationElements)
                    {
                        string name = animationElement.Attribute("name")?.Value;
                        float delayInMilliseconds = float.Parse(animationElement.Attribute("delay")?.Value ?? "0");
                        TimeSpan delay = TimeSpan.FromMilliseconds(delayInMilliseconds);

                        List<TextureRegion> frames = new List<TextureRegion>();

                        var frameElements = animationElement.Elements("Frame");

                        if (frameElements != null)
                        {
                            foreach (var frameElement in frameElements)
                            {
                                string regionName = frameElement.Attribute("region")?.Value;
                                if (string.IsNullOrEmpty(regionName))
                                    throw new FormatException($"Frame element is missing 'region' attribute in animation '{name}'.");

                                TextureRegion region = atlas.GetRegion(regionName);
                                frames.Add(region);
                            }
                        }

                        Animation animation = new Animation(frames, delay);
                        atlas.AddAnimation(name, animation);
                    }
                }

                return atlas;
            }
        }

        /// <summary>
        /// Creates a new sprite using the region from this texture atlas with the specified name.
        /// </summary>
        public Sprite CreateSprite(string regionName)
        {
            TextureRegion region = GetRegion(regionName);
            return new Sprite(region);
        }

        /// <summary>
        /// Creates a new animated sprite using the animation from this texture atlas with the specified name.
        /// </summary>
        public AnimatedSprite CreateAnimatedSprite(string animationName)
        {
            Animation animation = GetAnimation(animationName);
            return new AnimatedSprite(animation);
        }
    }
}
