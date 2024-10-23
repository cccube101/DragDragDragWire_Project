/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FancyScrollView.Example03
{
    class StageFrame : FancyCell<ItemData, Context>
    {
        // ---------------------------- SerializeField
        [SerializeField] Animator animator = default;
        [SerializeField] Button button = default;

        [SerializeField] private TMP_Text _numberText;
        [SerializeField] private TMP_Text _stageText;
        [SerializeField] private GameObject[] _coinObj;
        [SerializeField] private GameObject[] _hpObj;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _topTimeText;

        [SerializeField] private AudioSource _audio;
        [SerializeField] private AudioClip _clip;

        // ---------------------------- Field

        // ---------------------------- Method
        static class AnimatorHash
        {
            public static readonly int Scroll = Animator.StringToHash("scroll");
        }

        void Start()
        {
            button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
        }

        public override void UpdateContent(ItemData itemData)
        {
            var selectStage = Index + 1;
            var sceneEnum = (SceneName)selectStage;
            var scoreList = Helper.Data.ScoreList[sceneEnum.ToString()];

            _numberText.text = selectStage.ToString();
            _stageText.text = sceneEnum.ToString();

            for (int i = 0; i < _coinObj.Length; i++)
            {
                _coinObj[i].SetActive(i < scoreList.Coin);
            }
            for (int i = 0; i < _hpObj.Length; i++)
            {
                _hpObj[i].SetActive(i < scoreList.HP);
            }
            _timeText.text = scoreList.Time.ToString("00.00");
            _scoreText.text = scoreList.Score.ToString();
            _topTimeText.text = Helper.Data.TopTimeList[sceneEnum.ToString()].ToString("0.00");

        }

        public override void UpdatePosition(float position)
        {
            if ((currentPosition < -0.1 || 0.1 < currentPosition)
                && -0.1 < position && position < 0.1
                && !_audio.isPlaying)
            {
                _audio.PlayOneShot(_clip);
            }

            currentPosition = position;

            if (animator.isActiveAndEnabled)
            {
                animator.Play(AnimatorHash.Scroll, -1, position);
            }

            animator.speed = 0;
        }

        // GameObject が非アクティブになると Animator がリセットされてしまうため
        // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
        float currentPosition = 0;

        void OnEnable() => UpdatePosition(currentPosition);



    }
}


