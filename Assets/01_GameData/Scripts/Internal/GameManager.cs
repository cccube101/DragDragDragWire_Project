using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Helper;

public class GameManager : MonoBehaviour
{

    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("デバッグスイッチ")] private Switch _onDebug;

    [SerializeField, Required, BoxGroup("フェードSE")] private UnityEvent _fadeClip;

    [SerializeField, Required, BoxGroup("ベース")] private CanvasGroup _baseCanvas;

    [SerializeField, Required, BoxGroup("ボタン/オプション")] private Button _btn_optionBack;
    [SerializeField, Required, BoxGroup("ボタン/オプション")] private Button _btn_optionRetry;
    [SerializeField, Required, BoxGroup("ボタン/オプション")] private Button _btn_optionTitle;

    [SerializeField, Required, BoxGroup("ボタン/クリア")] private Button _btn_clearRetry;
    [SerializeField, Required, BoxGroup("ボタン/クリア")] private Button _btn_clearTitle;
    [SerializeField, Required, BoxGroup("ボタン/クリア")] private Button _btn_clearNext;

    [SerializeField, Required, BoxGroup("ボタン/ゲームオーバー")] private Button _btn_overRetry;
    [SerializeField, Required, BoxGroup("ボタン/ゲームオーバー")] private Button _btn_overTitle;


    // ---------------------------- Field
    private static GameManager _instance;
    private int _points, _coinCount;    //  スコア

    // ---------------------------- ReactiveProperty
    //  ゲームステート
    private readonly ReactiveProperty<GameState> _state = new();
    public ReadOnlyReactiveProperty<GameState> State => _state;

    //  時間
    private readonly ReactiveProperty<float> _currentTime = new();
    public ReadOnlyReactiveProperty<float> CurrentTime => _currentTime;

    // ---------------------------- Property
    public static GameManager Instance => _instance;
    public bool OnDebug => _onDebug == Switch.ON;

    public float LimitTime => Helper.Data.LimitTime;
    public (int Points, int Count) Score => (_points, _coinCount);





    // ---------------------------- UnityMessage
    private void Awake()
    {

        _instance = this;

#if UNITY_EDITOR
        if (Data.ScoreList.Count == 0)
        {
            Data.ScoreInit();
        }
#endif

        //  DOTween初期設定
        DG.Tweening.DOTween.SetTweensCapacity(tweenersCapacity: 20000, sequencesCapacity: 200);

        //  フレームレート固定
        Application.targetFrameRate = 60;

        //  初期化
        _state.Value = GameState.PAUSE;
        _currentTime.Value = Helper.Data.LimitTime;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex + 1 == (int)SceneName.Title)
        {
            _btn_clearNext.gameObject.SetActive(false);
        }

        Tasks.FadeClip = _fadeClip;   //  フェードSE設定

        EventObserver();    //  イベント監視
    }

    private void Update()
    {
        if (_state.Value == GameState.DEFAULT)
        {
            _currentTime.Value -= Time.deltaTime;   //  時間更新
        }

        if (OnDebug)
        {
            BaseDebug();
        }
    }



    // ---------------------------- PublicMethod
    /// <summary>
    /// ステート変更
    /// </summary>
    /// <param name="state"></param>
    public void ChangeState(GameState state)
    {
        SetState(state);
    }

    /// <summary>
    /// スコア入力
    /// </summary>
    /// <param name="item"></param>
    public void SetScore(int point)
    {
        _points += point;
        _coinCount++;
    }


    // ---------------------------- PrivateMethod
    /// <summary>
    /// イベント監視
    /// </summary>
    private void EventObserver()
    {
        //  ------  時間イベント
        _currentTime.Subscribe(time =>
        {
            //  ゲームオーバー分岐
            if (time < 0)
            {
                SetState(GameState.GAMEOVER);
            }
        })
        .AddTo(this);

        //  ------  HP
        PlayerController.Instance.HP.Subscribe(hp =>
        {
            if (hp <= 0)
            {
                SetState(GameState.GAMEOVER);
            }
        })
        .AddTo(this);

        //  ------  ボタンイベント
        var current = SceneManager.GetActiveScene().buildIndex;

        //  デフォルト
        _btn_optionBack.OnClickAsObservable()
        .Subscribe(_ =>
        {
            SetState(GameState.DEFAULT);
        })
        .AddTo(this);

        _btn_optionRetry.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {

            await Tasks.SceneChange(current, _baseCanvas, ct);
        })
        .AddTo(this);

        _btn_optionTitle.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            await Tasks.SceneChange((int)SceneName.Title, _baseCanvas, ct);
        })
        .AddTo(this);


        //  クリア
        _btn_clearRetry.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            await Tasks.SceneChange(current, _baseCanvas, ct);
        })
        .AddTo(this);

        _btn_clearTitle.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            await Tasks.SceneChange((int)SceneName.Title, _baseCanvas, ct);
        })
        .AddTo(this);

        _btn_clearNext.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            await Tasks.SceneChange(current + 1, _baseCanvas, ct);
        })
        .AddTo(this);


        //  ゲームオーバー
        _btn_overRetry.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            await Tasks.SceneChange(current, _baseCanvas, ct);
        })
        .AddTo(this);

        _btn_overTitle.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            await Tasks.SceneChange((int)SceneName.Title, _baseCanvas, ct);
        })
        .AddTo(this);
    }

    /// <summary>
    /// ステートの変更
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private void SetState(GameState state)
    {
        switch (state)
        {
            case GameState.DEFAULT:
                Decision(GameState.PAUSE);
                break;

            case GameState.PAUSE:
            case GameState.GAMEOVER:
            case GameState.GAMECLEAR:
                Decision(GameState.DEFAULT);
                break;
        }

        //  判定
        void Decision(GameState decisionValue)
        {
            if (!Tasks.IsFade
                && !UIController.Instance.IsMoveMenu
                && _state.Value == decisionValue)
            {
                _state.Value = state;
            }
        }
    }



    /// <summary>
    /// デバッグ
    /// </summary>
    private void BaseDebug()
    {
        var current = Keyboard.current;
        var oneKey = current.digit1Key;
        var twoKey = current.digit2Key;
        var threeKey = current.digit3Key;
        var fourKey = current.digit4Key;
        var fiveKey = current.digit5Key;
        var sixKey = current.digit6Key;

        if (oneKey.wasPressedThisFrame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; //ゲームシーン終了
#else
            Application.Quit(); //build後にゲームプレイ終了が適用
#endif
        }
        if (twoKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1.0f;
        }
        if (threeKey.wasPressedThisFrame)
        {

        }
        if (fourKey.wasPressedThisFrame)
        {

        }
        if (fiveKey.wasPressedThisFrame)
        {

        }
        if (sixKey.wasPressedThisFrame)
        {

        }

        var uKey = current.uKey;
        var jKey = current.jKey;
        var pKey = current.pKey;

        if (uKey.wasPressedThisFrame)
        {

        }
        if (jKey.wasPressedThisFrame)
        {

        }
        if (pKey.wasPressedThisFrame)
        {

        }
    }
}
