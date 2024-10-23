using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public sealed class MenuItemRenamer : EditorWindow
{
    /*-<MenuItemRenamer.cs>-------
    オブジェクト名称の一括変更ツール
    ----------------------------*/

    //=======================================================================================================
    //. 定数
    //=======================================================================================================
    #region -- 定数

    const string MENU_PATH = "ExTools/Object Renamer";
    const string TOOL_NAME = "⇄ Object Renamer";

    static readonly string[] TAB_NAMES = new string[] {
            "Add",
            "Replace",
            "SerialNum",
            "Delete"
        };

    #endregion

    //=======================================================================================================
    //. enum
    //=======================================================================================================
    #region -- enum

    enum ETab
    {
        Add,
        Replace,
        SerialNum,
        Delete
    }

    enum EAddPopup
    {
        Prefix,
        Suffix
    }

    enum ESerialNumPopup
    {
        Prefix,
        Suffix
    }

    enum EDeletePopup
    {
        Prefix,
        Suffix,
        Select
    }

    #endregion

    //=======================================================================================================
    //. メンバ
    //=======================================================================================================
    #region -- フィールド

    Vector2 m_BeforeScrollPos = Vector2.zero;
    Vector2 m_AfterScrollPos = Vector2.zero;
    List<string> m_AfterObjNames = new List<string>();
    ETab m_SelecteTab = ETab.Add;

    // Add
    int m_AddPosition = 0;
    string m_AddText = "";
    EAddPopup m_AddPopup = EAddPopup.Prefix;

    // Replace
    string m_ReplaceBeforeText = "";
    string m_ReplaceAfterText = "";

    // SerialNum
    int m_SerialNumStart = 0;
    int m_SerialNumDigit = 1;
    bool m_FlagOverride = false;
    string m_OverrideText = "";
    ESerialNumPopup m_SerialNumPopup = ESerialNumPopup.Suffix;

    // Delete
    int m_DeleteNumStart = 0;
    int m_DeleteNumDigit = 1;
    EDeletePopup m_DeletePopup = EDeletePopup.Prefix;

    #endregion

    //=======================================================================================================
    //. イベント
    //=======================================================================================================
    #region -- イベント

    /// <summary>
    /// メニューのWindowに追加
    /// </summary>
    [MenuItem(MENU_PATH, priority = 21)]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow<MenuItemRenamer>(TOOL_NAME);
    }

    /// <summary>
    /// Hierarchyで選択時
    /// </summary>
    void OnSelectionChange()
    {
        //再描画
        Repaint();
    }

    #endregion

    //=======================================================================================================
    //. UI
    //=======================================================================================================
    #region -- UI

    /// <summary>
    /// メイン描画処理
    /// </summary>
    void OnGUI()
    {
        m_SelecteTab = (ETab)GUILayout.Toolbar((int)m_SelecteTab, TAB_NAMES, EditorStyles.toolbarButton);

        switch (m_SelecteTab)
        {
            case ETab.Add:
                LayoutAdd();
                break;
            case ETab.Replace:
                LayoutReplace();
                break;
            case ETab.SerialNum:
                LayoutSerialNum();
                break;
            case ETab.Delete:
                LayoutDelete();
                break;
            default:
                break;
        }

        LayoutPreview();

        EditorGUILayout.Space();
        EditorGUIEx.DrawSeparator();
        EditorGUILayout.Space();

        if (GUILayout.Button("Rename"))
        {
            Rename();
        }
        EditorGUILayout.Space();
    }

    /// <summary>
    /// 追加のレイアウト設定
    /// </summary>
    void LayoutAdd()
    {
        if (EditorGUIEx.BeginGroup("Add : Options"))
        {
            m_AddPopup = (EAddPopup)EditorGUILayout.EnumPopup("Prefix/Suffix", (System.Enum)m_AddPopup);
            m_AddPosition = EditorGUILayout.IntField("Digit", m_AddPosition);
            m_AddText = EditorGUILayout.TextField("Add Test", m_AddText);

            EditorGUIEx.EndGroup();
        }
    }

    /// <summary>
    /// 置換のレイアウト設定
    /// </summary>
    void LayoutReplace()
    {
        if (EditorGUIEx.BeginGroup("Replace : Options"))
        {
            m_ReplaceBeforeText = EditorGUILayout.TextField("Before", m_ReplaceBeforeText);
            m_ReplaceAfterText = EditorGUILayout.TextField("After", m_ReplaceAfterText);

            EditorGUIEx.EndGroup();
        }
    }

    /// <summary>
    /// 連番のレイアウト設定
    /// </summary>
    void LayoutSerialNum()
    {
        if (EditorGUIEx.BeginGroup("SerialNum : Options"))
        {
            m_FlagOverride = EditorGUILayout.Toggle("Is Override", m_FlagOverride);
            if (m_FlagOverride)
            {
                m_OverrideText = EditorGUILayout.TextField("Replace Text", m_OverrideText);
            }
            m_SerialNumPopup = (ESerialNumPopup)EditorGUILayout.EnumPopup("Prefix/Suffix", (System.Enum)m_SerialNumPopup);
            m_SerialNumStart = EditorGUILayout.IntField("Start Num", m_SerialNumStart);
            m_SerialNumDigit = EditorGUILayout.IntField("Digit Num", m_SerialNumDigit);

            EditorGUIEx.EndGroup();
        }
    }

    /// <summary>
    /// 削除のレイアウト設定
    /// </summary>
    void LayoutDelete()
    {
        if (EditorGUIEx.BeginGroup("Delete : Options"))
        {
            m_DeletePopup = (EDeletePopup)EditorGUILayout.EnumPopup("Delete Type", (System.Enum)m_DeletePopup);
            if (m_DeletePopup == EDeletePopup.Select)
            {
                m_DeleteNumStart = EditorGUILayout.IntField("Start Digit", m_DeleteNumStart);
                m_DeleteNumDigit = EditorGUILayout.IntField("Digit", m_DeleteNumDigit);
            }
            else
            {
                m_DeleteNumDigit = EditorGUILayout.IntField("Digit", m_DeleteNumDigit);
            }

            EditorGUIEx.EndGroup();
        }
    }

    /// <summary>
    /// 変更確認のレイアウト
    /// </summary>
    void LayoutPreview()
    {
        if (EditorGUIEx.BeginGroup("Preview"))
        {
            // 何も選択されていなかったら
            if (!Selection.activeGameObject)
            {
                EditorGUILayout.HelpBox("Select Game Object from hierarchy.", MessageType.Warning);
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    LayoutPreviewBefore();
                    LayoutPreviewAfter();
                }
            }
        }
    }

    /// <summary>
    /// 変更前のレイアウト
    /// </summary>
    void LayoutPreviewBefore()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            EditorGUIEx.DrawSubTitle("Before");
            using (var scroll = new EditorGUILayout.ScrollViewScope(m_BeforeScrollPos, GUI.skin.box))
            {
                m_BeforeScrollPos = scroll.scrollPosition;

                foreach (var gameObj in GetSelectionGameObjectsSortHierarchyIndex())
                {
                    EditorGUILayout.LabelField(gameObj.name);
                }
            }
        }
    }

    /// <summary>
    /// 変更後のレイアウト
    /// </summary>
    void LayoutPreviewAfter()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            EditorGUIEx.DrawSubTitle("After");
            using (var scroll = new EditorGUILayout.ScrollViewScope(m_AfterScrollPos, GUI.skin.box))
            {
                m_AfterScrollPos = scroll.scrollPosition;

                switch (m_SelecteTab)
                {
                    case ETab.Add:
                        m_AfterObjNames = AddName();
                        break;
                    case ETab.Replace:
                        m_AfterObjNames = ReplaceName();
                        break;
                    case ETab.SerialNum:
                        m_AfterObjNames = AddSerialNum();
                        break;
                    case ETab.Delete:
                        m_AfterObjNames = DeleteName();
                        break;
                    default:
                        break;
                }

                foreach (var objName in m_AfterObjNames)
                {
                    EditorGUILayout.LabelField(objName);
                }
            }
        }
    }

    #endregion

    //=======================================================================================================
    //. 設定
    //=======================================================================================================
    #region -- 設定

    /// <summary>
    /// 各リネーム処理の反映処理
    /// </summary>
    void Rename()
    {
        var names = new List<string>();
        switch (m_SelecteTab)
        {
            case ETab.Add:
                names = AddName();
                break;
            case ETab.Replace:
                names = ReplaceName();
                break;
            case ETab.SerialNum:
                names = AddSerialNum();
                break;
            case ETab.Delete:
                names = DeleteName();
                break;
        }

        var selectGameObjs = new List<GameObject>(Selection.gameObjects);
        // Hierarchyの上からの順にソート
        selectGameObjs.Sort((a, b) => a.transform.GetSiblingIndex() - b.transform.GetSiblingIndex());

        for (int i = 0; i < selectGameObjs.Count; i++)
        {
            Undo.RecordObject(selectGameObjs[i], TOOL_NAME);
            selectGameObjs[i].name = names[i];
        }
    }

    /// <summary>
    /// 追加の処理
    /// </summary>
    /// <returns>処理後のオブジェクト名のリスト</returns>
    List<string> AddName()
    {
        var objNames = new List<string>();

        foreach (var gameObj in GetSelectionGameObjectsSortHierarchyIndex())
        {
            if (m_AddPopup == EAddPopup.Prefix)
            {
                objNames.Add(gameObj.name.Insert(m_AddPosition, m_AddText));
            }
            else
            {
                objNames.Add(gameObj.name.Insert(gameObj.name.Length - m_AddPosition, m_AddText));
            }
        }

        return objNames;
    }

    /// <summary>
    /// 置換の処理
    /// </summary>
    /// <returns>処理後のオブジェクト名のリスト</returns>
    List<string> ReplaceName()
    {
        var objNames = new List<string>();

        foreach (var gameObj in GetSelectionGameObjectsSortHierarchyIndex())
        {
            // 名前に置換前の文字が含まれていない or 置換前の文字列が""だったら
            if (!gameObj.name.Contains(m_ReplaceBeforeText) || m_ReplaceBeforeText == "")
            {
                objNames.Add(gameObj.name);
            }
            else
            {
                objNames.Add((gameObj.name.Replace(m_ReplaceBeforeText, m_ReplaceAfterText)));
            }
        }

        return objNames;
    }


    /// <summary>
    /// 連番の処理
    /// </summary>
    /// <returns>処理後のオブジェクト名のリスト</returns>
    List<string> AddSerialNum()
    {
        var objNames = new List<string>();
        var serialNumCount = m_SerialNumStart;

        foreach (var gameObj in GetSelectionGameObjectsSortHierarchyIndex())
        {
            var serialNumName = gameObj.name;

            if (m_FlagOverride)
            {
                serialNumName = m_OverrideText;
            }

            if (m_SerialNumPopup == ESerialNumPopup.Prefix)
            {
                objNames.Add(serialNumCount.ToString("D" + m_SerialNumDigit) + serialNumName);
            }
            else
            {
                objNames.Add(serialNumName + serialNumCount.ToString("D" + m_SerialNumDigit));
            }

            serialNumCount++;
        }

        return objNames;
    }

    /// <summary>
    /// 名前の文字列を削除する処理
    /// </summary>
    /// <returns>処理後のオブジェクト名のリスト</returns>
    List<string> DeleteName()
    {
        var objNames = new List<string>();

        foreach (var gameObj in GetSelectionGameObjectsSortHierarchyIndex())
        {
            if (m_DeleteNumDigit <= 0 || m_DeleteNumDigit > gameObj.name.Length)
            {
                objNames.Add(gameObj.name);
            }
            else
            {
                switch (m_DeletePopup)
                {
                    case EDeletePopup.Prefix:
                        objNames.Add(gameObj.name.Remove(0, m_DeleteNumDigit));
                        break;
                    case EDeletePopup.Suffix:
                        objNames.Add(gameObj.name.Remove(gameObj.name.Length - m_DeleteNumDigit));
                        break;
                    case EDeletePopup.Select:
                        objNames.Add(gameObj.name.Remove(m_DeleteNumStart, m_DeleteNumDigit));
                        break;
                    default:
                        objNames.Add(gameObj.name);
                        break;
                }
            }
        }

        return objNames;
    }

    #endregion

    //=======================================================================================================
    //. 取得
    //=======================================================================================================
    #region -- 取得

    /// <summary>
    /// 選択オブジェクトをHierarchyの上から順に取得
    /// </summary>
    /// <returns>Hierarchyの上から順にソートした選択オブジェクト</returns>
    List<GameObject> GetSelectionGameObjectsSortHierarchyIndex()
    {
        var selecGameObjs = new List<GameObject>(Selection.gameObjects);

        // Hierarchyの上からの順にソート
        selecGameObjs.Sort((a, b) => a.transform.GetSiblingIndex() - b.transform.GetSiblingIndex());

        return selecGameObjs;
    }

    #endregion
}
