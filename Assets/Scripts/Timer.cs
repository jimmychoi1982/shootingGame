using UnityEngine;

#region Timer
public class Timer
{
    /// <summary>
    /// 経過時間
    /// </summary>
    public float CurrentTime { get; private set; }

    /// <summary>
    /// 残り時間
    /// </summary>
    public float RemainingTime
    {
        get
        {
            return LimitTime - CurrentTime;
        }
        private set
        {
        }
    }

    /// <summary>
    /// 停止時間
    /// </summary>
    public float LimitTime { get; set; }

    /// <summary>
    /// LimitTimeまで時間が進んだら呼ばれる
    /// </summary>
    public Delegate.VoidDelegate TimeOutDelete { get; set; }

    bool isEnable = false;
    public bool IsEnable
    {
        get
        {
            return isEnable;
        }

        set
        {
            isEnable = value;
            if (value == false)
            {
                CurrentTime = 0;
            }
        }
    }

    public void Reset()
    {
        CurrentTime = 0;
        isEnable = true;
    }
    /// <summary>
    /// 駆動中または有効になっていない場合はFalse、
    /// 時間に来たらTrueを返す。
    /// </summary>
    public bool Update()
    {
        if (IsEnable)
        {
            CurrentTime += Time.deltaTime;
            if (CurrentTime >= LimitTime)
            {
                CurrentTime = 0;
                if (TimeOutDelete != null)
                {
                    TimeOutDelete();
                }
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }
}
#endregion

#region Delegate
public static class Delegate
{
    public delegate void VoidDelegate();
}
#endregion
