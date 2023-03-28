using UnityEngine;

public class ProbeSample {

    public Vector3 position;

    public bool isPlayerVisible;
    public bool isInPlayerFrustum;
    public float distToPlayer;

    public ProbeSample(Vector3 pos) {
        position = pos;
    }
}