using Alchemy.Inspector;
using Helper;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugController : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("デバッグ")] private Switch _GUI;


    // ---------------------------- Field
    private bool _isOpenMenu;
    private bool _isDeleteData;

    // ---------------------------- UnityMessage
    private void OnGUI()
    {
        if (_GUI == Switch.ON)
        {
            var pos = Base.LogParam.pos;
            var style = Base.LogParam.style;

            if (_isOpenMenu)
            {
                GUI.TextField(pos[1], $"Data Delete??? [Y or N]", style);
            }
            if (_isDeleteData)
            {
                GUI.TextField(pos[2], $"Data Delete", style);
            }
        }
    }

    private void Update()
    {
        var current = Keyboard.current;
        var key_del = current.deleteKey;
        var key_y = current.yKey;
        var key_n = current.nKey;

        if (key_del.wasPressedThisFrame)
        {
            _isOpenMenu = true;
        }

        if (_isOpenMenu)
        {
            if (key_y.wasPressedThisFrame)
            {
                Data.DeleteData();
                _isDeleteData = true;
            }
            else if (key_n.wasPressedThisFrame)
            {
                _isOpenMenu = false;
            }
        }

    }
    // ---------------------------- PublicMethod





    // ---------------------------- PrivateMethod





}
