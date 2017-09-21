using UnityEngine;
using System.Collections;
using System;

public static class Utils
{
    /// <summary>
    /// 縦画面か？
    /// エディタの場合は縦画面用MAINUIが存在するかで判断
    /// それ以外はScreen.orientationの中身で判断
    /// </summary>
    /// <returns></returns>
    public static bool IsScreenPortrait()
    {
        return Global.IsScreenPortrait;
    }
}