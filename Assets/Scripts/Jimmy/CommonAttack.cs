using System;
using System.Collections;
using com.jimmychoi.shootingGame.Weapon;
using UnityEngine;
using com.jimmychoi.shootingGame.ui.utility;

namespace com.jimmychoi.shootingGame.AttackSystem
{
    public class CommonAttack : AttackBase
    {
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="airplaneBase"></param>
        /// <param name="weapon"></param>
        public override void Init(AirplaneBase airplaneBase, params WeaponBase[] weapon)
        {
            base.Init(airplaneBase, weapon);
            m_weapon = weapon[0];
            m_airplaneBase = airplaneBase;
        }

        /// <summary>
        /// 开始攻击
        /// </summary>
        public void Execute(Direction direction)
        {
            var go = m_weapon.bulletPool.Borrow();
            go.transform.position = new Vector2(m_airplaneBase.transform.position.x, m_airplaneBase.transform.position.y);

            if(direction == Direction.Up)
                go.GetComponent<Rigidbody2D>().velocity = go.transform.up.normalized * 10;
            else if(direction == Direction.Down)
                go.GetComponent<Rigidbody2D>().velocity = -go.transform.up.normalized * 10;
            else if (direction == Direction.Left)
                go.GetComponent<Rigidbody2D>().velocity = -go.transform.right.normalized * 10;
            else if (direction == Direction.Right)
                go.GetComponent<Rigidbody2D>().velocity = go.transform.right.normalized * 10;

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
            m_weapon.bulletPool.Return(obj);
        }
    }
}
