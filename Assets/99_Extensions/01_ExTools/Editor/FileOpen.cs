using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class FileOpen
{
    const int WIDTH = 10;

    [InitializeOnLoadMethod]
    static void Example()
    {
        EditorApplication.projectWindowItemOnGUI += OnGUI;
    }

    static void OnGUI(string guid, Rect selectionRect)
    {
        var pos = selectionRect;
        pos.x = pos.xMax - WIDTH * 0.5f;
        pos.width = WIDTH;
        pos.height = WIDTH * 0.8f;

        if (!GUI.Button(pos, "!"))
        {
            return;
        }

        var path = AssetDatabase.GUIDToAssetPath(guid);
        path = path.Replace("/", "\\");
        Process.Start("explorer.exe", "/select," + path);
    }
}