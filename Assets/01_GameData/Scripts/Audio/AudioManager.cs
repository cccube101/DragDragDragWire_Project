using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Alchemy.Inspector;
using Helper;


public class AudioManager : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("パラメータ")] private AudioMixer _mixer;
    [SerializeField, Required, BoxGroup("パラメータ")] private Slider _masterSlider;
    [SerializeField, Required, BoxGroup("パラメータ")] private Slider _bgmSlider;
    [SerializeField, Required, BoxGroup("パラメータ")] private Slider _seSlider;

    // ---------------------------- UnityMessage
    private void Start()
    {
        if (!Audio.Params.TryGetValue(Audio.MASTER, out var value))
        {
#if UNITY_EDITOR
            Audio.CreateParam();   //  デバッグ用にパラメータを初期化
            Debug.Log("AudioParam生成");
#else
            //  データが見つからない場合は初期化シーンに移動
            SceneManager.LoadScene((int)SceneName.BaseInit);
#endif
        }

        //  パラメータにスライダーを保存
        SetParam(Audio.MASTER, _masterSlider);
        SetParam(Audio.BGM, _bgmSlider);
        SetParam(Audio.SE, _seSlider);

        void SetParam(string group, Slider slider)
        {
            var volume = Audio.Params[group].Volume;

            slider.value = volume;  //  スライダー
            Audio.Params[group].Bar = slider;
            _mixer.SetFloat(group, ConvertVolumeToDb(volume));  //  ミキサー
        }
    }


    // ---------------------------- PublicMethod
    #region ------ ChangeSliderVolume
    /// <summary>
    /// マスターボリューム変更
    /// </summary>
    /// <param name="value"></param>
    public void ChangeValue_Master(float value)
    {
        ChangeValue(Audio.MASTER, value);
    }

    /// <summary>
    /// BGMボリューム変更
    /// </summary>
    /// <param name="value"></param>
    public void ChangeValue_BGM(float value)
    {
        ChangeValue(Audio.BGM, value);
    }

    /// <summary>
    /// SEボリューム変更
    /// </summary>
    /// <param name="value"></param>
    public void ChangeValue_SE(float value)
    {
        ChangeValue(Audio.SE, value);
    }

    #endregion ---




    // ---------------------------- PrivateMethod
    /// <summary>
    /// ボリューム変更
    /// </summary>
    /// <param name="group"></param>
    /// <param name="value"></param>
    private void ChangeValue(string group, float value)
    {
        Audio.Params[group].Volume = value;
        _mixer.SetFloat(group, ConvertVolumeToDb(value));
    }

    /// <summary>
    /// スライダーの値をミキサー用に調整(Volume -> Db)
    /// </summary>
    /// <param name="volume"></param>
    /// <returns></returns>
    private float ConvertVolumeToDb(float volume)
    {
        return Mathf.Clamp(Mathf.Log10(Mathf.Clamp(volume, 0f, 1f)) * 20f, -80f, 20f);
    }
}
