using Alchemy.Inspector;
using UnityEngine;

public class StuckEnemyController : MonoBehaviour, IEnemyDamageable
{
    // ---------------------------- SerializeField

    [SerializeField, Required, BoxGroup("パラメータ")] private float _turnSpeed;
    [SerializeField, Required, BoxGroup("パラメータ")] private int _damage;
    [SerializeField, Required, BoxGroup("パラメータ")] private float _knockBackForce;

    [SerializeField, Required, BoxGroup("エフェクト")] private GameObject _knockEffect;

    // ---------------------------- Field
    private SpriteRenderer _sr = null;


    // ---------------------------- UnityMessage
    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_sr.isVisible)
        {
            transform.eulerAngles += new Vector3(0, 0, _turnSpeed * Time.deltaTime);
        }
    }




    // ---------------------------- PublicMethod
    /// <summary>
    /// プレイヤーへのダメージ
    /// </summary>
    public int Damage(GameObject obj)
    {
        //  エフェクト
        Instantiate(_knockEffect, transform.position, Quaternion.identity);

        //  ノックバック
        var dir = (obj.transform.position - transform.position).normalized;
        obj.GetComponent<Rigidbody2D>().AddForce(dir * _knockBackForce);

        //  ダメージ
        return _damage;
    }

    /// <summary>
    /// 敵消滅
    /// </summary>
    public void Die()
    {
        //  エフェクト
        Instantiate(_knockEffect, transform.position, Quaternion.identity);

        //  削除
        Destroy(gameObject);
    }
}
