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

    protected SpriteRenderer _sr = null;
    protected Rigidbody2D _rb = null;

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
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// プレイヤーへのダメージ
    /// </summary>
    /// <param name="player">プレイヤーオブジェクト</param>
    /// <returns></returns>
    public virtual int Damage(GameObject player)
    {
        //  ノックバック
        var dir = (player.transform.position - transform.position).normalized;
        player.GetComponent<Rigidbody2D>().AddForce(dir * _knockBackForce);

        return _damage;
    }

    /// <summary>
    /// 敵消滅イベント
    /// </summary>
    public virtual void Die()
    {
        //  エフェクト生成
        Instantiate(_knockEffect, transform.position, Quaternion.identity);

        //  削除
        Destroy(gameObject);
    }
}
