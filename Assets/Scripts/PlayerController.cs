using com.jimmychoi.shootingGame.ui.utility;
using com.jimmychoi.shootingGame.AttackSystem;
using com.jimmychoi.shootingGame.Weapon;
using UnityEngine;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PrefabManasablePool commonBulletPool; // 普通子弹

    [SerializeField]
    private PlayerAirplane playerAirplane;

    public bool canFire = false;

    private MotionBase move; // 移动的Action
    private AttackBase commonAttack;
    private WeaponBase commonWeapon;

    private void Start()
    {
        move = new Move(playerAirplane); // 生成一个移动Action
        commonWeapon = new CommonBullet(commonBulletPool); // 设定武器
        commonAttack = new CommonAttack(commonWeapon, playerAirplane); // 设定攻击模式

        StartCoroutine(startCommonAttack()); // 攻击开始
    }

    void Update()
    {
        updateMove();
    }

    /// <summary>
    /// 移动
    /// </summary>
    private void updateMove()
    {
        // 右・左
        float x = Input.GetAxisRaw("Horizontal");

        // 上・下
        float y = Input.GetAxisRaw("Vertical");

        // 移動する向きを求める
        Vector2 direction = new Vector2(x, y).normalized;

        // 移動の制限
        if (move != null)
            move.Execute(direction);
    }

    IEnumerator startCommonAttack()
    {
        while (true)
        {
            if (canFire)
            {
                yield return new WaitForSeconds(playerAirplane.shotDelay);
                if(commonAttack != null)
                    commonAttack.Execute();
            }
        }
    }
}
