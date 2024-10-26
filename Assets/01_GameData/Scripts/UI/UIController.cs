using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using Alchemy.Inspector;
using R3;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Helper;

public class UIController : MonoBehaviour
{
    private class LogoParam
    {
        public LogoParam(GameObject obj, RectTransform rect, TMP_Text text, Vector2 scale)
        {
            Obj = obj;
            Rect = rect;
            Text = text;
            Scale = scale;
        }
        public GameObject Obj { get; set; }
        public RectTransform Rect { get; set; }
        public TMP_Text Text { get; set; }

        public Vector2 Scale { get; set; }
    }

    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("ボタン")] private Button _pauseButton = null;
    [SerializeField, Required, BoxGroup("ボタン")] private Button _clearButton = null;
    [SerializeField, Required, BoxGroup("ボタン")] private Button _overButton = null;

    [SerializeField, Required, BoxGroup("キャンバス")] private CanvasGroup _frame;
    [SerializeField, Required, BoxGroup("キャンバス")] private GameObject _defaultFrame;
    [SerializeField, Required, BoxGroup("キャンバス")] private GameObject _optionFrame;
    [SerializeField, Required, BoxGroup("キャンバス")] private float _optionDuration;

    [SerializeField, Required, BoxGroup("スタート")] private TMP_Text _stageNameText;
    [SerializeField, Required, BoxGroup("スタート")] private float _waitStageNameFade;
    [SerializeField, Required, BoxGroup("スタート")] private float _stageNameFadeDuration;

    [SerializeField, Required, BoxGroup("UI")] private TMP_Text _timeText;
    [SerializeField, Required, BoxGroup("UI")] private GameObject[] _hpObjects;
    [SerializeField, Required, BoxGroup("UI")] private GameObject _guide;
    [SerializeField, Required, BoxGroup("UI")] private Image _guideImage;
    [SerializeField, Required, BoxGroup("UI")] private Sprite[] _guideSprite;

    [SerializeField, Required, BoxGroup("アラート")] private Color _originColor;
    [SerializeField, Required, BoxGroup("アラート")] private Color _heartAlertColor;
    [SerializeField, Required, BoxGroup("アラート")] private Color _alertColor;
    [SerializeField, Required, BoxGroup("アラート")] private Image _alertPanel;
    [SerializeField, Required, BoxGroup("アラート")] private float _alertHpSize;
    [SerializeField, Required, BoxGroup("アラート")] private float _alertHpDuration;
    [SerializeField, Required, BoxGroup("アラート")] private float _alertTextSize;
    [SerializeField, Required, BoxGroup("アラート")] private float _alertEndValue;
    [SerializeField, Required, BoxGroup("アラート")] private float _alertTime;
    [SerializeField, Required, BoxGroup("アラート")] private float _alertDuration;


    [SerializeField, Required, BoxGroup("リザルト")] private GameObject _resultFrame;
    [SerializeField, Required, BoxGroup("リザルト")] private float _initResultUIPos;
    [SerializeField, Required, BoxGroup("リザルト")] private Image _resultPanel;
    [SerializeField, Required, BoxGroup("リザルト")] private float _resultPanelAlpha;
    [SerializeField, Required, BoxGroup("リザルト")] private GameObject _clearLogo;
    [SerializeField, Required, BoxGroup("リザルト")] private GameObject _gameOverLogo;
    [SerializeField, Required, BoxGroup("リザルト")] private float _logoDuration;
    [SerializeField, Required, BoxGroup("リザルト")] private Vector2 _gameClearLogSize;
    [SerializeField, Required, BoxGroup("リザルト")] private Vector2 _gameOverLogSize;
    [SerializeField, Required, BoxGroup("リザルト")] private float _sizeDuration;

    [SerializeField, Required, BoxGroup("リザルト")] private GameObject[] _coinObjects;
    [SerializeField, Required, BoxGroup("リザルト")] private GameObject[] _heartObjects;
    [SerializeField, Required, BoxGroup("リザルト")] private float _fadeBgmVolume;
    [SerializeField, Required, BoxGroup("リザルト")] private Vector3 _timeScoreLimit;
    [SerializeField, Required, BoxGroup("リザルト")] private Vector3 _timeScore;
    [SerializeField, Required, BoxGroup("リザルト")] private float _resultWait;
    [SerializeField, Required, BoxGroup("リザルト")] private RectTransform[] _clearUI;
    [SerializeField, Required, BoxGroup("リザルト")] private RectTransform[] _gameOverUI;
    [SerializeField, Required, BoxGroup("リザルト")] private float _resultUIDuration;
    [SerializeField, Required, BoxGroup("リザルト")] private float _resultUIShifting;
    [SerializeField, Required, BoxGroup("リザルト")] private TMP_Text _timeResultText;
    [SerializeField, Required, BoxGroup("リザルト")] private TMP_Text _coinScoreText;
    [SerializeField, Required, BoxGroup("リザルト")] private TMP_Text _timeScoreText;
    [SerializeField, Required, BoxGroup("リザルト")] private TMP_Text _hpScoreText;
    [SerializeField, Required, BoxGroup("リザルト")] private TMP_Text _scoreText;
    [SerializeField, Required, BoxGroup("リザルト")] private float _scoreDuration;


    [SerializeField, Required, BoxGroup("オーディオ")] private AudioSource _bgmSource;
    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _resultClips;
    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _scoreClip;
    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _gameOverClip;
    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _alertClip;


    // ---------------------------- Field
    private static UIController _instance;

    //  ボタン
    private Dictionary<GameState, Button> _selectButtons;

    //  フレーム
    private RectTransform _optionFrameRect = null;

    //  シーン遷移
    private bool _isMoveMenu = false;
    private bool _isGameStart = false;

    //  コイン
    private readonly int MAX_COIN = 3;
    private readonly int GET_HUNDRED = 100;

    // アラート
    private bool _isAlert = false;

    //  リザルト
    private LogoParam _gameClearParam = null;
    private LogoParam _gameOverParam = null;
    private readonly List<float> _clearUIPos = new();
    private readonly List<float> _gameOverUIPos = new();

    // ---------------------------- Property
    public static UIController Instance => _instance;
    public bool IsMoveMenu => _isMoveMenu;



    // ---------------------------- UnityMessage
    private void Awake()
    {
        _instance = this;
    }

    private async void Start()
    {
        //  パラメータ保存
        ImplementParam();

        //  イベント監視
        BaseEventObserve();
        PlayerEventObserve();

        //  スタートイベント
        await Tasks.Canceled(StartEvent(this.GetCancellationTokenOnDestroy()));
    }

    // ---------------------------- PublicMethod



    // ---------------------------- PrivateMethod
    /// <summary>
    /// パラメータ保存
    /// </summary>
    private void ImplementParam()
    {
        //  ボタンキャッシュ
        _selectButtons = new()
        {
            {GameState.PAUSE, _pauseButton},
            {GameState.GAMECLEAR ,_clearButton},
            {GameState.GAMEOVER, _overButton},
        };

        //  フレームレクトキャッシュ
        _optionFrameRect = _optionFrame.GetComponent<RectTransform>();

        //  ロゴパラメータキャッシュ
        _gameClearParam = CreateLogoParam(_clearLogo, _gameClearLogSize);
        _gameOverParam = CreateLogoParam(_gameOverLogo, _gameOverLogSize);
        static LogoParam CreateLogoParam(GameObject obj, Vector2 scale)
        {
            return new LogoParam(obj, obj.GetComponent<RectTransform>(), obj.GetComponent<TMP_Text>(), scale);
        }
    }

    /// <summary>
    /// ベースイベント監視
    /// </summary>
    private void BaseEventObserve()
    {
        var Game = GameManager.Instance;

        //  ------  時間イベント
        Game.CurrentTime.Subscribe(time =>
        {
            _timeText.text = $"TIME {time:00}";
        })
        .AddTo(this);
        //  残時間によりアラート開始
        Game.CurrentTime.Where((time) => !_isAlert && time <= _alertTime)
        .SubscribeAwait(async (_, ct) =>
        {
            _isAlert = true;

            //  アラート回数算出
            var loopTime = (int)(_alertTime / _alertDuration);

            var tcb = TweenCancelBehaviour.KillAndCancelAwait;  // UniTaskへの変換時に必要な設定
            var tasks = new List<UniTask>()
            {
                FadePanel(),
                TextColor(),
                TextScale(),
                PlayClip(),
            };
            //  パネル点滅
            async UniTask FadePanel()
            {
                await _alertPanel.DOFade(_alertEndValue, _alertDuration)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true)
                    .SetLink(_alertPanel.gameObject)
                    .SetLoops(loopTime, LoopType.Yoyo)
                    .ToUniTask(tcb, cancellationToken: ct);
            }
            //  テキスト点滅
            async UniTask TextColor()
            {
                await DOVirtual.Color(_timeText.color, _alertColor, _alertDuration,
                    (color) =>
                    {
                        _timeText.color = color;
                    })
                    .SetEase(Ease.Linear)
                    .SetUpdate(true)
                    .SetLink(_timeText.gameObject)
                    .SetLoops(loopTime, LoopType.Yoyo)
                    .ToUniTask(tcb, cancellationToken: ct);
            }
            //  テキストサイズ
            async UniTask TextScale()
            {
                await _timeText.GetComponent<RectTransform>().DOScale(_alertTextSize, _alertDuration)
                    .SetEase(Ease.OutExpo)
                    .SetUpdate(true)
                    .SetLink(_timeText.gameObject)
                    .SetLoops(loopTime, LoopType.Yoyo)
                    .ToUniTask(tcb, cancellationToken: ct);
            }
            //  オーディオ再生
            async UniTask PlayClip()
            {
                //  ループ回数分処理
                for (int i = 0; i < loopTime / 2; i++)
                {
                    //  ステート判定
                    if (Game.State.CurrentValue == GameState.DEFAULT)
                    {
                        //  再生
                        _alertClip?.Invoke();
                    }
                    //  待機
                    await Tasks.DelayTime(_alertDuration * 2, ct);
                }
            }
            await UniTask.WhenAll(tasks);

            //  アラート終了時イメージ更新
            //  結果画面表示前に０に戻す
            await Fade_Img(_alertPanel, 0, _alertDuration, Ease.Linear, ct);

        }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);



        //  ------  ステートイベント
        Game.State.Where(_ => _isGameStart)
            .SubscribeAwait(async (state, ct) =>
            {
                switch (state)
                {
                    case GameState.DEFAULT:
                        Implement(true);
                        await MoveOptionFrame(false, Base.WIDTH, ct); //  メニュー移動

                        break;

                    case GameState.PAUSE:
                        Implement(false);
                        await MoveOptionFrame(true, 0, ct); //  メニュー移動

                        break;

                    case GameState.GAMECLEAR:
                        Implement(false);
                        await ResultStaging(_gameClearParam, _clearUI, _clearUIPos, ct); //  リザルト表示

                        break;

                    case GameState.GAMEOVER:
                        Implement(false);
                        await ResultStaging(_gameOverParam, _gameOverUI, _gameOverUIPos, ct);    //  リザルト表示

                        break;
                }

                //  ステータス設定
                void Implement(bool isDefault)
                {
                    _defaultFrame.SetActive(isDefault); //  フレームアクティブ
                    UISelect(); //  選択UI
                }

            }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);

    }

    /// <summary>
    /// プレイヤーイベント監視
    /// </summary>
    private void PlayerEventObserve()
    {
        var Player = PlayerController.Instance;

        //  ------  入力方法変更イベント
        Player.Scheme.Subscribe(scheme =>
        {
            switch (scheme)
            {
                case Scheme.KeyboardMouse:
                    Cursor.visible = true;  //  カーソル表示
                    EventSystem.current.SetSelectedGameObject(null);    //  UIのセレクトを解除
                    _guideImage.sprite = _guideSprite[0];

                    break;

                case Scheme.Gamepad:
                    Cursor.visible = false; //  カーソル非表示
                    UISelect();
                    _guideImage.sprite = _guideSprite[1];

                    break;
            }
        })
        .AddTo(this);

        //  ------  HPイベント
        //  HPオブジェクト
        Player.HP.SubscribeAwait(async (hp, ct) =>
        {
            //  HP入力
            for (int i = 0; i < _hpObjects.Length; i++)
            {
                _hpObjects[i].SetActive(i < hp && i < Player.MaxHP);
            }

            if (hp > 1)
            {
                //  インタスク
                var inTask = new List<UniTask>();
                HPEvent(inTask, _originColor, _heartAlertColor, _alertHpSize);
                await UniTask.WhenAll(inTask);

                //  アウトタスク
                var outTask = new List<UniTask>();
                HPEvent(outTask, _heartAlertColor, _originColor, 1);
                await UniTask.WhenAll(outTask);


                //  HPオブジェクトイベント
                void HPEvent(List<UniTask> tasks, Color startColor, Color endColor, float endScale)
                {
                    //  オブジェクト数分更新
                    foreach (var obj in _hpObjects)
                    {
                        if (obj != null)
                        {
                            //  イメージの色変更
                            var img = obj.GetComponent<Image>();
                            tasks.Add(ColorChange_Img());
                            async UniTask ColorChange_Img()
                            {
                                await DOVirtual.Color(startColor, endColor, _alertHpDuration,
                                     (color) =>
                                     {
                                         img.color = color;
                                     })
                                     .SetEase(Ease.Linear)
                                     .SetUpdate(true)
                                     .SetLink(img.gameObject)
                                     .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
                            }

                            //  スケール変更
                            var rect = obj.GetComponent<RectTransform>();
                            tasks.Add(Scale_Rect(rect, endScale, _alertHpDuration, Ease.Linear, ct));
                        }
                    }
                }
            }
            else
            {
                //  HP残基低下時アラート処理
                var tasks = new List<UniTask>();

                //  オブジェクト数分更新
                foreach (var obj in _hpObjects)
                {
                    if (obj != null)
                    {
                        tasks.Add(ColorChange(obj));
                        tasks.Add(ScaleChange(obj));
                    }
                }

                async UniTask ColorChange(GameObject obj)
                {
                    await DOVirtual.Color(_originColor, _heartAlertColor, _alertHpDuration,
                        (color) =>
                        {
                            obj.GetComponent<Image>().color = color;
                        })
                        .SetEase(Ease.Linear)
                        .SetUpdate(true)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetLink(obj)
                        .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
                }
                async UniTask ScaleChange(GameObject obj)
                {
                    await obj.GetComponent<RectTransform>().DOScale(_alertHpSize, _alertHpDuration)
                        .SetEase(Ease.Linear)
                        .SetUpdate(true)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetLink(obj)
                        .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
                }
            }

        }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);
        //  画面全体のパネル
        Player.HP.SubscribeAwait(async (hp, ct) =>
        {
            //  インタスク
            await Fade_Img(_alertPanel, _alertEndValue, _alertDuration, Ease.Linear, ct);

            //  アウトタスク
            await Fade_Img(_alertPanel, 0, _alertDuration, Ease.Linear, ct);

        }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);
    }

    /// <summary>
    /// 開始イベント
    /// </summary>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>開始イベント処理</returns>
    private async UniTask StartEvent(CancellationToken ct)
    {
        //  ステージ名
        var scene = SceneManager.GetActiveScene();
        _stageNameText.text = $"No.{scene.buildIndex} {scene.name}";

        //  リザルトUI位置保存
        var initTasks = new List<UniTask>();
        SetPos(_clearUI, _clearUIPos);  //  クリアUI
        SetPos(_gameOverUI, _gameOverUIPos);    //  ゲームオーバーUI
        void SetPos(RectTransform[] setPos, List<float> getPos)
        {
            foreach (var rect in setPos)
            {
                getPos.Add(rect.anchoredPosition.y);    //  保存
                initTasks.Add(InitPos(rect, _initResultUIPos)); //  初期化
            }
        }
        await UniTask.WhenAll(initTasks);

        //  位置初期化
        await InitPos(_gameClearParam.Rect, 0);   //  クリアロゴ
        await InitPos(_gameOverParam.Rect, 0);    //  ゲームオーバーロゴ
        async UniTask InitPos(RectTransform rect, float pos)
        {
            await rect.DOAnchorPosY(pos, 0)
                .SetLink(rect.gameObject)
                .SetUpdate(true)
                .ToUniTask(cancellationToken: ct);  //  定位置に初期化
        }

        //  フェードアウト
        var fadeTasks = new List<UniTask>()
        {
            Tasks.FadeOut(ct),    //  フェードアウト
            StageNameFade(1,ct),
        };

        await UniTask.WhenAll(fadeTasks);

        _isGameStart = true;
        GameManager.Instance.ChangeState(GameState.DEFAULT);

        //  BGM再生
        if (_bgmSource.isActiveAndEnabled)
        {
            _bgmSource.Play();
        }

        //  ステージ名フェードイン
        await StageNameFade(0, ct);
        async UniTask StageNameFade(float endValue, CancellationToken ct)
        {
            await Tasks.DelayTime(_waitStageNameFade, ct);
            await DOVirtual.Float(_stageNameText.alpha, endValue, _stageNameFadeDuration,
                (value) =>
                {
                    _stageNameText.alpha = value;
                })
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetLink(_stageNameText.gameObject)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
        }
        _stageNameText.gameObject.SetActive(false);
    }

    /// <summary>
    /// UI選択
    /// </summary>
    private void UISelect()
    {
        //  スキームとステートで判定
        var scheme = PlayerController.Instance.Scheme.CurrentValue;
        var state = GameManager.Instance.State.CurrentValue;
        //  Scheme判定
        if (scheme == Scheme.Gamepad)
        {
            switch (state)
            {
                case GameState.PAUSE:
                case GameState.GAMECLEAR:
                case GameState.GAMEOVER:

                    _selectButtons[state].Select();

                    break;
            }
        }
    }

    /// <summary>
    /// オプション画面移動
    /// </summary>
    /// <param name="isPause">ポーズ状態</param>
    /// <param name="target">移動位置</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>オプション画面移動処理</returns>
    private async UniTask MoveOptionFrame
        (bool isPause
        , float target
        , CancellationToken ct)
    {
        //  設定初期化
        _isMoveMenu = true;
        if (_optionFrame.activeSelf)    //  開閉判定
        {
            Helper.Audio.SaveVolume();
            BlocksRayCasts(false);  //  接触可否
        }
        else
        {
            _optionFrame.SetActive(true);
        }

        //  移動
        await MoveX_Rect(_optionFrameRect, target, _optionDuration, Ease.OutBack, ct);

        //  設定変更
        _optionFrame.SetActive(isPause);
        BlocksRayCasts(isPause);  //  接触可否
        _isMoveMenu = false;
    }

    /// <summary>
    /// リザルト演出
    /// </summary>
    /// <param name="logo"></param>
    /// <param name="ui"></param>
    /// <returns></returns>
    private async UniTask ResultStaging
        (LogoParam logo
        , RectTransform[] ui
        , List<float> uiPos
        , CancellationToken ct)
    {
        //  パラメータ取得
        var Game = GameManager.Instance;
        var TimeValue = Game.CurrentTime.CurrentValue;
        var remainingHour = Game.LimitTime - TimeValue;
        var State = Game.State.CurrentValue;
        var (Points, Count) = Game.Score;
        var hp = PlayerController.Instance.HP.CurrentValue;

        //  ------  設定初期化
        _guide.SetActive(false);
        _resultFrame.SetActive(true);
        //  クリア時UI表示設定
        if (State == GameState.GAMECLEAR)
        {
            //  枚数分表示
            for (int i = 0; i < _coinObjects.Length; i++)
            {
                _coinObjects[i].SetActive(i < Count);
            }
            _timeResultText.text = $"{remainingHour:00.00}";

            //  個数分表示
            for (int i = 0; i < _heartObjects.Length; i++)
            {
                _heartObjects[i].SetActive(i < hp);
            }
        }
        BlocksRayCasts(false);  //  接触可否

        //  ------  SE再生
        switch (State)
        {
            case GameState.GAMECLEAR:
                _resultClips?.Invoke();
                break;

            case GameState.GAMEOVER:
                _gameOverClip?.Invoke();
                break;
        }

        //  ------  ロゴ演出
        var logoTasks = new List<UniTask>
        {
            Fade_Img(_resultPanel,_resultPanelAlpha,_logoDuration,Ease.Linear,ct),
            Scale_Rect(logo.Rect,1,_logoDuration,Ease.OutBack,ct),
            AudioFade(_bgmSource, _fadeBgmVolume, _logoDuration,ct),

            logo.Text.DOFade(1, _logoDuration)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetLink(logo.Obj)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct),
        };

        await UniTask.WhenAll(logoTasks);

        //  ------  待機処理
        await Tasks.DelayTime(_resultWait, ct);

        //  ------  UI移動処理
        var uiTasks = new List<UniTask>();
        //  移動時間を徐々に増加
        //  UIの数だけずらす
        for (int i = 0; i < ui.Length; i++)
        {
            var duration = _resultUIDuration + _resultUIShifting * i;
            uiTasks.Add(MoveY_Rect(ui[i], uiPos[i], duration, Ease.OutBack, ct));
        }
        if (State == GameState.GAMEOVER)
        {
            uiTasks.Add(Scale_Rect(_gameOverParam.Rect, _gameOverLogSize.y, _resultUIDuration, Ease.OutBack, ct));
        }
        await UniTask.WhenAll(uiTasks);

        BlocksRayCasts(true);   //  接触可否

        //  ------  ロゴアニメーション
        LogoAnimation().Forget();
        async UniTask LogoAnimation()
        {
            await Scale_Rect(logo.Rect, logo.Scale.x, _sizeDuration, Ease.Linear, ct);
            await logo.Rect.DOScale(logo.Scale.y, _sizeDuration)
                .SetEase(Ease.OutBack)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true)
                .SetLink(logo.Obj)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
        }

        //  ------  スコア表示
        if (State == GameState.GAMECLEAR)
        {
            //  スコア算出
            var coinScore = Points + BonusPoint(); //  コイン
            int BonusPoint()    //  ボーナス
            {
                if (Count == MAX_COIN)
                {
                    return GET_HUNDRED;
                }
                else
                {
                    return 0;
                }
            }
            var timeScore = TimeScore(); //  時間
            int TimeScore()
            {
                //  指定範囲によってスコア変動
                if (TimeValue > _timeScoreLimit.x)
                {
                    return (int)_timeScore.x;
                }
                else if (TimeValue > _timeScoreLimit.y)
                {
                    return (int)_timeScore.y;
                }
                else if (TimeValue > _timeScoreLimit.z)
                {
                    return (int)_timeScore.z;
                }
                else
                {
                    return 0;
                }
            }

            var hpScore = hp * GET_HUNDRED;  //  HP
            var totalScore = coinScore + timeScore + hpScore;  //  合計

            //  スコア保存
            var indexName = ((SceneName)SceneManager.GetActiveScene().buildIndex).ToString();   //  シーン名取得
            Data.SaveScore(indexName, Count, hp, remainingHour, totalScore);

            //  カウント
            await CountTask(_coinScoreText, coinScore, ct); //  コイン
            await CountTask(_hpScoreText, hpScore, ct);     //  HP
            await CountTask(_timeScoreText, timeScore, ct); //  時間
            await CountTask(_scoreText, totalScore, ct);    //  合計



            // カウントタスク
            async UniTask CountTask(TMP_Text text, int value, CancellationToken ct)
            {
                _scoreClip?.Invoke();
                await DOVirtual.Int(0, value, _scoreDuration,
                    (value) =>
                    {
                        text.text = value.ToString();
                    })
                    .SetEase(Ease.OutBack)
                    .SetLink(text.gameObject)
                    .SetUpdate(true)
                    .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
            }
        }

        //  ------  音声フェードイン
        await AudioFade(_bgmSource, 1, _logoDuration * 2, ct);
    }

    /// <summary>
    /// UI接触判定変更
    /// </summary>
    /// <param name="rayCast"></param>
    private void BlocksRayCasts(bool rayCast)
    {
        if (_frame != null)
        {
            _frame.blocksRaycasts = rayCast;
        }
    }

    #region ------ TweenTask
    /// <summary>
    /// レクト移動X
    /// </summary>
    /// <param name="rect">レクトトランスフォーム</param>
    /// <param name="endValue">移動位置</param>
    /// <param name="duration">移動時間</param>
    /// <param name="ease">イース</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>移動処理</returns>
    private async UniTask MoveX_Rect
        (RectTransform rect
        , float endValue
        , float duration
        , Ease ease
        , CancellationToken ct)
    {
        await rect.DOAnchorPosX(endValue, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetLink(rect.gameObject)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
    }

    /// <summary>
    /// レクト移動Y
    /// </summary>
    /// <param name="rect">レクトトランスフォーム</param>
    /// <param name="endValue">移動位置</param>
    /// <param name="duration">移動時間</param>
    /// <param name="ease">イース</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>移動処理</returns>
    private async UniTask MoveY_Rect
        (RectTransform rect
        , float endValue
        , float duration
        , Ease ease
        , CancellationToken ct)
    {
        await rect.DOAnchorPosY(endValue, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetLink(rect.gameObject)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
    }

    /// <summary>
    /// 音量フェード
    /// </summary>
    /// <param name="source">変更オーディオソース</param>
    /// <param name="endValue">変更値</param>
    /// <param name="duration">変更時間</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>音量フェード処理</returns>
    private async UniTask AudioFade
        (AudioSource source
        , float endValue
        , float duration
        , CancellationToken ct)
    {
        await DOVirtual.Float
            (source.volume, endValue, duration
            , (value) =>
            {
                source.volume = value;
            })
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .SetLink(source.gameObject)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
    }

    /// <summary>
    /// 拡大縮小
    /// </summary>
    /// <param name="rect">レクトトランスフォーム</param>
    /// <param name="endValue">変更値</param>
    /// <param name="duration">変更時間</param>
    /// <param name="ease">イース</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>拡縮処理</returns>
    private async UniTask Scale_Rect
        (RectTransform rect
        , float endValue
        , float duration
        , Ease ease
        , CancellationToken ct)
    {
        await rect.DOScale(endValue, duration)
                .SetEase(ease)
                .SetUpdate(true)
                .SetLink(rect.gameObject)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
    }

    /// <summary>
    /// フェード_Image
    /// </summary>
    /// <param name="img">変更イメージ</param>
    /// <param name="endValue">変更値</param>
    /// <param name="duration">変更時間</param>
    /// <param name="ease">イース</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns></returns>
    private async UniTask Fade_Img
        (Image img
        , float endValue
        , float duration
        , Ease ease
        , CancellationToken ct)
    {
        await img.DOFade(endValue, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetLink(img.gameObject)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
    }

    #endregion
}
