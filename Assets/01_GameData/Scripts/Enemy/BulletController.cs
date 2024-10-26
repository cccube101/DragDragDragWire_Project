using Alchemy.Inspector;
using UnityEngine;

public class BulletController : MonoBehaviour, IEnemyDamageable
{
    // ---------------------------- SerializeField

    [SerializeField, Required, BoxGroup("基礎")] private float _moveSpeed;
    [SerializeField, Required, BoxGroup("基礎")] private int _damage;
    [SerializeField, Required, BoxGroup("基礎")] private float _knockBackForce;

    [SerializeField, Required, BoxGroup("エフェクト")] private GameObject _knockEffect;
    [SerializeField, Required, BoxGroup("エフェクト")] private GameObject _shootClip;
    [SerializeField, Required, BoxGroup("エフェクト")] private GameObject _hitClip;

    // ---------------------------- Field
    private Vector3 _addDir;

    // ---------------------------- Property
    /// <summary>
    /// 方向更新
    /// </summary>
    public (Vector3 Dir, Quaternion Rotation) Dir
    {
        set
        {
            _addDir = value.Dir;
            transform.rotation = value.Rotation;
        }
    }


    // ---------------------------- UnityMessage
    private void Start()
    {
        //  射撃音再生
        Instantiate(_shootClip, transform.position, Quaternion.identity);
    }

    private void Update()
    {
        //  移動処理
        transform.position += _addDir * _moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var obj = collision.gameObject;
        if (obj.CompareTag(TagName.Ground))
        {
            Die();
        }
        else if (obj.CompareTag(TagName.Belt))
        {
            Die();
        }
    }




    // ---------------------------- PublicMethod
    /// <summary>
    /// プレイヤーへのダメージ
    /// </summary>
    /// <param name="player">プレイヤーオブジェクト</param>
    /// <returns>ダメージ量</returns>
    public int Damage(GameObject player)
    {
        //  プレイヤーへのノックバック処理
        var dir = (player.transform.position - transform.position).normalized;
        player.GetComponent<Rigidbody2D>().AddForce(dir * _knockBackForce);

        return _damage;
    }

    /// <summary>
    /// 敵消滅
    /// </summary>
    public void Die()
    {
        //  エフェクト生成
        Instantiate(_hitClip, transform.position, Quaternion.identity);
        Instantiate(_knockEffect, transform.position, Quaternion.identity);

        //  削除
        Destroy(gameObject);
    }
}
