using UnityEngine;

namespace BalconyGarden
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("Audio Clips (Tùy chọn - Kéo file âm thanh của bạn vào đây)")]
        public AudioClip sfxWatering;
        public AudioClip sfxPlanting;
        public AudioClip sfxHarvest;
        public AudioClip sfxCoin;
        public AudioClip sfxLevelUp;
        public AudioClip sfxClick;
        public AudioClip sfxBreeding;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }
        }

        public void PlayClick()
        {
            PlaySfx(sfxClick, 600f, 0.05f);
        }

        public void PlayWatering()
        {
            PlaySfx(sfxWatering, 350f, 0.15f);
        }

        public void PlayPlanting()
        {
            PlaySfx(sfxPlanting, 420f, 0.1f);
        }

        public void PlayHarvest()
        {
            PlaySfx(sfxHarvest, 800f, 0.12f);
        }

        public void PlayCoin()
        {
            PlaySfx(sfxCoin, 950f, 0.15f);
        }

        public void PlayLevelUp()
        {
            PlaySfx(sfxLevelUp, 1200f, 0.35f);
        }

        public void PlayBreeding()
        {
            PlaySfx(sfxBreeding, 700f, 0.25f);
        }

        private void PlaySfx(AudioClip clip, float synthFreq, float duration)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
            else
            {
                // Âm thanh tổng hợp tự động nếu chưa gán AudioClip
                PlayProceduralTone(synthFreq, duration);
            }
        }

        private void PlayProceduralTone(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1.0f - ((float)i / sampleCount); // Fade out mượt mà
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * 0.25f;
            }

            AudioClip synthClip = AudioClip.Create("SynthTone", sampleCount, 1, sampleRate, false);
            synthClip.SetData(samples, 0);
            sfxSource.PlayOneShot(synthClip);
        }
    }
}
