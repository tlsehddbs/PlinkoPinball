using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    [DisallowMultipleComponent]
    public sealed class PlinkoPinFloatingTextReaction : MonoBehaviour, IPlinkoPinReaction
    {
        [Header("Prefab")]
        [SerializeField] private FloatingTextView floatingTextPrefab;

        [Header("References")]
        [SerializeField] private PlinkoPinRuntime runtime;

        [Header("Text")]
        [SerializeField] private string normalText = "+1";
        [SerializeField] private string bonusFormat = "+{0}";
        [SerializeField] private string multiplierFormat = "x{0:0.##}";
        [SerializeField] private string bonusMultiplierFormat = "+{0} x{1:0.##}";

        [Header("Position")]
        [SerializeField] private bool useHitPoint = true;
        [SerializeField] private Vector3 spawnOffset = new(0f, 0.35f, 0f);

        [Header("Spam Control")]
        [SerializeField, Min(0f)] private float cooldown = 0.03f;

        private float _lastShownTime = -999f;


        private void Awake()
        {
            if (runtime == null)
            {
                runtime = GetComponentInParent<PlinkoPinRuntime>();
            }
        }

        public void OnPinHit(Rigidbody ball, Vector3 hitPoint)
        {
            if (floatingTextPrefab == null || runtime == null)
            {
                return;
            }

            if (Time.time < _lastShownTime + cooldown)
            {
                return;
            }

            PlinkoPinModifierData data = runtime.ExportState();

            if (data.StateKind == PlinkoPinStateKind.Error)
            {
                return;
            }

            string displayText = BuildText(data);

            _lastShownTime = Time.time;

            Vector3 spawnPosition = useHitPoint ? hitPoint : transform.position;
            spawnPosition += spawnOffset;

            FloatingTextView view = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);
            view.Play(displayText, spawnPosition);
        }

        private string BuildText(in PlinkoPinModifierData data)
        {
            bool hasBonus = data.ValueBonus > 0;
            bool hasMultiplier = !Mathf.Approximately(data.Multiplier, 1f);

            if (hasBonus && hasMultiplier)
            {
                return string.Format(bonusMultiplierFormat, data.ValueBonus, data.Multiplier);
            }

            if (hasBonus)
            {
                return string.Format(bonusFormat, data.ValueBonus);
            }

            if (hasMultiplier)
            {
                return string.Format(multiplierFormat, data.Multiplier);
            }

            return normalText;
        }
    }
}