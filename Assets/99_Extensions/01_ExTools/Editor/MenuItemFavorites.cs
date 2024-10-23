using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public sealed class MenuItemFavorites : EditorWindow
{
    /*-<MenuItemFavorites.cs>-----------
    エディタ内タブにお気に入りリストを表示
    ----------------------------------*/

    const string MENU_PATH = "ExTools/Favorites";

    [MenuItem(MENU_PATH,priority =20)]
    static void ShowWindow()
    {
        GetWindow<MenuItemFavorites>("★ Favorites");
    }

    Vector2 _scrollView;
    AssetInfo _lastOpenedAsset;

    [System.Serializable]
    class AssetInfo
    {
        public string guid;
        public string path;
        public string name;
        public string type;
    }

    [System.Serializable]
    class AssetInfoList
    {
        public List<AssetInfo> infoList = new List<AssetInfo>();
    }

    [SerializeField] static AssetInfoList _assetCache = null;
    static AssetInfoList _assets
    {
        get
        {
            if (_assetCache == null)
            {
                _assetCache = LoadPrefs();
            }
            return _assetCache;
        }
    }

    /// <summary>
    /// 内容保存キー生成
    /// </summary>
    /// <returns>保存キー</returns>
    static string PrefsKey()
    {
        return $"{Application.productName}-Favs";
    }

    /// <summary>
    /// 内容を保存
    /// </summary>
    static void SavePrefs()
    {
        string prefsJson = JsonUtility.ToJson(_assets);
        EditorPrefs.SetString(PrefsKey(), prefsJson);
    }

    /// <summary>
    /// 内容を読み込む
    /// </summary>
    /// <returns>セーブ内容</returns>
    static AssetInfoList LoadPrefs()
    {
        //Debug.Log("Loading favorites presets...");
        string prefsKey = PrefsKey();
        if (!EditorPrefs.HasKey(prefsKey))
        {
            return new AssetInfoList();
        }

        string prefsJson = EditorPrefs.GetString(prefsKey);
        var assets = JsonUtility.FromJson<AssetInfoList>(prefsJson);
        if (assets == null)
        {
            Debug.LogError("Failed to load favorites presets.");
            return new AssetInfoList();
        }
        return assets;
    }

    void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope("Box"))
        {
            GUILayout.FlexibleSpace();
            var content = new GUIContent("★Add", "Add asset in favorites");
            if (GUILayout.Button(content, GUILayout.Width(100), GUILayout.Height(40)))
            {
                BookmarkAsset();
            }
            using (new EditorGUILayout.VerticalScope())
            {
                if (GUILayout.Button("▼ Sort by type", GUILayout.MaxWidth(200)))
                {
                    SortByType();
                }
                if (GUILayout.Button("▼ Sort by alphabetical", GUILayout.MaxWidth(200)))
                {
                    SortByName();
                }
            }
            GUILayout.FlexibleSpace();
        }

        using (new EditorGUILayout.ScrollViewScope(_scrollView, "Box"))
        {
            foreach (var info in _assets.infoList)
            {
                using (new EditorGUILayout.HorizontalScope("Box"))
                {
                    bool isCanceled = DrawAssetRow(info);
                    if (isCanceled)
                    {
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 縦に保存アセットを表示
    /// </summary>
    /// <param name="info"></param>
    /// <returns>消去されたか</returns>
    bool DrawAssetRow(AssetInfo info)
    {
        bool isCanceled = false;
        {
            var content = new GUIContent(" ◎ ", "Highlight asset location");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(false)))
            {
                HighlightAsset(info);
            }
        }
        {
            DrawAssetItemButton(info);
        }
        {
            var content = new GUIContent("X", "Delete item from favorites");
            if (GUILayout.Button(content, GUILayout.ExpandWidth(false)))
            {
                RemoveAsset(info);
                isCanceled = true;
            }
        }
        return isCanceled;
    }

    /// <summary>
    /// アセット読み込み
    /// </summary>
    /// <param name="info"></param>
    void DrawAssetItemButton(AssetInfo info)
    {
        var content = new GUIContent($" {info.name}", AssetDatabase.GetCachedIcon(info.path));
        var style = GUI.skin.button;
        var originalAlignment = style.alignment;
        var originalFontStyle = style.fontStyle;
        var originalTextColor = style.normal.textColor;
        style.alignment = TextAnchor.MiddleLeft;
        if (info == _lastOpenedAsset)
        {
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.yellow;
        }

        float width = position.width - 70f;
        if (GUILayout.Button(content, style, GUILayout.MaxWidth(width), GUILayout.Height(18)))
        {
            OpenAsset(info);
        }

        style.alignment = originalAlignment;
        style.fontStyle = originalFontStyle;
        style.normal.textColor = originalTextColor;
    }

    /// <summary>
    /// ブックマーク化したアセット
    /// </summary>
    void BookmarkAsset()
    {
        foreach (string assetGuid in Selection.assetGUIDs)
        {
            if (_assets.infoList.Exists(x => x.guid == assetGuid))
            {
                continue;
            }

            var info = new AssetInfo();
            info.guid = assetGuid;
            info.path = AssetDatabase.GUIDToAssetPath(assetGuid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(info.path);
            info.name = asset.name;
            info.type = asset.GetType().ToString();
            _assets.infoList.Add(info);
        }
        SavePrefs();
    }

    /// <summary>
    /// アセットをお気に入りから消去
    /// </summary>
    /// <param name="info"></param>
    void RemoveAsset(AssetInfo info)
    {
        _assets.infoList.Remove(info);
        SavePrefs();
    }

    /// <summary>
    /// アセット位置をハイライト
    /// </summary>
    /// <param name="info"></param>
    void HighlightAsset(AssetInfo info)
    {
        var asset = AssetDatabase.LoadAssetAtPath<Object>(info.path);
        EditorGUIUtility.PingObject(asset);
    }

    /// <summary>
    /// アセットを開く
    /// </summary>
    /// <param name="info"></param>
    void OpenAsset(AssetInfo info)
    {
        // Mark last-opened non-folder asset
        if (info.type != "UnityEditor.DefaultAsset")
        {
            _lastOpenedAsset = info;
        }

        // Open scene asset
        if (Path.GetExtension(info.path).Equals(".unity"))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(info.path, OpenSceneMode.Single);
            }
            return;
        }

        // Open other type asset
        var asset = AssetDatabase.LoadAssetAtPath<Object>(info.path);
        AssetDatabase.OpenAsset(asset);
    }

    /// <summary>
    /// 種別順ソート
    /// </summary>
    void SortByType()
    {
        SortByName();
        _assets.infoList.Sort((a, b) =>
        {
            return a.type.CompareTo(b.type);
        });
    }

    /// <summary>
    /// 五十音順ソート
    /// </summary>
    void SortByName()
    {
        _assets.infoList.Sort((a, b) =>
        {
            return a.name.CompareTo(b.name);
        });
    }
}
