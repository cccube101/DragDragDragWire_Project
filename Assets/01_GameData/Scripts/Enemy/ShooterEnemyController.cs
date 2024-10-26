using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class ShooterEnemyController : MonoBehaviour, IEnemyDamageable
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("オブジェクト")] private GameObject _muzzle;
    [SerializeField, Required, BoxGroup("オブジェクト")] private GameObject _bulletObj;

    [SerializeField, Required, BoxGroup("弾パラメータ")] private float _generationVolume;
    [SerializeField, Required, BoxGroup("弾パラメータ")] private float _generationRate;
    [SerializeField, Required, BoxGroup("弾パラメータ")] private float _generationInterval;

    [SerializeField, Required, BoxGroup("ダメージ")] private int _damage;
    [SerializeField, Required, BoxGroup("ダメージ")] private float _knockBackForce;

    [SerializeField, Required, BoxGroup("エフェクト")] private GameObject _knockEffect;

    // ---------------------------- Field
    //  初期化
    private SpriteRenderer _sr = null;
    private Rigidbody2D _rb = null;

    //  追従
    private readonly float LOOK = 0.8f;




    // ---------------------------- UnityMessage
    private async void Start()
    {
        //  キャッシュ
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();

        //  射撃サイクル開始
        await Helper.Tasks.Canceled(ShooterCycle(destroyCancellationToken));
    }

    private void Update()
    {
        //  移動処理
        Move();
    }




    // ---------------------------- PublicMethod
    /// <summary>
    /// プレイヤーへのダメージ
    /// </summary>
    /// <param name="player">プレイヤーオブジェクト</param>
    /// <returns>ダメージ量</returns>
    public int Damage(GameObject player)
    {
        var dir = (player.transform.position - transform.position).normalized;
        player.GetComponent<Rigidbody2D>().AddForce(dir * _knockBackForce);
        return _damage;
    }

    /// <summary>
    /// 敵消滅
    /// </summary>
    public void Die()
    {
        Instantiate(_knockEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }




    // ---------------------------- PrivateMethod
    /// <summary>
    /// 射撃サイクル
    /// </summary>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>射撃サイクル</returns>
    private async UniTask ShooterCycle(CancellationToken ct)
    {
        while (true)
        {
            //  画面内にオブジェクトがあるかどうか
            if (_sr.isVisible)
            {
                //  指定数射撃
                for (int i = 0; i < _generationVolume; i++)
                {
                    //  パラメータ生成
                    var bullet = Instantiate(_bulletObj, transform.position, Quaternion.identity);
                    var dir = _muzzle.transform.position - transform.position;

                    //  弾方向指定
                    bullet.GetComponent<BulletController>().Dir = (dir, transform.rotation);

                    //  待機
                    await Helper.Tasks.DelayTime(_generationRate, ct);
                }
            }
            //  待機
            await Helper.Tasks.DelayTime(_generationInterval, ct);
            await UniTask.Yield(cancellationToken: ct);
        }
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    private void Move()
    {
        //  画面内にオブジェクトがあるかどうか
        if (_sr.isVisible)
        {
            var playerPos = PlayerController.Instance.transform.position;   //  プレイヤー位置取得

            //  プレイヤー方向へ回転
            var dir = Vector3.Lerp(playerPos, transform.position, LOOK);
            var diff = (playerPos - dir).normalized;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, diff);
        }
        else
        {
            //  停止
            _rb.Sleep();
        }
    }
}
