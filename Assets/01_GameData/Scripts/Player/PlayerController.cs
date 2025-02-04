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
    [SerializeField, Required, BoxGroup("�f�o�b�O")] private Switch _GUI;

    [SerializeField, Required, BoxGroup("��b�p�����[�^")] private int _maxHp;
    [SerializeField, Required, BoxGroup("��b�p�����[�^")] private float _groundRayDis;
    [SerializeField, Required, BoxGroup("��b�p�����[�^")] private Transform _respawnPos;

    [SerializeField, Required, BoxGroup("���C��")] private LineRenderer _ropeLine;
    [SerializeField, Required, BoxGroup("���C��")] private LineRenderer _indicatorLine;
    [SerializeField, Required, BoxGroup("���C��")] private LineRenderer _highLightLine;

    [SerializeField, Required, BoxGroup("�t�b�N")] private LayerMask[] _layerMasks;
    [SerializeField, Required, BoxGroup("�t�b�N")] private LayerMask _layerMask;
    [SerializeField, Required, BoxGroup("�t�b�N")] private GameObject _hookHead;
    [SerializeField, Required, BoxGroup("�t�b�N")] private float _wireDis;
    [SerializeField, Required, BoxGroup("�t�b�N")] private float _wireAddForce;
    [SerializeField, Required, BoxGroup("�t�b�N")] private float _trackingAddForce;
    [SerializeField, Required, BoxGroup("�t�b�N")] private float _boundForce;
    [SerializeField, Required, BoxGroup("�t�b�N")] private float _hookAnimeDuration;
    [SerializeField, Required, BoxGroup("�t�b�N")] private Color _initColor;

    [SerializeField, Required, BoxGroup("�G�t�F�N�g")] private GameObject _catchEffect;
    [SerializeField, Required, BoxGroup("�G�t�F�N�g")] private GameObject _boundEffect;
    [SerializeField, Required, BoxGroup("�G�t�F�N�g")] private GameObject _damageEffect;
    [SerializeField, Required, BoxGroup("�G�t�F�N�g")] private int _flashingLimit;
    [SerializeField, Required, BoxGroup("�G�t�F�N�g")] private float _flashingTime;

    [SerializeField, Required, BoxGroup("�I�[�f�B�I")] private UnityEvent _contactClip;
    [SerializeField, Required, BoxGroup("�I�[�f�B�I")] private UnityEvent _damageClip;
    [SerializeField, Required, BoxGroup("�I�[�f�B�I")] private UnityEvent _hitClip;
    [SerializeField, Required, BoxGroup("�I�[�f�B�I")] private UnityEvent _unHitClip;

    // ---------------------------- Field
    private static PlayerController _instance;
    private GameObject _obj = null;
    private Transform _tr = null;
    private Rigidbody2D _rb2d = null;
    private PlayerInput _input = null;
    private SpriteRenderer _sr = null;


    //  �C���v�b�g�V�X�e��
    private readonly string DEFAULT_MAP = "Player", PAUSE_MAP = "Pause";
    private Vector3 _lookPos = Vector3.zero;

    //  ���X�|�[��
    private Vector3 _saveRespawnPos;

    //  �_���[�W
    private bool _canDamageTaken = true;

    //  �ړ�
    private Vector2 _moveForce;

    //  �t�b�N
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
    //  �X�L�[��
    private readonly ReactiveProperty<Scheme> _scheme = new();
    public ReadOnlyReactiveProperty<Scheme> Scheme => _scheme;

    //  HP
    private readonly ReactiveProperty<int> _hp = new();
    public ReadOnlyReactiveProperty<int> HP => _hp;

    //  �t�b�N
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
        //  �L���b�V��
        _instance = this;
        _obj = gameObject;
        _tr = transform;
        _rb2d = GetComponent<Rigidbody2D>();
        _input = GetComponent<PlayerInput>();
        _sr = GetComponent<SpriteRenderer>();

        //  ���[�v��[�ʒu ��ޖ��ۑ�
        foreach (var type in Enum.GetValues(typeof(LineType)).Cast<LineType>())
        {
            _headPos.Add(type, Vector3.zero);
        }

        //  HP������
        _hp.Value = _maxHp;
    }

    private void Start()
    {
        _saveRespawnPos = _respawnPos.position; //  ���X�|�[���n�_�ۑ�
        EventObserve();    //  �C�x���g�Ď�
        ContactEventObserve();     //  �ڐG�C�x���g�Ď�
    }

    private void FixedUpdate()
    {
        if (IsDefault())   //  �|�[�Y���X�V��~
        {
            _rb2d.AddForce(_moveForce);   //  �����o��//  �ړ��o��
        }
    }

    private void Update()
    {
        HookLineUpdate();   //  ���C���X�V

        if (IsDefault())   //  �|�[�Y���X�V��~
        {
            GroundDecision();   //  �ڒn����

            HookShot();     //  �t�b�N����
        }
    }

    // ---------------------------- PublicMethod
    #region ------ Input
    /// <summary>
    /// �X�e�B�b�N����
    /// </summary>
    /// <param name="ctx">�R�[���o�b�N�R���e�L�X�g</param>
    public void OnLook(InputAction.CallbackContext ctx)
    {
        //  ���[�v��������
        _lookPos = _tr.position + (Vector3)ctx.ReadValue<Vector2>();
    }

    /// <summary>
    /// �}�E�X�ʒu����
    /// </summary>
    /// <param name="ctx">�R�[���o�b�N�R���e�L�X�g</param>
    public void OnAim(InputAction.CallbackContext ctx)
    {
        //  �J�����擾
        if (Camera.main != null)
        {
            //  �}�E�X�ʒu�X�V
            var camera = Camera.main.ScreenToWorldPoint((Vector3)ctx.ReadValue<Vector2>());
            camera.z = 0;
            _lookPos = camera;
        }
    }

    /// <summary>
    /// �ˏo����
    /// </summary>
    /// <param name="ctx">�R�[���o�b�N�R���e�L�X�g</param>
    public void OnHookShot(InputAction.CallbackContext ctx)
    {
        _shotPhase = ctx.phase;
    }

    /// <summary>
    /// �|�[�Y
    /// </summary>
    /// <param name="ctx">�R�[���o�b�N�R���e�L�X�g</param>
    public void OnPause(InputAction.CallbackContext ctx)
    {
        GameManager.Instance.ChangeState(GameState.PAUSE);
    }

    /// <summary>
    /// �o�b�N
    /// </summary>
    /// <param name="ctx">�R�[���o�b�N�R���e�L�X�g</param>
    public void OnBack(InputAction.CallbackContext ctx)
    {
        GameManager.Instance.ChangeState(GameState.DEFAULT);
    }

    /// <summary>
    /// ���͕��@�ύX
    /// </summary>
    public void OnControlsChanged()
    {
        //  Scheme�������Enum�Ŕ���
        if (_input == null) return; //  PlayerInput�̃L���b�V�������Ă���s��
        var schemeString = _input.currentControlScheme;

        //  ���͕��@����
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
    /// �C�x���g�Ď�
    /// </summary>
    private void EventObserve()
    {
        //  �X�e�[�g�C�x���g
        GameManager.Instance.State.Subscribe(state =>
        {
            //  �X�e�[�g����
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

            //  �X�e�[�^�X�ݒ�
            void Implement(int time, string actionMap)
            {
                Time.timeScale = time;  //  �^�C���X�P�[��
                _input.SwitchCurrentActionMap(actionMap); //  �A�N�V�����}�b�v
            }

        })
        .AddTo(this);

        //  �t�b�N�C�x���g
        _isHookActive.SubscribeAwait(async (active, ct) =>
        {
            _isHookAnimation = true;
            try
            {
                //  �t�b�N�A�N�e�B�u����
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
            //  �F������
            _sr.color = _initColor;

        }, AwaitOperation.Switch)
        .RegisterTo(destroyCancellationToken);

        async UniTask ActiveTask(CancellationToken ct)
        {
            //  �A�j���[�V����
            await DOVirtual.Float(0, 1, _hookAnimeDuration
                , (head) =>
                {
                    _headPos[LineType.ROPE] = Vector3.Lerp(_tr.position, _ropeHeadPos, head);
                })
                .SetLink(_obj)
                .SetEase(Ease.Linear)
                .ToUniTask(Tasks.TCB, cancellationToken: ct);


            //  �ڐG�G�t�F�N�g
            Instantiate(_catchEffect, _headPos[LineType.ROPE], Quaternion.identity);

            //  UnHit�̂Ƃ��A�t�b�N��߂�
            if ((_isFirstHookProcess && _activeHitPos == HitCollider.UnHit))
            {
                //  ���ʉ��Đ�
                _unHitClip?.Invoke();
                //  �_�ŏ���
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

                //  ��Ԃ�߂�
                _isFirstHookProcess = false;
                _isHookActive.Value = false;
            }
            else
            {
                //  ���ʉ��Đ�
                _hitClip?.Invoke();
            }
        }

        async UniTask UnActiveTask(CancellationToken ct)
        {
            //  �A�j���[�V����
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
    /// �ڐG�C�x���g
    /// </summary>
    private void ContactEventObserve()
    {
        //  �R���W�����G���^�[
        this.OnCollisionEnter2DAsObservable().Subscribe((collision) =>
        {
            var obj = collision.gameObject;
            var contactPoint = collision.contacts[0].point;

            //  �n��
            if (obj.CompareTag(TagName.Ground))
            {
                _contactClip?.Invoke();

                //  �t�b�N�V���b�g���o�E���h����
                if (_isHookHit)
                {
                    var dir = ((Vector2)_tr.position - contactPoint).normalized;
                    _rb2d.AddForce(dir * _boundForce);
                }
                Instantiate(_boundEffect, contactPoint, Quaternion.identity);
            }
            //  �x���g
            else if (obj.CompareTag(TagName.Belt))
            {
                _contactClip?.Invoke();
                Instantiate(_boundEffect, contactPoint, Quaternion.identity);
            }
        })
        .AddTo(this);
        //  �_���[�W����
        this.OnCollisionEnter2DAsObservable().Where(_ => _canDamageTaken)
            .SubscribeAwait(async (collision, ct) =>
        {
            var obj = collision.gameObject;

            //  �G
            if (obj.CompareTag(TagName.Enemy))
            {
                var damage = obj.GetComponent<IEnemyDamageable>().Damage();
                if (damage != 0)
                {
                    await DamageTaken(damage, ct);
                }
            }
            //  ���X�|�[��
            else if (obj.CompareTag(TagName.Respawn))
            {
                _tr.position = _saveRespawnPos;
                await DamageTaken(1, ct);
            }

        }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);

        //  �g���K�[�G���^�[
        this.OnTriggerEnter2DAsObservable().Subscribe((collision) =>
        {
            var obj = collision.gameObject;
            var Game = GameManager.Instance;

            //  �S�[��
            if (collision.CompareTag(TagName.Goal))
            {
                Game.ChangeState(GameState.GAMECLEAR);
            }
            //  ���X�|�[���ʒu
            else if (obj.CompareTag(TagName.RespawnPoint))
            {
                _saveRespawnPos = obj.GetComponent<RespawnPoint>().Pos;
            }
            //  �A�C�e��
            else if (obj.CompareTag(TagName.Item))
            {
                var item = obj.GetComponent<ItemController>();
                Game.SetScore(item.Point);
                item.Destroy();
            }
            //  �G
            else if (obj.CompareTag(TagName.Enemy))
            {
                obj.GetComponent<IEnemyDamageable>().Die();
            }
            //  �|�[�^��
            else if (obj.CompareTag(TagName.Portal))
            {
                _shotPhase = InputActionPhase.Canceled;
            }
        })
        .AddTo(this);
        //  �_���[�W����
        this.OnTriggerEnter2DAsObservable().Where(_ => _canDamageTaken)
            .SubscribeAwait(async (collision, ct) =>
        {
            var obj = collision.gameObject;
            //  �G
            if (obj.CompareTag(TagName.Enemy))
            {
                await DamageTaken(obj.GetComponent<IEnemyDamageable>().Damage(), ct);
            }

        }, AwaitOperation.Drop)
        .RegisterTo(destroyCancellationToken);

    }

    #endregion

    /// <summary>
    /// �f�t�H���g����
    /// </summary>
    /// <returns>�f�t�H���g���</returns>
    private bool IsDefault() => GameManager.Instance.State.CurrentValue == GameState.DEFAULT;


    /// <summary>
    /// �ڒn����
    /// </summary>
    private void GroundDecision()
    {
        //  ���C����
        var ray = new Ray2D(_tr.position, new Vector2(0, -1));
        var hit = Physics2D.Raycast
            (ray.origin
            , ray.direction
            , _groundRayDis
            , _layerMasks[(int)HitCollider.Hit]);

        Debug.DrawRay(ray.origin, ray.direction * _groundRayDis, Color.green);

        //  �R���x�A�����ǉ�
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
    /// ��_���[�W
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    /// <param name="ct">�L�����Z���g�[�N��</param>
    /// <returns>��_���[�W����</returns>
    private async UniTask DamageTaken(int damage, CancellationToken ct)
    {
        //  �_���[�W��t��
        if (_canDamageTaken)
        {
            _canDamageTaken = false;
            try
            {
                //  �_���[�W����
                _hp.Value -= damage;    //  �_���[�W
                _damageClip?.Invoke();  //  ���ʉ�
                Instantiate(_damageEffect, _tr.position, Quaternion.identity) //  �G�t�F�N�g
                    .transform.SetParent(transform);    //  �q�I�u�W�F�N�g�ɐݒ�

                //  �_�ŏ���
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
            _contactClip?.Invoke(); //  ���ʉ�
        }
    }

    #region ------ HookShot
    /// <summary>
    /// �ˏo����
    /// </summary>
    private void HookShot()
    {
        //  �ˏo����
        if (_shotPhase == InputActionPhase.Performed)
        {
            HookActive();
        }
        else
        {
            _isFirstHookProcess = true; //  ���͂��Ȃ��Ȃ����珉����
            HookInactive();
        }

        _hookHead.transform.position = _headPos[LineType.ROPE];    //  �ڐG�ʒu�ړ�
    }

    /// <summary>
    /// �N��������
    /// </summary>
    private void HookActive()
    {
        //  �p�����[�^�擾
        var playerPos = _tr.position; //  �v���C���[�ʒu
        var type = GetHitColliderType();    //  �ڐG�R���C�_�[�̎��

        //  �ڐG����
        if (!_isHookHit)
        {
            _activeHitPos = type;   //  �ڐG��f�[�^�ۑ�
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

        //  �p�����[�^�X�V
        switch (_activeHitPos)
        {
            case HitCollider.Hit:
                Implement(_hitPoint, playerPos, AddForce(_hitPoint), true);
                break;

            case HitCollider.UnHit:
                Implement(_hitPoint, _hitPoint, Vector2.zero, true);
                break;

            case HitCollider.TrackingHit:
                //  �g���b�L���O�I�u�W�F�N�g��Null�`�F�b�N
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

        //  ������
        Vector2 AddForce(Vector2 point)
        {
            return (point - (Vector2)playerPos).normalized * _wireAddForce;
        }

        //  �p�����[�^
        void Implement
            (Vector3 rope
            , Vector3 indicator
            , Vector2 force
            , bool hookActive)
        {
            //  �ʒu
            _ropeHeadPos = rope;
            _headPos[LineType.INDICATOR] = indicator;

            //  �t�b�N�L�k��
            if (_isFirstHookProcess)
            {
                _isHookActive.Value = hookActive;
            }

            //  ����
            _moveForce = force;

        }

        //  ���[�v�ʒu�X�V
        if (!_isHookAnimation)  //  �t�b�N�A�j���[�V�����I����
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
    /// ��N��������
    /// </summary>
    /// <param name="type">�ڐG���Ă���</param>
    private void HookInactive()
    {
        var playerPos = _tr.position;
        var type = GetHitColliderType();

        //  ��[�ʒu���
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

        //  �p�����[�^
        void Implement(int lineSort, Vector3 indicatorPos)
        {
            //  �ڐG����
            _isHookHit = false;
            //  �C���W�P�[�^���C���̕\�������擾
            var indicatorLine = _indicatorLine.sortingOrder;
            //  �ڐG���ɂ��\�����̕ύX
            _highLightLine.sortingOrder = indicatorLine + lineSort;
            //  �ʒu
            _headPos[LineType.ROPE] = playerPos;
            _headPos[LineType.INDICATOR] = indicatorPos;
            //  ������
            _moveForce = Vector2.zero;

            _isHookActive.Value = false;
        }

        if (!_isHookAnimation)  //  �t�b�N�A�j���[�V�����I����
        {
            _headPos[LineType.ROPE] = playerPos;
        }
    }

    /// <summary>
    /// �ڐG���C�w��
    /// </summary>
    /// <returns>�ڐG�������C�ɑΉ������񋓌^�ϐ�(RayType)</returns>
    private HitCollider GetHitColliderType()
    {
        //  ���C�`��
        var playerPos = _tr.position;
        var rayDir = (_lookPos - playerPos).normalized;
        Debug.DrawRay(playerPos, rayDir * _wireDis, Color.green);

        //  �ڐG����
        var hit = Physics2D.RaycastAll(playerPos, rayDir, _wireDis, _layerMask);
        if (hit.Count() > 0)
        {
            //  layer�ɍ��킹��Enum�̕Ԃ�l
            var first = hit.First();
            var layer = first.transform.gameObject.layer;
            var hitCollider = (HitCollider)Enum.Parse(typeof(HitCollider), LayerMask.LayerToName(layer));

            //  �ڐG���p�����[�^
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
    /// ���C���X�V
    /// </summary>
    private void HookLineUpdate()
    {
        //  ���[�X�V
        LineUpdate(_ropeLine, LineType.ROPE);
        LineUpdate(_indicatorLine, LineType.INDICATOR);
        LineUpdate(_highLightLine, LineType.INDICATOR);

        void LineUpdate(LineRenderer line, LineType type)
        {
            line.gameObject.SetActive(IsDefault()); //  �ʏ펞�A�N�e�B�u

            if (line.gameObject != null)
            {
                //  �\����
                var decision = IsDefault() && !Tasks.IsFade;
                var headPos = decision ? _headPos[type] : _tr.position;
                //  �A�j���[�V�����I������
                var indicatorHeadPos = !_isHookAnimation ? headPos : _tr.position;
                //  ���[�v�^�C�v����
                var typeHeadPos = type == LineType.ROPE ? headPos : indicatorHeadPos;
                //  �ʒu�X�V
                line.SetPositions(new Vector3[] { _tr.position, typeHeadPos });
            }
        }
    }

    #endregion
}