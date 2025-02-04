using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using DG.Tweening;
using Alchemy.Inspector;
using R3;
using Cysharp.Threading.Tasks;
using System.Threading;
using R3.Triggers;
using System.Collections.Generic;
using Helper;

public class PlayerController : MonoBehaviour
{
    // ---------------------------- Enum
    public enum LineType
    {
        ROPE, INDICATOR
    }
    public enum LinePos
    {
        ORIGIN, HEAD
    }

    private enum HitCollider
    {
        Hit, UnHit, TrackingHit,
        Null
    }

    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("デバッグ")] private Switch _GUI;

    [SerializeField, Required, BoxGroup("基礎パラメータ")] private int _maxHp;
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private float _groundRayDis;
    [SerializeField, Required, BoxGroup("基礎パラメータ")] private Transform _respawnPos;

    [SerializeField, Required, BoxGroup("ライン")] private LineRenderer _ropeLine;
    [SerializeField, Required, BoxGroup("ライン")] private LineRenderer _indicatorLine;
    [SerializeField, Required, BoxGroup("ライン")] private LineRenderer _highLightLine;

    [SerializeField, Required, BoxGroup("フック")] private LayerMask[] _layerMasks;
    [SerializeField, Required, BoxGroup("フック")] private LayerMask _layerMask;
    [SerializeField, Required, BoxGroup("フック")] private GameObject _hookHead;
    [SerializeField, Required, BoxGroup("フック")] private float _wireDis;
    [SerializeField, Required, BoxGroup("フック")] private float _wireAddForce;
    [SerializeField, Required, BoxGroup("フック")] private float _trackingAddForce;
    [SerializeField, Required, BoxGroup("フック")] private float _boundForce;
    [SerializeField, Required, BoxGroup("フック")] private float _hookAnimeDuration;
    [SerializeField, Required, BoxGroup("フック")] private Color _initColor;

    [SerializeField, Required, BoxGroup("エフェクト")] private GameObject _catchEffect;
    [SerializeField, Required, BoxGroup("エフェクト")] private GameObject _boundEffect;
    [SerializeField, Required, BoxGroup("エフェクト")] private GameObject _damageEffect;
    [SerializeField, Required, BoxGroup("エフェクト")] private int _flashingLimit;
    [SerializeField, Required, BoxGroup("エフェクト")] private float _flashingTime;

    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _contactClip;
    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _damageClip;
    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _hitClip;
    [SerializeField, Required, BoxGroup("オーディオ")] private UnityEvent _unHitClip;

    // ---------------------------- Field
    private static PlayerController _instance;
    private GameObject _obj = null;
    private Transform _tr = null;
    private Rigidbody2D _rb2d = null;
    private PlayerInput _input = null;
    private SpriteRenderer _sr = null;


    //  インプットシステム
    private readonly string DEFAULT_MAP = "Player", PAUSE_MAP = "Pause";
    private Vector3 _lookPos = Vector3.zero;

    //  リスポーン
    private Vector3 _saveRespawnPos;

    //  ダメージ
    private bool _canDamageTaken = true;

    //  移動
    private Vector2 _moveForce;

    //  フック
    private bool _isHookHit = false;
    private bool _isHookAnimation = false;
    private bool _isFirstHookProcess = false;
    private InputActionPhase _shotPhase;
    private HitCollider _activeHitPos;

    private Vector3 _ropeHeadPos;

    private Vector3 _hitPoint;
    private GameObject _targetObj = null;

    private readonly Dictionary<LineType, Vector3> _headPos = new();

    // ---------------------------- ReactiveProperty
    //  スキーム
    private readonly ReactiveProperty<Scheme> _scheme = new();
    public ReadOnlyReactiveProperty<Scheme> Scheme => _scheme;

    //  HP
    private readonly ReactiveProperty<int> _hp = new();
    public ReadOnlyReactiveProperty<int> HP => _hp;

    //  フック
    private readonly ReactiveProperty<bool> _isHookActive = new();

    // ---------------------------- Property
    public static PlayerController Instance => _instance;
    public GameObject Obj => _obj;
    public Transform Tr => _tr;

    public Rigidbody2D RB2D => _rb2d;
    public InputActionPhase ShotPhase { get => _shotPhase; set => _shotPhase = value; }
    public int MaxHP => _maxHp;



    // ---------------------------- OnGUI
    private void OnGUI()
    {
        if (_GUI == Switch.ON)
        {
            var pos = Base.LogParam.pos;
            var style = Base.LogParam.style;

            GUI.TextField(pos[4], $"HitCollider: {GetHitColliderType()}", style);
            GUI.TextField(pos[3], $"HP: {HP.CurrentValue}", style);
            GUI.TextField(pos[1], $"hitPos: {_activeHitPos}", style);
        }
    }

    // ---------------------------- UnityMessage
    private void Awake()
    {
        //  キャッシュ
        _instance = this;
        _obj = gameObject;
        _tr = transform;
        _rb2d = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInput>();
        _sr = GetComponent<SpriteRenderer>();

        //  ロープ先端位置 種類名保存
        foreach (var type in Enum.GetValues(typeof(LineType)).Cast<LineType>())
        {
            _headPos.Add(type, Vector3.zero);
        }

        //  HP初期化
        _hp.Value = _maxHp;
    }

    private void Start()
    {
        _saveRespawnPos = _respawnPos.position; //  リスポーン地点保存
        EventObserve();    //  イベント監視
        ContactEventObserve();     //  接触イベント監視
    }

    private void FixedUpdate()
    {
        if (IsDefault())   //  ポーズ中更新停止
        {
            _rb2d.AddForce(_moveForce);   //  加速出力//  移動出力
        }
    }

    private void Update()
    {
        HookLineUpdate();   //  ライン更新

        if (IsDefault())   //  ポーズ中更新停止
        {
            GroundDecision();   //  接地判定

            HookShot();     //  フック処理
        }
    }

    // ---------------------------- PublicMethod
    #region ------ Input
    /// <summary>
    /// スティック入力
    /// </summary>
    /// <param name="ctx">コールバックコンテキスト</param>
    public void OnLook(InputAction.CallbackContext ctx)
    {
        //  ロープ方向入力
        _lookPos = _tr.position + (Vector3)ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// マウス位置入力
    /// </summary>
    /// <param name="ctx">コールバックコンテキスト</param>
    public void OnAim(InputAction.CallbackContext ctx)
    {
        //  カメラ取得
        if (Camera.main != null)
        {
            //  マウス位置更新
            var camera = Camera.main.ScreenToWorldPoint((Vector3)ctx.ReadValue<Vector2>());
            camera.z = 0;
            _lookPos = camera;
        }
    }

    /// <summary>
    /// 射出入力
    /// </summary>
    /// <param name="ctx">コールバックコンテキスト</param>
    public void OnHookShot(InputAction.CallbackContext ctx)
    {
        _shotPhase = ctx.phase;
    }

    /// <summary>
    /// ポーズ
    /// </summary>
    /// <param name="ctx">コールバックコンテキスト</param>
    public void OnPause(InputAction.CallbackContext ctx)
    {
        GameManager.Instance.ChangeState(GameState.PAUSE);
    }

    /// <summary>
    /// バック
    /// </summary>
    /// <param name="ctx">コールバックコンテキスト</param>
    public void OnBack(InputAction.CallbackContext ctx)
    {
        GameManager.Instance.ChangeState(GameState.DEFAULT);
    }

    /// <summary>
    /// 入力方法変更
    /// </summary>
    public void OnControlsChanged()
    {
        //  Scheme文字列をEnumで判定
        if (_input == null) return; //  PlayerInputのキャッシュをしてから行う
        var schemeString = _input.currentControlScheme;

        //  入力方法分岐
        if (schemeString == Helper.Scheme.KeyboardMouse.ToString())
        {
            _scheme.Value = Helper.Scheme.KeyboardMouse;
        }
        else if (schemeString == Helper.Scheme.Gamepad.ToString())
        {
            _scheme.Value = Helper.Scheme.Gamepad;
        }
    }

    #endregion



    // ---------------------------- PrivateMethod
    #region ------ Observer
    /// <summary>
    /// イベント監視
    /// </summary>
    private void EventObserve()
    {
        //  ステートイベント
        GameManager.Instance.State.Subscribe(state =>
        {
            //  ステート分岐
            switch (state)
            {
                case GameState.DEFAULT:
                    Implement(1, DEFAULT_MAP);

                    break;

                case GameState.PAUSE:
                case GameState.GAMECLEAR:
                case GameState.GAMEOVER:
                    Implement(0, PAUSE_MAP);

                    break;
            }

            //  ステータス設定
            void Implement(int time, string actionMap)
            {
                Time.timeScale = time;  //  タイムスケール
                _input.SwitchCurrentActionMap(actionMap); //  アクションマップ
            }

        })
        .AddTo(this);

        //  フックイベント
        _isHookActive.SubscribeAwait(async (active, ct) =>
        {
            _isHookAnimation = true;
            try
            {
                //  フックアクティブ判定
                if (active)
                {
                    await ActiveTask(ct);
                }
                else
                {
                    await UnActiveTask(ct);
                }
            }
            finally
            {
                _isHookAnimation = false;
            }
            //  色初期化
            _sr.color = _initColor;

        }, AwaitOperation.Switch)
        .RegisterTo(destroyCancellationToken);

        async UniTask ActiveTask(CancellationToken ct)
        {
            //  アニメーション
            await DOVirtual.Float(0, 1, _hookAnimeDuration
                , (head) =>
                {
                    _headPos[LineType.ROPE] = Vector3.Lerp(_tr.position, _ropeHeadPos, head);
                })
                .SetLink(_obj)
                .SetEase(Ease.Linear)
                .ToUniTask(Tasks.TCB, cancellationToken: ct);


            //  接触エフェクト
            Instantiate(_catchEffect, _headPos[LineType.ROPE], Quaternion.identity);

            //  UnHitのとき、フックを戻す
            if ((_isFirstHookProcess && _activeHitPos == HitCollider.UnHit))
            {
                //  効果音再生
                _unHitClip?.Invoke();
                //  点滅処理
                await DOVirtual.Color(Color.black, _initColor
                , _flashingTime
                , (value) =>
                {
                    GetComponent<SpriteRenderer>().color = value;
                })
                .SetEase(Ease.Linear)
                .SetLoops(1, LoopType.Yoyo)
                .SetLink(_obj)
                .ToUniTask(Tasks.TCB, cancellationToken: ct);

                //  状態を戻す
                _isFirstHookProcess = false;
                _isHookActive.Value = false;
            }
            else
            {
                //  効果音再生
                _hitClip?.Invoke();
            }
        }

        async UniTask UnActiveTask(CancellationToken ct)
        {
            //  アニメーション
            await DOVirtual.Float(1, 0, _hookAnimeDuration
                , (head) =>
                {
                    _headPos[LineType.ROPE] = Vector3.Lerp(_tr.position, _ropeHeadPos, head);
                })
                .SetLink(_obj)
                .SetEase(Ease.Linear)
                .ToUniTask(Tasks.TCB, cancellationToken: ct);
        }
    }

    /// <summary>
    /// 接触イベント
    /// </summary>
    private void ContactEventObserve()
    {
        //  コリジョンエンター
        this.OnCollisionEnter2DAsObservable().Subscribe((collision) =>
        {
            var obj = collision.gameObject;
            var contactPoint = collision.contacts[0].point;

            //  地面
            if (obj.CompareTag(TagName.Ground))
            {
                _contactClip?.Invoke();

                //  フックショット時バウンド制御
                if (_isHookHit)
                {
                    var dir = ((Vector2)_tr.position - contactPoint).normalized;
                    _rb2d.AddForce(dir * _boundForce);
                }
                Instantiate(_boundEffect, contactPoint, Quaternion.identity);
            }
            //  ベルト
            else if (obj.CompareTag(TagName.Belt))
            {
                _contactClip?.Invoke();
                Instantiate(_boundEffect, contactPoint, Quaternion.identity);
            }
        })
        .AddTo(this);
        //  ダメージ処理
        this.OnCollisionEnter2DAsObservable().Where(_ => _canDamageTaken)
            .SubscribeAwait(async (collision, ct) =>
        {
            var obj = collision.gameObject;

            //  敵
            if (obj.CompareTag(TagName.Enemy))
            {
                var damage = obj.GetComponent<IEnemyDamageable>().Damage();
                if (damage != 0)
                {
                    await DamageTaken(damage, ct);
                }
            }
            //  リスポーン
            else if (obj.CompareTag(TagName.Respawn))
            {
                _tr.position = _saveRespawnPos;
                await DamageTaken(1, ct);
            }

        }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);

        //  トリガーエンター
        this.OnTriggerEnter2DAsObservable().Subscribe((collision) =>
        {
            var obj = collision.gameObject;
            var Game = GameManager.Instance;

            //  ゴール
            if (collision.CompareTag(TagName.Goal))
            {
                Game.ChangeState(GameState.GAMECLEAR);
            }
            //  リスポーン位置
            else if (obj.CompareTag(TagName.RespawnPoint))
            {
                _saveRespawnPos = obj.GetComponent<RespawnPoint>().Pos;
            }
            //  アイテム
            else if (obj.CompareTag(TagName.Item))
            {
                var item = obj.GetComponent<ItemController>();
                Game.SetScore(item.Point);
                item.Destroy();
            }
            //  敵
            else if (obj.CompareTag(TagName.Enemy))
            {
                obj.GetComponent<IEnemyDamageable>().Die();
            }
            //  ポータル
            else if (obj.CompareTag(TagName.Portal))
            {
                _shotPhase = InputActionPhase.Canceled;
            }
        })
        .AddTo(this);
        //  ダメージ処理
        this.OnTriggerEnter2DAsObservable().Where(_ => _canDamageTaken)
            .SubscribeAwait(async (collision, ct) =>
        {
            var obj = collision.gameObject;
            //  敵
            if (obj.CompareTag(TagName.Enemy))
            {
                await DamageTaken(obj.GetComponent<IEnemyDamageable>().Damage(), ct);
            }

        }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);

    }

    #endregion

    /// <summary>
    /// デフォルト判定
    /// </summary>
    /// <returns>デフォルト状態</returns>
    private bool IsDefault() => GameManager.Instance.State.CurrentValue == GameState.DEFAULT;


    /// <summary>
    /// 接地判定
    /// </summary>
    private void GroundDecision()
    {
        //  レイ生成
        var ray = new Ray2D(_tr.position, new Vector2(0, -1));
        var hit = Physics2D.Raycast
            (ray.origin
            , ray.direction
            , _groundRayDis
            , _layerMasks[(int)HitCollider.Hit]);

        Debug.DrawRay(ray.origin, ray.direction * _groundRayDis, Color.green);

        //  コンベア加速追加
        if (hit.collider)
        {
            var obj = hit.collider.gameObject;
            if (obj.CompareTag(TagName.Belt))
            {
                var add = obj.GetComponent<ConveyorController>().GetSpeed();
                _rb2d.AddForce(new Vector2(add, 0));
            }
        }
    }



    /// <summary>
    /// 被ダメージ
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>被ダメージ処理</returns>
    private async UniTask DamageTaken(int damage, CancellationToken ct)
    {
        //  ダメージ受付可否
        if (_canDamageTaken)
        {
            _canDamageTaken = false;
            try
            {
                //  ダメージ処理
                _hp.Value -= damage;    //  ダメージ
                _damageClip?.Invoke();  //  効果音
                Instantiate(_damageEffect, _tr.position, Quaternion.identity) //  エフェクト
                    .transform.SetParent(transform);    //  子オブジェクトに設定

                //  点滅処理
                await DOVirtual.Color(Color.black, _sr.color
                , _flashingTime
                , (value) =>
                {
                    _sr.color = value;
                })
                .SetEase(Ease.Linear)
                .SetLoops(_flashingLimit, LoopType.Yoyo)
                .SetLink(_obj)
                .ToUniTask(Tasks.TCB, cancellationToken: ct);
            }
            finally
            {
                _canDamageTaken = true;
            }
        }
        else
        {
            _contactClip?.Invoke(); //  効果音
        }
    }

    #region ------ HookShot
    /// <summary>
    /// 射出処理
    /// </summary>
    private void HookShot()
    {
        //  射出入力
        if (_shotPhase == InputActionPhase.Performed)
        {
            HookActive();
        }
        else
        {
            _isFirstHookProcess = true; //  入力がなくなったら初期化
            HookInactive();
        }

        _hookHead.transform.position = _headPos[LineType.ROPE];    //  接触位置移動
    }

    /// <summary>
    /// 起動時処理
    /// </summary>
    private void HookActive()
    {
        //  パラメータ取得
        var playerPos = _tr.position; //  プレイヤー位置
        var type = GetHitColliderType();    //  接触コライダーの種類

        //  接触判定
        if (!_isHookHit)
        {
            _activeHitPos = type;   //  接触先データ保存
            switch (type)
            {
                case HitCollider.Hit:
                case HitCollider.TrackingHit:
                case HitCollider.UnHit:
                    _isHookHit = true;
                    break;

                case HitCollider.Null:
                    _isHookHit = false;
                    break;
            }
        }

        //  パラメータ更新
        switch (_activeHitPos)
        {
            case HitCollider.Hit:
                Implement(_hitPoint, playerPos, AddForce(_hitPoint), true);
                break;

            case HitCollider.UnHit:
                Implement(_hitPoint, _hitPoint, Vector2.zero, true);
                break;

            case HitCollider.TrackingHit:
                //  トラッキングオブジェクトのNullチェック
                if (_targetObj != null)
                {
                    var targetPos = _targetObj.transform.position;
                    Implement(targetPos, playerPos, AddForce(targetPos) * _trackingAddForce, true);
                }
                else
                {
                    Implement(playerPos, playerPos, Vector2.zero, true);
                }
                break;

            case HitCollider.Null:
                var indicator = playerPos - (playerPos - _lookPos).normalized * _wireDis;
                Implement(playerPos, indicator, Vector2.zero, false);
                break;
        }

        //  加速力
        Vector2 AddForce(Vector2 point)
        {
            return (point - (Vector2)playerPos).normalized * _wireAddForce;
        }

        //  パラメータ
        void Implement
            (Vector3 rope
            , Vector3 indicator
            , Vector2 force
            , bool hookActive)
        {
            //  位置
            _ropeHeadPos = rope;
            _headPos[LineType.INDICATOR] = indicator;

            //  フック伸縮可否
            if (_isFirstHookProcess)
            {
                _isHookActive.Value = hookActive;
            }

            //  入力
            _moveForce = force;

        }

        //  ロープ位置更新
        if (!_isHookAnimation)  //  フックアニメーション終了後
        {
            switch (_activeHitPos)
            {
                case HitCollider.TrackingHit:
                    _headPos[LineType.ROPE] = _targetObj.transform.position;
                    break;

                case HitCollider.UnHit:
                case HitCollider.Null:
                    _headPos[LineType.ROPE] = playerPos;
                    break;
            }
        }
    }

    /// <summary>
    /// 非起動時処理
    /// </summary>
    /// <param name="type">接触している</param>
    private void HookInactive()
    {
        var playerPos = _tr.position;
        var type = GetHitColliderType();

        //  先端位置代入
        var up = 1;
        var down = -1;
        switch (type)
        {
            case HitCollider.Hit:
                Implement(up, _hitPoint);
                break;

            case HitCollider.UnHit:
                Implement(down, _hitPoint);
                break;

            case HitCollider.TrackingHit:
                Implement(up, _targetObj.transform.position);
                break;

            case HitCollider.Null:
                Implement(down, playerPos - (playerPos - _lookPos).normalized * _wireDis);
                break;
        }

        //  パラメータ
        void Implement(int lineSort, Vector3 indicatorPos)
        {
            //  接触判定
            _isHookHit = false;
            //  インジケータラインの表示順を取得
            var indicatorLine = _indicatorLine.sortingOrder;
            //  接触物による表示順の変更
            _highLightLine.sortingOrder = indicatorLine + lineSort;
            //  位置
            _headPos[LineType.ROPE] = playerPos;
            _headPos[LineType.INDICATOR] = indicatorPos;
            //  加速力
            _moveForce = Vector2.zero;

            _isHookActive.Value = false;
        }

        if (!_isHookAnimation)  //  フックアニメーション終了後
        {
            _headPos[LineType.ROPE] = playerPos;
        }
    }

    /// <summary>
    /// 接触レイ指定
    /// </summary>
    /// <returns>接触したレイに対応した列挙型変数(RayType)</returns>
    private HitCollider GetHitColliderType()
    {
        //  レイ描写
        var playerPos = _tr.position;
        var rayDir = (_lookPos - playerPos).normalized;
        Debug.DrawRay(playerPos, rayDir * _wireDis, Color.green);

        //  接触判定
        var hit = Physics2D.RaycastAll(playerPos, rayDir, _wireDis, _layerMask);
        if (hit.Count() > 0)
        {
            //  layerに合わせたEnumの返り値
            var first = hit.First();
            var layer = first.transform.gameObject.layer;
            var hitCollider = (HitCollider)Enum.Parse(typeof(HitCollider), LayerMask.LayerToName(layer));

            //  接触時パラメータ
            if (!_isHookHit)
            {
                _hitPoint = first.point;
                _targetObj = first.transform.gameObject;
            }

            return hitCollider;
        }
        else
        {
            return HitCollider.Null;
        }
    }

    /// <summary>
    /// ライン更新
    /// </summary>
    private void HookLineUpdate()
    {
        //  両端更新
        LineUpdate(_ropeLine, LineType.ROPE);
        LineUpdate(_indicatorLine, LineType.INDICATOR);
        LineUpdate(_highLightLine, LineType.INDICATOR);

        void LineUpdate(LineRenderer line, LineType type)
        {
            line.gameObject.SetActive(IsDefault()); //  通常時アクティブ

            if (line.gameObject != null)
            {
                //  表示可否
                var decision = IsDefault() && !Tasks.IsFade;
                var headPos = decision ? _headPos[type] : _tr.position;
                //  アニメーション終了判定
                var indicatorHeadPos = !_isHookAnimation ? headPos : _tr.position;
                //  ロープタイプ判定
                var typeHeadPos = type == LineType.ROPE ? headPos : indicatorHeadPos;
                //  位置更新
                line.SetPositions(new Vector3[] { _tr.position, typeHeadPos });
            }
        }
    }

    #endregion
}