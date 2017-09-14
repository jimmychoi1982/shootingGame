using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using com.jimmychoi.shootingGame.Weapon;

namespace com.jimmychoi.shootingGame.AttackSystem
{
    public class AttackBase : MonoBehaviour
    {
        protected WeaponBase m_weapon; // 该攻击所用的武器
        protected AirplaneBase m_airplaneBase;

        public virtual void Execute()
        {
            // 动作开始，具体实现在各个子类中
        }

        public virtual void Init(AirplaneBase airplaneBase, params WeaponBase[] weapon)
        {
            // 攻击初期化
        }
    }
}
