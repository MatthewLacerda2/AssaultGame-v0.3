using UnityEngine;

public class PainMaster : MonoBehaviour {

    public RuntimeAnimatorController deathAnimes;

    Animator animator;

    float magni;

    void Awake() {
        animator = GetComponent<Animator>();
    }

    void Update() {
        magni = animator.GetFloat("magni");
        magni -= Time.deltaTime * 20;
    }

    public bool Twitch(Damage damage) {
        if (this.enabled == false) {
            //Debug.Log("Quem me chama?!");
            return false;
        }

        string sourceName = damage.collider.name;

        if (sourceName.Contains("Arm") || sourceName.Contains("Hand")) {
            animator.SetTrigger("Arm");

        } else if (sourceName.Contains("Leg") || sourceName.Contains("Foot")) {
            animator.SetTrigger("Leg");

        } else if (sourceName.Contains("Head") || sourceName.Contains("Neck")) {
            animator.SetTrigger("Head");

        } else {
            animator.SetTrigger("Body");
        }

        if (sourceName.Contains("Left")) {
            animator.SetTrigger("Left");
        }

        if (sourceName.Contains("Right")) {
            animator.SetTrigger("Right");
        }

        if (magni < damage.impulso) {
            magni = damage.impulso;
            animator.SetFloat("magni", magni);
        }

        return true;
    }

    public bool AnimateDeath(Damage damage) {
        if (this.enabled == false || deathAnimes == null) {
            animator.enabled = false;
            this.enabled = false;
            return false;
        }
        /*
        Vector3 auxVec3 = lendaLib.playerPos;
        auxVec3.y = transform.position.y;
        transform.rotation = Quaternion.LookRotation(auxVec3 - transform.position);
        */
        animator.runtimeAnimatorController = deathAnimes;

        return Twitch(damage);
    }
}