using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman.Scripts.Strategies
{
    internal class LurkingStrategy : Enemy
    {
        public LurkingStrategy(int maxHp, int xPos, int yPos, int speed) : base(maxHp, xPos, yPos, speed)
        {
        }
    }
}
