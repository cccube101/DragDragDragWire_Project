using Alchemy.Inspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonAnimation : UIAnimatorBase, IUIAnimation
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("パラメータ")] private TMP_Text _text;
    [SerializeField, Required, BoxGroup("パラメータ")] private float _pressedTextPos;

    [SerializeField, Required, BoxGroup("ノーマル")] private Sprite _normalImage;

    [SerializeField, Required, BoxGroup("ハイライト")] private Sprite _highlightImage;
    [SerializeField, Required, BoxGroup("ハイライト")] private UnityEvent _highlightClip;

    [SerializeField, Required, BoxGroup("プレス")] private Sprite _pressedImage;
    [SerializeField, Required, BoxGroup("プレス")] private UnityEvent _pressedClip;

    // ---------------------------- Field
    //  アニメーション
    private bool _isPlayPressClip;
    private Vector3 _initPos;


    // ---------------------------- UnityMessage
    public override void Awake()
    {
        StartEvent();

        //  テキスト初期位置保存
        _initPos = _text.rectTransform.anchoredPosition;
    }

    // ---------------------------- PublicMethod
    #region ------ StateAnimation
    /// <summary>
    /// ノーマルイベント
    /// </summary>
    public void Normal()
    {
        UpdateAnimation
            (Color.white
            , _initPos
            , null
            , _normalImage);

        _isPlayPressClip = false;
    }

    /// <summary>
    /// ハイライトイベント
    /// </summary>
    public void Highlighted()
    {
        UpdateAnimation
            (Color.black
            , _initPos
            , _highlightClip
            , _highlightImage);
    }

    /// <summary>
    /// プレスイベント
    /// </summary>
    public void Pressed()
    {
        UpdateAnimation
            (Color.black
            , new Vector3(_initPos.x, _pressedTextPos, _initPos.z)
            , _pressedClip
            , _pressedImage);
        _isPlayPressClip = true;
    }

    /// <summary>
    /// セレクトイベント
    /// </summary>
    public void Selected()
    {
        UpdateAnimation
            (Color.black
            , _initPos
            , _highlightClip
            , _highlightImage);
        _isPlayPressClip = false;

    }

    /// <summary>
    /// ディサブルイベント
    /// </summary>
    public void Disabled()
    {

    }

    #endregion

    // ---------------------------- PrivateMethod
    /// <summary>
    /// アニメーション更新
    /// </summary>
    /// <param name="textColor">テキスト色</param>
    /// <param name="textPos">テキスト位置</param>
    /// <param name="clip">効果音</param>
    /// <param name="image">変更イメージ</param>
    private void UpdateAnimation
        (Color textColor
        , Vector3 textPos
        , UnityEvent clip
        , Sprite image)
    {
        _text.color = textColor;
        _text.rectTransform.anchoredPosition = textPos;
        if (!_isPlayPressClip)
        {
            clip?.Invoke();
        }
        GetComponent<Image>().sprite = image;
    }
}
