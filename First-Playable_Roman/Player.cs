using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman
{
    internal class Player
    {
        public string Name { get; set; }
        public int Hp { get; private set; }
        public Position _position;
        private int _speed = 2;

        public Player(string name, int hp, int xPos, int yPos)
        {
            Hp = hp;
            Name = name;
            _position = new Position { _xPos = xPos, _yPos = yPos };
        }

        public void Input()
        {
            int playerInputX = 0;
            int playerInputY = 0;

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                playerInputY--;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                playerInputY++;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                playerInputX--;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                playerInputX++;
            }

            _position._xPos += playerInputX * _speed;
            _position._yPos += playerInputY * _speed;

            if (_position._xPos < 0)
            {
                _position._xPos = 0;
            }
            if (_position._yPos < 0)
            {
                _position._yPos = 0;
            }
        }

        public void TakeDamage(int damage)
        {
            Hp -= damage;
        }

    }
}
