using System;
using System.Collections;
using com.jimmychoi.shootingGame.Weapon;
using UnityEngine;
using com.jimmychoi.shootingGame.ui.utility;

namespace com.jimmychoi.shootingGame.AttackSystem
{
    public class CommonAttack : AttackBase
    {
        [SerializeField]
        PlayerAirplane player;

        [SerializeField]
        private PrefabManasablePool commonBulletPool; // 普通子弹

        public void Init(WeaponBase weapon, PlayerAirplane airplaneBase)
        {
            base.Init(weapon, airplaneBase);
            m_weapon = weapon;
            m_airplaneBase = airplaneBase;
        }

        /// <summary>
        /// 开始攻击
        /// </summary>
        public override void Execute()
        {
            base.Execute();
            //var go = commonBulletPool.Borrow();
            //go.transform.position = new Vector2(player.transform.position.x, player.transform.position.y);
            //go.GetComponent<Rigidbody2D>().velocity = go.transform.up.normalized * 10;

            var go = m_weapon.bulletPool.Borrow();
            go.transform.position = new Vector2(m_airplaneBase.transform.position.x, m_airplaneBase.transform.position.y);
            go.GetComponent<Rigidbody2D>().velocity = go.transform.up.normalized * 10;

            StartCoroutine(returnBulletPool(go));
        }

        /// <summary>
        /// 回收子弹
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        IEnumerator returnBulletPool(GameObject obj)
        {
            yield return new WaitForSeconds(1.0f);
            commonBulletPool.Return(obj);
        }
    }
}
