using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace First_Playable_Roman.Scripts.Items
{
    public class KeyItem : Items
    {
        public KeyItem(Vector2 position, Sprite sprite) : base(position, sprite)
        {
            if (sprite != null)
            {
                sprite.Scale = new Vector2(0.2f, 0.2f);
            }
        }

        public override void Collect(Player player)
        {
            base.Collect(player);
        }
    }
}
