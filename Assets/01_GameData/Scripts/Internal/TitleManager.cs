using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Helper;
using Alchemy.Inspector;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TitleManager : MonoBehaviour
{
    private enum State
    {
        TITLE, STAGESELECT
    }

    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("デバッグ")] private Helper.Switch _GUI;

    [SerializeField, Required, BoxGroup("ベース")] private CanvasGroup _baseCanvas;

    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _fadeClip;
    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _pressAnyClip;
    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _countClip;
    [SerializeField, Required, BoxGroup("オーディオ")] private AudioSource _bgmSource;

    [SerializeField, Required, BoxGroup("ロゴアニメーション")] private ParticleSystem _headParticle;
    [SerializeField, Required, BoxGroup("ロゴアニメーション")] private RectTransform _lineRect;
    [SerializeField, Required, BoxGroup("ロゴアニメーション")] private Vector2 _lineMinMax;
    [SerializeField, Required, BoxGroup("ロゴアニメーション")] private float _animeDuration;
    [SerializeField, Required, BoxGroup("ロゴアニメーション")] private float _particleDuration;
    [SerializeField, Required, BoxGroup("ロゴアニメーション")] private TMP_Text _titleLogo;
    [SerializeField, Required, BoxGroup("ロゴアニメーション")] private Color _startColor;
    [SerializeField, Required, BoxGroup("ロゴアニメーション")] private Color _toColor;

    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private CanvasGroup _stageSelectCanvasGroup;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private GameObject _logoCanvas;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private GameObject _stageCanvas;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private float _canvasMoveDuration;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private RectTransform _pressAny;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private float _pressAnySize;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private float _pressAnyDuration;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private float _pressAnyDelay;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private float _pressAnyPushSize;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private float _pressAnyPushDuration;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private ParticleSystem _pressAnyEffect;
    [SerializeField, Required, BoxGroup("キャンバスアニメーション")] private Ease _canvasMoveEase;



    [SerializeField, Required, BoxGroup("音量アニメーション")] private RectTransform _volumeFrame;
    [SerializeField, Required, BoxGroup("音量アニメーション")] private Vector2 _volumeFramePos;
    [SerializeField, Required, BoxGroup("音量アニメーション")] private float _volumeFrameDuration;
    [SerializeField, Required, BoxGroup("音量アニメーション")] private Ease _volumeMoveEase;

    [SerializeField, Required, BoxGroup("スコアボード")] private RectTransform _scoreBoard;
    [SerializeField, Required, BoxGroup("スコアボード")] private float _scoreBoardScale;
    [SerializeField, Required, BoxGroup("スコアボード")] private float _scoreBoardDuration;
    [SerializeField, Required, BoxGroup("スコアボード")] private TMP_Text _totalScoreText;
    [SerializeField, Required, BoxGroup("スコアボード")] private TMP_Text _averageTimeText;
    [SerializeField, Required, BoxGroup("スコアボード")] private float _countDuration;

    [SerializeField, Required, BoxGroup("ボタン")] private Button _backTitleButton;
    [SerializeField, Required, BoxGroup("ボタン")] private Button _playButton;
    [SerializeField, Required, BoxGroup("ボタン")] private Button _volumeButton;
    [SerializeField, Required, BoxGroup("ボタン")] private Button _quitButton;


    [SerializeField, Required, BoxGroup("終了")] private CanvasGroup _quitCanvas;
    [SerializeField, Required, BoxGroup("終了")] private Vector2 _quitFadeValue;
    [SerializeField, Required, BoxGroup("終了")] private float _quitFadeDuration;




    [SerializeField] private FancyScrollView.Scroller _scroller;

    // ---------------------------- Field
    private readonly ReactiveProperty<State> _state = new(State.TITLE);
    private bool _isMoveCanvas = false;

    private RectTransform _logoRect;
    private RectTransform _stageRect;

    private bool _isVolumeFrameUp = false;

    private Tweener _pressAnyIdleAnime;

    // ---------------------------- Property
    public Vector2 QuitFadeValue => _quitFadeValue;



    // ---------------------------- UnityMessage

    private void Awake()
    {
        //  フレームレート固定
        Application.targetFrameRate = 60;

        //  スコア初期化
#if UNITY_EDITOR
        //  データが無かったら
        if (Data.ScoreList.Count == 0)
        {
            Data.ScoreInit();
        }
#endif
    }

    private async void Start()
    {
        ParamImplement();   //  パラメータ保存
        EventObserve(); //  イベント監視

        //  スタート時処理
        await Tasks.Canceled(StartEvent(destroyCancellationToken));
    }

    private void OnGUI()
    {
        if (_GUI == Helper.Switch.ON)
        {
            var pos = new Rect[30];
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = new Rect(10, 1080 - i * 30, 300, 30);
            }
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 25;

            GUI.TextField(pos[1], $"Focus", style);
        }
    }

    // ---------------------------- PublicMethod
    /// <summary>
    /// ゲーム開始
    /// </summary>
    /// <param name="ctx">コールバックコンテキスト</param>
    public async void OnAny(InputAction.CallbackContext ctx)
    {
        //  入力判定
        //  ステート判定
        //  UIの移動判定
        if (ctx.phase == InputActionPhase.Performed
            && _state.Value == State.TITLE
            && !_isMoveCanvas)
        {
            //  待機処理
            //  アニメーションが開始、終了する前にもう一度ステート変更されてしまうため待機処理を挟む
            await Tasks.DelayTime(0.1f, destroyCancellationToken);
            _state.Value = State.STAGESELECT;
        }
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    /// <param name="ctx">コールバックコンテキスト</param>
    public async void OnGameQuit(InputAction.CallbackContext ctx)
    {
        //  ステート判定
        //  UI移動判定
        if (_state.Value == State.STAGESELECT
            && !_isMoveCanvas)
        {
            //  ゲーム終了画面表示
            await FadeQuitCanvas(true, _quitFadeValue.x, _quitFadeValue.y, destroyCancellationToken);
        }
    }

    /// <summary>
    /// ゲーム終了キャンバス表示非表示処理
    /// </summary>
    /// <param name="isOpen">UI移動状態</param>
    /// <param name="from">フェード開始状態</param>
    /// <param name="to">フェード終了状態</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>ゲーム終了キャンバス表示非表示処理</returns>
    public async UniTask FadeQuitCanvas(bool isOpen, float from, float to, CancellationToken ct)
    {
        //  状態更新
        _isMoveCanvas = isOpen;
        _stageSelectCanvasGroup.blocksRaycasts = !isOpen;
        _quitCanvas.gameObject.SetActive(true);

        //  フェード
        await DOVirtual.Float(from, to, _quitFadeDuration, fade =>
        {
            _quitCanvas.alpha = fade;
        })
        .SetEase(Ease.Linear)
        .SetLink(_quitCanvas.gameObject)
        .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);

        //  状態更新
        _quitCanvas.gameObject.SetActive(isOpen);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// パラメータ保存
    /// </summary>
    private void ParamImplement()
    {
        Tasks.FadeClip = _fadeClip;   //  フェードSE設定

        //  Rect保存
        _logoRect = _logoCanvas.GetComponent<RectTransform>();
        _stageRect = _stageCanvas.GetComponent<RectTransform>();

    }

    /// <summary>
    /// イベント監視
    /// </summary>
    private void EventObserve()
    {
        //  ステート監視
        _state.SubscribeAwait(async (state, ct) =>
        {
            //  UI移動中
            _isMoveCanvas = true;
            switch (state)
            {
                case State.TITLE:
                    //  UI判定ブロック
                    _stageSelectCanvasGroup.blocksRaycasts = false;

                    //  キャンバス移動
                    await MoveCanvas(0, -Base.HEIGHT, ct);
                    //  ステージキャンバス非表示
                    _stageCanvas.SetActive(false);

                    //  UI判定再開
                    _stageSelectCanvasGroup.blocksRaycasts = true;

                    break;

                case State.STAGESELECT:
                    //  UI判定ブロック
                    _stageSelectCanvasGroup.blocksRaycasts = false;

                    //  ステージキャンバス表示
                    _stageCanvas.SetActive(true);

                    //  アイドルアニメーションを停止
                    _pressAnyIdleAnime.Kill();

                    //  テキスト周囲でエフェクト再生
                    _pressAnyEffect.Play();
                    _pressAnyClip?.Invoke();
                    //  アニメーション
                    await _pressAny.DOScale(_pressAnyPushSize, _pressAnyPushDuration)
                            .SetEase(Ease.OutBack)
                            .SetUpdate(true)
                            .SetLink(_pressAny.gameObject)
                            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);

                    //  すぐにキャンバスを移動せずに待機
                    await Tasks.DelayTime(_pressAnyDelay, ct);

                    //  キャンバス移動
                    await MoveCanvas(Base.HEIGHT, 0, ct);

                    // --- スコア表示
                    //  スケールアニメーション
                    await ScoreBoardSize(_scoreBoardScale);
                    //  カウントアニメーション
                    await CountTask(_totalScoreText, Data.TotalScore, "0", _countDuration, ct);
                    await CountTask(_averageTimeText, Data.AverageTime, "0.00", _countDuration, ct);
                    //  待機
                    await Tasks.DelayTime(_scoreBoardDuration, ct);
                    //  通常サイズに戻す
                    await ScoreBoardSize(1);


                    // カウントタスク
                    async UniTask CountTask(TMP_Text text, float value, string writing, float duration, CancellationToken ct)
                    {
                        _countClip?.Invoke();
                        await DOVirtual.Float(0, value, duration,
                            (value) =>
                            {
                                text.text = value.ToString(writing);
                            })
                            .SetEase(Ease.OutBack)
                            .SetLink(text.gameObject)
                            .SetUpdate(true)
                            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
                    }
                    //  スコアボードサイズ
                    async UniTask ScoreBoardSize(float size)
                    {
                        await _scoreBoard.DOScale(size, _scoreBoardDuration)
                            .SetEase(Ease.OutBack)
                            .SetLink(_scoreBoard.gameObject)
                            .SetUpdate(true)
                            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
                    }

                    //  UI判定再開
                    _stageSelectCanvasGroup.blocksRaycasts = true;

                    break;
            }
            _isMoveCanvas = false;

        }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);

        // ボタン監視
        _backTitleButton.OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                //  ステート判定
                if (_state.Value == State.STAGESELECT)
                {
                    //  オーディオ保存
                    Audio.SaveVolume();

                    //  オーディオフレーム位置初期化
                    await MoveY(_volumeFrame.gameObject, _volumeFrame, _volumeFramePos.x, _volumeFrameDuration, _volumeMoveEase, ct);

                    //  テキストアイドルアニメーション再生
                    _pressAnyIdleAnime = PressAnyAnimation();
                    _state.Value = State.TITLE;
                }
            }, AwaitOperation.Drop)
            .RegisterTo(destroyCancellationToken);

        _playButton.OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                //  オーディオ保存
                Audio.SaveVolume();
                //  指定シーンへ移行
                await Tasks.SceneChange(_scroller.Index + 1, _baseCanvas, ct);

            }, AwaitOperation.Drop)
            .RegisterTo(destroyCancellationToken);

        _volumeButton.OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                //  オーディオフレームの移動状態
                _isVolumeFrameUp = !_isVolumeFrameUp;
                //  移動先指定
                var pos = _isVolumeFrameUp ? _volumeFramePos.y : _volumeFramePos.x;
                //  移動
                await MoveY(_volumeFrame.gameObject, _volumeFrame, pos, _volumeFrameDuration, _volumeMoveEase, ct);

            }, AwaitOperation.Drop)
            .RegisterTo(destroyCancellationToken);

        _quitButton.OnClickAsObservable()
             .SubscribeAwait(async (_, ct) =>
             {
                 // 終了画面表示
                 await FadeQuitCanvas(true, _quitFadeValue.x, _quitFadeValue.y, ct);

             }, AwaitOperation.Drop)
             .RegisterTo(destroyCancellationToken);
    }

    /// <summary>
    /// スタートイベント
    /// </summary>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>スタートイベント処理</returns>
    private async UniTask StartEvent(CancellationToken ct)
    {
        //  オプション位置初期化
        await MoveY(_stageCanvas, _stageRect, -Base.HEIGHT, 0, Ease.Linear, ct);

        //  フェードアウト
        await Tasks.FadeOut(ct);
        //  BGM再生
        if (_bgmSource.isActiveAndEnabled)
        {
            _bgmSource.Play();
        }

        //  ロゴ色変更
        _ = DOVirtual.Color(_startColor, _toColor, _particleDuration, (color) =>
            {
                _titleLogo.color = color;
            })
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(gameObject);

        _pressAnyIdleAnime = PressAnyAnimation();

        //  アニメーション再生ループ
        AnimationLoop(ct).Forget();
        async UniTask AnimationLoop(CancellationToken ct)
        {
            await Helper.Tasks.DelayTime(_particleDuration, ct);
            while (true)
            {
                //  フック部分アニメーション
                await _lineRect.DOAnchorPosX(_lineMinMax.y, _animeDuration)
                    .SetEase(Ease.Linear)
                    .SetLink(_lineRect.gameObject)
                    .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);

                //  先端部分でエフェクト再生
                _headParticle.Play();

                //  待機処理
                await Helper.Tasks.DelayTime(_particleDuration, ct);

                //  フック部分戻すアニメーション
                await _lineRect.DOAnchorPosX(_lineMinMax.x, _animeDuration)
                    .SetEase(Ease.Linear)
                    .SetLink(_lineRect.gameObject)
                    .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
            }
        }
    }

    /// <summary>
    /// 呼び出し用アニメーション
    /// </summary>
    /// <returns>アニメーション</returns>
    private Tweener PressAnyAnimation()
    {
        //  スケール初期化
        _pressAny.transform.localScale = Vector3.one;

        return _pressAny.DOScale(_pressAnySize, _pressAnyDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(_pressAny.gameObject);
    }

    /// <summary>
    /// キャンバス移動
    /// </summary>
    /// <param name="logoPos">タイトルロゴ位置</param>
    /// <param name="stagePos">ステージ選択画面位置</param>
    /// <returns>キャンバス移動処理</returns>
    private async UniTask MoveCanvas(float logoPos, float stagePos, CancellationToken ct)
    {
        var moveTasks = new List<UniTask>()
            {
                //  タイトルロゴ移動
                MoveY(_logoCanvas,_logoRect, logoPos, _canvasMoveDuration, _canvasMoveEase, ct),
                //  ステージ選択画面移動
                MoveY(_stageCanvas, _stageRect, stagePos, _canvasMoveDuration, _canvasMoveEase, ct),
            };
        await UniTask.WhenAll(moveTasks);
    }

    /// <summary>
    /// Y軸移動
    /// </summary>
    /// <param name="obj">移動オブジェクト</param>
    /// <param name="rect">レクトトランスフォーム</param>
    /// <param name="toValue">終了位置</param>
    /// <param name="duration">アニメーション時間</param>
    /// <param name="ease">イーズ</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>移動処理</returns>
    private async UniTask MoveY
        (GameObject obj
        , RectTransform rect
        , float toValue
        , float duration
        , Ease ease
        , CancellationToken ct)
    {
        await rect.DOAnchorPosY(toValue, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetLink(obj)
            .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
    }
}
