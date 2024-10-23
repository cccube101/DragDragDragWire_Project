using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("スイッチ")] private Helper.Switch _generate;

    [SerializeField, Required, BoxGroup("パラメータ")] private int _spawnLimit;
    [SerializeField, Required, BoxGroup("パラメータ")] private Vector2 _generateTime;
    [SerializeField, Required, BoxGroup("パラメータ/敵オブジェクト")] private GameObject _enemy;

    // ---------------------------- Field
    private SpriteRenderer _sr = null;
    private readonly List<GameObject> _totalCount = new();




    // ---------------------------- UnityMessage
    private async void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        await Helper.Tasks.Canceled(EnemyGeneration(destroyCancellationToken));
    }





    // ---------------------------- PrivateMethod
    /// <summary>
    /// 生成制御
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async UniTask EnemyGeneration(CancellationToken ct)
    {
        while (true)
        {
            //  生成間隔
            var time = Random.Range(_generateTime.x, _generateTime.y);
            await Helper.Tasks.DelayTime(time, ct);

            //  生成判定
            if (_generate == Helper.Switch.OFF && !_sr.isVisible)
            {
                continue;
            }

            //  生成
            var enemy = Instantiate(_enemy, transform.position, Quaternion.identity);
            _totalCount.Add(enemy);

            //  生成数制限
            if (_totalCount.Count > _spawnLimit)
            {
                _totalCount[0]?.GetComponent<IEnemyDamageable>().Die();
                _totalCount.RemoveAt(0);
            }

            await UniTask.Yield(cancellationToken: ct);
        }
    }
}
