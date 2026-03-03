using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman.Scripts
{
    internal class LurkingEnemy : Enemy
    {
        public LurkingEnemy(int maxHp, int xPos, int yPos, int speed) : base(maxHp, xPos, yPos, speed)
        {
        }
    }
}
