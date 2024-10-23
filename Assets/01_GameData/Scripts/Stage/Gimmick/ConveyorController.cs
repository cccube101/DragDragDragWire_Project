using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.UIElements;

public class ConveyorController : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("パラメータ")] private float _addSpeed;

    // ---------------------------- Property
    public float GetSpeed() => _addSpeed;




#if UNITY_EDITOR
    // ---------------------------- SerializeField
    [Title("Transform からの scale 変更不可、以下 Inspector 上で編集")]
    [SerializeField, Required, BoxGroup("スケールパラメータ")] private Transform _navScale;
    [SerializeField, Required, BoxGroup("スケールパラメータ")] private Vector2 _size;

    // ---------------------------- UnityMessage
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += IsOnValidate;
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// インスペクター同機更新
    /// </summary>
    private void IsOnValidate()
    {
        UnityEditor.EditorApplication.delayCall -= IsOnValidate;
        if (this == null) return;
        GetComponent<SpriteRenderer>().size = _size;
        _navScale.localScale = _size;
    }
#endif
}
