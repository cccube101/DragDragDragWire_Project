using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Helper;

public class InitManager : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private AudioMixer _mixer;



    // ---------------------------- Field




    // ---------------------------- UnityMessage
    private void Awake()
    {
        //  データ初期化
        Data.Init(_mixer);
    }

    private void Start()
    {
        //  シーン遷移
        SceneManager.LoadScene((int)SceneName.Title);
        Time.timeScale = 1.0f;
    }

    // ---------------------------- PublicMethod





    // ---------------------------- PrivateMethod
#if UNITY_EDITOR
    [Button]
    public void DataLog()
    {
        Debug.Log($"IsOneTime:{PlayerPrefs.GetInt("IsOnetimeInit")}");
        Debug.Log($"{Audio.MASTER}:{PlayerPrefs.GetFloat(Audio.MASTER)}");
        Debug.Log($"{Audio.BGM}:{PlayerPrefs.GetFloat(Audio.BGM)}");
        Debug.Log($"{Audio.SE}:{PlayerPrefs.GetFloat(Audio.SE)}");

    }

    [Button]
    public void DeleteData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log($"IsOneTime:{PlayerPrefs.GetInt("IsOnetimeInit")}");
    }
#endif
}
