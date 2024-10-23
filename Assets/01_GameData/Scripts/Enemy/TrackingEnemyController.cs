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
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        Move();
    }





    // ---------------------------- PublicMethod
    /// <summary>
    /// プレイヤーへのダメージ
    /// </summary>
    public int Damage(GameObject obj)
    {
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
        Instantiate(_knockEffect, transform.position, Quaternion.identity); //  エフェクト
        Destroy(gameObject);    //  削除
    }




    // ---------------------------- PrivateMethod
    /// <summary>
    /// 移動処理
    /// </summary>
    private void Move()
    {
        if (_sr.isVisible)
        {
            //  追従移動
            if (_move == Helper.Switch.ON)
            {
                _agent.SetDestination(PlayerController.Instance.transform.position);
            }
            //  プレイヤー方向へ回転
            var playerPos = PlayerController.Instance.transform.position;
            var dir = Vector3.Lerp(playerPos, transform.position, LOOK);
            var diff = (playerPos - dir).normalized;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, diff);
        }
        else
        {
            _rb.Sleep();
        }
    }
}
