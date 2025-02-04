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
    [SerializeField, Required, BoxGroup("�f�o�b�O")] private Helper.Switch _GUI;

    [SerializeField, Required, BoxGroup("�x�[�X")] private CanvasGroup _baseCanvas;

    [SerializeField, Required, BoxGroup("�I�[�f�B�I")] private UnityEvent _fadeClip;
    [SerializeField, Required, BoxGroup("�I�[�f�B�I")] private UnityEvent _pressAnyClip;
    [SerializeField, Required, BoxGroup("�I�[�f�B�I")] private UnityEvent _countClip;
    [SerializeField, Required, BoxGroup("�I�[�f�B�I")] private AudioSource _bgmSource;

    [SerializeField, Required, BoxGroup("���S�A�j���[�V����")] private ParticleSystem _headParticle;
    [SerializeField, Required, BoxGroup("���S�A�j���[�V����")] private RectTransform _lineRect;
    [SerializeField, Required, BoxGroup("���S�A�j���[�V����")] private Vector2 _lineMinMax;
    [SerializeField, Required, BoxGroup("���S�A�j���[�V����")] private float _animeDuration;
    [SerializeField, Required, BoxGroup("���S�A�j���[�V����")] private float _particleDuration;
    [SerializeField, Required, BoxGroup("���S�A�j���[�V����")] private TMP_Text _titleLogo;
    [SerializeField, Required, BoxGroup("���S�A�j���[�V����")] private Color _startColor;
    [SerializeField, Required, BoxGroup("���S�A�j���[�V����")] private Color _toColor;

    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private CanvasGroup _stageSelectCanvasGroup;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private GameObject _logoCanvas;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private GameObject _stageCanvas;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private float _canvasMoveDuration;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private RectTransform _pressAny;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private float _pressAnySize;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private float _pressAnyDuration;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private float _pressAnyDelay;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private float _pressAnyPushSize;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private float _pressAnyPushDuration;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private ParticleSystem _pressAnyEffect;
    [SerializeField, Required, BoxGroup("�L�����o�X�A�j���[�V����")] private Ease _canvasMoveEase;



    [SerializeField, Required, BoxGroup("���ʃA�j���[�V����")] private RectTransform _volumeFrame;
    [SerializeField, Required, BoxGroup("���ʃA�j���[�V����")] private Vector2 _volumeFramePos;
    [SerializeField, Required, BoxGroup("���ʃA�j���[�V����")] private float _volumeFrameDuration;
    [SerializeField, Required, BoxGroup("���ʃA�j���[�V����")] private Ease _volumeMoveEase;

    [SerializeField, Required, BoxGroup("�X�R�A�{�[�h")] private RectTransform _scoreBoard;
    [SerializeField, Required, BoxGroup("�X�R�A�{�[�h")] private float _scoreBoardScale;
    [SerializeField, Required, BoxGroup("�X�R�A�{�[�h")] private float _scoreBoardDuration;
    [SerializeField, Required, BoxGroup("�X�R�A�{�[�h")] private TMP_Text _totalScoreText;
    [SerializeField, Required, BoxGroup("�X�R�A�{�[�h")] private TMP_Text _averageTimeText;
    [SerializeField, Required, BoxGroup("�X�R�A�{�[�h")] private float _countDuration;

    [SerializeField, Required, BoxGroup("�{�^��")] private Button _backTitleButton;
    [SerializeField, Required, BoxGroup("�{�^��")] private Button _playButton;
    [SerializeField, Required, BoxGroup("�{�^��")] private Button _volumeButton;
    [SerializeField, Required, BoxGroup("�{�^��")] private Button _quitButton;


    [SerializeField, Required, BoxGroup("�I��")] private CanvasGroup _quitCanvas;
    [SerializeField, Required, BoxGroup("�I��")] private Vector2 _quitFadeValue;
    [SerializeField, Required, BoxGroup("�I��")] private float _quitFadeDuration;

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
        //  �t���[�����[�g�Œ�
        Application.targetFrameRate = 60;

        //  �X�R�A������
#if UNITY_EDITOR
        //  �f�[�^������������
        if (Data.ScoreList.Count == 0)
        {
            Data.ScoreInit();
        }
#endif
    }

    private async void Start()
    {
        ParamImplement();   //  �p�����[�^�ۑ�
        EventObserve(); //  �C�x���g�Ď�

        //  �X�^�[�g������
        await Tasks.Canceled(StartEvent(destroyCancellationToken));
    }

#if UNITY_EDITOR
    private void Update()
    {

    }
#endif

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
    /// �Q�[���J�n
    /// </summary>
    /// <param name="ctx">�R�[���o�b�N�R���e�L�X�g</param>
    public async void OnAny(InputAction.CallbackContext ctx)
    {
        //  ���͔���
        //  �X�e�[�g����
        //  UI�̈ړ�����
        if (ctx.phase == InputActionPhase.Performed
            && _state.Value == State.TITLE
            && !_isMoveCanvas)
        {
            //  �ҋ@����
            //  �A�j���[�V�������J�n�A�I������O�ɂ�����x�X�e�[�g�ύX����Ă��܂����ߑҋ@����������
            await Tasks.DelayTime(0.1f, destroyCancellationToken);
            _state.Value = State.STAGESELECT;
        }
    }

    /// <summary>
    /// �Q�[���I��
    /// </summary>
    /// <param name="ctx">�R�[���o�b�N�R���e�L�X�g</param>
    public async void OnGameQuit(InputAction.CallbackContext ctx)
    {
        //  �X�e�[�g����
        //  UI�ړ�����
        if (_state.Value == State.STAGESELECT
            && !_isMoveCanvas)
        {
            //  �Q�[���I����ʕ\��
            await FadeQuitCanvas(true, _quitFadeValue.x, _quitFadeValue.y, destroyCancellationToken);
        }
    }

    /// <summary>
    /// �Q�[���I���L�����o�X�\����\������
    /// </summary>
    /// <param name="isOpen">UI�ړ����</param>
    /// <param name="from">�t�F�[�h�J�n���</param>
    /// <param name="to">�t�F�[�h�I�����</param>
    /// <param name="ct">�L�����Z���g�[�N��</param>
    /// <returns>�Q�[���I���L�����o�X�\����\������</returns>
    public async UniTask FadeQuitCanvas(bool isOpen, float from, float to, CancellationToken ct)
    {
        var quitCanvasObj = _quitCanvas.gameObject;
        //  ��ԍX�V
        _isMoveCanvas = isOpen;
        _stageSelectCanvasGroup.blocksRaycasts = !isOpen;
        quitCanvasObj.SetActive(true);

        //  �t�F�[�h
        await DOVirtual.Float(from, to, _quitFadeDuration, fade =>
        {
            _quitCanvas.alpha = fade;
        })
        .SetEase(Ease.Linear)
        .SetLink(quitCanvasObj)
        .ToUniTask(Tasks.TCB, cancellationToken: ct);

        //  ��ԍX�V
        quitCanvasObj.SetActive(isOpen);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// �p�����[�^�ۑ�
    /// </summary>
    private void ParamImplement()
    {
        Tasks.FadeClip = _fadeClip;   //  �t�F�[�hSE�ݒ�

        //  Rect�ۑ�
        _logoRect = _logoCanvas.GetComponent<RectTransform>();
        _stageRect = _stageCanvas.GetComponent<RectTransform>();

    }

    /// <summary>
    /// �C�x���g�Ď�
    /// </summary>
    private void EventObserve()
    {
        //  �X�e�[�g�Ď�
        _state.SubscribeAwait(async (state, ct) =>
        {
            //  UI�ړ���
            _isMoveCanvas = true;
            switch (state)
            {
                case State.TITLE:
                    //  UI����u���b�N
                    _stageSelectCanvasGroup.blocksRaycasts = false;

                    //  �L�����o�X�ړ�
                    await MoveCanvas(0, -Base.HEIGHT, ct);
                    //  �X�e�[�W�L�����o�X��\��
                    _stageCanvas.SetActive(false);

                    //  UI����ĊJ
                    _stageSelectCanvasGroup.blocksRaycasts = true;

                    break;

                case State.STAGESELECT:
                    //  UI����u���b�N
                    _stageSelectCanvasGroup.blocksRaycasts = false;

                    //  �X�e�[�W�L�����o�X�\��
                    _stageCanvas.SetActive(true);

                    //  �A�C�h���A�j���[�V�������~
                    _pressAnyIdleAnime.Kill();

                    //  �e�L�X�g���͂ŃG�t�F�N�g�Đ�
                    _pressAnyEffect.Play();
                    _pressAnyClip?.Invoke();
                    //  �A�j���[�V����
                    await _pressAny.DOScale(_pressAnyPushSize, _pressAnyPushDuration)
                            .SetEase(Ease.OutBack)
                            .SetUpdate(true)
                            .SetLink(_pressAny.gameObject)
                            .ToUniTask(Tasks.TCB, cancellationToken: ct);

                    //  �����ɃL�����o�X���ړ������ɑҋ@
                    await Tasks.DelayTime(_pressAnyDelay, ct);

                    //  �L�����o�X�ړ�
                    await MoveCanvas(Base.HEIGHT, 0, ct);

                    // --- �X�R�A�\��
                    //  �X�P�[���A�j���[�V����
                    await ScoreBoardSize(_scoreBoardScale);
                    //  �J�E���g�A�j���[�V����
                    await CountTask(_totalScoreText, Data.TotalScore, "0", _countDuration, ct);
                    await CountTask(_averageTimeText, Data.AverageTime, "0.00", _countDuration, ct);
                    //  �ҋ@
                    await Tasks.DelayTime(_scoreBoardDuration, ct);
                    //  �ʏ�T�C�Y�ɖ߂�
                    await ScoreBoardSize(1);


                    // �J�E���g�^�X�N
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
                            .ToUniTask(Tasks.TCB, cancellationToken: ct);
                    }
                    //  �X�R�A�{�[�h�T�C�Y
                    async UniTask ScoreBoardSize(float size)
                    {
                        await _scoreBoard.DOScale(size, _scoreBoardDuration)
                            .SetEase(Ease.OutBack)
                            .SetLink(_scoreBoard.gameObject)
                            .SetUpdate(true)
                            .ToUniTask(Tasks.TCB, cancellationToken: ct);
                    }

                    //  UI����ĊJ
                    _stageSelectCanvasGroup.blocksRaycasts = true;

                    break;
            }
            _isMoveCanvas = false;

        }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);

        // �{�^���Ď�
        _backTitleButton.OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                //  �X�e�[�g����
                if (_state.Value == State.STAGESELECT)
                {
                    //  �I�[�f�B�I�ۑ�
                    Audio.SaveVolume();

                    //  �I�[�f�B�I�t���[���ʒu������
                    await MoveY(_volumeFrame, _volumeFramePos.x, _volumeFrameDuration, _volumeMoveEase, ct);

                    //  �e�L�X�g�A�C�h���A�j���[�V�����Đ�
                    _pressAnyIdleAnime = PressAnyAnimation();
                    _state.Value = State.TITLE;
                }
            }, AwaitOperation.Drop)
            .RegisterTo(destroyCancellationToken);

        _playButton.OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                //  �I�[�f�B�I�ۑ�
                Audio.SaveVolume();
                //  �w��V�[���ֈڍs
                await Tasks.SceneChange(_scroller.Index + 1, _baseCanvas, ct);

            }, AwaitOperation.Drop)
            .RegisterTo(destroyCancellationToken);

        _volumeButton.OnClickAsObservable()
            .SubscribeAwait(async (_, ct) =>
            {
                //  �I�[�f�B�I�t���[���̈ړ����
                _isVolumeFrameUp = !_isVolumeFrameUp;
                //  �ړ���w��
                var pos = _isVolumeFrameUp ? _volumeFramePos.y : _volumeFramePos.x;
                //  �ړ�
                await MoveY(_volumeFrame, pos, _volumeFrameDuration, _volumeMoveEase, ct);

            }, AwaitOperation.Drop)
            .RegisterTo(destroyCancellationToken);

        _quitButton?.OnClickAsObservable()
             .SubscribeAwait(async (_, ct) =>
             {
                 // �I����ʕ\��
                 await FadeQuitCanvas(true, _quitFadeValue.x, _quitFadeValue.y, ct);

             }, AwaitOperation.Drop)
             .RegisterTo(destroyCancellationToken);
    }

    /// <summary>
    /// �X�^�[�g�C�x���g
    /// </summary>
    /// <param name="ct">�L�����Z���g�[�N��</param>
    /// <returns>�X�^�[�g�C�x���g����</returns>
    private async UniTask StartEvent(CancellationToken ct)
    {
        //  �I�v�V�����ʒu������
        await MoveY(_stageRect, -Base.HEIGHT, 0, Ease.Linear, ct);

        //  �t�F�[�h�A�E�g
        await Tasks.FadeOut(ct);
        //  BGM�Đ�
        if (_bgmSource.isActiveAndEnabled)
        {
            _bgmSource.Play();
        }

        //  ���S�F�ύX
        _ = DOVirtual.Color(_startColor, _toColor, _particleDuration, (color) =>
            {
                _titleLogo.color = color;
            })
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(gameObject);

        _pressAnyIdleAnime = PressAnyAnimation();

        //  �A�j���[�V�����Đ����[�v
        AnimationLoop(ct).Forget();
        async UniTask AnimationLoop(CancellationToken ct)
        {
            var lineRectObj = _lineRect.gameObject;
            await Tasks.DelayTime(_particleDuration, ct);
            while (true)
            {
                //  �t�b�N�����A�j���[�V����
                await _lineRect.DOAnchorPosX(_lineMinMax.y, _animeDuration)
                    .SetEase(Ease.Linear)
                    .SetLink(lineRectObj)
                    .ToUniTask(Tasks.TCB, cancellationToken: ct);

                //  ��[�����ŃG�t�F�N�g�Đ�
                _headParticle.Play();

                //  �ҋ@����
                await Tasks.DelayTime(_particleDuration, ct);

                //  �t�b�N�����߂��A�j���[�V����
                await _lineRect.DOAnchorPosX(_lineMinMax.x, _animeDuration)
                    .SetEase(Ease.Linear)
                    .SetLink(lineRectObj)
                    .ToUniTask(Tasks.TCB, cancellationToken: ct);
            }
        }
    }

    /// <summary>
    /// �Ăяo���p�A�j���[�V����
    /// </summary>
    /// <returns>�A�j���[�V����</returns>
    private Tweener PressAnyAnimation()
    {
        //  �X�P�[��������
        _pressAny.transform.localScale = Vector3.one;

        return _pressAny.DOScale(_pressAnySize, _pressAnyDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetLink(_pressAny.gameObject);
    }

    /// <summary>
    /// �L�����o�X�ړ�
    /// </summary>
    /// <param name="logoPos">�^�C�g�����S�ʒu</param>
    /// <param name="stagePos">�X�e�[�W�I����ʈʒu</param>
    /// <returns>�L�����o�X�ړ�����</returns>
    private async UniTask MoveCanvas(float logoPos, float stagePos, CancellationToken ct)
    {
        var moveTasks = new List<UniTask>()
            {
                //  �^�C�g�����S�ړ�
                MoveY(_logoRect, logoPos, _canvasMoveDuration, _canvasMoveEase, ct),
                //  �X�e�[�W�I����ʈړ�
                MoveY( _stageRect, stagePos, _canvasMoveDuration, _canvasMoveEase, ct),
            };
        await UniTask.WhenAll(moveTasks);
    }

    /// <summary>
    /// Y���ړ�
    /// </summary>
    /// <param name="obj">�ړ��I�u�W�F�N�g</param>
    /// <param name="rect">���N�g�g�����X�t�H�[��</param>
    /// <param name="toValue">�I���ʒu</param>
    /// <param name="duration">�A�j���[�V��������</param>
    /// <param name="ease">�C�[�Y</param>
    /// <param name="ct">�L�����Z���g�[�N��</param>
    /// <returns>�ړ�����</returns>
    private async UniTask MoveY
        (RectTransform rect
        , float toValue
        , float duration
        , Ease ease
        , CancellationToken ct)
    {
        await rect.DOAnchorPosY(toValue, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetLink(rect.gameObject)
            .ToUniTask(Tasks.TCB, cancellationToken: ct);
    }
}
