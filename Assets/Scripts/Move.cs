using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Move : MotionBase
{
    public Move(AirplaneBase target)
    {
        airPlaneBase = target;
    }

    public override void Execute(Vector2 direction)
    {
        base.Execute(direction);

        // 画面左下のワールド座標をビューポートから取得
        Vector2 min = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));

        // 画面右上のワールド座標をビューポートから取得
        Vector2 max = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

        // プレイヤーの座標を取得
        Vector2 pos = airPlaneBase.transform.position;

        // 移動量を加える
        pos += direction * ((PlayerAirplane)AirPlaneBase).speed * Time.deltaTime;

        // プレイヤーの位置が画面内に収まるように制限をかける
        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);

        // 制限をかけた値をプレイヤーの位置とする
        airPlaneBase.transform.position = pos;
    }
}
