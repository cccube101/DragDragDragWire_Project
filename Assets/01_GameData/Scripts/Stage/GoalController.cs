using Alchemy.Inspector;
using DG.Tweening;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField, Required, BoxGroup("エフェクト")] private float _scale;
    [SerializeField, Required, BoxGroup("エフェクト")] private float _duration;

    private void Start()
    {
        transform.DOScale
            (_scale, _duration)
            .SetEase(Ease.OutBack)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }
}
