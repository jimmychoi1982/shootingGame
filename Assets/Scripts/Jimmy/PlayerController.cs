﻿using com.jimmychoi.shootingGame.ui.utility;
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
    private PlayerAirplane playerAirplane; // 主角飞机

    [SerializeField]
    private Animator animator; // 飞机的动画

    public bool canFire = false;

    private Move move; // 移动的Action
    private CommonBullet commonBullet;

    [SerializeField]
    private CommonAttack commonAttack;

    private void Start()
    {
        move = new Move(playerAirplane); // 生成一个移动Action
        commonBullet = new CommonBullet(commonBulletPool); // 设定武器

        commonAttack.Init(playerAirplane, commonBullet); // 设定攻击模式

        animator.SetBool("ToRight", false);
        animator.SetBool("ToLeft", false);

        animator.SetBool("Normal", true);
    }

    float fireDelay = 0;

    void Update()
    {
        updateMove();

        fireDelay += Time.deltaTime;

        if(fireDelay >= 2)
        {
            fireDelay = 0;
            if (commonAttack != null)
                commonAttack.Execute(CommonAttack.Direction.Up);
        }
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

        Debug.Log(direction);

        updateAnimator(direction);
    }

    private void updateAnimator(Vector2 direction)
    {
        var x = direction.x;

        if(x > 0)
        {
            // Right
            animator.SetBool("Normal", false);
            animator.SetBool("ToLeft", false);

            animator.SetBool("ToRight", true);
        }
        else if(x < 0)
        {
            // Left
            animator.SetBool("Normal", false);
            animator.SetBool("ToRight", false);

            animator.SetBool("ToLeft", true);
        }
        else
        {
            animator.SetBool("ToRight", false);
            animator.SetBool("ToLeft", false);

            animator.SetBool("Normal", true);
        }
    }
}
