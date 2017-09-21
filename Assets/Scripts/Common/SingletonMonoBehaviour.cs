using UnityEngine;
using System.Collections;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{

    protected SingletonMonoBehaviour()
    {

    }

    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                // くっつけるオブジェクトはグローバルで保管する一個のみにしてみる
                instance = Global.GetSingletonObject().AddComponent<T>();

            }

            return instance;
        }
    }

    public void DestroyInstance()
    {
        Destroy(this);
    }


}
