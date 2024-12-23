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
    private Transform _tr = null;
    private SpriteRenderer _sr = null;
    private readonly List<GameObject> _totalCount = new();




    // ---------------------------- UnityMessage
    private async void Start()
    {
        //  キャッシュ
        _tr = transform;
        _sr = GetComponent<SpriteRenderer>();

        //  生成開始
        await Helper.Tasks.Canceled(EnemyGeneration(destroyCancellationToken));
    }





    // ---------------------------- PrivateMethod
    /// <summary>
    /// 生成制御
    /// </summary>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>タスク処理</returns>
    private async UniTask EnemyGeneration(CancellationToken ct)
    {
        while (true)
        {
            //  生成間隔
            var time = Random.Range(_generateTime.x, _generateTime.y);
            await Helper.Tasks.DelayTime(time, ct);

            //  生成判定
            //  スイッチ オン ＆＆ 画面内にオブジェクトがあるかどうか
            if (_generate == Helper.Switch.OFF && !_sr.isVisible)
            {
                continue;
            }

            //  生成
            var enemy = Instantiate(_enemy, _tr.position, Quaternion.identity);
            _totalCount.Add(enemy);

            //  生成数制限
            //  最大数を超えたとき順次削除
            if (_totalCount.Count > _spawnLimit)
            {
                _totalCount[0]?.GetComponent<IEnemyDamageable>().Die();
                _totalCount.RemoveAt(0);
            }

            await UniTask.Yield(cancellationToken: ct);
        }
    }
}
