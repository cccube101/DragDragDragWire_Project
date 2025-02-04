using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using unityroom.Api;

namespace Helper
{
    /// <summary>
    /// �f�[�^����
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
        /// ������
        /// </summary>
        /// <param name="mixer">�ݒ�p���ʃ~�L�T�[</param>
        public static void Init(AudioMixer mixer)
        {
            //  �X�R�A������
            ScoreInit();

            //  �I�[�f�B�I����
            Audio.CreateParam();

            //  �����ς݂��ǂ���
            if (!GetBool(IS_ONETIME_INIT))
            {
                //  �p�����[�^������
                Audio.InitParam(mixer);

                //  �f�[�^�ۑ�
                Audio.SaveVolume();

                //  ������ԕۑ�
                SetBool(IS_ONETIME_INIT, true);
            }

            //  �ǂݍ���
            Audio.LoadVolume();
        }


        /// <summary>
        /// �X�R�A������
        /// </summary>
        public static void ScoreInit()
        {
            //  �V�[����������
            foreach (var name in Enum.GetNames(typeof(SceneName)))
            {
                //  �������V�[�� ���� �^�C�g���V�[�� �X�L�b�v
                if (name != "BaseInit" && name != "Title")
                {
                    //  �p�����[�^�擾
                    var coin = PlayerPrefs.GetInt(name + COIN, 0);
                    var hp = PlayerPrefs.GetInt(name + HP, 0);
                    var time = PlayerPrefs.GetFloat(name + TIME, 0);
                    var score = PlayerPrefs.GetFloat(name + SCORE, 0);

                    //  �V�[�����Ƃɍ��킹���X�g�ɕۑ�
                    _scoreList.Add(name, new ScoreData(coin, hp, time, score));

                    //  �g�b�v�^�C���擾
                    var topTime = PlayerPrefs.GetFloat(name + TOPTIME, LIMIT_TIME);

                    //  �V�[�����Ƃɍ��킹���X�g�ɕۑ�
                    _topTimeList.Add(name, topTime);
                }
            }

            //  �g�[�^���X�R�A�v�Z
            TotalScoreCalculation();
        }

        /// <summary>
        /// �X�R�A�ۑ�
        /// </summary>
        /// <param name="name">�V�[����</param>
        /// <param name="coin">�R�C����</param>
        /// <param name="hp">�cHP��</param>
        /// <param name="time">�N���A�^�C��</param>
        /// <param name="score">�X�e�[�W�X�R�A</param>
        public static void SaveScore(string name, int coin, int hp, float time, float score)
        {
            //  �x�X�g�X�R�A�X�V
            if (score >= _scoreList[name].Score)
            {
                //  �p�����[�^�ۑ�
                PlayerPrefs.SetInt(name + COIN, coin);
                PlayerPrefs.SetInt(name + HP, hp);
                PlayerPrefs.SetFloat(name + TIME, time);
                PlayerPrefs.SetFloat(name + SCORE, score);

                //  ���X�g�֍X�V
                _scoreList[name] = new ScoreData(coin, hp, time, score);
            }

            //  �g�b�v�^�C���X�V
            if (time < _topTimeList[name])
            {
                //  �ۑ�
                PlayerPrefs.SetFloat(name + TOPTIME, time);

                //  ���X�g�֍X�V
                _topTimeList[name] = time;
            }

            //  �g�[�^���X�R�A�v�Z
            TotalScoreCalculation();
        }

        /// <summary>
        /// �f�[�^�폜
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
        /// �g�[�^���X�R�A�v�Z
        /// </summary>
        private static void TotalScoreCalculation()
        {
            //  ���v�Z�o
            float totalScore = 0;
            foreach (var item in ScoreList)
            {
                totalScore += item.Value.Score;
            }
            TotalScore = totalScore;    //  �X�V

            //  ���ώZ�o
            float averageTime = 0;
            foreach (var item in TopTimeList)
            {
                averageTime += item.Value;
            }
            AverageTime = averageTime / TopTimeList.Count;  //  �X�V

            //  UnityRoom�����L���O�X�V
            if (_isUnityRoomApi)
            {
                UnityroomApiClient.Instance.SendScore(1, TotalScore, ScoreboardWriteMode.Always);
                UnityroomApiClient.Instance.SendScore(2, AverageTime, ScoreboardWriteMode.Always);
            }
        }

        /// <summary>
        /// bool�f�[�^�擾
        /// int�^�̃f�[�^�� bool�^�ɕϊ����Ď擾
        /// </summary>
        /// <param name="key">�ۑ��於</param>
        /// <returns></returns>
        private static bool GetBool(string key)
        {
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        /// <summary>
        /// bool�f�[�^�ۑ�
        /// bool�^�̃f�[�^�� int�^�ɕϊ����ĕۑ�
        /// </summary>
        /// <param name="key">�ۑ��於</param>
        /// <param name="value">�ۑ��f�[�^</param>
        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }
}
