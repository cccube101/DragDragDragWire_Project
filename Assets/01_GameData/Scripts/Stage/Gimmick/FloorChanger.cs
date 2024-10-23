using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class FloorChanger : MonoBehaviour
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
    private Dictionary<GameObject, Tilemap> _floors = new();


    // ---------------------------- UnityMessage
    private async void Start()
    {
        foreach (var obj in _floorsObjects)
        {
            _floors.Add(obj, obj.GetComponent<Tilemap>());
            obj.SetActive(false);
        }
        _floorsObjects[0].SetActive(true);

        await Helper.Tasks.Canceled(StartEvent(destroyCancellationToken));
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// ŠJŽn
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async UniTask StartEvent(CancellationToken ct)
    {
        while (true)
        {
            foreach (var floor in _floors)
            {
                floor.Key.SetActive(true);

                await DOVirtual.Color(_endColor, _toColor, _duration, (color) =>
                {
                    floor.Value.color = color;
                })
                .SetEase(Ease.Linear)
                .SetLink(floor.Key)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: ct);


                await Helper.Tasks.DelayTime(_waitTime, ct);

                var tasks = new List<UniTask>()
                {
                    Fade(),
                    PlayClip(),
                };
                async UniTask Fade()
                {
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
                    for (var i = 0; i < _loopTime / 2 + 1; i++)
                    {
                        _alertClip?.Invoke();
                        await Helper.Tasks.DelayTime(_duration * 2, ct);
                    }
                }
                await UniTask.WhenAll(tasks);

                PlayerController.Instance.ShotPhase = UnityEngine.InputSystem.InputActionPhase.Canceled;
                floor.Key.SetActive(false);
            }

            await UniTask.Yield(cancellationToken: ct);
        }
    }
}
