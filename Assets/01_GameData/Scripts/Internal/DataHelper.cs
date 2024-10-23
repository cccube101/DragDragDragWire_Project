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
        private static readonly float LIMIT_TIME = 80;

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
        /// <param name="mixer"></param>
        public static void Init(AudioMixer mixer)
        {
            ScoreInit();

            Audio.CreateParam();

            if (!GetBool(IS_ONETIME_INIT))
            {
                Audio.InitParam(mixer);

                Audio.SaveVolume();

                SetBool(IS_ONETIME_INIT, true);
            }

            Audio.LoadVolume();
        }


        /// <summary>
        /// スコア初期化
        /// </summary>
        public static void ScoreInit()
        {
            foreach (var name in Enum.GetNames(typeof(SceneName)))
            {
                if (name != "BaseInit" && name != "Title")
                {
                    var coin = PlayerPrefs.GetInt(name + COIN, 0);
                    var hp = PlayerPrefs.GetInt(name + HP, 0);
                    var time = PlayerPrefs.GetFloat(name + TIME, 0);
                    var score = PlayerPrefs.GetFloat(name + SCORE, 0);

                    _scoreList.Add(name, new ScoreData(coin, hp, time, score));

                    var topTime = PlayerPrefs.GetFloat(name + TOPTIME, LIMIT_TIME);

                    _topTimeList.Add(name, topTime);
                }
            }

            TotalScoreCalculation();
        }

        /// <summary>
        /// スコア保存
        /// </summary>
        /// <param name="name"></param>
        /// <param name="coin"></param>
        /// <param name="hp"></param>
        /// <param name="time"></param>
        /// <param name="score"></param>
        public static void SaveScore(string name, int coin, int hp, float time, float score)
        {
            if (score >= _scoreList[name].Score)
            {
                PlayerPrefs.SetInt(name + COIN, coin);
                PlayerPrefs.SetInt(name + HP, hp);
                PlayerPrefs.SetFloat(name + TIME, time);
                PlayerPrefs.SetFloat(name + SCORE, score);

                _scoreList[name] = new ScoreData(coin, hp, time, score);
            }

            if (time < _topTimeList[name])
            {
                PlayerPrefs.SetFloat(name + TOPTIME, time);

                _topTimeList[name] = time;
            }

            TotalScoreCalculation();
        }

        /// <summary>
        /// トータルスコア計算
        /// </summary>
        private static void TotalScoreCalculation()
        {
            float totalScore = 0;
            foreach (var item in ScoreList)
            {
                totalScore += item.Value.Score;
            }
            TotalScore = totalScore;

            float averageTime = 0;
            foreach (var item in TopTimeList)
            {
                averageTime += item.Value;
            }
            AverageTime = averageTime / TopTimeList.Count;

            UnityroomApiClient.Instance.SendScore(1, Data.TotalScore, ScoreboardWriteMode.Always);
            UnityroomApiClient.Instance.SendScore(2, Data.AverageTime, ScoreboardWriteMode.Always);
        }

        /// <summary>
        /// boolデータ保存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool GetBool(string key)
        {
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        /// <summary>
        /// boolデータ取得
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }
}
