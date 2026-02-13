using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Playable_Roman
{
    internal class Health
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }

        public void TakeDamage(int damage)
        {
            if (damage < 0)
                return;

            CurrentHealth -= damage;
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
        }

        public Health(int maxHealth)
        {
            if (maxHealth <= 0)
                return;

            MaxHealth = maxHealth;
            CurrentHealth = MaxHealth;
        }
    }
}
