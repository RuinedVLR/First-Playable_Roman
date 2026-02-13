using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman
{
    internal class Enemy
    {
        public Health Health { get; private set; }
        public Position _position;
        public int _speed;

        public void TakeDamage(int damage)
        {
            Health.TakeDamage(damage);
        }

        public void Attack(Player player)
        {
            player.TakeDamage(Health.CurrentHealth);
        }

        public Enemy(int xPos, int yPos, int speed)
        {
            Health = new Health(10);
            _position = new Position { _xPos = xPos, _yPos = yPos };
            _speed = speed;
        }
    }
}
