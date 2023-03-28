using UnityEngine;

public class Swing : MonoBehaviour {

    public float amount = 0.065f;
    public float maxAmount = 0.13f;
    public float smoothAmount = 6.0f;

    Vector3 initialPosition;

    float movementX;
    float movementY;
    
    void Start() {
        initialPosition = transform.localPosition;
    }

    void Update() {
        movementX = -Input.GetAxis("Mouse X") * amount;
        movementY = -Input.GetAxis("Mouse Y") * amount;

        movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);
        movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);

        Vector3 finalPosition = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }
}