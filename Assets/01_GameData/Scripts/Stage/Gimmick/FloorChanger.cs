using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class FloorChanger : GimmickBase
{
    // ---------------------------- SerializeField
    [SerializeField] private GameObject[] _floorsObjects;
    [SerializeField] private float _waitTime;
    [SerializeField] private float _duration;
    [SerializeField] private int _loopTime;
    [SerializeField] private Color _toColor;
    [SerializeField] private Color _endColor;
    [SerializeField] private UnityEvent _alertClip;

    // ---------------------------- Field
    private readonly Dictionary<GameObject, Tilemap> _floors = new();


    // ---------------------------- UnityMessage
    private async void Start()
    {
        //  オブジェクトに紐づいたタイルマップのキャッシュ
        foreach (var obj in _floorsObjects)
        {
            _floors.Add(obj, obj.GetComponent<Tilemap>());
            obj.SetActive(false);
        }
        //  リストの初めの部分のみアクティブ化
        _floorsObjects[0].SetActive(true);

        //  フロア切換え開始
        await Helper.Tasks.Canceled(StartEvent(destroyCancellationToken));
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// 開始
    /// </summary>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>開始処理</returns>
    private async UniTask StartEvent(CancellationToken ct)
    {
        while (true)
        {
            //  順次フロアの切換え
            foreach (var floor in _floors)
            {
                floor.Key.SetActive(true);

                //  見えるようにスプライトの色を戻す
                await DOVirtual.Color(_endColor, _toColor, _duration, (color) =>
                {
                    floor.Value.color = color;
                })
                .SetEase(Ease.Linear)
                .SetLink(floor.Key)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);

                //  切換えまで待機
                await Helper.Tasks.DelayTime(_waitTime, ct);

                //  切換え処理
                var tasks = new List<UniTask>()
                {
                    Fade(),
                    PlayClip(),
                };
                async UniTask Fade()
                {
                    //  指定回数アラートに合わせ色をフェード
                    await DOVirtual.Color(_toColor, _endColor, _duration, (color) =>
                        {
                            floor.Value.color = color;
                        })
                        .SetEase(Ease.Linear)
                        .SetLoops(_loopTime, LoopType.Yoyo)
                        .SetLink(floor.Key)
                        .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);
                }
                async UniTask PlayClip()
                {
                    //  指定回数アラートを再生
                    for (var i = 0; i < _loopTime / 2 + 1; i++)
                    {
                        _alertClip?.Invoke();
                        await Helper.Tasks.DelayTime(_duration * 2, ct);
                    }
                }
                await UniTask.WhenAll(tasks);

                //  消失時プレイヤーのフック判定をキャンセル
                PlayerController.Instance.ShotPhase = UnityEngine.InputSystem.InputActionPhase.Canceled;
                //  フロアを非アクティブ化
                floor.Key.SetActive(false);
            }

            await UniTask.Yield(cancellationToken: ct);
        }
    }
}
