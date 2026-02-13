using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman
{
    internal class Enemy
    {
        public Position _position;
        public int _speed;

        public Enemy(int xPos, int yPos, int speed)
        {
            _position = new Position { _xPos = xPos, _yPos = yPos };
            _speed = speed;
        }
    }
}
