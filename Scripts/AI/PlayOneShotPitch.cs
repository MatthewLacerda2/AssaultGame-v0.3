using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayOneShotPitch : MonoBehaviour {

    AudioSource source;

    public AudioClip[] clips;

    void Awake() {
        source = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start() {
        source.clip = AudioArray.GetRandomClipFrom(clips);
        source.Play();

        Destroy(gameObject, source.clip.length);
    }

    void Update() {
        source.pitch = Time.timeScale;
    }
}