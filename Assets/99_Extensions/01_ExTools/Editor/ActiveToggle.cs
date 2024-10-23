using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ActiveToggle
{
    private const int WIDTH = 16;

    static ActiveToggle()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
    }

    private static void OnGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject == null) return;

        var position = selectionRect;

        position.x = position.xMax - WIDTH;
        position.width = WIDTH;

        var newActive = GUI.Toggle(position, gameObject.activeSelf, string.Empty);

        if (newActive == gameObject.activeSelf) return;

        gameObject.SetActive(newActive);
    }
}