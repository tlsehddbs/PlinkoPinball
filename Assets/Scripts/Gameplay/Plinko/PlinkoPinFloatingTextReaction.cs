using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    [DisallowMultipleComponent]
    public sealed class PlinkoPinFloatingTextReaction : MonoBehaviour, IPlinkoPinReaction
    {
        [Header("Prefab")]
        [SerializeField] private FloatingTextView floatingTextPrefab;

        [Header("Text")]
        [SerializeField] private string text = "+1";

        [Header("Position")]
        [SerializeField] private bool useHitPoint = true;
        [SerializeField] private Vector3 spawnOffset = new(0f, 0.35f, 0f);

        [Header("Spam Control")]
        [SerializeField, Min(0f)] private float cooldown = 0.03f;

        private float _lastShownTime = -999f;

        
        public void OnPinHit(Rigidbody ball, Vector3 hitPoint)
        {
            if (floatingTextPrefab == null)
            {
                return;
            }

            if (Time.time < _lastShownTime + cooldown)
            {
                return;
            }

            _lastShownTime = Time.time;

            Vector3 spawnPosition = useHitPoint ? hitPoint : transform.position;
            spawnPosition += spawnOffset;

            FloatingTextView view = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);
            view.Play(text, spawnPosition);
        }
    }
}