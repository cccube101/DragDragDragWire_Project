using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PortalController : GimmickBase
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("パラメータ")] private Transform _warpPos;
    [SerializeField, Required, BoxGroup("パラメータ")] private float _duration;
    [SerializeField, Required, BoxGroup("パラメータ")] private SpriteRenderer _fadeRing;
    [SerializeField, Required, BoxGroup("パラメータ")] private float _fadeValue;
    [SerializeField, Required, BoxGroup("パラメータ")] private float _scaleValue;
    [SerializeField, Required, BoxGroup("パラメータ")] private float _fadeDuration;
    [SerializeField, Required, BoxGroup("パラメータ")] private AudioSource _audio;
    [SerializeField, Required, BoxGroup("パラメータ")] private AudioClip _clip;


    // ---------------------------- Field
    private static bool _isWarping = false;
    private float _ringScaleInit = 0;


    // ---------------------------- UnityMessage
    private void Start()
    {
        //  初期パラメータ保存
        _ringScaleInit = _fadeRing.transform.localScale.x;  //  サイズ
        _fadeRing.color = new Color(_color.r, _color.g, _color.b, 0);   //  色
    }

    private async void OnTriggerEnter2D(Collider2D collision)
    {
        // ------ 非プレイヤー時早期リターン
        if (!collision.gameObject.CompareTag(TagName.Player)) return;

        //  色変更アニメーション開始
        FadeColor(destroyCancellationToken).Forget();

        async UniTask FadeColor(CancellationToken ct)
        {
            //  変更
            List<UniTask> toTasks = new()
                {
                    FadeTask(_fadeValue),
                    ScaleTask(_scaleValue),
                };
            await UniTask.WhenAll(toTasks);

            //  戻す
            List<UniTask> endTasks = new()
                {
                    FadeTask(0),
                    ScaleTask(_ringScaleInit),
                };
            await UniTask.WhenAll(endTasks);


            async UniTask FadeTask(float value)
            {
                await _fadeRing.DOFade(value, _fadeDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .SetLink(gameObject)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
            }

            async UniTask ScaleTask(float value)
            {
                await _fadeRing.transform.DOScale(value, _fadeDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .SetLink(gameObject)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
            }
        }



        // ------ ワープ時拒否
        if (_isWarping) return;

        //  ワープ
        collision.transform.position = _warpPos.position;

        _isWarping = true;

        //  効果音
        _audio.PlayOneShot(_clip);
        //  連続でワープしないように待機
        await Helper.Tasks.Canceled(Helper.Tasks.DelayTime(_duration, destroyCancellationToken));

        _isWarping = false;
    }

    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("ギズモパラメータ")] private Helper.Switch _gizmoSwitch;
    [SerializeField, Required, BoxGroup("ギズモパラメータ")] private Color _color;

#if UNITY_EDITOR
    // ---------------------------- UnityMessage
    void OnDrawGizmos()
    {
        if (_gizmoSwitch == Helper.Switch.ON)
        {
            Gizmos.color = _color;
            Gizmos.DrawWireSphere(transform.position, transform.localScale.y);
        }
    }

#endif
}
