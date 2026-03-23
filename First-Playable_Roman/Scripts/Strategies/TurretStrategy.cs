using MonoGameLibrary;
using First_Playable_Roman.Scenes;
using Microsoft.Xna.Framework;

namespace First_Playable_Roman.Scripts.Movements
{
    internal class TurretStrategy : Enemy
    {
        public bool IsShooting { get; private set; }
        public int _fireSpeed;
        private Vector2 _fireOrigin;
        private Circle _projectile;
        private int _projectileRad = 5;

        public TurretStrategy(int maxHp, int xPos, int yPos, int fireSpeed, Rooms room) : base(maxHp, xPos, yPos, fireSpeed, room)
        {
            _fireSpeed = fireSpeed;
            _fireOrigin = new Vector2 ( _fireOrigin.X = xPos + 50, _fireOrigin.Y = yPos + 50 );
        }

        public override Vector2 Move()
        {
            return new Vector2(0, 0);
        }

        public Circle Shoot()
        {
            _projectile = new Circle((int)_fireOrigin.X, (int)_fireOrigin.Y, _projectileRad);

            IsShooting = true;

            return _projectile;
        }
    }
}
