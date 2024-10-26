using R3;
using UnityEngine;
using UnityEngine.UI;
using Helper;
using Alchemy.Inspector;

public class QuitButtonObserver : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("マネージャー")] private TitleManager _manager;
    [SerializeField, Required, BoxGroup("マネージャー")] private CanvasGroup _baseCanvas;
    [SerializeField, Required, BoxGroup("ボタン")] private Button _quitYesButton;
    [SerializeField, Required, BoxGroup("ボタン")] private Button _quitNoButton;

    // ---------------------------- Field



    // ---------------------------- UnityMessage
    private void OnEnable()
    {
        _quitYesButton.OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                //  ゲーム終了
                await Tasks.ApplicationQuit(_baseCanvas, destroyCancellationToken);

            }, AwaitOperation.Drop)
            .RegisterTo(destroyCancellationToken);

        _quitNoButton.OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                //  終了画面を閉じる
                var value = _manager.QuitFadeValue;
                await _manager.FadeQuitCanvas(false, value.y, value.x, ct);

            }, AwaitOperation.Drop)
            .RegisterTo(destroyCancellationToken);
    }
}
