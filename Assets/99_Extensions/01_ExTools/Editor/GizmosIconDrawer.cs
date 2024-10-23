using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class GizmosIconDrawer
{
    private const string MENU_PATH = "ExTools/GizmosIconDrawer";

    /// <summary>
    /// 切換え保存
    /// </summary>
    [MenuItem(MENU_PATH, priority = 41)]
    private static void MenuToggle()
    {
        EditorPrefs.SetBool(MENU_PATH, !EditorPrefs.GetBool(MENU_PATH, false));
    }

    /// <summary>
    /// 切換え
    /// </summary>
    /// <returns></returns>
    [MenuItem(MENU_PATH, true, priority = 41)]
    private static bool MenuToggleValidate()
    {
        Menu.SetChecked(MENU_PATH, EditorPrefs.GetBool(MENU_PATH, false));
        return true;
    }

    /// <summary>
    /// 判定
    /// </summary>
    private static bool IsValid()
    {
        return EditorPrefs.GetBool(MENU_PATH, false);
    }

    /// <summary>
    /// Transformアイコン表示
    /// </summary>
    /// <param name="scr"></param>
    /// <param name="gizmoType"></param>
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    private static void DrawGizmo(Transform scr, GizmoType gizmoType)
    {
        if (IsValid())
        {
            Gizmos.DrawIcon(scr.transform.position, "TransformIcon.png", true);
        }
    }
}
