using MonoGameLibrary;
using Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace First_Playable_Roman.Scripts.Movements
{
    internal class TurretStrategy : Enemy
    {
        public bool IsShooting { get; private set; }
        public int _fireSpeed;
        private Position _fireOrigin;
        private Circle _projectile;
        private int _projectileRad = 5;

        public TurretStrategy(int maxHp, int xPos, int yPos, int fireSpeed) : base(maxHp, xPos, yPos, fireSpeed)
        {
            _fireSpeed = fireSpeed;
            _fireOrigin = new Position { _xPos = xPos + 50, _yPos = yPos + 50 };
        }

        public Circle Shoot()
        {
            _projectile = new Circle(_fireOrigin._xPos, _fireOrigin._yPos, _projectileRad);

            IsShooting = true;

            return _projectile;
        }
    }
}
