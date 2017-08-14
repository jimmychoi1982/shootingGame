using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace com.jimmychoi.shootingGame.ui.utility
{
    /// <summary>
    /// 簡易プレハブ製造機
    /// </summary>
    [System.Serializable]
    public struct PrefabInstantiater
    {
        [SerializeField, PrefabOnly, Tooltip("複製対象となるプレハブです")]
        private GameObject m_prefab;

        [SerializeField, Tooltip("この直下に、複製されたプレハブを「子」として配置します")]
        private Transform m_parent;

        [SerializeField, Tooltip("親のトランスフォームを用いて、複製されたプレハブのトランスフォームを変更するかを指定します")]
        private bool m_worldPositionStays;

        [SerializeField, HideInInspector]
        private GameObject m_createdInstance;

        // 通常変数
        public GameObject Instance 
        { 
            get { return m_createdInstance; }
            private set { m_createdInstance = value; } 
        }

        // アクセサ
        public Transform TargetParent
        {
            get { return m_parent; }
            set { m_parent = value; }
        }

        // 旧式
        public GameObject GetInstance()
        {
            return Instance;
        }

        public GameObject Instantiate()
        {
            if (m_prefab == null)
            {
                Debug.LogError("プレハブが設定されていないようです！");
                return null;
            }

            Instance = Object.Instantiate(m_prefab, m_parent, m_worldPositionStays);
            return Instance;
        }
        public T InstantiateComponent<T>()
            where T : Component
        {
            if (m_prefab == null)
            {
                Debug.LogError("プレハブが設定されていないようです！");
                return null;
            }

            return (Instantiate() != null) ? Instance.GetComponent<T>() : null;
        }

        public GameObject SafeInstantiate()
        {
            if (GetInstance() == null)
            {
                return Instantiate();
            }
            return GetInstance();
        }
        public T SafeInstantiateComponent<T>()
            where T : Component
        {
            if (m_prefab == null)
                return null;

            return (SafeInstantiate() != null) ? Instance.GetComponent<T>() : null;
        }

        public void Destroy()
        {
            if (Instance != null)
            {
                Object.Destroy(Instance);
                Instance = null;
            }
        }

        // note: warning 抑制のため
        private PrefabInstantiater(GameObject obj, Transform parent, bool isPosStay)
        {
            m_prefab = obj;
            m_parent = parent;
            m_worldPositionStays = isPosStay;
            m_createdInstance = null;
        }

        /// <summary>
        /// UIを新規生成の為の処理、
        /// m_prefab,m_parentを設定する必要があるので、このメソッドを作った
        /// 具体的な処理が「周りPCゲージを生成」です。
        /// </summary>
        public void SetPrefabTo(GameObject go, Transform parent)
        {
            m_prefab = go;
            m_parent = parent;
        }
    }

    // todo 重複処理多し。共通化

    /// <summary>
    /// プール付きプレハブ複製機
    /// </summary>
    #region  public class PrefabPool...

    [System.Serializable]
    public class PrefabPool : IGameObjectPool
    {
        [SerializeField, PrefabOnly, Tooltip("複製対象となるプレハブです")]
        private GameObject m_prefab = null;

        [SerializeField, Tooltip("この直下に、複製されたプレハブを「子」として配置します")]
        public Transform m_parent = null;

        [SerializeField, Tooltip("親のトランスフォームを用いて、複製されたプレハブのトランスフォームを変更するかを指定します")]
        private bool m_worldPositionStays = false;

        [System.NonSerialized]
        private ConvexBody.Pool<GameObject> m_pool = null;

        //  アクセサ
        public Transform TargetParent
        {
            get { return m_parent; }
            set { m_parent = value; }
        }

        public PrefabPool()
        {
            m_pool = new ConvexBody.Pool<GameObject>(CreateFunction);
        }
        public GameObject Borrow()
        {
            if (m_prefab == null)
            {
                Debug.LogError("プレハブが設定されていないようです！");
                return null;
            }

            var obj = m_pool.Borrow();
            if (obj)
            {
                obj.SetActive(true);
            }
            return obj;
        }
        public GameObject BorrowIf(System.Func<GameObject, bool> initFunc)
        {
            if (m_prefab == null)
            {
                Debug.LogError("プレハブが設定されていないようです！");
                return null;
            }

            var obj = m_pool.Borrow();
            if (obj == null)
            {
                return null;
            }
            obj.SetActive(true);

            // ユーザ定義初期化
            if (initFunc != null)
            {
                if (!initFunc(obj))
                {
                    Return(obj);
                    return null;
                }
            }
            return obj;
        }
        public T Borrow<T>()
            where T : Component
        {
            var obj = Borrow();
            if (obj == null)
                return null;

            var script = obj.GetComponent<T>();
            if (script == null)
            {
                Return(obj);
                return null;
            }
            return script;
        }

        public T BorrowIf<T>(System.Func<T, bool> initFunc)
            where T : Component
        {
            var obj = Borrow();
            var script = obj.GetComponent<T>();
            if (script == null)
            {
                Return(obj);
                return null;
            }
            if (initFunc != null)
            {
                if (!initFunc(script))
                {
                    Return(obj);
                    return null;
                }
            }
            return script;
        }

        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            m_pool.Return(obj);
        }

        public void Reserve(int count)
        {
            var tmpArray = new GameObject[count];

            // 作って返すだけ
            for (int i = 0; i < tmpArray.Length; ++i)
                tmpArray[i] = m_pool.Borrow();

            for (int i = 0; i < tmpArray.Length; ++i)
                Return(tmpArray[i]);
        }

        private GameObject CreateFunction()
        {
            // 複製
            var obj = Object.Instantiate(m_prefab, m_parent, m_worldPositionStays);
            return obj;
        }
    }
    #endregion

    /// <summary>
    /// プール付きで編集が可能なプレハブ複製機
    /// </summary>
    #region  public class CustomPrefabPool...

    [System.Serializable]
    public class CustomPrefabPool : IGameObjectPool
    {
        // シリアライズ変数
        [SerializeField, PrefabOnly, Tooltip("複製対象となるプレハブです")]
        private GameObject m_prefab = null;

        [SerializeField, Tooltip("唯一 作成される 複製元となるオブジェクトが配置される先を指定します")]
        public Transform m_prefabPlaceTarget = null;

        [SerializeField, Tooltip("この直下に、複製されたプレハブを「子」として配置します")]
        public Transform m_clonePlaceTarget = null;

        [SerializeField, Tooltip("親のトランスフォームを用いて、複製されたプレハブのトランスフォームを変更するかを指定します")]
        private bool m_worldPositionStays = false;

        [System.NonSerialized]
        private ConvexBody.Pool<GameObject> m_pool = null;

        [System.NonSerialized]
        private GameObject m_placedPrefab = null;

        //  アクセサ
        public bool IsPlacedPrefabCreated { get { return m_placedPrefab == null; } }

        public GameObject Prefab
        {
            get
            {
                if (m_placedPrefab == null)
                {
                    m_placedPrefab = CreatePlacedPrefab();
                }
                return m_placedPrefab;
            }
        }

        public CustomPrefabPool()
        {
            m_pool = new ConvexBody.Pool<GameObject>(CreateCloneFunction);
        }
        public GameObject Borrow()
        {
            if (m_prefab == null)
            {
                Debug.LogError("プレハブが設定されていないようです！");
                return null;
            }

            var obj = m_pool.Borrow();
            if (obj != null)
            {
                obj.SetActive(true);
            }
            return obj;
        }

        public T Borrow<T>()
            where T : Component
        {
            var obj = Borrow();
            if (obj == null)
                return null;

            var script = obj.GetComponent<T>();
            if (script == null)
            {
                Return(obj);
                return null;
            }
            return script;
        }
        public T BorrowIf<T>(System.Func<T, bool> initFunc)
            where T : Component
        {
            var obj = Borrow();
            var script = obj.GetComponent<T>();
            if (script == null)
            {
                Return(obj);
                return null;
            }
            if (initFunc != null)
            {
                if (!initFunc(script))
                {
                    Return(obj);
                    return null;
                }
            }
            return script;
        }

        public GameObject BorrowIf(System.Func<GameObject, bool> initFunc)
        {
            var obj = m_pool.Borrow();
            if (obj == null)
            {
                return null;
            }
            obj.SetActive(true);

            // ユーザ定義初期化
            if (initFunc != null)
            {
                if (!initFunc(obj))
                {
                    obj.SetActive(false);
                    m_pool.Return(obj);
                    return null;
                }
            }
            return obj;
        }
        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            m_pool.Return(obj);
        }
        public void Reserve(int count)
        {
            var tmpArray = new GameObject[count];

            // 作って返すだけ
            for (int i = 0; i < tmpArray.Length; ++i)
                tmpArray[i] = m_pool.Borrow();

            for (int i = 0; i < tmpArray.Length; ++i)
                Return(tmpArray[i]);
        }

        private GameObject CreatePlacedPrefab()
        {
            if (m_prefab == null)
            {
                Debug.Log("***error: 複製元のプレハブが設定されていません");
                return null;
            }

            // プレハブから複製
            var obj = Object.Instantiate(m_prefab);
            obj.name = obj.name.Replace("(Clone)", "");

            // 親子関係を構築
            if (m_prefabPlaceTarget != null)
            {
                obj.transform.SetParent(m_prefabPlaceTarget, m_worldPositionStays);
            }
            return obj;
        }
        public void Destroy()
        {
            var enumer = m_pool.GetEnumerator();
            while (enumer.MoveNext())
            {
                GameObject.Destroy(enumer.Current);
            }
            m_pool.Clear();
        }

        public void PrefabOverride(GameObject obj)
        {
            // 既存のオブジェクトを破棄
            Destroy();

            // 差し替え作業
            obj.transform.SetParent(m_prefabPlaceTarget, m_worldPositionStays);
            m_placedPrefab = obj;
        }

        private GameObject CreateCloneFunction()
        {
            // 配置されたプレハブの複製から複製
            var obj = Object.Instantiate(Prefab, m_clonePlaceTarget, m_worldPositionStays);
            return obj;
        }
    }
    #endregion

    /// <summary>
    /// プレハブじゃないオブジェクト用複製機
    /// </summary>
    #region  public class ClonePool...

    [System.Serializable]
    public class ClonePool : IGameObjectPool
    {
        [SerializeField, Tooltip("複製対象となるオブジェクトです")]
        private GameObject m_originalObject = null;

        [SerializeField, Tooltip("この直下に、複製されたプレハブを「子」として配置します。nullの場合、OriginalObjectのParentが扱われます")]
        public Transform m_clonePlaceTarget = null;

        [SerializeField, Tooltip("既に複製済みのオブジェクトがあればここに登録できます。")]
        private GameObject[] m_clonedObject = null;

        [SerializeField, Tooltip("親のトランスフォームを用いて、複製されたプレハブのトランスフォームを変更するかを指定します")]
        private bool m_worldPositionStays = false;

        [System.NonSerialized]
        private ConvexBody.Pool<GameObject> m_pool = null;

        //  アクセサ
        public GameObject Original
        {
            get { return m_originalObject; }
        }
        public Transform TargetParent
        {
            get { return (m_clonePlaceTarget != null) ? m_clonePlaceTarget : m_originalObject.transform.parent; }
        }

        public ClonePool()
        {
            m_pool = new ConvexBody.Pool<GameObject>(CreateFunction);
        }

        public void InitializeForGame()
        {
            if (m_originalObject != null)
                m_originalObject.SetActive(false);            

            m_pool.Clear();
            if (m_clonedObject != null)
            {
                foreach (var clone in m_clonedObject)
                    Return(clone);
            }
        }

        public GameObject Borrow()
        {
            if (m_originalObject == null)
            {
                Debug.Log("***error: 複製元が設定されていません！");
                return null;
            }

            var obj = m_pool.Borrow();
            if (obj)
            {
                obj.SetActive(true);
            }
            return obj;
        }
        public GameObject BorrowIf(System.Func<GameObject, bool> initFunc)
        {
            if (m_originalObject == null)
            {
                Debug.Log("***error: 複製元が設定されていません！");
                return null;
            }

            var obj = m_pool.Borrow();
            if (obj == null)
            {
                return null;
            }
            obj.SetActive(true);

            // ユーザ定義初期化
            if (initFunc != null)
            {
                if (!initFunc(obj))
                {
                    Return(obj);
                    return null;
                }
            }
            return obj;
        }
        public T Borrow<T>()
            where T : Component
        {
            var obj = Borrow();
            var script = obj.GetComponent<T>();
            if (script == null)
            {
                Return(obj);
                return null;
            }
            return script;
        }
        public T BorrowIf<T>(System.Func<T, bool> initFunc)
            where T : Component
        {
            var obj = Borrow();
            var script = obj.GetComponent<T>();
            if (script == null)
            {
                Return(obj);
                return null;
            }
            if (initFunc != null)
            {
                if (!initFunc(script))
                {
                    Return(obj);
                    return null;
                }
            }
            return script;
        }

        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            m_pool.Return(obj);
        }

        public void Reserve(int count)
        {
            var tmpArray = new GameObject[count];

            // 作って返すだけ
            for (int i = 0; i < tmpArray.Length; ++i)
                tmpArray[i] = m_pool.Borrow();

            for (int i = 0; i < tmpArray.Length; ++i)
                Return(tmpArray[i]);
        }

        private GameObject CreateFunction()
        {
            // 複製
            var obj = Object.Instantiate(m_originalObject, TargetParent, m_worldPositionStays);
            return obj;
        }
    }
    #endregion

    public interface IGameObjectPool
    {
        GameObject Borrow();
        GameObject BorrowIf(System.Func<GameObject, bool> initFunc);
        T Borrow<T>() where T : Component;
        T BorrowIf<T>(System.Func<T, bool> initFunc) where T : Component;

        void Return(GameObject obj);
        void Reserve(int count);
    }

    /// <summary>
    /// 貸し出し中オブジェクト管理機能付きプール
    /// </summary>
    #region  public class ManasablePool(GameObject)...

    [System.Serializable]
    public class BasicManasablePool<Tpool>
        where Tpool : IGameObjectPool, new()
    {
        [SerializeField]
        private Tpool m_pool = new Tpool();

        [System.NonSerialized]
        private List<GameObject> m_activeList = new List<GameObject>();

        // アクセサ
        public Tpool Pool { get { return m_pool; } }

        public List<GameObject> ActiveList { get { return m_activeList; } }

        public GameObject Borrow()
        {
            var obj = m_pool.Borrow();
            if (obj == null)
                return null;

            m_activeList.Add(obj);
            return obj;
        }
        public GameObject BorrowIf(System.Func<GameObject, bool> initFunc)
        {
            var obj = m_pool.BorrowIf(initFunc);
            if (obj == null)
                return null;

            m_activeList.Add(obj);
            return obj;
        }
        public T Borrow<T>()
            where T : Component
        {
            var obj = Borrow();
            if (obj == null)
                return null;

            var script = obj.GetComponent<T>();
            if (script == null)
            {
                Return(obj);
                return null;
            }
            return script;
        }

        public void Return(GameObject obj)
        {
            if (!m_activeList.Remove(obj))
            {
                // todo なんか警告
            }
            m_pool.Return(obj);
        }
        public void Return(int index)
        {
            Return(m_activeList[index]);
        }
        public void ReturnAll()
        {
            foreach (var i in m_activeList)
                m_pool.Return(i.gameObject);

            m_activeList.Clear();
        }
    }
    [System.Serializable] public class PrefabManasablePool       : BasicManasablePool<PrefabPool>{};
    [System.Serializable] public class CustomPrefabManasablePool : BasicManasablePool<CustomPrefabPool> { };
    [System.Serializable] public class CloneManasablePool        : BasicManasablePool<ClonePool>
    {
        public void InitializeForGame()
        {
            if(Pool != null)
                Pool.InitializeForGame();
        }
    };
    #endregion
    // -------------------------------------
    #region  public class ManasablePool(Component)...

    [System.Serializable]
    public class BasicManasablePool<Tpool, Tcomponent> 
        where Tpool  : IGameObjectPool, new()
        where Tcomponent : Component
    {
        [SerializeField]
        private Tpool m_pool = new Tpool(); 

        [System.NonSerialized]
        private List<Tcomponent> m_activeList = new List<Tcomponent>();

        // アクセサ
        public Tpool Pool { get { return m_pool; } }
        public List<Tcomponent> ActiveList { get { return m_activeList; } }

        public Tcomponent Borrow()
        {
            var script = m_pool.Borrow<Tcomponent>();
            if (script == null)
                return null;

            m_activeList.Add(script);
            return script;
        }
        public Tcomponent BorrowIf(System.Func<Tcomponent, bool> initFunc)
        {
            var script = m_pool.BorrowIf(initFunc);
            if (script == null)
                return null;

            m_activeList.Add(script);
            return script;
        }
        public void Return(Tcomponent script)
        {
            if (!m_activeList.Remove(script))
            {
                // todo なんか警告
            }
            m_pool.Return(script.gameObject);
        }
        public void Return(int index)
        {
            Return(m_activeList[index]);
        }
        public void ReturnAll()
        {
            foreach(var i in m_activeList)
                m_pool.Return(i.gameObject);

            m_activeList.Clear();
        }
    }
    #endregion

} //end namespace 


#region Editor
#if UNITY_EDITOR
namespace com.jimmychoi.shootingGame
{
    using UnityEditor;
    using ui.utility;

    /// <summary>
    /// 簡易プレハブ製造機(インスペクタ表示制御用)
    /// note: カオス。要整理 もう少し工夫できそうな気はする
    /// </summary>
    [CustomPropertyDrawer(typeof(PrefabInstantiater))]
    public class PrefabInstantiaterDrawer : PropertyDrawer
    {
        private bool m_isInitialized = false;

        // 追加のサポートアトリビュート
        private EnumeratedAttributeHelper m_enumeratedAttr = null;

        /// <summary> インスペクタ表示前準備 </summary>
        // -------------------------------------------------
        private void PreSetup(SerializedProperty property, GUIContent label)
        {
            if (m_isInitialized)
                return;

            m_enumeratedAttr = new EnumeratedAttributeHelper(fieldInfo);
            m_isInitialized  = true;
        }

        /// <summary> インスペクタへ表示 </summary>
        // -------------------------------------------------
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PreSetup(property, label);
            OnGUIAndCalcHeight(position, property, label);
        }

        /// <summary> この設定項目の高さ </summary>
        // -------------------------------------------------
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PreSetup(property, label);
            return OnGUIAndCalcHeight(Rect.zero, property, label, bDraw:false);
        }

        /// <summary> インスペクタへの表示と表示高さの計算 </summary>
        // -------------------------------------------------
        private float OnGUIAndCalcHeight(Rect position, SerializedProperty property, GUIContent label, bool bDraw = true)
        {
            float totalHeight = 0;

            var labelArea = CalcLabelDrawArea(position, label);
            totalHeight   = labelArea.height;

            EditorGUI.BeginProperty(position, label, property);
            label.text = m_enumeratedAttr.GiveViewName(property, defaultName: label.text);

            if (bDraw)
                property.isExpanded = EditorGUI.Foldout(labelArea, property.isExpanded, label);

            if (property.isExpanded)
            {
                // コンテンツの表示域 (１行下)
                EditorGUI.indentLevel += 1;
                Rect contentRect = new Rect(position);
                contentRect.y += EditorStyles.foldout.CalcSize(label).y + EditorStyles.foldout.padding.top;

                totalHeight += EditorStyles.foldout.padding.top;
                totalHeight += OnGUIContentAndCalcHeight(contentRect, property, bDraw);
                totalHeight += EditorStyles.foldout.padding.bottom;
                totalHeight += EditorStyles.foldout.margin.bottom;
                EditorGUI.indentLevel -= 1;
            }
            EditorGUI.EndProperty();
            return totalHeight;
        }

        /// <summary> インスペクタへの表示と表示高さの計算 </summary>
        // -------------------------------------------------
        private float OnGUIContentAndCalcHeight(Rect position, SerializedProperty property, bool bDraw = true)
        {
            float totalHeight = 0;

            var drawArea = position;

            totalHeight += DrawObjectField(property.FindPropertyRelative("m_prefab"), ref drawArea, bDraw);
            totalHeight += DrawObjectField(property.FindPropertyRelative("m_parent"), ref drawArea, bDraw);
            totalHeight += DrawToggle(property.FindPropertyRelative("m_worldPositionStays"), ref drawArea, bDraw);

            // ここからボタンの表示とか
            {
                var leftArea   = EditorGUI.IndentedRect(drawArea);
                leftArea.width = leftArea.width / 2;

                var rightArea  = new Rect(leftArea);
                rightArea.x   += leftArea.width;

                var leftHaight  = DrawButton("事前生成", ref leftArea, () => PreCreate(property), bDraw);
                var rightHaight = DrawButton("破棄", ref rightArea, () => PreDestroy(property), bDraw);

                totalHeight += Mathf.Max(leftHaight, rightHaight);
                drawArea.y  += Mathf.Max(leftHaight, rightHaight);
            }
            return totalHeight;
        }

        #region callback系...

        /// <summary> [callback] 事前にプレハブ生成の要求 </summary>
        // -------------------------------------------------
        private void PreCreate(SerializedProperty property)
        {
            try
            {
                // 既にある
                var instanceProperty = property.FindPropertyRelative("m_createdInstance");
                if (instanceProperty.objectReferenceValue != null)
                    return;

                var prefab = property.FindPropertyRelative("m_prefab").objectReferenceValue as GameObject;
                var parent = property.FindPropertyRelative("m_parent").objectReferenceValue as RectTransform;
                var isStay = property.FindPropertyRelative("m_worldPositionStays").boolValue;

                instanceProperty.objectReferenceValue =
                    Object.Instantiate(prefab, parent, isStay);

                instanceProperty.serializedObject.ApplyModifiedProperties();
            }
            catch( System.Exception e)
            {
                Debug.LogError(e);
            }
        }
        /// <summary> [callback] 事前に生成したプレハブの破棄 </summary>
        // -------------------------------------------------
        private void PreDestroy(SerializedProperty property)
        {
            try
            {
                var instanceProperty = property.FindPropertyRelative("m_createdInstance");
                if (instanceProperty.objectReferenceValue == null)
                    return;

                Object.DestroyImmediate(instanceProperty.objectReferenceValue);
                instanceProperty.serializedObject.ApplyModifiedProperties();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
        #endregion

        private static Rect CalcLabelDrawArea(Rect basePosition, GUIContent label)
        {
            var position = EditorGUI.IndentedRect(basePosition);
            position.height = EditorStyles.foldout.CalcSize(label).y;
            return position;
        }
        private static Rect CalcButtonDrawArea(Rect basePosition, GUIContent label)
        {
            var position = EditorGUI.IndentedRect(basePosition);
            position.height = EditorStyles.miniButton.CalcSize(label).y +
                              EditorStyles.miniButton.padding.vertical;
            return position;
        }


        // todo ↓ 気が向いたら汎用化
        private static float DrawObjectField(SerializedProperty componentProperty, ref Rect drawArea, bool bDraw = true)
        {
            if (componentProperty == null)
                return 0;

            drawArea.y += EditorStyles.objectField.padding.top;
            drawArea.height = EditorGUI.GetPropertyHeight(componentProperty);

            if(bDraw)
                EditorGUI.PropertyField(drawArea, componentProperty);

            drawArea.y += drawArea.height +
                          EditorStyles.objectField.padding.bottom;

            return drawArea.height +
                    EditorStyles.objectField.padding.vertical;
        }

        private static float DrawToggle(SerializedProperty componentProperty, ref Rect drawArea, bool bDraw = true)
        {
            if (componentProperty == null)
                return 0;

            drawArea.y += EditorStyles.toggle.padding.top;
            drawArea.height = EditorGUI.GetPropertyHeight(componentProperty);

            if(bDraw)
                EditorGUI.PropertyField(drawArea, componentProperty);

            drawArea.y += drawArea.height +
                          EditorStyles.toggle.padding.bottom;

            return drawArea.height +
                    EditorStyles.toggle.padding.vertical;
        }
        private static float DrawButton(string labelstring, ref Rect drawArea, System.Action OnPushed = null, bool bDraw = true)
        {
            return DrawButton(new GUIContent(labelstring), ref drawArea, OnPushed, bDraw);
        }
        private static float DrawButton(GUIContent label, ref Rect drawArea, System.Action OnPushed = null, bool bDraw = true)
        {
            drawArea.y += EditorStyles.miniButton.margin.top;

            drawArea.x     += EditorStyles.miniButton.margin.left;
            drawArea.width -= EditorStyles.miniButton.margin.horizontal;

            // note: 少し大きくする
            drawArea.height = EditorStyles.miniButton.CalcSize(label).y + EditorStyles.miniButton.padding.vertical;
            if (bDraw)
            {
                if (GUI.Button(drawArea, label))
                {
                    if (OnPushed != null)
                        OnPushed();
                }
            }
            drawArea.y += drawArea.height +
                          EditorStyles.miniButton.margin.bottom;

            return drawArea.height +
                    EditorStyles.toggle.margin.vertical;
        }
        private static float DrawLabel(string text, ref Rect drawArea, bool bDraw = true)
        {
            var content     = new GUIContent(text);

            drawArea.y     += EditorStyles.label.padding.top;
            drawArea.height = EditorStyles.label.CalcSize(content).y;

            if (bDraw)
                EditorGUI.LabelField(drawArea, content);

            drawArea.y += drawArea.height +
                          EditorStyles.label.padding.bottom;

            return drawArea.height +
                    EditorStyles.label.padding.vertical;
        }

    }
}
#endif
#endregion