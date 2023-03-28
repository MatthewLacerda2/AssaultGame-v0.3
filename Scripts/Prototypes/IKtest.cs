using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(Life))]
public class IKtest : MonoBehaviour {

    public float recover;

    TwoBoneIKConstraint[] IKs;

    void Awake() {
        IKs = GetComponentInChildren<Rig>().GetComponentsInChildren<TwoBoneIKConstraint>();
        GetComponent<Life>().delegados += OnDamage;
    }

    void Update() {
        foreach(TwoBoneIKConstraint k in IKs) {
            k.transform.localPosition = k.data.root.transform.localPosition ;
        }
    }

    void OnDamage(Damage dam) {
        /*
        if (dam.killed) {
            GetComponent<Life>().delegados -= OnDamage;
            StopAllCoroutines();
            this.enabled = false;
            return;
        }*/

        int i = 0;
        foreach(TwoBoneIKConstraint k in IKs) {
            if (k.data.mid == dam.collider.transform) {
                StartCoroutine(HitBoneCoroutine(k, dam));
                return;
            }
            i++;
        }
    }

    System.Collections.IEnumerator HitBoneCoroutine(TwoBoneIKConstraint k, Damage damn) {

        Vector3 off = 0.0025f * damn.amount * damn.direction.normalized;
        
        if (off.magnitude <= 0.01f) {
            yield return null;
        }

        Debug.Log(off + ", " + off.magnitude);
        k.transform.localPosition += off;

        float x = recover;
        while (x > 0) {
            x -= Time.deltaTime;

            k.transform.localPosition -= (off/(recover*Time.deltaTime));            

            yield return null;
        }
    }
}