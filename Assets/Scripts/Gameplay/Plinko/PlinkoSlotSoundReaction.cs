using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// 플링코 슬롯에 공이 도착했을 때 사운드를 재생하는 로컬 반응
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlinkoSlotSoundReaction : MonoBehaviour, IPlinkoSlotReaction
    {
        [Header("Audio")]
        [SerializeField] private AudioClip[] clips;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;

        [Header("Pitch")]
        [SerializeField] private bool randomPitch = true;
        [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.1f);

        [Header("Spam Control")]
        [SerializeField, Min(0f)] private float cooldown = 0.02f;

        private float _lastPlayTime = -999f;

        public void OnSlotResolved(
            PlinkoBallActor ball,
            Vector3 hitPoint,
            PlinkoSlotRuntime runtime,
            in PlinkoSlotModifierData modifier,
            int finalReward)
        {
            if (clips == null || clips.Length == 0)
            {
                return;
            }

            if (Time.time < _lastPlayTime + cooldown)
            {
                return;
            }

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null)
            {
                return;
            }

            _lastPlayTime = Time.time;

            float pitch = 1f;

            if (randomPitch)
            {
                float min = Mathf.Min(pitchRange.x, pitchRange.y);
                float max = Mathf.Max(pitchRange.x, pitchRange.y);
                pitch = Mathf.Approximately(min, max) ? min : Random.Range(min, max);
            }

            PlinkoAudioPool.Play(clip, hitPoint, volume, spatialBlend, pitch);
        }
    }
}
