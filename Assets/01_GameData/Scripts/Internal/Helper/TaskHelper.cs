using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Helper
{
    /// <summary>
    /// タスク処理
    /// </summary>
    public static class Tasks
    {
        // ---------------------------- Field
        private static UnityEvent _fadeClip = null;

        private static readonly float FADE_TIME = 2;
        private static bool _isFade = false;
        // ---------------------------- Property
        public static TweenCancelBehaviour TCB => TweenCancelBehaviour.KillAndCancelAwait;
        public static bool IsFade => _isFade;

        public static UnityEvent FadeClip { set => _fadeClip = value; }



        // ---------------------------- PublicMethod
        #region ------ Fade
        /// <summary>
        /// フェードイン
        /// </summary>
        /// <param name="canvas">キャンバスグループ</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>フェードイン処理</returns>
        public static async UniTask FadeIn(CanvasGroup canvas, CancellationToken ct)
        {
            //  UI判定ブロック
            canvas.blocksRaycasts = false;

            //  フェード処理
            await FadeTask(0, 1, FadeImage.ImageType.FADEIN, ct);
        }

        /// <summary>
        /// フェードアウト
        /// </summary>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>フェードアウト処理</returns>
        public static async UniTask FadeOut(CancellationToken ct)
        {
            //  フェード処理
            await FadeTask(1, 0, FadeImage.ImageType.FADEOUT, ct);
        }

        /// <summary>
        /// フェード
        /// </summary>
        /// <param name="start">スタート時画像状態</param>
        /// <param name="end">終了時画像状態</param>
        /// <param name="type">イメージの指定</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>フェード処理</returns>
        private static async UniTask FadeTask
            (float start
            , float end
            , FadeImage.ImageType type
            , CancellationToken ct)
        {
            //  時間停止
            Time.timeScale = 0.0f;
            _isFade = true;

            //  イメージ変更
            FadeImage.Instance.UpdateMaskTexture(type);
            //  効果音再生
            _fadeClip?.Invoke();
            //  フェード処理再生
            await Canceled(Fade.Instance.FadeTask(start, end, FADE_TIME, ct));

            //  時間再生
            _isFade = false;
            Time.timeScale = 1.0f;
        }

        #endregion

        #region ------ TransitionEvent
        /// <summary>
        /// シーン遷移
        /// </summary>
        /// <param name="scene">遷移先シーン</param>
        /// <param name="canvas">キャンバスグループ</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>シーン遷移処理</returns>
        public static async UniTask SceneChange(int scene, CanvasGroup canvas, CancellationToken ct)
        {
            //  フェード処理
            await FadeIn(canvas, ct);

            //  シーン遷移
            SceneManager.LoadScene(scene);
            //  時間再生
            Time.timeScale = 1.0f;
        }

        /// <summary>
        /// 終了
        /// </summary>
        /// <param name="canvas">キャンバスグループ</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>終了処理</returns>
        public static async UniTask ApplicationQuit(CanvasGroup canvas, CancellationToken ct)
        {
            //  フェード処理
            await FadeIn(canvas, ct);

            //  音量保存
            Audio.SaveVolume();

            //  終了
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; //ゲームシーン終了
#else
        Application.Quit(); //build後にゲームプレイ終了が適用
#endif
        }

        #endregion

        /// <summary>
        /// 待機
        /// </summary>
        /// <param name="time">待機時間</param>
        /// <param name="ct">キャンセルトークン</param>
        /// <returns>待機処理</returns>
        public static async UniTask DelayTime(float time, CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(time), true, cancellationToken: ct);
        }

        /// <summary>
        /// UniTaskキャンセル
        /// </summary>
        /// <param name="task">キャンセルしたいタスク</param>
        /// <returns>キャンセル処理</returns>
        public static async UniTask Canceled(UniTask task)
        {
            if (await task.SuppressCancellationThrow()) { return; }
        }
    }
}
