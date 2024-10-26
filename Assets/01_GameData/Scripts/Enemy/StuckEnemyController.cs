using Alchemy.Inspector;
using UnityEngine;

public class StuckEnemyController : EnemyBase
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("パラメータ")] private float _turnSpeed;

    // ---------------------------- UnityMessage
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
    public override int Damage(GameObject player)
    {
        //  エフェクト
        Instantiate(_knockEffect, transform.position, Quaternion.identity);

        //  ノックバック
        var dir = (player.transform.position - transform.position).normalized;
        player.GetComponent<Rigidbody2D>().AddForce(dir * _knockBackForce);

        return _damage;
    }
}
