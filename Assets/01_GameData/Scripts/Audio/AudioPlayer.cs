using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField, Required, BoxGroup("オーディオ")] private AudioSource _source;
    [SerializeField, Required, BoxGroup("オーディオ")] private AudioClip[] _clips;

    private GameObject _obj = null;

    private async void Start()
    {
        _obj = gameObject;

        foreach (var clip in _clips)
        {
            _source.PlayOneShot(clip);
        }

        await Helper.Tasks.Canceled(UniTask.WaitUntil(
            () => !_source.isPlaying, cancellationToken: destroyCancellationToken));

        if (_obj != null)
        {
            Destroy(gameObject);
        }
    }
}
