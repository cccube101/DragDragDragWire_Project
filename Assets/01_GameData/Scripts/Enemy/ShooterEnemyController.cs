using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class ShooterEnemyController : EnemyBase
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("オブジェクト")] private GameObject _muzzle;
    [SerializeField, Required, BoxGroup("オブジェクト")] private GameObject _bulletObj;

    [SerializeField, Required, BoxGroup("弾パラメータ")] private float _generationVolume;
    [SerializeField, Required, BoxGroup("弾パラメータ")] private float _generationRate;
    [SerializeField, Required, BoxGroup("弾パラメータ")] private float _generationInterval;

    // ---------------------------- Field
    //  キャッシュ
    private Transform _muzzleTr = null;

    //  追従
    private readonly float LOOK = 0.8f;




    // ---------------------------- UnityMessage
    public override async void Start()
    {
        StartEvent();
        _muzzleTr = _muzzle.transform;

        //  射撃サイクル開始
        await Helper.Tasks.Canceled(ShooterCycle(destroyCancellationToken));
    }

    private void Update()
    {
        //  移動処理
        Move();
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
                    var bullet = Instantiate(_bulletObj, _tr.position, Quaternion.identity);
                    var dir = _muzzleTr.position - _tr.position;

                    //  弾方向指定
                    bullet.GetComponent<BulletController>().Dir = (dir, _tr.rotation);

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
            var playerPos = PlayerController.Instance.Tr.position;   //  プレイヤー位置取得

            //  プレイヤー方向へ回転
            var dir = Vector3.Lerp(playerPos, _tr.position, LOOK);
            var diff = (playerPos - dir).normalized;
            _tr.rotation = Quaternion.FromToRotation(Vector3.up, diff);
        }
        else
        {
            //  停止
            _rb2d.Sleep();
        }
    }
}
