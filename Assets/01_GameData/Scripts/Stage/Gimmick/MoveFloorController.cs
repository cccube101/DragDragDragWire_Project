using UnityEngine;
using DG.Tweening;
using Alchemy.Inspector;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

public class MoveFloorController : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("移動パラメータ")] private Transform[] _pos;
    [SerializeField, Required, BoxGroup("移動パラメータ")] private float _time;
    [SerializeField, Required, BoxGroup("移動パラメータ")] private float _waitTime;
    [SerializeField, Required, BoxGroup("移動パラメータ")] private LoopType _loopType;
    [SerializeField, Required, BoxGroup("移動パラメータ")] private PathType _pathType;
    [SerializeField, Required, BoxGroup("移動パラメータ")] private bool _setOption;







    // ---------------------------- UnityMessage
    private void Start()
    {
        Tasks(destroyCancellationToken).Forget();
    }

    private async UniTask Tasks(CancellationToken ct)
    {
        //  移動経由地初期化
        Vector3[] positions = new Vector3[_pos.Length];
        for (int i = 0; i < _pos.Length; i++)
        {
            positions[i] = _pos[i].position;
        }

        //  待機
        await Helper.Tasks.DelayTime(_waitTime, ct);

        //  移動処理
        await transform.DOPath
                (positions
                , _time
                , _pathType
                , PathMode.Sidescroller2D)
                .SetEase(Ease.Linear)
                .SetLoops(-1, _loopType)
                .SetOptions(_setOption)
                .SetLink(gameObject)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
    }
}
