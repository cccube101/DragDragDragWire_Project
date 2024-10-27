using Alchemy.Inspector;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IEnemyDamageable
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("基礎")] protected int _damage;
    [SerializeField, Required, BoxGroup("基礎")] protected float _knockBackForce;
    [SerializeField, Required, BoxGroup("基礎")] protected GameObject _knockEffect;


    // ---------------------------- Field
    protected IEnemyDamageable _enemyDamageable;

    protected GameObject _obj = null;
    protected Transform _tr = null;
    protected SpriteRenderer _sr = null;
    protected Rigidbody2D _rb2d = null;

    // ---------------------------- UnityMessage
    public virtual void Start()
    {
        StartEvent();
    }



    // ---------------------------- PublicMethod
    /// <summary>
    /// 開始イベント
    /// </summary>
    public void StartEvent()
    {
        _obj = gameObject;
        _tr = transform;
        _sr = GetComponent<SpriteRenderer>();
        _rb2d = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// プレイヤーへのダメージ
    /// </summary>
    /// <returns>ダメージ量</returns>
    public virtual int Damage()
    {
        KnockBackPlayer();

        return _damage;
    }

    /// <summary>
    /// ノックバック
    /// </summary>
    public void KnockBackPlayer()
    {
        //  ノックバック
        var player = PlayerController.Instance;
        var dir = (player.Tr.position - _tr.position).normalized;
        player.RB2D.AddForce(dir * _knockBackForce);
    }

    /// <summary>
    /// 敵消滅イベント
    /// </summary>
    public virtual void Die()
    {
        //  エフェクト生成
        Instantiate(_knockEffect, _tr.position, Quaternion.identity);

        //  削除
        Destroy(_obj);
    }
}
