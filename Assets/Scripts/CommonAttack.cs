using System;
using System.Collections;
using com.jimmychoi.shootingGame.Weapon;
using UnityEngine;

namespace com.jimmychoi.shootingGame.AttackSystem
{
    public class CommonAttack : AttackBase
    {
        public CommonAttack(WeaponBase weapon, AirplaneBase airplaneBase)
        {
            base.weapon = weapon;
            base.airplaneBase = airplaneBase;
        }

        public override void Execute()
        {
            base.Execute();
            var go = weapon.bulletPool.Borrow();
            go.transform.position = new Vector2(airplaneBase.transform.position.x, airplaneBase.transform.position.y);
            go.GetComponent<Rigidbody2D>().velocity = go.transform.up.normalized * ((CommonBullet)weapon).speed;
            returnBulletPool(go);
        }

        IEnumerator returnBulletPool(GameObject obj)
        {
            yield return new WaitForSeconds(((CommonBullet)weapon).lifeTime);
            weapon.bulletPool.Return(obj);
        }
    }
}
