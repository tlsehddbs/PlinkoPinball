using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;

namespace PlinkoPinball.Gameplay.Components.Reactions
{
    [DisallowMultipleComponent]
    public sealed class PinballSoundReaction : MonoBehaviour, ITableEventReaction
    {
        [Header("Filter")]
        [SerializeField] private bool useEventTypeFilter;
        [SerializeField] private TableEventType eventType = TableEventType.Hit;
        [SerializeField] private string requiredEventId = "";
        [SerializeField] private string requiredTag = "";

        [Header("Audio")]
        [SerializeField] private AudioClip[] clips;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private bool playAtEventPosition = true;
        [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;

        [Header("Pitch")]
        [SerializeField] private bool randomPitch = true;
        [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.08f);

        [Header("Spam Control")]
        [SerializeField, Min(0f)] private float cooldownSeconds = 0.03f;

        private float _nextAllowedUnscaledTime;

        public void OnTableEvent(in TableEvent e)
        {
            if (!Matches(in e))
            {
                return;
            }

            if (clips == null || clips.Length == 0)
            {
                return;
            }

            if (cooldownSeconds > 0f && Time.unscaledTime < _nextAllowedUnscaledTime)
            {
                return;
            }

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null)
            {
                return;
            }

            _nextAllowedUnscaledTime = Time.unscaledTime + cooldownSeconds;

            Vector3 position = playAtEventPosition ? e.position : transform.position;
            GameObject tempAudio = new GameObject($"{name}_Sfx");
            tempAudio.transform.position = position;

            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = spatialBlend;

            if (randomPitch)
            {
                float min = Mathf.Min(pitchRange.x, pitchRange.y);
                float max = Mathf.Max(pitchRange.x, pitchRange.y);
                source.pitch = Mathf.Approximately(min, max) ? min : Random.Range(min, max);
            }

            source.Play();
            Destroy(tempAudio, clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch)) + 0.1f);
        }

        private bool Matches(in TableEvent e)
        {
            if (useEventTypeFilter && e.eventType != eventType)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(requiredEventId) && e.eventId != requiredEventId)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(requiredTag) && !e.HasTag(requiredTag))
            {
                return false;
            }

            return true;
        }
    }
}
