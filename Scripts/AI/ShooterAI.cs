using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioArray))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BotInterface))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CollisionSounds))]
[RequireComponent(typeof(Life))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PainMaster))]
[RequireComponent(typeof(ProbeAgent))]
[RequireComponent(typeof(Rigidbody))]
public class ShooterAI : MonoBehaviour {
    
    [SerializeField] AmmoDrop weapon;
    [SerializeField] AnimationCurve damageDropoff;
    [SerializeField] AudioClip weaponSound, meleeClip;
    [SerializeField] GameObject bulletInst, bulletFlyBy, bloodSplatter, decal;
    [SerializeField] GameObject bloodParty, metalParty, woodParty, hitParty;
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] Rigidbody capsula;
    [SerializeField] RuntimeAnimatorController ragdollAnime;
    [SerializeField] Transform capsuleSpawn;
    [SerializeField] Vector2 spread = new(2, 4);

    [SerializeField] float twitchThres = 1.0f, staggerThres = 2.0f, staggerDuration = 1f, rotationSpeed = 120.0f;
    [SerializeField] float meleeRange = 0.6f, meleeFirate = 0.83f, meleeDamage = 40f;
    [SerializeField] float firerate, impact;

    [HideInInspector] public float distPlayer, timeWait, timeViewing, range;

    public AnimationCurve reflexDropoff;

    public BotInterface botInterface {
        get;
        private set;
    }
    public float reactionReflex {
        get;
        private set;
    }

    public float fov;
    public bool isRagdollable = true;

    Animator animator;
    AudioArray walkClips;
    AudioSource source;
    CollisionSounds colSounds;
    Life life;
    PainMaster painMaster;
    ProbeAgent probeAgent;
    Rigidbody[] rigidbuddies;

    void Awake() {
        animator = GetComponent<Animator>();
        botInterface = GetComponent<BotInterface>();
        colSounds = GetComponent<CollisionSounds>();
        life = GetComponent<Life>();
        probeAgent = GetComponent<ProbeAgent>();
        source = GetComponent<AudioSource>();
        walkClips = GetComponent<AudioArray>();
        painMaster = GetComponent<PainMaster>();
        rigidbuddies = GetComponentsInChildren<Rigidbody>();

        life.delegados += OnDamage;
        range = lendaLib.GetLastKey(damageDropoff).time;
        //lendaLib.RigidbodiesBalance(rigidbuddies, GetComponent<Rigidbody>().mass);

        if (lendaLib.probeSystem != null) {
            if (transform.root != lendaLib.probeSystem.transform) {
                Debug.LogError("Bot fora do Director.transform");
            }
        }
    }

    public void TakeDueAction() {
        if (animator.enabled == false) {
            return;
        }

        enumBotState state = botInterface.state;

        switch (state) {
            case enumBotState.Aim:
                Aim();
                break;
            case enumBotState.Covering:
                Covering();
                break;
            case enumBotState.Flank:
                Flank();
                break;
            case enumBotState.Idle:
                Idle();
                break;
            case enumBotState.Melee:
                Melee();
                break;
            case enumBotState.Reposition:
                Reposition();
                break;
            case enumBotState.ShortestSight:
                ShortestSight();
                break;
            case enumBotState.Stagger:
                Stagger();
                break;
            case enumBotState.StraightPath:
                StraightPath();
                break;
        }
    }

    void LookAtPlayer(float rotSpeed) {
        Vector3 auxVec3 = lendaLib.playerPos;
        auxVec3.y = transform.position.y;

        Quaternion rotationAngle = Quaternion.LookRotation(auxVec3 - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotationAngle, rotSpeed * Time.deltaTime / 25.0f);
    }

    void Covering() {
        Aim();
    }

    void Aim() {
        Walking(false);

        LookAtPlayer(rotationSpeed);

        if (timeViewing >= timeWait + firerate) {
            Fire();
        }
    }

    void Fire() {

        timeViewing = timeWait;
        botInterface.timePlayerSpotted = timeViewing;

        float ranX = Random.Range(-spread.x, spread.x);
        float ranY = Random.Range(-spread.y, spread.y);

        Transform tracer = botInterface.tracer;

        tracer.LookAt(lendaLib.playerPos + (ProbeSystem.up/2));
        tracer.Rotate(ranX, ranY, 0, Space.Self);

        if (bulletInst) {
            Instantiate(bulletInst, tracer.position, tracer.rotation);
        }

        if (Physics.Raycast(tracer.position, tracer.forward, out RaycastHit rayHit, range)) {
            
            Decal instDecal = Instantiate(decal, rayHit.point, Quaternion.Euler(rayHit.normal)).GetComponent<Decal>();

            float damage = damageDropoff.Evaluate(rayHit.distance);

            Rigidbody rigidbuddy = rayHit.transform.GetComponentInChildren<Rigidbody>();
            if (rigidbuddy != null) {
                rigidbuddy.AddForce(damage * impact * tracer.forward, ForceMode.Impulse);
            }

            Life lie = rayHit.transform.GetComponentInParent<Life>();
            if (lie != null) {

                if (lie.transform.root == lendaLib.playerTransf && botInterface.state == enumBotState.Covering && Director.numShooters > 1) {
                    damage = 0f;
                }

                if (lie.transform.name.Contains("head")) {
                    damage *= 2;
                } else if (lie.transform.name.Contains("arm") || lie.transform.name.Contains("leg")) {
                    damage *= 0.75f;
                }


                lie.AddDamage(damage, damage * impact, gameObject, tracer.forward, rayHit.collider, enumDamageType.scan);

                if (lie.transform.root != lendaLib.playerTransf) {
                    Instantiate(bloodSplatter, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                    Instantiate(bloodParty, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                }

                instDecal.Setup(enumPhysicMaterial.Flash);
            } else {
                instDecal.Setup(rayHit.collider.material.name);

                if (rayHit.collider.material.name.Contains("metal")) {
                    Instantiate(metalParty, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                } else if (rayHit.collider.material.name.Contains("wood")) {
                    Instantiate(woodParty, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                } else {
                    Instantiate(hitParty, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                }

                //float randomY = Random.Range(0f, 360f);
                //instDecal.transform.Rotate(0, randomY, 0, Space.Self);
            }

        } else {
            FlyBy();
        }

        if (capsula != null) {
            Rigidbody rigidbuddy = Instantiate(capsula, capsuleSpawn.position, capsuleSpawn.rotation);
            rigidbuddy.AddForce(rigidbuddy.transform.forward * 5, ForceMode.Impulse);
        }

        if (muzzleFlash != null) {
            muzzleFlash.Play(true);
        }

        if (source.clip != weaponSound) {
            source.clip = weaponSound;
        }
        source.Play();

        animator.SetTrigger("shoot");
    }

    void FlyBy() {
        if (bulletFlyBy == null) {
            return;
        }

        Transform tracer = botInterface.tracer;
        Vector3 point = lendaLib.NearestPointInLine(tracer.position, tracer.forward, lendaLib.cameraPos);

        float distLinePlayer = Vector3.Distance(lendaLib.cameraPos, point);
        float distTracerPlayer = Vector3.Distance(lendaLib.cameraPos, tracer.position);

        if (distLinePlayer <= 2.5f && distTracerPlayer > 5f) {
            GameObject go = Instantiate(bulletFlyBy, point, Quaternion.identity);

            AudioSource source = go.GetComponent<AudioSource>();
            source.clip = go.GetComponent<AudioArray>().GetRandomClip();
            source.Play();
        }
    }

    void Flank() {
        Walking(true);
        probeAgent.Flankear();
    }

    void Walking(bool walk) {
        probeAgent.SetWalk(walk);

        if (walk) {
            if (!source.isPlaying) {
                source.clip = walkClips.GetRandomClip();
                source.Play();
            }
        } else {
            if (timeViewing <= reactionReflex) {
                LookAtPlayer(rotationSpeed);
            }
        }
    }

    void Idle() {
        Walking(false);
    }

    void Melee() {

        if (distPlayer > meleeRange) {
            probeAgent.ShortestSight();
            return;
        }

        LookAtPlayer(rotationSpeed * 2);

        if (timeViewing < meleeFirate) {
            return;
        }

        Vector3 pos = weapon.transform.position;
        Vector3 dir = lendaLib.cameraPos - pos;

        if (meleeClip != null) {
            source.clip = meleeClip;
            source.Play();
        }

        if (distPlayer <= meleeRange) {
            lendaLib.playerTransf.GetComponent<Life>().AddDamage(meleeDamage, meleeDamage / 2.5f, gameObject, dir, null, enumDamageType.physical);
        }

        botInterface.SetState(enumBotState.Melee, staggerDuration);

        timeViewing = timeWait;
        botInterface.timePlayerSpotted = timeViewing;
    }

    void Reposition() {
        Debug.LogError("Funcao nao implementada. Falling back to ShortestSight");
        ShortestSight();
    }

    void ShortestSight() {
        Walking(true);
        probeAgent.ShortestSight();
    }

    void StraightPath() {
        Walking(true);
        probeAgent.SetDestination(lendaLib.playerPos);
    }

    void Stagger() {
        Walking(false);
        botInterface.SetState(enumBotState.Stagger, 0.666f);
        timeViewing = timeWait - 1.3f;
        botInterface.timePlayerSpotted = timeViewing;
    }

    void OnDamage(Damage damage) {

        if (damage.killed) {
            Death(damage);
            return;
        }

        if (damage.damageType == enumDamageType.physical && isRagdollable) {
            Debug.Log(damage.amount + "," + staggerThres / 2);
            if (damage.amount >= staggerThres / 2) {
                Ragdoll();
                return;
            }
        }

        if (damage.amount < twitchThres) {
            return;
        }

        if (damage.amount >= staggerThres) {
            Stagger();
            //this is a subterfugio, o certo é usar o PainMaster
            animator.Play("Staggerando", 0);
        }

        painMaster.Twitch(damage);
    }

    void Death(Damage damage) {

        bool animatedDeath = animator.enabled && painMaster.AnimateDeath(damage);

        if (animatedDeath) {
            StartCoroutine(DeathRagdollCoroutine(damage));
        } else {
            animator.enabled = false;
            colSounds.enabled = true;
            DeathRagdoll(damage);
        }

        weapon.transform.SetParent(null);
        weapon.enabled = true;

        this.enabled = false;
        probeAgent.enabled = false;

        botInterface.state = enumBotState.Idle;
        botInterface.RemoveInterfaceFromDirector();
    }

    System.Collections.IEnumerator DeathRagdollCoroutine(Damage damage) {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        source.enabled = false;
        damage.collider = null;
        DeathRagdoll(damage);
    }

    void DeathRagdoll(Damage damage) {

        foreach (Rigidbody budy in rigidbuddies) {
            budy.isKinematic = false;
        }

        if (damage.collider != null) {

            Rigidbody body = damage.collider.transform.GetComponentInParent<Rigidbody>();
            Vector3 forca = damage.direction.normalized * damage.impulso;

            //forca= lendaLib.Vector3ClampMagnitude(forca, 0, 30);

            body.AddForce(forca, ForceMode.Impulse);
        }
    }

    public void Ragdoll() {
        if (ragdollAnime == null) {
            return;
        }

        StartCoroutine(RagdollCoroutine());
    }

    System.Collections.IEnumerator RagdollCoroutine() {

        botInterface.state = enumBotState.Idle;

        colSounds.enabled = true;
        foreach (Rigidbody budy in rigidbuddies) {
            budy.isKinematic = false;
        }
        animator.enabled = false;

        yield return new WaitForSeconds(3f);

        RuntimeAnimatorController aux = animator.runtimeAnimatorController;
        animator.runtimeAnimatorController = ragdollAnime;

        colSounds.enabled = false;
        foreach (Rigidbody budy in rigidbuddies) {
            budy.isKinematic = true;
        }
        animator.enabled = true;

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 0.3f);

        animator.runtimeAnimatorController = aux;
    }
}