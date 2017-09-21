/*
 * 这是一个UI管理类，
 * 追加的UI类都继承UIScript，这样就可以由这个类来管理
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonoBehaviour<UIManager>
{
    public enum UIType
    {
        None,
        MainUI,                // メイン
    }

    public enum CanvasType
    {
        Default,            // 通常
        Effect,             // エフェクト用   タッチ無視
        DEBUG,
        Menu,               // 
        Tutorial,           // 
        System,             // システム系
        Transition,         // 画面切り替え系  タッチ無視
        SystemL2,           // 最上位システム系 タッチ無視
        Extra,              // 
        SuperDialog,        // 最上位ダイアログ系 何よりも前に出現する タッチ可
    }

    //
    // タッチを受け付けないキャンバスリスト
    //
    private CanvasType[] notAcceptTouchCanvasType = new CanvasType[] {
        CanvasType.Effect ,
        CanvasType.Transition,
        CanvasType.SystemL2
    };

    [System.Flags]
    public enum UISetFlag : byte
    {
        IsNeedLockWeakInput = 0x01
    }

    private class UISet
    {
        public int InstanceID { set; get; }
        public UIType UIType { set; get; }
        public Canvas Canvas { set; get; }

        public UISetFlag Flag { set; get; }

        public UISet(int instance_id, UIType uitype, GameObject obj, MonoBehaviour scr, Canvas canvas, UISetFlag flag) { InstanceID = instance_id; UIType = uitype; gameObject = obj; script = scr; Canvas = canvas; Flag = flag; }

        public GameObject gameObject;
        public MonoBehaviour script;
    }

    private List<UISet> uiobjects = new List<UISet>();


    private Dictionary<CanvasType, Canvas> canvaslist = new Dictionary<CanvasType, Canvas>();
    private const bool canvasUnique = false;    // trueにすると一つのUIで一つのキャンバスを持つようになる



    public GameObject CreateUI(UIType type, bool multiple = false)
    {
        if (!multiple)
        {
            // 二重登録チェック
            for (int i = 0; i < uiobjects.Count; i++)
            {
                UISet uiset = uiobjects[i];
                if (uiset != null && uiset.UIType == type)
                {
                    Debug.LogError("タイプの二重登録です：type = " + type);

                    if (uiset.script == null && uiset.gameObject == null)
                    {
                        // なぜか完全に死んでるので許してあげる
                        Debug.LogError("なぜか完全に死んでるので許した：type = " + type);
                        uiobjects.RemoveAt(i);
                    }
                    else
                    {
                        return null;
                    }

                }
            }
        }

        GameObject obj = null;
        MonoBehaviour script = null;
        UISetFlag flag = 0;
        switch (type)
        {
            //  ----------------------------------------------------------------------------------

            case UIType.MainUI:

                //obj = createInstance(Resources.Load("Prefab/UI/Default/MainUI"));
                //script = obj.GetComponent<MainUI>();

                break;
            // 可以自行添加新的UI部件

            default:
                Debug.LogError("タイプ指定のされていないオブジェクトです：type = " + type);
                return null;
        }

        if (obj != null)
        {
            obj.name = obj.name.Replace("(Clone)", "");

            Transform parentobj = obj.transform.parent;
            Canvas canvas = null;
            if (parentobj != null)
            {
                canvas = parentobj.GetComponent<Canvas>();
            }

            if (script == null)
            {
                // スクリプトが指定されてないので強制でベースをくっつける
                UIScript uiscript = obj.AddComponent<UIScript>();
                uiscript.Type = type;
                uiscript.ParentGameObjectInstanceID = obj.GetInstanceID();

                script = uiscript as MonoBehaviour;
            }

            if (script)
            {
                UIScript uiscript = script as UIScript;
                if (uiscript)
                {
                    uiscript.ParentCanvas = canvas;
                    uiscript.Type = type;
                    uiscript.ParentGameObjectInstanceID = obj.GetInstanceID();
                }
            }

            uiobjects.Add(new UISet(obj.GetInstanceID(), type, obj, script, canvas, flag));

        }

        return obj;
    }

    /// <summary>
    /// 指定のキャンバスにUIオブジェクトを生成する
    /// </summary>
    /// <param name="newobj"></param>
    /// <param name="canvastype"></param>
    /// <returns></returns>
    private GameObject createInstance(UnityEngine.Object newobj, CanvasType canvastype = CanvasType.Default)
    {
        GameObject obj = null;
        Canvas canvas = null;

        if (canvasUnique || !canvaslist.ContainsKey(canvastype) || canvaslist[canvastype] == null)
        {
            // とりあえず全パターン同じ種類のキャンバス生成
            obj = Instantiate(Resources.Load("Prefab/UI/CanvasDefault") as GameObject);
            if (obj != null)
            {
                string name = "";

                canvas = obj.GetComponent<Canvas>();
                if (canvas != null)
                {

                    // 指定のキャンバスを生成する
                    switch (canvastype)
                    {
                        case CanvasType.Effect:
                            name = "CanvasEffect";
                            canvas.sortingOrder = 0;
                            break;
                        case CanvasType.Default:
                            name = "CanvasDefault";
                            canvas.sortingOrder = 1;
                            break;
                        case CanvasType.DEBUG:
                            name = "CanvasDEBUG";
                            canvas.sortingOrder = 2;
                            break;
                        case CanvasType.Menu:
                            name = "CanvasMenu";
                            canvas.sortingOrder = 3;
                            break;
                        case CanvasType.SystemL2:
                            name = "CanvasSystemL2";
                            canvas.sortingOrder = 4;
                            break;
                        case CanvasType.SuperDialog:
                            name = "CanvasSuperDialog";
                            canvas.sortingOrder = 10;
                            break;
                        default:
                            name = "Canvas";
                            break;
                    }

                    // 縦画面のときはスケールを変更する
                    if (Utils.IsScreenPortrait())
                    {
                        CanvasScaler canvasscaler = obj.GetComponent<CanvasScaler>();
                        canvasscaler.referenceResolution = new Vector2(canvasscaler.referenceResolution.y, canvasscaler.referenceResolution.x);
                    }

                    // UIカメラを設定する
                    GameObject camera = null;// Global.GameCameraManager.GetObjectCameraGameObject(GameCameraManager2.CameraObjectType.UI);
                    if (camera)
                    {
                        canvas.worldCamera = camera.GetComponent<Camera>();
                    }

                    // タッチを受け付ける？
                    for (int i = 0; i < notAcceptTouchCanvasType.Length; i++)
                    {
                        // タッチ無効キャンバスタイプリストに登録されていたら
                        if (notAcceptTouchCanvasType[i] == canvastype)
                        {
                            GraphicRaycaster raycaster = obj.GetComponent<GraphicRaycaster>();
                            if (raycaster != null)
                            {
                                raycaster.enabled = false;
                            }
                            break;
                        }
                    }

                    if (!canvasUnique)
                    {
                        if (canvaslist.ContainsKey(canvastype))
                        {
                            canvaslist[canvastype] = canvas;
                        }
                        else
                        {
                            canvaslist.Add(canvastype, canvas);
                        }
                    }
                }

                if (canvasUnique)
                {
                    obj.name = name + newobj.name;
                }
                else
                {
                    obj.name = name;
                }
            }

        }
        else
        {
            canvas = canvaslist[canvastype];
        }

        // もしキャンバスが見つからなかったらNULL
        if (canvas == null)
        {
            Debug.LogError("キャンバスが見つかりませんでした : " + canvastype);
            return null;
        }

        // カメラが見つからなかったら警告
        if (canvas.worldCamera == null)
        {
            Debug.LogError("カメラを設定できませんでした : " + canvastype);
        }

        obj = Instantiate(newobj as GameObject);

        obj.transform.SetParent(canvas.transform, false);

        // 作って返す
        return obj;
    }

    /// <summary>
    /// 指定されたタイプで指定されたインスタンスIDのUIを削除
    /// </summary>
    /// <param name="type"></param>
    /// <param name="instance_id"></param>
    /// <returns></returns>
    public bool CloseUI(UIType type, int instance_id)
    {
        foreach (UISet uiset in uiobjects)
        {
            if (uiset != null && uiset.UIType == type && uiset.InstanceID == instance_id)
            {
                removeUIObject(uiset);
                return true;
            }
        }
        return false;
    }

    private void removeUIObject(UISet uiset)
    {
        if (uiset != null)
        {
            // 破棄前事前処理
            if (uiset.script is UIScript)
                (uiset.script as UIScript).OnClosing();

            // 実際の破棄作業
            if (canvasUnique)
            {
                Destroy(uiset.Canvas.gameObject);
            }
            else
            {
                Destroy(uiset.gameObject);
            }

            uiobjects.Remove(uiset);
        }

    }
}
