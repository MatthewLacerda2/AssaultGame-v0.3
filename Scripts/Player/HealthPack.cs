using UnityEngine;

public class HealthPack : MonoBehaviour {

    [SerializeField] bool locRot;
    [SerializeField] AudioClip clip;
    [SerializeField] float healthAmount;
    [SerializeField] float range;

    // Update is called once per frame
    void Update() {

        Vector3 pos = lendaLib.playerPos;
        pos.y = transform.position.y;

        float dist=Vector3.Distance(pos, transform.position);

        if (dist <= range) {
            GiveHealth();
            Destroy(gameObject);
            return;
        }
        /*
        if (locRot == false) {
            return;
        }
        */
        //flutuar, girar...
    }

    void GiveHealth() {
        lendaLib.playerTransf.GetComponent<Life>().AddDamage(new Damage(healthAmount, 0, gameObject, Vector3.zero, null, enumDamageType.logical));

        if(clip!= null) {
            AudioSource.PlayClipAtPoint(clip, transform.position, 1f);
        }
    }
}