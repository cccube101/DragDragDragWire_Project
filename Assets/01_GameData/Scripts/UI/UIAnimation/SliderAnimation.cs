using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using DG.Tweening;
using Alchemy.Inspector;

public class SliderAnimation : UIAnimatorBase, IUIAnimation
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("パラメータ")] private Image _fillFrameImg;
    [SerializeField, Required, BoxGroup("パラメータ")] private Image _fillImg;
    [SerializeField, Required, BoxGroup("パラメータ")] private Image _handleFrameImg;
    [SerializeField, Required, BoxGroup("パラメータ")] private Image _handleImg;
    [SerializeField, Required, BoxGroup("パラメータ")] private float _animeDuration;

    [SerializeField, Required, BoxGroup("ノーマル")] private Color _normalColor;
    [SerializeField, Required, BoxGroup("ノーマル")] private Color _normalFrameColor;

    [SerializeField, Required, BoxGroup("ハイライト")] private Color _highlightColor;
    [SerializeField, Required, BoxGroup("ハイライト")] private Color _highlightFrameColor;
    [SerializeField, Required, BoxGroup("ハイライト")] private UnityEvent _highlightClip;


    // ---------------------------- Field



    // ---------------------------- UnityMessage



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
