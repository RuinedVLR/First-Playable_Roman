using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace First_Playable_Roman.Scripts.Items
{
    public class HeartItem : Items
    {
        private int _healAmount;

        public HeartItem(Vector2 position, Sprite sprite, int healAmount = 30) : base(position, sprite)
        {
            _healAmount = healAmount;

            if (sprite != null)
            {
                sprite.Scale = new Vector2(0.2f, 0.2f);
            }
        }

        public override void Collect(Player player)
        {
            base.Collect(player);
            player.Health.Heal(_healAmount);
        }
    }
}
