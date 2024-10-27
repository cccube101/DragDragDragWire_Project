using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.AI;

public class TrackingEnemyController : EnemyBase
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("スイッチ")] private Helper.Switch _move;
    [SerializeField, Required, BoxGroup("パラメータ")] private float _speed;

    // ---------------------------- Field
    //  初期化
    private NavMeshAgent _agent = null;

    //  追従
    private const float LOOK = 0.8f;




    // ---------------------------- UnityMessage
    public override void Start()
    {
        StartEvent();

        //  キャッシュ
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //  移動処理
        Move();
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
            //  プレイヤー位置取得
            var playerPos = PlayerController.Instance.Tr.position;
            //  追従移動
            if (_move == Helper.Switch.ON)
            {
                _agent.SetDestination(playerPos);
            }
            //  プレイヤー方向へ回転
            var dir = Vector3.Lerp(playerPos, _tr.position, LOOK);    //  方向決定
            var diff = (playerPos - dir).normalized;    //  ノーマライズ処理
            _tr.rotation = Quaternion.FromToRotation(Vector3.up, diff);
        }
        else
        {
            //  停止
            _rb2d.Sleep();
        }
    }
}
