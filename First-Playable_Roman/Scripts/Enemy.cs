using First_Playable_Roman.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman.Scripts
{
    internal class Enemy : Entity
    {
        public void TakeDamage(int damage)
        {
            Health.TakeDamage(damage);
        }

        public void Attack(Player player)
        {
            player.TakeDamage(Health.CurrentHealth);
        }

        public Enemy(int maxHp, int xPos, int yPos, int speed) : base(maxHp, xPos, yPos, speed)
        {
            
        }
    }
}
