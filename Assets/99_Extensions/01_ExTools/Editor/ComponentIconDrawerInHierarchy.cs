#nullable enable
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Hierarchyウィンドウにコンポーネントのアイコンを表示する拡張機能
/// </summary>
/// <remarks>
/// <para>Unity2020.3.5f1で動作確認。</para>
/// <para>
/// <list type="bullet">
/// <item><description>Transform以外のコンポーネントのアイコン表示。</description></item>
/// <item><description>スクリプトのアイコンは複数付与されていても1つのみ表示。</description></item>
/// <item><description>コンポーネントが無効になっている場合はアイコン色が半透明になっている。</description></item>
/// <item><description>Hierarchyウィンドウで右クリックで表示されるメニュー「コンポーネントアイコン表示切替」の選択で表示/非表示の切替可能。</description></item>
/// </list>
/// </para>
/// </remarks>
public static class ComponentIconDrawerInHierarchy
{
    private const int IconSize = 16;

    private const string MENU_PATH = "ExTools/ComponentsDrawer";

    private const string ScriptIconName = "cs Script Icon";

    private static readonly Color colorWhenDisabled = new Color(1.0f, 1.0f, 1.0f, 0.5f);

    private static Texture? scriptIcon;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        UpdateEnabled();

#pragma warning disable UNT0023 // Coalescing assignment on Unity objects
        scriptIcon ??= EditorGUIUtility.IconContent(ScriptIconName).image;
#pragma warning restore UNT0023
    }

    /// <summary>
    /// 切換え保存
    /// </summary>
    [MenuItem(MENU_PATH, priority = 40)]
    private static void MenuToggle()
    {
        EditorPrefs.SetBool(MENU_PATH, !EditorPrefs.GetBool(MENU_PATH, false));
    }

    /// <summary>
    /// 切換え
    /// </summary>
    /// <returns></returns>
    [MenuItem(MENU_PATH, true, priority = 40)]
    private static bool MenuToggleValidate()
    {
        Menu.SetChecked(MENU_PATH, EditorPrefs.GetBool(MENU_PATH, false));
        UpdateEnabled();
        return true;
    }

    /// <summary>
    /// 判定
    /// </summary>
    public static bool IsValid()
    {
        return EditorPrefs.GetBool(MENU_PATH, false);
    }

    private static void UpdateEnabled()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= DisplayIcons;
        if (IsValid())
            EditorApplication.hierarchyWindowItemOnGUI += DisplayIcons;
    }

    private static void DisplayIcons(int instanceID, Rect selectionRect)
    {
        // instanceIDをオブジェクト参照に変換
        if (!(EditorUtility.InstanceIDToObject(instanceID) is GameObject gameObject)) return;

        var pos = selectionRect;
        pos.x = pos.xMax - IconSize * 2;
        pos.width = IconSize;
        pos.height = IconSize;

        // オブジェクトが所持しているコンポーネント一覧を取得
        var components
            = gameObject
                .GetComponents<Component>()
                .Where(x => !(x is Transform || x is ParticleSystemRenderer))
                .Reverse()
                .ToList();

        var existsScriptIcon = false;
        foreach (var component in components)
        {
            Texture image = AssetPreview.GetMiniThumbnail(component);
            if (image == null) continue;

            // Scriptのアイコンは1つのみ表示
            if (image == scriptIcon)
            {
                if (existsScriptIcon) continue;
                existsScriptIcon = true;
            }

            // アイコン描画
            DrawIcon(ref pos, image, component.IsEnabled() ? Color.white : colorWhenDisabled);
        }
    }

    private static void DrawIcon(ref Rect pos, Texture image, Color? color = null)
    {
        Color? defaultColor = null;
        if (color.HasValue)
        {
            defaultColor = GUI.color;
            GUI.color = color.Value;
        }

        GUI.DrawTexture(pos, image, ScaleMode.ScaleToFit);
        pos.x -= pos.width;

        if (defaultColor.HasValue)
            GUI.color = defaultColor.Value;
    }

    /// <summary>
    /// コンポーネントが有効かどうかを確認する拡張メソッド
    /// </summary>
    /// <param name="this">拡張対象</param>
    /// <returns>コンポーネントが有効となっているかどうか</returns>
    private static bool IsEnabled(this Component @this)
    {
        var property = @this.GetType().GetProperty("enabled", typeof(bool));
        return (bool)(property?.GetValue(@this, null) ?? true);
    }
}