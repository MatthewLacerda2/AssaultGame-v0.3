using UnityEngine;
using UnityEngine.UI;

public class HitIndicator : MonoBehaviour {

    public AnimationCurve curva;
    public Sprite near, far;
    public float lifetime;

    Image image;
    Vector3 source;
    float timer;

    public void Setup(Vector3 origin) {
        image = GetComponentInChildren<Image>();
        source = origin;
    }

    void Update() {

        float dist = Vector3.Distance(lendaLib.playerPos, source);
        if (dist >= 4f) {
            image.sprite = far;
        } else {
            image.sprite = near;
        }

        Vector3 targetDir = source - lendaLib.playerPos;
        Vector3 forward = lendaLib.playerTransf.forward;
        float y = Vector3.SignedAngle(targetDir, forward, Vector3.up);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, y));

        Color collor = image.color;
        collor.a = curva.Evaluate(timer);
        image.color = collor;

        timer += Time.deltaTime;

        if (timer > lifetime || collor.a == 0) {
            Destroy(gameObject);
            return;
        }
    }
}