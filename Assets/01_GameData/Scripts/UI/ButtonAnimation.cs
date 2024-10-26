using Alchemy.Inspector;
using R3;
using R3.Triggers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private UnityEvent[] _event;
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private TMP_Text _text;
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private float _pressedTextPos;

    [SerializeField, Required, BoxGroup("ノーマル")] private Sprite _normalImage;

    [SerializeField, Required, BoxGroup("ハイライト")] private Sprite _highlightImage;
    [SerializeField, Required, BoxGroup("ハイライト")] private UnityEvent _highlightClip;

    [SerializeField, Required, BoxGroup("プレス")] private Sprite _pressedImage;
    [SerializeField, Required, BoxGroup("プレス")] private UnityEvent _pressedClip;

    // ---------------------------- Field
    //  実行メソッド
    private Dictionary<string, UnityEvent> _actions;

    //  アニメーション
    private bool _isPlayPressClip;
    private Vector3 _initPos;


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

        //  テキスト初期位置保存
        _initPos = _text.rectTransform.anchoredPosition;
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
                        _actions[item.Key]?.Invoke();    //  実行
                    }
                }
            })
            .AddTo(this);
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
        GetComponent<Button>().Select();
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
