using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using R3;
using R3.Triggers;
using Alchemy.Inspector;

public class UIAnimatorBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("基礎パラメータ")] protected UnityEvent[] _event;


    // ---------------------------- Field
    protected Dictionary<string, UnityEvent> _actions;
    protected Animator _animator = null;


    // ---------------------------- UnityMessage
    public virtual void Awake()
    {
        StartEvent();
    }

    public virtual void OnEnable()
    {
        AnimatorStateObserve();
    }

    // ---------------------------- PublicMethod
    /// <summary>
    /// 開始イベント
    /// </summary>
    public void StartEvent()
    {
        //  キャッシュ
        _animator = GetComponent<Animator>();

        //  レイヤー名取得
        var layer = _animator.GetLayerName(0);
        var clips = _animator.runtimeAnimatorController.animationClips;

        //  メソッド格納
        _actions = new Dictionary<string, UnityEvent>(clips.Length);
        for (int i = 0; i < clips.Length; i++)
        {
            //  "レイヤー.ステート名"
            _actions.Add($"{layer}.{clips[i].name}", _event[i]);
        }
    }

    /// <summary>
    /// アニメーターステートの監視
    /// </summary>
    public void AnimatorStateObserve()
    {
        //  アニメーターステート監視
        if (_animator == null) return;
        _animator.GetBehaviour<ObservableStateMachineTrigger>()
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


    // ---------------------------- PrivateMethod
}
