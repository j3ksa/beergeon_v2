using UnityEngine;

namespace Praktyki.EQ
{
    [RequireComponent(typeof(AudioSource))]
    public class MinecraftEQAudio : MonoBehaviour
    {
        public static MinecraftEQAudio Instance { get; private set; }

        [Header("Custom Audio Clips (Optional)")]
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip pickUpClip;
        [SerializeField] private AudioClip placeDownClip;
        [SerializeField] private AudioClip dropWorldClip;

        private AudioSource audioSource;
        private AudioClip proceduralClickClip;
        private AudioClip proceduralPopClip;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D UI sound
            }

            GenerateProceduralClips();
        }

        private void GenerateProceduralClips()
        {
            try
            {
                int sampleRate = 44100;
                float clickDuration = 0.05f;
                int clickSamples = Mathf.Max(256, Mathf.FloorToInt(sampleRate * clickDuration));
                float[] clickData = new float[clickSamples];
                for (int i = 0; i < clickSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Mathf.Exp(-t * 90f);
                    float wave = Mathf.Sin(2f * Mathf.PI * 1200f * t);
                    clickData[i] = wave * envelope * 0.45f;
                }
                proceduralClickClip = AudioClip.Create("Procedural_Click", clickSamples, 1, sampleRate, false);
                if (proceduralClickClip != null && clickData.Length > 0)
                {
                    proceduralClickClip.SetData(clickData, 0);
                }

                float popDuration = 0.08f;
                int popSamples = Mathf.Max(256, Mathf.FloorToInt(sampleRate * popDuration));
                float[] popData = new float[popSamples];
                for (int i = 0; i < popSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float freq = Mathf.Lerp(800f, 350f, t / popDuration);
                    float envelope = Mathf.Exp(-t * 50f);
                    float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                    popData[i] = wave * envelope * 0.45f;
                }
                proceduralPopClip = AudioClip.Create("Procedural_Pop", popSamples, 1, sampleRate, false);
                if (proceduralPopClip != null && popData.Length > 0)
                {
                    proceduralPopClip.SetData(popData, 0);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[MinecraftEQAudio] Pominięto generowanie dźwięków proceduralnych: " + ex.Message);
            }
        }

        public void PlayOpen()
        {
            if (openClip != null) PlayClip(openClip, 0.7f, 1.0f);
            else PlayClip(proceduralClickClip, 0.6f, 1.2f);
        }

        public void PlayClose()
        {
            if (closeClip != null) PlayClip(closeClip, 0.7f, 0.9f);
            else PlayClip(proceduralClickClip, 0.5f, 0.85f);
        }

        public void PlayPickUp()
        {
            if (pickUpClip != null) PlayClip(pickUpClip, 0.7f, 1.0f);
            else PlayClip(proceduralClickClip, 0.7f, Random.Range(1.1f, 1.25f));
        }

        public void PlayPlaceDown()
        {
            if (placeDownClip != null) PlayClip(placeDownClip, 0.7f, 1.0f);
            else PlayClip(proceduralPopClip, 0.7f, Random.Range(0.95f, 1.05f));
        }

        public void PlayDropWorld()
        {
            if (dropWorldClip != null) PlayClip(dropWorldClip, 0.8f, 1.0f);
            else PlayClip(proceduralPopClip, 0.8f, 0.8f);
        }

        private void PlayClip(AudioClip clip, float volume, float pitch)
        {
            if (audioSource == null || clip == null) return;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
