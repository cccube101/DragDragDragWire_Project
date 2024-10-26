using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using DG.Tweening;
using System.Collections.Generic;
using R3;
using R3.Triggers;
using Alchemy.Inspector;

public class SliderAnimation : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private UnityEvent[] _event;

    [SerializeField, Required, BoxGroup("基礎パラメータ")] private Image _fillFrameImg;
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private Image _fillImg;
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private Image _handleFrameImg;
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private Image _handleImg;
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private float _animeDuration;

    [SerializeField, Required, BoxGroup("ノーマル")] private Color _normalColor;
    [SerializeField, Required, BoxGroup("ノーマル")] private Color _normalFrameColor;

    [SerializeField, Required, BoxGroup("ハイライト")] private Color _highlightColor;
    [SerializeField, Required, BoxGroup("ハイライト")] private Color _highlightFrameColor;
    [SerializeField, Required, BoxGroup("ハイライト")] private UnityEvent _highlightClip;


    // ---------------------------- Field
    //  実行メソッド
    private Dictionary<string, UnityEvent> _actions;

    // ---------------------------- UnityMessage
    private void Start()
    {
        //  レイヤー名取得
        var layer = GetComponent<Animator>().GetLayerName(0);
        var clips = GetComponent<Animator>().runtimeAnimatorController.animationClips;

        //  メソッド格納
        _actions = new Dictionary<string, UnityEvent>(clips.Length);
        for (int i = 0; i < clips.Length; i++)
        {
            //  "レイヤー.ステート名"
            _actions.Add($"{layer}.{clips[i].name}", _event[i]);
        }
    }

    private void OnEnable()
    {
        //  アニメーターステート監視
        GetComponent<Animator>().GetBehaviour<ObservableStateMachineTrigger>()
            .OnStateEnterAsObservable()
            .Subscribe(state =>
            {
                //  アクション数分処理
                foreach (var item in _actions)
                {
                    if (state.StateInfo.IsName(item.Key))   //  ステート名で判定
                    {
                        _actions[item.Key].Invoke();    //  実行
                    }
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    #region ------ StateAnimation
    /// <summary>
    /// ノーマル
    /// </summary>
    public void Normal()
    {
        UpdateAnimation(_normalColor, _normalFrameColor);
    }

    /// <summary>
    /// ハイライト
    /// </summary>
    public void Highlighted()
    {
        UpdateAnimation(_highlightColor, _highlightFrameColor);
        _highlightClip?.Invoke();
        GetComponent<Slider>().Select();
    }

    /// <summary>
    /// プレス
    /// </summary>
    public void Pressed()
    {

    }

    /// <summary>
    /// セレクト
    /// </summary>
    public void Selected()
    {

    }

    /// <summary>
    /// ディサブル
    /// </summary>
    public void Disabled()
    {

    }

    #endregion


    // ---------------------------- PrivateMethod

    /// <summary>
    /// アニメーション更新
    /// </summary>
    /// <param name="content">背景色</param>
    /// <param name="frame">フレーム色</param>
    private void UpdateAnimation(Color content, Color frame)
    {
        ChangeColor(_fillImg, content);
        ChangeColor(_fillFrameImg, frame);
        ChangeColor(_handleImg, content);
        ChangeColor(_handleFrameImg, frame);
    }

    /// <summary>
    /// 色変更
    /// </summary>
    /// <param name="img">変更先</param>
    /// <param name="toColor">変更色</param>
    private void ChangeColor(Image img, Color toColor)
    {
        DOVirtual.Color
            (img.color, toColor
            , _animeDuration,
            (result) =>
            {
                img.color = result;
            })
            .SetUpdate(true)
            .SetEase(Ease.Linear)
            .SetLink(gameObject);
    }
}
