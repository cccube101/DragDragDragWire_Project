using UnityEngine;

namespace Helper
{
    // ---------------------------- Enum
    public enum Switch
    {
        ON, OFF
    }
    public enum Scheme
    {
        KeyboardMouse, Gamepad
    }
    public enum GameState
    {
        DEFAULT, PAUSE, GAMECLEAR, GAMEOVER
    }



    /// <summary>
    /// 基礎処理
    /// </summary>
    public static class Base
    {
        // ---------------------------- Field
        private static (Rect[], GUIStyle) _logParam = GetLogParam();

        // ---------------------------- Property
        public static readonly float HEIGHT = 1080;
        public static readonly float WIDTH = 1920;
        public static (Rect[] pos, GUIStyle style) LogParam => _logParam;



        // ---------------------------- PublicMethod
        /// <summary>
        /// ログパラメータ取得
        /// </summary>
        /// <returns>ログ用パラメータ</returns>
        private static (Rect[], GUIStyle) GetLogParam()
        {
            //  パラメータ生成
            var pos = new Rect[30];

            //  位置保存
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = new Rect(10, 1080 - i * 30, 300, 30);
            }

            //  出力スタイル保存
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 25;


            return (pos, style);

        }
    }
}
