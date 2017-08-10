using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Move move;
    [SerializeField]
    private PlayerAirplane playerAirplane;

    private void Start()
    {
        move = new Move(playerAirplane);
    }
    void Update()
    {
        // 右・左
        float x = Input.GetAxisRaw("Horizontal");

        // 上・下
        float y = Input.GetAxisRaw("Vertical");

        // 移動する向きを求める
        Vector2 direction = new Vector2(x, y).normalized;

        // 移動の制限
        if(move != null)
            move.Execute(direction);
    }
}
