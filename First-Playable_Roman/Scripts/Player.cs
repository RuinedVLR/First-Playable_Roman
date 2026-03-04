using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGameLibrary.Input;
using MonoGameLibrary;
using First_Playable_Roman.Scripts;

namespace First_Playable_Roman.Scripts
{
    internal class Player : Entity
    {
        public string Name { get; set; }
        public int _speed;

        public void TakeDamage(int damage)
        {
            Health.TakeDamage(damage);
        }

        public void Attack(Enemy enemy)
        {
            enemy.TakeDamage(Health.CurrentHealth);
        }

        public Player(string name, int hp, int xPos, int yPos, int speed) : base(hp, xPos, yPos)
        {
            Name = name;
            _speed = speed;
        }
    }
}
