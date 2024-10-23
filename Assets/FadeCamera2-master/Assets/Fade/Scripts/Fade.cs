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
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class Fade : MonoBehaviour
{

    // ---------------------------- Field
    private static Fade _instance = null;
    private IFade fade;
    private float cutoutRange = 1;

    // ---------------------------- Property
    public static Fade Instance => _instance;



    // ---------------------------- UnityMessage
    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        fade = GetComponent<IFade>();
        fade.Range = cutoutRange;
    }

    private void OnValidate()
    {
        fade = GetComponent<IFade>();
    }

    // ---------------------------- PublicMethod
    /// <summary>
    /// フェードタスク
    /// </summary>
    /// <param name="from"></param>
    /// <param name="endValue"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public async UniTask FadeTask
        (float from
        , float endValue
        , float time
        , CancellationToken token)
    {
        //  フェード
        await DOVirtual.Float
            (from, endValue, time
            , (value) =>
           {
               fade.Range = value;
           })
           .SetEase(Ease.Linear)
           .SetUpdate(true)
           .SetLink(gameObject)
           .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken: token);

        //  初期化
        cutoutRange = endValue;
        fade.Range = endValue;
    }
}