using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.jimmychoi.shootingGame
{
    /// <summary>
    /// インスペクタへの名前の表示を置き換える
    /// </summary>
    public class ViewNameAttribute : PropertyAttribute
    {
        public string viewName = string.Empty;

        public ViewNameAttribute(string name)
        {
            viewName = name;
        }
    }


    /// <summary>
    /// インスペクタにヘルプボックスを表示させる(簡易実装)
    /// </summary>
    public class HelpBoxAttribute : PropertyAttribute
    {
        public string message = string.Empty;

        public HelpBoxAttribute(string msg)
        {
            message = msg;
        }
    }

    /// <summary>
    /// プレハブしか受け付けない
    /// </summary>
    public class PrefabOnlyAttribute : PropertyAttribute
    {
    }

    /// <summary>
    /// 列挙名に対応するフラグセット表示用
    /// </summary>
    public class EnumFlagsByAttribute : PropertyAttribute
    {
        public System.Type m_enumType = null;
        public string[] m_enumNames = null;

        public EnumFlagsByAttribute(System.Type type)
        {
            m_enumType = type;
            m_enumNames = System.Enum.GetNames(m_enumType);
        }

        public int EnumCount
        {
            get { return m_enumNames.Length; }
        }
        public string[] Names
        {
            get { return m_enumNames; }
        }
    }

    /// <summary>
    /// 表示変更総合
    /// </summary>
    public class EtCustomDraw : PropertyAttribute
    {
    }


    /// <summary>
    /// 列挙体名でラベルを上書き
    /// </summary>

    public class EtEnumeratedByAttribute : PropertyAttribute
    {
        public System.Type m_enumType = null;
        public string[] m_enumNames = null;

        public EtEnumeratedByAttribute(System.Type type)
        {
            m_enumType = type;
            m_enumNames = System.Enum.GetNames(m_enumType);
        }

        public string EnumTypeName
        {
            get { return m_enumType.Name; }
        }

        public int EnumSize
        {
            get { return m_enumNames.Length; }
        }

        public string[] Names
        {
            get { return m_enumNames; }
        }
    }
}
#if UNITY_EDITOR
namespace com.jimmychoi.shootingGame
{
    using UnityEditor;

    /// <summary>
    /// インスペクタへの名前の表示を置き換える
    /// </summary>
    [CustomPropertyDrawer(typeof(ViewNameAttribute))]
    public class ViewNameAttributeDrawer : PropertyDrawer
    {
        // note:
        // 一部のAttributeとの相性が悪い (Rangeなど)
        // 必要になり次第 追々対応
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var copyContent = new GUIContent(label);
            var viewName = attribute as ViewNameAttribute;

            if (viewName != null)
                copyContent.text = viewName.viewName;

            EditorGUI.PropertyField(position, property, copyContent);
        }
    }

    /// <summary>
    /// インスペクタにヘルプボックスを表示させる(簡易実装)
    /// </summary>
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : DecoratorDrawer
    {
        private const int EX_MARGIN = 5;

        public override void OnGUI(Rect position)
        {
            var messageAttr = attribute as HelpBoxAttribute;
            position.y += EditorStyles.helpBox.margin.top + EX_MARGIN;
            position.height = EditorStyles.helpBox.CalcHeight(new GUIContent(messageAttr.message), Screen.width) + EditorStyles.helpBox.padding.vertical;

            EditorGUI.HelpBox(position, messageAttr.message, MessageType.Info);
        }
        public override float GetHeight()
        {
            var messageAttr = attribute as HelpBoxAttribute;
            var content = new GUIContent(messageAttr.message);

            return EditorStyles.helpBox.CalcHeight(new GUIContent(messageAttr.message), Screen.width)
                    + EditorStyles.helpBox.margin.vertical
                    + EditorStyles.helpBox.padding.vertical
                    + (EX_MARGIN * 2);
        }
    }

    /// <summary>
    /// プレハブしか受け付けない
    /// ref: http://qiita.com/r-ngtm/items/7bafd2ff45d635e07c3f
    /// </summary>
    [CustomPropertyDrawer(typeof(PrefabOnlyAttribute))]
    public class PrefabOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null)
            {
                var prefabType = PrefabUtility.GetPrefabType(property.objectReferenceValue);
                switch (prefabType)
                {
                    case PrefabType.Prefab:
                    case PrefabType.ModelPrefab:
                        break;
                    default:
                        // Prefab以外がアタッチされた場合アタッチを外す
                        property.objectReferenceValue = null;
                        break;
                }
            }
            label.text += " (Prefab Only)";
            EditorGUI.PropertyField(position, property, label);
        }
    }

    /// <summary>
    /// 描画変更（テスト中）
    /// </summary>
    [CustomPropertyDrawer(typeof(EtCustomDraw))]
    public class EtCustomDrawDrawer : PropertyDrawer
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
            m_isInitialized = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PreSetup(property, label);

            if (m_enumeratedAttr != null)
            {
                label.text = m_enumeratedAttr.GiveViewName(property, defaultName: label.text);
            }
            EditorGUI.PropertyField(position, property, label);
        }
    }


    /// <summary>
    /// 列挙名に対応するフラグセット表示用
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumFlagsByAttribute))]
    public class EnumFlagsByAttributeDrawer : PropertyDrawer
    {
        /// <summary> インスペクタへ表示 </summary>
        // -------------------------------------------------
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnGUIAndCalcHeight(position, property, label);
        }

        /// <summary> インスペクタへの表示と表示高さの計算 </summary>
        // -------------------------------------------------
        private float OnGUIAndCalcHeight(Rect position, SerializedProperty property, GUIContent label, bool bDraw = true)
        {
            float totalHeight = 0;

            var labelArea = EditorGUI.IndentedRect(position);
            labelArea.height = EditorStyles.foldout.CalcSize(label).y;
            totalHeight = labelArea.height;

            label.text += ":";

            EditorGUI.BeginProperty(position, label, property);
            {
                if (bDraw)
                    EditorGUI.LabelField(labelArea, label);

                EditorGUI.indentLevel += 1;
                Rect contentRect = EditorGUI.IndentedRect(position);
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
        // -------------------------------------------------
        private float OnGUIContentAndCalcHeight(Rect position, SerializedProperty property, bool bDraw = true)
        {
            long flagsSet = GetFlagsSet(property);
            float totalHeight = 0;

            var exAttribute = attribute as EnumFlagsByAttribute;
            for (int i = 0; i < exAttribute.EnumCount; ++i)
            {
                bool b = (flagsSet & (1 << i)) != 0;
                int index = i; // note: ラムダでキャプチャされるIndex値がすべておなじ値になると解釈されるのを防ぐ

                totalHeight += UtilsCustomProperty.DrawToggle(exAttribute.Names[index], b, ref position, bDraw,
                    (result) =>
                    {
                        flagsSet ^= (1 << index);
                        SetFlagsSet(property, flagsSet);
                    });
            }
            return totalHeight;
        }


        /// <summary> この設定項目の高さ </summary>
        // -------------------------------------------------
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return OnGUIAndCalcHeight(Rect.zero, property, label, bDraw: false);
        }

        /// <summary> フラグセット値の取得 </summary>
        // -------------------------------------------------
        private long GetFlagsSet(SerializedProperty property)
        {
            return property.longValue;
        }
        /// <summary> フラグセット値の設定</summary>
        // -------------------------------------------------
        private void SetFlagsSet(SerializedProperty property, long value)
        {
            property.longValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

    }

    /// <summary>
    /// 列挙体名でラベルを上書きサポート用
    /// </summary>
    public class EnumeratedAttributeHelper
    {
        private EtEnumeratedByAttribute m_enumeratedAttr = null;

        public EnumeratedAttributeHelper(System.Reflection.FieldInfo info)
        {
            var objs = info.GetCustomAttributes(typeof(EtEnumeratedByAttribute), false);

            if (objs != null && objs.Length > 0)
                m_enumeratedAttr = objs[0] as EtEnumeratedByAttribute;
        }

        public string GiveViewName(SerializedProperty componentProperty, string defaultName)
        {
            if (m_enumeratedAttr == null)
                return defaultName;

            int index = GetIndex(componentProperty);
            if (index == -1)
                return defaultName;

            if (index >= m_enumeratedAttr.EnumSize)
                return defaultName;

            return m_enumeratedAttr.Names[index];
        }

        private int GetIndex(SerializedProperty property)
        {
            var elem = property;
            var path = elem.propertyPath;
            var num = path.Substring(path.LastIndexOf('[') + 1);
            num = num.Substring(0, num.Length - 1);
            int index = 0;
            if (!int.TryParse(num, out index))
            {
                Debug.LogWarningFormat("Failed to parse int from: {0}, originally: {1}", num, path);
                return -1;
            }
            return index;
        }

        public void SetArraySize(SerializedProperty property)
        {
            var elem = property;
            var path = elem.propertyPath;
            var subPath = path.Substring(0, path.LastIndexOf(".data["));
            var arraySizePath = subPath + ".size";

            property.serializedObject.FindProperty(arraySizePath).intValue = m_enumeratedAttr.EnumSize;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    public static class UtilsCustomProperty
    {
        /// <summary> チェックボタンの描画 </summary>
        // -------------------------------------------------
        public static float DrawToggle(string labelstring, bool value, ref Rect drawArea, bool bDraw = true, System.Action<bool> OnChanged = null)
        {
            return DrawToggle(new GUIContent(labelstring), value, ref drawArea, bDraw, OnChanged);
        }
        // -------------------------------------------------
        public static float DrawToggle(GUIContent label, bool value, ref Rect drawArea, bool bDraw = true, System.Action<bool> OnChanged = null)
        {
            drawArea.y += EditorStyles.toggle.padding.top;
            drawArea.height = EditorStyles.toggle.CalcSize(label).y;

            if (bDraw)
            {
                if (GUI.Toggle(drawArea, value, label) != value)
                {
                    if (OnChanged != null)
                        OnChanged(!value);
                }
            }
            drawArea.y += drawArea.height +
                          EditorStyles.toggle.padding.bottom;

            return drawArea.height +
                    EditorStyles.toggle.padding.vertical;
        }
    }
}
#endif