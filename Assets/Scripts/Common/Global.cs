using UnityEngine;


public static class Global
{
    public static bool IsScreenPortrait = false;        // 縦画面か？　どっかで初期化書こう

    private static GameObject singletonObject = null;
    private static readonly string SingletonObjectName = "SingletonObject";
    public static GameObject GetSingletonObject()
    {
        if (singletonObject == null)
        {
            singletonObject = new GameObject(SingletonObjectName);
            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(singletonObject);
            }
        }
        return singletonObject;
    }

    /// <summary>
    /// UIマネージャ
    /// </summary>
    public static UIManager UIManager
    {
        get { return UIManager.Instance; }
    }


}