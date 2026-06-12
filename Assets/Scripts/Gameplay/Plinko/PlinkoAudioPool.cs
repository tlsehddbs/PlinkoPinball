using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    [DisallowMultipleComponent]
    public sealed class PlinkoAudioPool : MonoBehaviour
    {
        private static PlinkoAudioPool instance;

        [SerializeField, Min(1)] private int maxPoolSize = 64;

        private readonly Queue<AudioSource> pool = new();

        public static void Play(
            AudioClip clip,
            Vector3 position,
            float volume,
            float spatialBlend,
            float pitch)
        {
            if (clip == null)
            {
                return;
            }

            PlinkoAudioPool audioPool = ResolveInstance();
            audioPool.PlayInternal(clip, position, volume, spatialBlend, pitch);
        }

        private static PlinkoAudioPool ResolveInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindFirstObjectByType<PlinkoAudioPool>();
            if (instance != null)
            {
                return instance;
            }

            GameObject poolObject = new GameObject("PlinkoAudioPool");
            instance = poolObject.AddComponent<PlinkoAudioPool>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void PlayInternal(AudioClip clip, Vector3 position, float volume, float spatialBlend, float pitch)
        {
            AudioSource source = GetSource();
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = spatialBlend;
            source.pitch = pitch;
            source.gameObject.SetActive(true);
            source.Play();

            float duration = clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch)) + 0.1f;
            StartCoroutine(ReleaseAfter(source, duration));
        }

        private AudioSource GetSource()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            GameObject sourceObject = new GameObject("PooledPlinkoSfx");
            sourceObject.transform.SetParent(transform, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sourceObject.SetActive(false);
            return source;
        }

        private IEnumerator ReleaseAfter(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (source == null)
            {
                yield break;
            }

            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            source.transform.SetParent(transform, false);

            if (pool.Count >= maxPoolSize)
            {
                Destroy(source.gameObject);
                yield break;
            }

            pool.Enqueue(source);
        }
    }
}
