using First_Playable_Roman.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace First_Playable_Roman.Scripts
{
    public abstract class Enemy : Entity
    {
        public Health Health { get; set; }
        public int Speed { get; set; }

        public void TakeDamage(int damage)
        {
            Health.TakeDamage(damage);
        }

        public void Attack(Player player)
        {
            player.TakeDamage(Health.CurrentHealth);
        }

        public Enemy(int maxHp, int xPos, int yPos, int speed) : base(xPos, yPos)
        {
            Health = new Health(maxHp);           
            Speed = speed;
        }

        public abstract Vector2 Move();
    }
}
