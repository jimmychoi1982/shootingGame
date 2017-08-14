using com.jimmychoi.shootingGame.ui.utility;
using com.jimmychoi.shootingGame.AttackSystem;
using com.jimmychoi.shootingGame.Weapon;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PrefabInstantiater playerAirPlanePrefab; // 飞机玩家
    [SerializeField]
    private PrefabManasablePool commonBulletPool; // 普通子弹

    private PlayerAirplane playerAirplane;

    private MotionBase move; // 移动的Action
    private AttackBase commonAttack;
    private WeaponBase commonWeapon;

    private void Start()
    {
        playerAirplane = playerAirPlanePrefab.SafeInstantiateComponent<PlayerAirplane>();

        move = new Move(playerAirplane); // 生成一个移动Action
        commonWeapon = new CommonBullet(commonBulletPool); // 设定武器
        commonAttack = new CommonAttack(commonWeapon, playerAirplane); // 设定攻击模式
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

    private void updateAttack()
    {
        // TODO间隔时间 用 Timer.cs
        commonAttack.Execute();
    }
}
