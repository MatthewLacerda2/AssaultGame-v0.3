using UnityEngine;

public class AudioArray : MonoBehaviour {

    public string descricao;

    public AudioClip[] clips;

    public AudioClip GetRandomClip() {
        return GetRandomClipFrom(clips);
    }
    
    public static AudioClip GetRandomClipFrom(AudioClip[] array) {
        return array[Random.Range(0, array.Length - 1)];
    }
}