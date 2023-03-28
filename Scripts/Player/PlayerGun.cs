using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]
public class PlayerGun : MonoBehaviour {
    
    [HideInInspector] public GunInterface gunInterf;
    [SerializeField] AnimationCurve damageDropoff;
    [SerializeField] AudioClip shot, empty;
    [SerializeField] GameObject decal, bloodSpray;
    [SerializeField] GameObject metalParty, woodParty, hitParty;
    [SerializeField] ParticleSystem partySys;
    [SerializeField] Vector2 recoil;
    [SerializeField] float recoilSnap, recoilRecover;
    
    [SerializeField] float firerate;
    [SerializeField] float impacto, enableTime;
    [SerializeField] float tracesPerShot;
    [SerializeField] bool isTapFire, infiniteAmmo;
    
    //FALTOU IMPLEMENTAR DELEGADOS E SPREAD
    public GameObject crosshair;
    public Vector2 spread;
    public Vector3 offsetPos;

    float timer, firerateTimer;

    AudioSource source;
    Animator anime;
    PlayerSystem playerSystem;

    // Start is called before the first frame update
    void Awake() {
        anime = GetComponent<Animator>();
        source=GetComponent<AudioSource>();
        gunInterf=GetComponent<GunInterface>();
        playerSystem = GetComponentInParent<PlayerSystem>();

        if (partySys == null) {
            partySys = GetComponentInChildren<ParticleSystem>();
        }
    }

    // Update is called once per frame
    void Update() {
        //anime.SetBool("walking", lendaLib.playerSpeed.magnitude >= 0.5f);

        source.pitch = Time.timeScale;

        if (Time.timeScale == 0) {
            return;
        }

        timer += Time.deltaTime;
        if (timer < enableTime) {
            return;
        }

        firerateTimer+= Time.deltaTime;

        bool shouldShoot = Input.GetKeyDown(KeyCode.Mouse0) && isTapFire;
        shouldShoot = shouldShoot || (Input.GetKey(KeyCode.Mouse0) && !isTapFire);

        if (shouldShoot && firerate <= firerateTimer) {

            Fire();
        }
    }

    void Fire() {

        if (infiniteAmmo == false && !gunInterf.TakeBullet()) {
            source.clip = empty;
            source.Play();
            return;
        }

        for (int i = 0; i < tracesPerShot; i++) {
            Trace();
        }

        firerateTimer = 0;

        if(source.clip!=shot) {
            source.clip = shot;
        }
        source.Play();

        if (partySys!=null) {
            partySys.Play();
        }

        anime.SetTrigger("shoot");
        playerSystem.AddRecoil(recoil, recoilSnap, recoilRecover);
    }

    void Trace() {
        Vector3 cameraDir = lendaLib.cameraDirection;
        float range = lendaLib.GetLastKey(damageDropoff).time;
        
        if (Physics.Raycast(lendaLib.cameraPos + (cameraDir * 0.5f), cameraDir, out RaycastHit rayHit, range)) {

            float damage = damageDropoff.Evaluate(rayHit.distance);
            string colliderMaterialName = rayHit.collider.material.name;

            Rigidbody rigidbuddy = rayHit.transform.GetComponentInChildren<Rigidbody>();
            if (rigidbuddy != null) {
                rigidbuddy.AddForce(damage * impacto / tracesPerShot * lendaLib.cameraDirection, ForceMode.Impulse);
            }

            Life lie = rayHit.transform.GetComponentInParent<Life>();
            if (lie != null) {
                if (lie.transform.name.Contains("head")) {
                    damage *= 2;
                }else if (lie.transform.name.Contains("leg") || lie.transform.name.Contains("arm")) {
                    damage *= 0.75f;
                }
                damage /= tracesPerShot;

                lie.AddDamage(damage, damage*impacto, transform.root.gameObject, lendaLib.cameraDirection, rayHit.collider, enumDamageType.scan);

                if (lie.transform.root != lendaLib.playerTransf) {
                    Instantiate(bloodSpray, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                }
            } else {

                //float randomY = Random.Range(0f, 360f);
                //instDecal.transform.Rotate(0, randomY, 0, Space.Self);
                Decal instDecal = Instantiate(decal, rayHit.point, Quaternion.LookRotation(-rayHit.normal)).GetComponent<Decal>();
                instDecal.Setup(colliderMaterialName);
                //instDecal.transform.localScale.Scale(new Vector3(decalScale / 10f, decalScale / 10f, decalScale / 10f));

                if (colliderMaterialName.Contains("metal", System.StringComparison.OrdinalIgnoreCase)) {
                    Instantiate(metalParty, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                } else if (colliderMaterialName.Contains("wood", System.StringComparison.OrdinalIgnoreCase)) {
                    Instantiate(woodParty, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                } else {
                    Instantiate(hitParty, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                }
            }
        }

        if (gunInterf.ammo.numBullets == 0) {
            AutoSwitch(0.6f);
        }
    }

    public void AutoSwitch(float delay) {
        StartCoroutine(AutoSwitchCoroutine(delay));
    }

    System.Collections.IEnumerator AutoSwitchCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
        WeaponsHolder wph = lendaLib.playerTransf.GetComponentInChildren<WeaponsHolder>();
        wph.Switch(wph.lastIndex);
    }

    void OnDisable() {
        DisableUI();
        timer = 0;
    }

    void OnEnable() {
        EnableUI();
    }

    void EnableUI() {

    }

    void DisableUI() {

    }
}