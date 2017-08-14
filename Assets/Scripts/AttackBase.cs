using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.jimmychoi.shootingGame.Weapon;

namespace com.jimmychoi.shootingGame.AttackSystem
{
    public class AttackBase
    {
        protected WeaponBase weapon; // 该攻击所用的武器
        protected AirplaneBase airplaneBase;

        public virtual void Execute()
        {
            // 动作开始，具体实现在各个子类中
        }
    }
}
