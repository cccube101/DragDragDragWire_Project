using System.Text;
using System;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Linq;
using UnityEngine;

public class NamesCreator : MonoBehaviour
{
    // 無効な文字を管理する配列
    private static readonly string[] INVALUD_CHARS =
    {
        " ", "!", "\"", "#", "$",
        "%", "&", "\'", "(", ")",
        "-", "=", "^",  "~", "\\",
        "|", "[", "{",  "@", "`",
        "]", "}", ":",  "*", ";",
        "+", "/", "?",  ".", ">",
        ",", "<"
    };

    private const string ITEM_NAME = "ExTools/CreateNames";    // コマンド名
    private const string PATH = "Assets/00_GameData/Scripts/Internal/Names.cs";        // ファイルパス

    private static readonly string FILENAME = Path.GetFileName(PATH);                   // ファイル名(拡張子あり)
    private static readonly string FILENAME_WITHOUT_EXTENSION = Path.GetFileNameWithoutExtension(PATH);   // ファイル名(拡張子なし)

    /// <summary>
    /// レイヤー名を定数で管理するクラスを作成します
    /// </summary>
    [MenuItem(ITEM_NAME, priority = 0)]
    public static void Create()
    {
        if (!CanCreate())
        {
            return;
        }

        CreateScript();

        EditorUtility.DisplayDialog(FILENAME, "Create success.", "OK");
    }

    /// <summary>
    /// スクリプトを作成します
    /// </summary>
    public static void CreateScript()
    {
        var builder = new StringBuilder();

        //  シーンEnum
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// シーン名をEnumで管理するクラス");
        builder.AppendLine("/// </summary>");
        builder.AppendFormat("public enum SceneName").AppendLine();
        builder.AppendLine("{");

        foreach (var n in EditorBuildSettings.scenes
            .Select(x => Path.GetFileNameWithoutExtension(x.path))
            .Distinct()
            .Select(c => $"{RemoveInvalidChars(c)},"))
        {
            builder.Append("\t").AppendFormat(@"{0},", RemoveInvalidChars(n)).AppendLine();
        }

        builder.AppendLine("}");
        builder.AppendLine();

        //  タグ名
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// タグ名を文字列で管理するクラス");
        builder.AppendLine("/// </summary>");
        builder.AppendFormat("public static class TagName").AppendLine();
        builder.AppendLine("{");

        foreach (var n in InternalEditorUtility.tags.
            Select(c => new { var = RemoveInvalidChars(c), val = c }))
        {
            builder.Append("\t").AppendFormat(@"public const string {0} = ""{1}"";", n.var, n.val).AppendLine();
        }

        builder.AppendLine("}");
        builder.AppendLine();

        //  レイヤー定数
        builder.AppendLine("/// <summary>");
        builder.AppendLine("/// レイヤー名を定数で管理するクラス");
        builder.AppendLine("/// </summary>");
        builder.AppendFormat("public static class LayerName").AppendLine();
        builder.AppendLine("{");

        foreach (var n in InternalEditorUtility.layers.
            Select(c => new { var = RemoveInvalidChars(c), val = LayerMask.NameToLayer(c) }))
        {
            builder.Append("\t").AppendFormat(@"public const int {0} = {1};", n.var, n.val).AppendLine();
        }
        foreach (var n in InternalEditorUtility.layers.
            Select(c => new { var = RemoveInvalidChars(c), val = 1 << LayerMask.NameToLayer(c) }))
        {
            builder.Append("\t").AppendFormat(@"public const int {0}Mask = {1};", n.var, n.val).AppendLine();
        }

        builder.AppendLine("}");
        builder.AppendLine();

        //  生成
        var directoryName = Path.GetDirectoryName(PATH);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        File.WriteAllText(PATH, builder.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
    }




    /// <summary>
    /// 管理するクラスを作成できるかどうかを取得します
    /// </summary>
    [MenuItem(ITEM_NAME, true)]
    public static bool CanCreate()
    {
        return !EditorApplication.isPlaying && !Application.isPlaying && !EditorApplication.isCompiling;
    }

    /// <summary>
    /// 無効な文字を削除します
    /// </summary>
    public static string RemoveInvalidChars(string str)
    {
        Array.ForEach(INVALUD_CHARS, c => str = str.Replace(c, string.Empty));
        return str;
    }
}
