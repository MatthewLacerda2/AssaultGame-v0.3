using UnityEngine;

public class BotWait : MonoBehaviour {

    public Bounds bounds;
    public Life protegido;
    public RuntimeAnimatorController anime;
    BotInterface botInterf;

    public float waitTimer;
    float timer;

    void Awake() {
        botInterf= GetComponent<BotInterface>();

        GetComponent<Life>().delegados += OnDamage;

        if (protegido != null) {
            protegido.delegados += OnDamage;
        }
    }

    void Update() {
        timer += Time.deltaTime;

        if (waitTimer > 0 && waitTimer < timer) {
            Attack("timer");
            return;
        }

        if (bounds.Contains(lendaLib.playerPos) || (botInterf.isInPlayerView && botInterf.timePlayerSpotted > 0)) {
            Attack("view player");
        }
    }

    void OnDamage(Damage damage) {
        if (damage.killed) {
            this.enabled = false;
            transform.root.GetComponent<Director>().RemoveWaiter(this);
            botInterf.RemoveInterfaceFromDirector();
            return;
        } else {
            Attack("damaged");
        }
    }

    public void Attack(string why) {

        if (this.enabled == false) {
            return;
        }

        if(anime!= null) {
            Animator animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = anime;
            animator.SetTrigger("trigger");

            Debug.LogError("faltou a coroutine pra devolver o animatorController");
        }

        //GetComponent<ShooterAI>().enabled = true;

        transform.root.GetComponent<Director>().RemoveWaiter(this);

        this.enabled = false;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawWireCube(bounds.center, bounds.extents);
    }
}