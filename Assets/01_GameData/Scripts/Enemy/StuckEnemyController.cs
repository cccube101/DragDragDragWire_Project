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
        //  キャッシュ
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //  方向転換
        //  画面内にオブジェクトがある際に処理
        if (_sr.isVisible)
        {
            transform.eulerAngles += new Vector3(0, 0, _turnSpeed * Time.deltaTime);
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
        //  エフェクト
        Instantiate(_knockEffect, transform.position, Quaternion.identity);

        //  ノックバック
        var dir = (player.transform.position - transform.position).normalized;
        player.GetComponent<Rigidbody2D>().AddForce(dir * _knockBackForce);

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
