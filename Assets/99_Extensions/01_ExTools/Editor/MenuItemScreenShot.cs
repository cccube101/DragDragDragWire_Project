using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public sealed class MenuItemScreenShot : Editor
{
    /*-<MenuItemScreenShot.cs>---
    Gameビューをスクリーンショット
    ---------------------------*/

    const string MENU_PATH = "ExTools/Screen Shot #%F12";

    [MenuItem(MENU_PATH, priority = 60)]
    static void CaptureScreenShot()
    {
        var filename = System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png";

        ScreenCapture.CaptureScreenshot(filename);

        var assembly = typeof(EditorWindow).Assembly;
        var type = assembly.GetType("UnityEditor.GameView");
        var gameview = EditorWindow.GetWindow(type);
        gameview.Repaint();

        Debug.Log("ScreenShot captured to " + filename + ".");
    }
}
