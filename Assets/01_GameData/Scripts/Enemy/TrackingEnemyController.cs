using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.AI;

public class TrackingEnemyController : MonoBehaviour, IEnemyDamageable
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("スイッチ")] private Helper.Switch _move;

    [SerializeField, Required, BoxGroup("パラメータ")] private float _speed;
    [SerializeField, Required, BoxGroup("パラメータ")] private int _damage;
    [SerializeField, Required, BoxGroup("パラメータ")] private float _knockBackForce;

    [SerializeField, Required, BoxGroup("エフェクト")] private GameObject _knockEffect;

    // ---------------------------- Field
    //  初期化
    private SpriteRenderer _sr = null;
    private Rigidbody2D _rb = null;
    private NavMeshAgent _agent = null;

    //  追従
    private const float LOOK = 0.8f;




    // ---------------------------- UnityMessage
    private void Start()
    {
        //  キャッシュ
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _agent = GetComponent<NavMeshAgent>();
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




    // ---------------------------- PrivateMethod
    /// <summary>
    /// 移動処理
    /// </summary>
    private void Move()
    {
        //  画面内にオブジェクトがあるかどうか
        if (_sr.isVisible)
        {
            //  追従移動
            if (_move == Helper.Switch.ON)
            {
                _agent.SetDestination(PlayerController.Instance.transform.position);
            }
            //  プレイヤー方向へ回転
            var playerPos = PlayerController.Instance.transform.position;   //  プレイヤー位置取得
            var dir = Vector3.Lerp(playerPos, transform.position, LOOK);    //  方向決定
            var diff = (playerPos - dir).normalized;    //  ノーマライズ処理
            transform.rotation = Quaternion.FromToRotation(Vector3.up, diff);
        }
        else
        {
            //  停止
            _rb.Sleep();
        }
    }
}
