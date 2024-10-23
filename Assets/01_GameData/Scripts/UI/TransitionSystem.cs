using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionSystem : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("遷移先シーン")] private SceneName _toScene;



    // ---------------------------- PublicMethod
    /// <summary>
    /// シーン変更
    /// </summary>
    public async void SceneChange()
    {
        await Helper.Tasks.Canceled(Helper.Tasks.SceneChange((int)_toScene, destroyCancellationToken));
    }

    /// <summary>
    /// 次ステージへ遷移
    /// </summary>
    public async void SceneChange_Next()
    {
        var next = SceneManager.GetActiveScene().buildIndex + 1;
        await Helper.Tasks.Canceled(Helper.Tasks.SceneChange(next, destroyCancellationToken));
    }
}
