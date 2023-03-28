using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CollisionSounds : MonoBehaviour {

    //public bool balancearOnAwake = false;
    [Space] public AudioClip[] array;

    AudioSource source;

    void Awake() {
        source = GetComponent<AudioSource>();
        /*
        if (balancearOnAwake) {
            lendaLib.RigidbodiesBalance(GetComponentsInChildren<Rigidbody>(), GetComponent<Rigidbody>().mass);
        }*/
    }

    void Update() {
        source.pitch = Time.timeScale;
    }

    void OnCollisionEnter(Collision collision) {

        if (source.enabled == false) {
            return;
        } else if (source.isPlaying && (source.time / source.clip.length < 0.5f)) {
            return;
        }

        source.clip = AudioArray.GetRandomClipFrom(array);
        source.Play();
    }
}