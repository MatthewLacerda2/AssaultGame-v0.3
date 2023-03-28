using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class testingsampling : MonoBehaviour {

    public int H, V;

    Light myLight;

    const int bounces = 3;

    void Awake() {
        myLight = GetComponent<Light>();
    }

    void Update() {
        ComputeLight();
    }

    void ComputeLight() {
        float stepsAzimuthal = H;
        float stepsPolar = V;

        for (int i = 0; i < stepsAzimuthal; i++) {
            float phi = (360 / stepsAzimuthal) * i * Mathf.Deg2Rad;
            for (int j = 0; j < stepsPolar; j++) {
                float theta = (180 / (stepsPolar - 1)) * j * Mathf.Deg2Rad;

                float x = Mathf.Sin(theta) * Mathf.Cos(phi);
                float y = Mathf.Cos(theta);
                float z = Mathf.Sin(theta) * Mathf.Sin(phi);
                Vector3 direction = new Vector3(x, y, z);
                direction.Normalize();

                RaycastHit[] hits;
                bool hasHit = lendaLib.RaycastReflect(transform.position, direction, out hits, myLight.range, bounces);

                for(int a = 1; a < hits.Length - 1; a++) {
                    RaycastHit raio;
                    bool sombra = Physics.Raycast(hits[a].point, hits[a].point - transform.position, out raio, myLight.range);

                    if (sombra) {
                        Debug.DrawLine(hits[a].point, raio.point, Color.grey, 0);
                    }
                }
            }
        }
    }
}