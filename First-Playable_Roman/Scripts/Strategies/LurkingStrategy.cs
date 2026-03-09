using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace First_Playable_Roman.Scripts.Strategies
{
    internal class LurkingStrategy : Enemy
    {
        int _posX;
        int _posY;

        public LurkingStrategy(int maxHp, int xPos, int yPos, int speed) : base(maxHp, xPos, yPos, speed)
        {
            _posX = xPos;
            _posY = yPos;
        }

        public override Vector2 Move()
        {
            // Generate a random angle.
            float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

            // Convert angle to a direction vector.
            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);
            Vector2 direction = new Vector2(x, y);

            // Multiply the direction vector by the movement speed.
            Vector2 slimeVelocity = new Vector2(direction.X * Speed, direction.Y * Speed);

            return slimeVelocity;
        }

        //public Vector2 AssignRandomSlimeVelocity()
        //{
            
        //}
    }
}
