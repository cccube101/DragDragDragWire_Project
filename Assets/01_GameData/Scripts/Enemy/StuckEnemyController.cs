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
            _tr.eulerAngles += new Vector3(0, 0, _turnSpeed * Time.deltaTime);
        }
    }




    // ---------------------------- PublicMethod
    /// <summary>
    /// プレイヤーへのダメージ
    /// </summary>
    /// <returns>ダメージ量</returns>
    public override int Damage()
    {
        //  エフェクト
        Instantiate(_knockEffect, _tr.position, Quaternion.identity);

        //  ノックバック
        KnockBackPlayer();

        return _damage;
    }
}
