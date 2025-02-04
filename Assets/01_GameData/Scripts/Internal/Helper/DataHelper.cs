using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using unityroom.Api;

namespace Helper
{
    /// <summary>
    /// データ処理
    /// </summary>
    public static class Data
    {
        public class ScoreData
        {
            public ScoreData(int coin, int hp, float time, float score)
            {
                Coin = coin;
                HP = hp;
                Time = time;
                Score = score;
            }
            public int Coin { get; set; }
            public int HP { get; set; }
            public float Time { get; set; }
            public float Score { get; set; }
        }

        // ---------------------------- Field
        private static readonly float LIMIT_TIME = 90;

        private static readonly string IS_ONETIME_INIT = "IsOnetimeInit";
        private static readonly string COIN = "Coin";
        private static readonly string HP = "HP";
        private static readonly string TIME = "Time";
        private static readonly string SCORE = "Score";
        private static readonly string TOPTIME = "TopTime";

        // ---------------------------- Property
        public static float LimitTime => LIMIT_TIME;
        public static Dictionary<string, ScoreData> ScoreList => _scoreList;
        private static readonly Dictionary<string, ScoreData> _scoreList = new();
        public static Dictionary<string, float> TopTimeList => _topTimeList;
        private static readonly Dictionary<string, float> _topTimeList = new();

        public static float TotalScore { get; set; }
        public static float AverageTime { get; set; }



        // ---------------------------- PublicMethod
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="mixer">設定用音量ミキサー</param>
        public static void Init(AudioMixer mixer)
        {
            //  スコア初期化
            ScoreInit();

            //  オーディオ生成
            Audio.CreateParam();

            //  生成済みかどうか
            if (!GetBool(IS_ONETIME_INIT))
            {
                //  パラメータ初期化
                Audio.InitParam(mixer);

                //  データ保存
                Audio.SaveVolume();

                //  生成状態保存
                SetBool(IS_ONETIME_INIT, true);
            }

            //  読み込み
            Audio.LoadVolume();
        }


        /// <summary>
        /// スコア初期化
        /// </summary>
        public static void ScoreInit()
        {
            //  シーン数分処理
            foreach (var name in Enum.GetNames(typeof(SceneName)))
            {
                //  初期化シーン ＆＆ タイトルシーン スキップ
                if (name != "BaseInit" && name != "Title")
                {
                    //  パラメータ取得
                    var coin = PlayerPrefs.GetInt(name + COIN, 0);
                    var hp = PlayerPrefs.GetInt(name + HP, 0);
                    var time = PlayerPrefs.GetFloat(name + TIME, 0);
                    var score = PlayerPrefs.GetFloat(name + SCORE, 0);

                    //  シーンごとに合わせリストに保存
                    _scoreList.Add(name, new ScoreData(coin, hp, time, score));

                    //  トップタイム取得
                    var topTime = PlayerPrefs.GetFloat(name + TOPTIME, LIMIT_TIME);

                    //  シーンごとに合わせリストに保存
                    _topTimeList.Add(name, topTime);
                }
            }

            //  トータルスコア計算
            TotalScoreCalculation();
        }

        /// <summary>
        /// スコア保存
        /// </summary>
        /// <param name="name">シーン名</param>
        /// <param name="coin">コイン数</param>
        /// <param name="hp">残HP数</param>
        /// <param name="time">クリアタイム</param>
        /// <param name="score">ステージスコア</param>
        public static void SaveScore(string name, int coin, int hp, float time, float score)
        {
            //  ベストスコア更新
            if (score >= _scoreList[name].Score)
            {
                //  パラメータ保存
                PlayerPrefs.SetInt(name + COIN, coin);
                PlayerPrefs.SetInt(name + HP, hp);
                PlayerPrefs.SetFloat(name + TIME, time);
                PlayerPrefs.SetFloat(name + SCORE, score);

                //  リストへ更新
                _scoreList[name] = new ScoreData(coin, hp, time, score);
            }

            //  トップタイム更新
            if (time < _topTimeList[name])
            {
                //  保存
                PlayerPrefs.SetFloat(name + TOPTIME, time);

                //  リストへ更新
                _topTimeList[name] = time;
            }

            //  トータルスコア計算
            TotalScoreCalculation();
        }

        /// <summary>
        /// データ削除
        /// </summary>
        public static void DeleteData()
        {
            PlayerPrefs.DeleteAll();
            _scoreList.Clear();
            _topTimeList.Clear();
            TotalScore = 0;
            AverageTime = 0;
            ScoreInit();
        }

        private static bool _isUnityRoomApi = true;

        /// <summary>
        /// トータルスコア計算
        /// </summary>
        private static void TotalScoreCalculation()
        {
            //  合計算出
            float totalScore = 0;
            foreach (var item in ScoreList)
            {
                totalScore += item.Value.Score;
            }
            TotalScore = totalScore;    //  更新

            //  平均算出
            float averageTime = 0;
            foreach (var item in TopTimeList)
            {
                averageTime += item.Value;
            }
            AverageTime = averageTime / TopTimeList.Count;  //  更新

            //  UnityRoomランキング更新
            if (_isUnityRoomApi)
            {
                UnityroomApiClient.Instance.SendScore(1, TotalScore, ScoreboardWriteMode.Always);
                UnityroomApiClient.Instance.SendScore(2, AverageTime, ScoreboardWriteMode.Always);
            }
        }

        /// <summary>
        /// boolデータ取得
        /// int型のデータを bool型に変換して取得
        /// </summary>
        /// <param name="key">保存先名</param>
        /// <returns></returns>
        private static bool GetBool(string key)
        {
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        /// <summary>
        /// boolデータ保存
        /// bool型のデータを int型に変換して保存
        /// </summary>
        /// <param name="key">保存先名</param>
        /// <param name="value">保存データ</param>
        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }
}
