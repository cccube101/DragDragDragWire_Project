using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Helper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("ドラッグ")] private Transform _arrowTr;
    [SerializeField, Required, BoxGroup("ドラッグ")] private Transform[] _dragPathTr;
    [SerializeField, Required, BoxGroup("ドラッグ")] private Transform _dragMouseTr;
    [SerializeField, Required, BoxGroup("ドラッグ")] private int _dragLoops;
    [SerializeField, Required, BoxGroup("ドラッグ")] private float _dragAnimeDuration;
    [SerializeField, Required, BoxGroup("ドラッグ")] private LineRenderer _dragLine;

    [SerializeField, Required, BoxGroup("ショット")] private GameObject _shotMouseNormal;
    [SerializeField, Required, BoxGroup("ショット")] private GameObject _shotMouseClick;
    [SerializeField, Required, BoxGroup("ショット")] private Transform _shotHeadTr;
    [SerializeField, Required, BoxGroup("ショット")] private Transform _shotPlayerTr;
    [SerializeField, Required, BoxGroup("ショット")] private Vector2 _endScale;
    [SerializeField, Required, BoxGroup("ショット")] private float _scaleDuration;
    [SerializeField, Required, BoxGroup("ショット")] private float _shotDuration;
    [SerializeField, Required, BoxGroup("ショット")] private float _moveDuration;
    [SerializeField, Required, BoxGroup("ショット")] private float _waitTime;
    [SerializeField, Required, BoxGroup("ショット")] private LineRenderer _shotRopeLine;

    // ---------------------------- Field
    private readonly float LOOK = 0.8f;

    // ---------------------------- UnityMessage
    private async void Start()
    {
        //  開始イベント
        await Tasks.Canceled(StartEvent(destroyCancellationToken));
    }

    private void Update()
    {
        //  ドラッグ
        LookDragMouse();
        DrawDragLine();

        //  ショット
        DrawShotLine();
    }

    // ---------------------------- PublicMethod





    // ---------------------------- PrivateMethod
    /// <summary>
    /// 開始イベント
    /// </summary>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>開始イベント処理</returns>
    private async UniTask StartEvent(CancellationToken ct)
    {
        //  描写可能まで待機
        await UniTask.WaitUntil(() => Decision());

        //  アニメーション開始
        var tasks = new List<UniTask>()
        {
            DragLoop(ct),
            ShotLoop(ct),
        };
        await UniTask.WhenAll(tasks);
    }


    /// <summary>
    /// ドラッグアニメーションループ
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async UniTask DragLoop(CancellationToken ct)
    {
        //  矢印回転用位置を保存
        var path = _dragPathTr.Select(tr => tr.position).ToArray();
        //  反転位置を保存
        var reversePath = path.Reverse().ToArray();

        while (true)
        {
            //  交互に回転
            await PathMove(path);
            await PathMove(reversePath);

            await UniTask.Yield(cancellationToken: ct);
        }

        async UniTask PathMove(Vector3[] path)
        {
            //  位置移動
            await _dragMouseTr.DOPath(path, _dragAnimeDuration, PathType.CatmullRom, PathMode.Sidescroller2D)
                .SetEase(Ease.Linear)
                .SetLoops(_dragLoops, LoopType.Restart)
                .SetOptions(true)
                .SetLink(_dragMouseTr.gameObject)
                .ToUniTask(Tasks.TCB, cancellationToken: ct);
        }
    }

    /// <summary>
    /// 矢印をマウスへ方向転換
    /// </summary>
    private void LookDragMouse()
    {
        //  パラメータ生成
        var mousePos = _dragMouseTr.position;   //  マウス画像位置取得
        var dir = Vector3.Lerp(mousePos, _arrowTr.position, LOOK);  //  ラープ処理
        var diff = (mousePos - dir).normalized; //  ノーマライズ

        //  回転
        _arrowTr.rotation = Quaternion.FromToRotation(Vector3.down, diff);
    }

    /// <summary>
    /// 線描写
    /// </summary>
    private void DrawDragLine()
    {
        //  先端位置取得
        //  描画可能かどうか判定 (マウス位置：矢印位置)
        var headPos = Decision() ? _dragMouseTr.position : _arrowTr.position;
        _dragLine.SetPositions(new Vector3[] { _arrowTr.position, headPos });
    }

    /// <summary>
    /// ショットアニメーションループ
    /// </summary>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>アニメーションループ処理</returns>
    private async UniTask ShotLoop(CancellationToken ct)
    {
        //  基準位置取得
        var originPlayerPos = _shotPlayerTr.position;
        var originAnchorPos = _shotRopeLine.transform.position;

        //  初期位置に向かって交互に動かす
        while (true)
        {
            await Tasks.DelayTime(_waitTime, ct);
            await Anime(originAnchorPos);
            await Tasks.DelayTime(_waitTime, ct);
            await Anime(originPlayerPos);
        }

        async UniTask Anime(Vector3 endPos)
        {
            //  フックとプレイヤーを交互に動かす
            var tasks = new List<UniTask>()
            {
                Move(_shotHeadTr,_shotDuration,endPos),
                Move(_shotPlayerTr,_moveDuration,endPos),
                //  マウスアニメーション切換え
                MouseAnime(),
            };
            await UniTask.WhenAll(tasks);
        }
        async UniTask Move(Transform tr, float duration, Vector3 endPos)
        {
            //  位置移動
            await tr.DOMove(endPos, duration)
                .SetEase(Ease.Linear)
                .SetLink(tr.gameObject)
                .ToUniTask(Tasks.TCB, cancellationToken: ct);
        }
        async UniTask MouseAnime()
        {
            //  画像切換え
            _shotMouseNormal.SetActive(false);
            _shotMouseClick.SetActive(true);

            //  スケール変更
            await Scale(_endScale.x);
            //  待機
            await Tasks.DelayTime(_moveDuration - _scaleDuration, ct);
            //  スケール変更
            await Scale(_endScale.y);

            //  画像切換え
            _shotMouseNormal.SetActive(true);
            _shotMouseClick.SetActive(false);
        }
        async UniTask Scale(float endScale)
        {
            //  スケール変更
            await _shotMouseClick.transform.DOScale(endScale, _scaleDuration)
              .SetEase(Ease.OutBack)
              .SetLink(_shotMouseClick)
              .ToUniTask(Tasks.TCB, cancellationToken: ct);
        }
    }

    /// <summary>
    /// 線描写
    /// </summary>
    private void DrawShotLine()
    {
        //  先端位置取得
        var headPos = Decision() ? _shotHeadTr.position : _shotPlayerTr.position;
        //  ラインレンダラー更新
        _shotRopeLine.SetPositions(new Vector3[] { _shotPlayerTr.position, headPos });
    }

    /// <summary>
    /// 描写可否
    /// </summary>
    /// <returns>描画可否</returns>
    private bool Decision()
    {
        //  ゲームステート取得
        var isDefault = GameManager.Instance.State.CurrentValue == GameState.DEFAULT;

        //  ラインレンダラー描画
        _dragLine.gameObject.SetActive(isDefault);
        _shotRopeLine.gameObject.SetActive(isDefault);

        // デフォルトステート ＆ 非遷移時
        return isDefault && !Tasks.IsFade;
    }
}
