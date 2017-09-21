using UnityEngine;
using System.Collections;

public class UIScript : MonoBehaviour
{

    public Canvas ParentCanvas { set; get; }

    public UIManager.UIType Type { set; get; }
    public int ParentGameObjectInstanceID { set; get; }

    public virtual void Close()
    {
        if (!Global.UIManager.CloseUI(Type, ParentGameObjectInstanceID))
        {
            Debug.LogError("UIClose失敗！ TYPE:" + Type + " InstanceID:" + ParentGameObjectInstanceID);
            // 失敗していたら勝手にしぬ
            OnClosing();
            Destroy(ParentCanvas);
        }
    }

    // 名前考え中... 破棄直前に呼び出す
    public virtual void OnClosing()
    {
        // user 
    }
}
