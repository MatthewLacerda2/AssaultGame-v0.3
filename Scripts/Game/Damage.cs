using UnityEngine;
[System.Serializable]
public class Damage {

    public Collider collider;
    public GameObject source;
    public Vector3 direction;

    public float amount, impulso;

    public bool killed;

    public enumDamageType damageType;
    /*
    public Dano() {
        amount = 0;
        impulso = 0;
        source = Vector3.zero;
        direction = Vector3.zero;
        collider = null;
        killed = false;
        damageType = danoType.tiro;
    }
    */
    public Damage(float damage, float impulse, GameObject sauce, Vector3 dir, Collider col, enumDamageType damType) {
        amount = damage;
        impulso = impulse;
        source = sauce;
        direction = dir.normalized;
        collider = col;
        damageType = damType;
    }
}