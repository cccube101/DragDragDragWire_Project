/*
 The MIT License (MIT)

Copyright (c) 2013 yamamura tatsuhiko

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using Alchemy.Inspector;
using System;
using UnityEngine;

public class FadeImage : UnityEngine.UI.Graphic, IFade
{
    public static FadeImage Instance;
    // ---------------------------- Enum
    public enum ImageType
    {
        FADEOUT, FADEIN
    }

    // ---------------------------- SerializeField
    [SerializeField, Required, BoxGroup("パラメータ"), Range(0, 1)] private float _cutoutRange;

    [SerializeField, Required, BoxGroup("パラメータ/0:フェードアウト　1:フェードイン")]
    [ListViewSettings(ShowAddRemoveFooter = false, ShowBoundCollectionSize = false)]
    private Texture[] _fadeTexture = new Texture[Enum.GetValues(typeof(ImageType)).Length];



    // ---------------------------- Property
    public float Range
    {
        get
        {
            return _cutoutRange;
        }
        set
        {
            _cutoutRange = value;
            UpdateMaskCutout(_cutoutRange);
        }
    }



    // ---------------------------- UnityMessage
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    protected override void Start()
    {
        base.Start();
        UpdateMaskTexture(ImageType.FADEOUT);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        UpdateMaskCutout(Range);
        UpdateMaskTexture(ImageType.FADEOUT);
    }
#endif



    // ---------------------------- PublicMethod
    /// <summary>
    /// フェードテクスチャ更新
    /// </summary>
    /// <param name="type"></param>
    public void UpdateMaskTexture(ImageType type)
    {
        //  イメージ更新
        material.SetTexture("_MaskTex", _fadeTexture[(int)type]);

        //  色更新
        material.SetColor("_Color", color);
    }







    // ---------------------------- PrivateMethod
    /// <summary>
    /// フェードレンジ更新
    /// </summary>
    /// <param name="range"></param>
    private void UpdateMaskCutout(float range)
    {
        enabled = true;
        material.SetFloat("_Range", 1 - range);

        if (range <= 0)
        {
            enabled = false;
        }
    }
}
