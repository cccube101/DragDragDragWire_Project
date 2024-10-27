using Alchemy.Inspector;
using UnityEngine;

public class BulletController : EnemyBase
{
    // ---------------------------- SerializeField

    [SerializeField, Required, BoxGroup("パラメータ")] private float _moveSpeed;
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
            _tr.rotation = value.Rotation;
        }
    }


    // ---------------------------- UnityMessage
    public override void Start()
    {
        StartEvent();

        //  射撃音再生
        Instantiate(_shootClip, _tr.position, Quaternion.identity);
    }

    private void Update()
    {
        //  移動処理
        _tr.position += _addDir * _moveSpeed * Time.deltaTime;
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
    /// 敵消滅
    /// </summary>
    public override void Die()
    {
        //  エフェクト生成
        Instantiate(_hitClip, _tr.position, Quaternion.identity);
        Instantiate(_knockEffect, _tr.position, Quaternion.identity);

        //  削除
        Destroy(_obj);
    }
}
