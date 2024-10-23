using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Helper
{
    /// <summary>
    /// 音声パラメータ処理
    /// </summary>
    public static class Audio
    {
        /// <summary>
        /// パラメータ保存クラス
        /// </summary>
        public class Param
        {
            public Param(float volume, Slider bar)
            {
                Volume = volume;
                Bar = bar;
            }

            public float Volume { get; set; }
            public Slider Bar { get; set; }
        }


        // ---------------------------- Field
        private static readonly Dictionary<string, Param> _params = new();



        // ---------------------------- Property
        public static readonly string MASTER = "Master", BGM = "BGM", SE = "SE";
        public static Dictionary<string, Param> Params => _params;



        // ---------------------------- PublicMethod
        /// <summary>
        /// パラメータ作成
        /// </summary>
        public static void CreateParam()
        {
            Create(MASTER);
            Create(BGM);
            Create(SE);

            static void Create(string group)
            {
                _params.Add(group, new Param(PlayerPrefs.GetFloat(group), null));

            }
        }


        /// <summary>
        /// 音量初期化
        /// </summary>
        /// <param name="mixer"></param>
        public static void InitParam(AudioMixer mixer)
        {
            foreach (var param in _params)
            {
                mixer.GetFloat(param.Key.ToString(), out float value);
                param.Value.Volume = Mathf.Clamp((float)Math.Pow(10, value / 20), 0f, 1f);
            }
        }

        /// <summary>
        /// 音量保存
        /// </summary>
        public static void SaveVolume()
        {
            foreach (var param in _params)
            {
                PlayerPrefs.SetFloat(param.Key, param.Value.Volume);
            }
        }

        /// <summary>
        /// 音量取得
        /// </summary>
        public static void LoadVolume()
        {
            foreach (var param in _params)
            {
                param.Value.Volume = PlayerPrefs.GetFloat(param.Key);
            }
        }
    }
}