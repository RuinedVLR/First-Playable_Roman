using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman.Scripts
{
    public class Entity
    {
        public Vector2 _position;
        public Vector2 _previousPosition;

        public Entity(int posX, int posY) 
        {
            _position = new Vector2(posX, posY);
        }
    }
}
