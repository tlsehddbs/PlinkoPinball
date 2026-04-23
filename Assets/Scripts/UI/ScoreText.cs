using UnityEngine;
using TMPro;
using PlinkoPinball.Gameplay.Systems;

namespace PlinkoPinball.UI
{
    public sealed class ScoreText : MonoBehaviour
    {
        [Header("Bind")]
        [SerializeField] private ScoreSystem scoreSystem;

        [Header("TMP")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private TMP_Text bestScoreText;    // 굳이?

        private void Awake()
        {
            if (scoreSystem == null)
                scoreSystem = FindFirstObjectByType<ScoreSystem>();
        }

        private void OnEnable()
        {
            if (scoreSystem == null) return;

            scoreSystem.OnScoreChanged += HandleScoreChanged;
            scoreSystem.OnMultiplierChanged += HandleMultiplierChanged;
            // scoreSystem.OnBestScoreChanged += HandleBestScoreChanged;

            // 초기 세팅
            HandleScoreChanged(scoreSystem.CurrentScore);
            HandleMultiplierChanged(scoreSystem.Multiplier);
            // HandleBestScoreChanged(scoreSystem.BestScore);
        }

        private void OnDisable()
        {
            if (scoreSystem == null) return;

            scoreSystem.OnScoreChanged -= HandleScoreChanged;
            scoreSystem.OnMultiplierChanged -= HandleMultiplierChanged;
            // scoreSystem.OnBestScoreChanged -= HandleBestScoreChanged;
        }

        private void HandleScoreChanged(int score)
        {
            if (scoreText != null)
                scoreText.text = score.ToString("N0");
        }

        private void HandleMultiplierChanged(float mul)
        {
            if (multiplierText != null)
                multiplierText.text = $"x{mul:0.00}";
        }

        // private void HandleBestScoreChanged(int best)
        // {
        //     if (bestScoreText != null)
        //         bestScoreText.text = best.ToString("N0");
        // }
    }
}
