using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGameLibrary.Input;
using MonoGameLibrary;

namespace First_Playable_Roman
{
    internal class Player
    {
        public string Name { get; set; }
        public Health Health { get; private set; }
        public Position _position;
        public int _speed = 2;

        public void TakeDamage(int damage)
        {
            Health.TakeDamage(damage);
        }

        public void Attack(Enemy enemy)
        {
            enemy.TakeDamage(Health.CurrentHealth);
        }

        public Player(string name, int hp, int xPos, int yPos)
        {
            Name = name;
            Health = new Health(hp);
            _position = new Position { _xPos = xPos, _yPos = yPos };
        }
    }
}
