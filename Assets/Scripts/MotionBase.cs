using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MotionBase : MonoBehaviour
{
    protected AirplaneBase airPlaneBase;

    public AirplaneBase AirPlaneBase
    {
        get { return airPlaneBase; }
    }

    /// <summary>
    /// 执行动作
    /// </summary>
    public virtual void Execute(Vector2 direction)
    {
        // 动作开始，具体实现在各个子类中
    }

    public virtual void Execute()
    {
        // 动作开始，具体实现在各个子类中
    }

    /// <summary>
    /// 初期化动作系统
    /// </summary>
    /// <param name="target">动作执行对象</param>
    public virtual void Init(AirplaneBase target)
    {
        airPlaneBase = target;
    }
}
