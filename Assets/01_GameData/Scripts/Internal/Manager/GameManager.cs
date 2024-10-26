using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Helper;

public class GameManager : MonoBehaviour
{

    // ---------------------------- SerializeField
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

    public float LimitTime => Helper.Data.LimitTime;
    public (int Points, int Count) Score => (_points, _coinCount);





    // ---------------------------- UnityMessage
    private void Awake()
    {
        //  シングルトンキャッシュ
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
        //  デフォルトステータス時更新
        if (_state.Value == GameState.DEFAULT)
        {
            _currentTime.Value -= Time.deltaTime;   //  時間更新
        }
    }



    // ---------------------------- PublicMethod
    /// <summary>
    /// ステート変更
    /// </summary>
    /// <param name="state">変更したいステート先</param>
    public void ChangeState(GameState state)
    {
        SetState(state);
    }

    /// <summary>
    /// スコア入力
    /// </summary>
    /// <param name="item">スコア</param>
    public void SetScore(int point)
    {
        //  スコア更新
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
            //  ゲームオーバー分岐
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
            //  デフォルトに変更
            SetState(GameState.DEFAULT);
        })
        .AddTo(this);

        _btn_optionRetry.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            //  シーン遷移
            await Tasks.SceneChange(current, _baseCanvas, ct);
        })
        .AddTo(this);

        _btn_optionTitle.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            //  シーン遷移
            await Tasks.SceneChange((int)SceneName.Title, _baseCanvas, ct);
        })
        .AddTo(this);


        //  クリア
        _btn_clearRetry.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            //  シーン遷移
            await Tasks.SceneChange(current, _baseCanvas, ct);
        })
        .AddTo(this);

        _btn_clearTitle.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            //  シーン遷移
            await Tasks.SceneChange((int)SceneName.Title, _baseCanvas, ct);
        })
        .AddTo(this);

        _btn_clearNext.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            //  シーン遷移
            await Tasks.SceneChange(current + 1, _baseCanvas, ct);
        })
        .AddTo(this);


        //  ゲームオーバー
        _btn_overRetry.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            //  シーン遷移
            await Tasks.SceneChange(current, _baseCanvas, ct);
        })
        .AddTo(this);

        _btn_overTitle.OnClickAsObservable()
        .SubscribeAwait(async (_, ct) =>
        {
            //  シーン遷移
            await Tasks.SceneChange((int)SceneName.Title, _baseCanvas, ct);
        })
        .AddTo(this);
    }

    /// <summary>
    /// ステートの変更
    /// </summary>
    /// <param name="state">変更したいステート先</param>
    /// <returns>ステート変更結果</returns>
    private void SetState(GameState state)
    {
        //  分岐
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
            //  遷移中か同か
            //  UIが移動しているかどうか
            //  ステートが変更可能なステートかどうか
            if (!Tasks.IsFade
                && !UIController.Instance.IsMoveMenu
                && _state.Value == decisionValue)
            {
                _state.Value = state;
            }
        }
    }
}
