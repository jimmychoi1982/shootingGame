using com.jimmychoi.shootingGame.ui.utility;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerAirplane playerAirplane;

    private Move move; // 移动的Action

    private PrefabInstantiater playerAirPlane; // 飞机玩家

    private void Start()
    {
        move = new Move(playerAirPlane.SafeInstantiateComponent<PlayerAirplane>()); // 生成一个移动Action
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
}
