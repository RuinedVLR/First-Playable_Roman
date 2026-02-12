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
        public int Hp { get; private set; }
        public Position _position;
        public int _speed = 2;
        

        public Player(string name, int hp, int xPos, int yPos)
        {
            Hp = hp;
            Name = name;
            _position = new Position { _xPos = xPos, _yPos = yPos };
        }

        public void TakeDamage(int damage)
        {
            Hp -= damage;
        }

    }
}
