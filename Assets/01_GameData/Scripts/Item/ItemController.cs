using UnityEngine;
using DG.Tweening;
using Alchemy.Inspector;

public class ItemController : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("スコア")] private int _point;

    [SerializeField, Required, BoxGroup("デフォルトエフェクト")] private Transform _circle;
    [SerializeField, Required, BoxGroup("デフォルトエフェクト")] private Vector3 _afterPos;
    [SerializeField, Required, BoxGroup("デフォルトエフェクト")] private Vector3 _afterTransform;
    [SerializeField, Required, BoxGroup("デフォルトエフェクト")] private float _duration;
    [SerializeField, Required, BoxGroup("デフォルトエフェクト")] private float _turnDuration;

    [SerializeField, Required, BoxGroup("デストロイエフェクト")] private GameObject _destroyEffect;
    [SerializeField, Required, BoxGroup("デストロイエフェクト")] private GameObject _audioPlayer;


    // ---------------------------- SerializeField
    private GameObject _obj = null;
    private Transform _tr = null;

    // ---------------------------- Property
    public int Point => _point;


    // ---------------------------- UnityMessage

    private void Start()
    {
        _obj = gameObject;
        _tr = transform;

        //  アニメーション開始
        Animation();
    }


    // ---------------------------- PublicMethod
    public void Destroy()
    {
        //  エフェクト
        Instantiate(_destroyEffect, _tr.position, Quaternion.identity);
        Instantiate(_audioPlayer, _tr.position, Quaternion.identity);

        //  削除
        Destroy(_obj);
    }




    // ---------------------------- PrivateMethod
    /// <summary>
    /// アニメーション
    /// </summary>
    private void Animation()
    {
        //  上下に移動
        transform.DOMove(transform.position + _afterPos, _duration)
            .SetEase(Ease.OutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(_obj);

        //  大きさを変更
        transform.DOScale(_afterTransform, _duration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(_obj);

        //  Y軸方向に回転
        _circle.DORotate(new Vector3(0, 360, 0), _turnDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental)
            .SetLink(_circle.gameObject);
    }
}
