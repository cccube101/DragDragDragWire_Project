using UnityEngine;

public class GimmickBase : MonoBehaviour
{
    // ---------------------------- SerializeField


    // ---------------------------- Field
    protected GameObject _obj = null;
    protected Transform _tr = null;


    // ---------------------------- UnityMessage
    public virtual void Awake()
    {
        Implement();
    }



    // ---------------------------- PublicMethod
    /// <summary>
    /// キャッシュ埋込
    /// </summary>
    public void Implement()
    {
        _obj = gameObject;
        _tr = transform;
    }




    // ---------------------------- PrivateMethod





}
