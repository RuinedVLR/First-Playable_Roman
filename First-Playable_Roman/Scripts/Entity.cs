using Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman.Scripts
{
    internal class Entity
    {
        public Health Health { get; set; }
        public Position _position;

        public Entity(int maxHp, int posX, int posY) 
        {
            Health = new Health(maxHp);
            _position = new Position { _xPos = posX, _yPos = posY };
        }
    }
}
