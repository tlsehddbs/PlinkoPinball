using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// 플링코 핀 충돌 시 사운드를 재생하는 로컬 반응
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlinkoPinSoundReaction : MonoBehaviour, IPlinkoPinReaction
    {
        [Header("Audio")]
        [SerializeField] private AudioClip[] clips;
        [SerializeField] private float volume = 1f;

        [Header("Pitch")]
        [SerializeField] private bool randomPitch = true;
        [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.1f);

        [Header("Spam Control")]
        [SerializeField] private float cooldown = 0.02f;

        private float _lastPlayTime = -999f;


        public void OnPinHit(Rigidbody ball, Vector3 hitPoint)
        {
            if (clips == null || clips.Length == 0)
            {
                return;
            }

            if (Time.time < _lastPlayTime + cooldown)
            {
                return;
            }

            _lastPlayTime = Time.time;

            AudioClip clip = clips[Random.Range(0, clips.Length)];

            GameObject tempAudio = new GameObject("PinSfx");
            tempAudio.transform.position = hitPoint;

            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 0f;

            if (randomPitch)
            {
                source.pitch = Random.Range(pitchRange.x, pitchRange.y);
            }

            source.Play();

            Destroy(tempAudio, clip.length + 0.1f);
        }
    }
}