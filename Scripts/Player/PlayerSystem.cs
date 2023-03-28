using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerSystem : MonoBehaviour {

    [SerializeField] HitIndicator hitIndicatorPrefab;
    [SerializeField] GameObject canvasPrefab, weaponsCanvasPrefab;
    [SerializeField] float hitSnap, hitRecover;

    [SerializeField] AudioClip meleeClip;
    [SerializeField] Animator playerBodyAnime;
    [SerializeField] float meleeFirerate, meleeDamage;

    Animator cameraAnime;
    FirstPersonController fpsControl;
    Transform recoilTarget;
    SphereCollider footSphere;

    //UI-stuff-only
    Life playerLife;
    TextMeshProUGUI cronometroTextM, lifeTextM;
    Slider bulletTimeSlider;
    GunCanvas[] gunCanvas;
    Image bloodVignette;

    bool canMelee = true;

    void Awake() {

        fpsControl = GetComponent<FirstPersonController>();
        playerLife = GetComponent<Life>();

        footSphere = playerBodyAnime.GetComponentInChildren<SphereCollider>();

        recoilTarget = Camera.main.transform;

        GetComponent<Life>().delegados += ApplyDamage;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraAnime = Camera.main.transform.parent.GetComponent<Animator>();
        footSphere.enabled = false;
    }

    void Start() {
        Time.timeScale = 1f;
        bulletTimeCounter = bulletTimeDuration;

        SetupCanvas();
    }

    void SetupCanvas() {
        GameObject canvasGo = Instantiate(canvasPrefab);

        lifeTextM=canvasGo.transform.Find("lifeTextM").GetComponent<TextMeshProUGUI>();
        bulletTimeSlider=canvasGo.transform.Find("bulletTimeSlider").GetComponent<Slider>();
        cronometroTextM = canvasGo.transform.Find("cronometroTextM").GetComponent<TextMeshProUGUI>();
        bloodVignette = canvasGo.transform.Find("BloodVignette").GetComponent<Image>();

        List<GunCanvas> gCanvaslist= new();
        Transform weaponsTransf = canvasGo.transform.Find("weaponsTransf");
        Vector3 offset = new(70, 0, 0);
        int i = 0;
        foreach (PlayerGun pg in lendaLib.playerTransf.GetComponentsInChildren<PlayerGun>(true)) {
            GunInterface gInterf = pg.GetComponent<GunInterface>();
            GameObject go = Instantiate(weaponsCanvasPrefab, weaponsTransf.position, Quaternion.identity, weaponsTransf);
            go.GetComponent<RectTransform>().localPosition = (offset * i);
            gCanvaslist.Add(new GunCanvas(gInterf, gInterf.ammo.icon, go.GetComponentInChildren<TextMeshProUGUI>()));
            i++;
        }
        gunCanvas = gCanvaslist.ToArray();
    }

    // Update is called once per frame
    void Update() {

        ManageCanvas();

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Pause();
            return;
        }

        if (Time.timeScale == 0) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.T) && playerLife.vida>0) {
            Restart(0);
            return;
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            if (Time.timeScale == 1 && bulletTimeCounter >= bulletTimeCooldown) {
                ActivateBulletTime();
            } else if (Time.timeScale > 0) {
                Time.timeScale = 1;
                bulletTimeCounter=Mathf.Clamp(bulletTimeCounter, 0, bulletTimeCooldown);
            }
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            Melee();
        }
    }

    [SerializeField] float bulletTimeDuration, bulletTimeCooldown;
    readonly float bulletTimeScale = 0.5f;
    float bulletTimeCounter;
    void ManageBulletTime() {
        
        if (Time.timeScale == 0f) {
            return;
        }else if (Time.timeScale == 1f) {
            bulletTimeCounter += Time.deltaTime;
        } else {
            bulletTimeCounter -= Time.unscaledDeltaTime;

            if (bulletTimeCounter <= 0f) {
                Time.timeScale = 1f;
            }
        }

        bulletTimeCounter = Mathf.Clamp(bulletTimeCounter, 0, bulletTimeDuration);

    }

    void ActivateBulletTime() {
        if (Time.timeScale == 1f) {
            Time.timeScale = bulletTimeScale;
        } else {
            Time.timeScale = 1f;

            bulletTimeCounter = Mathf.Clamp(bulletTimeCounter, 0, bulletTimeCooldown);
        }
    }

    void ManageCanvas() {

        ManageBulletTime();

        int aux = Mathf.Clamp((int)playerLife.vida, 0, 100);

        lifeTextM.text = "+" + aux.ToString();
        bulletTimeSlider.value = bulletTimeCounter;
        cronometroTextM.text = Time.timeSinceLevelLoad.ToString("n3");

        foreach(GunCanvas gc in gunCanvas) {
            gc.textMesh.text = gc.gInterf.ammo.numBullets.ToString();
        }

        Color color = bloodVignette.color;
        if (playerLife.vida > 100f / 3f) {
            color.a = 0;
        } else if(playerLife.vida<=0){
            color.a = 1f;
        } else {
            float x = Mathf.Sin(Time.timeSinceLevelLoad * 2.5f);
            x = Mathf.Abs(x);
            x *= 60f;

            color.a = (180f + x) / 255f;
        }
        bloodVignette.color = color;
    }

    void Melee() {
        if (!canMelee) {
            return;
        }

        StartCoroutine(MeleeCoroutine());
    }

    System.Collections.IEnumerator MeleeCoroutine() {

        footSphere.enabled = true;
        canMelee = false;
        cameraAnime.Play("Kicking");
        playerBodyAnime.Play("Kicking");
        AudioSource.PlayClipAtPoint(meleeClip, lendaLib.cameraPos);

        yield return new WaitForSeconds(meleeFirerate);

        foreach(Collider col in Physics.OverlapSphere(footSphere.transform.position, 0.3f)) {
            Life lie=col.GetComponent<Life>();
            if (lie != null) {
                lie.AddDamage(meleeDamage, meleeDamage / 2f, transform.root.gameObject, lendaLib.cameraDirection, col, enumDamageType.physical);
            }
        }/*

        Vector3 cameraDir = lendaLib.cameraDirection;
        if (Physics.Raycast(lendaLib.cameraPos + (cameraDir ), cameraDir, out RaycastHit rayHit, 1.8f)) {

            Rigidbody rigidbuddy = rayHit.transform.GetComponentInChildren<Rigidbody>();
            if (rigidbuddy != null) {
                rigidbuddy.AddForce(meleeDamage * lendaLib.cameraDirection, ForceMode.Impulse);
            }

            Life lie = rayHit.transform.GetComponentInParent<Life>();
            if (lie != null) {
                lie.AddDamage(meleeDamage, meleeDamage/2f, transform.root.gameObject, lendaLib.cameraDirection, rayHit.collider, enumDamageType.physical);
            }
        }*/

        canMelee = true;
        footSphere.enabled = false;
    }

    float escala = 1;
    void Pause() {
        if (Time.timeScale == 0) {

            Time.timeScale= escala;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            fpsControl.enabled = true;

        } else {

            escala = Time.timeScale;
            Time.timeScale = 0;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            fpsControl.enabled = false;
        }
    }

    void Restart(float delay) {

        if (delay == 0) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        StartCoroutine(RestartCoroutine(delay));
    }

    System.Collections.IEnumerator RestartCoroutine(float delay) {
        while (delay >= 0) {
            delay -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.T)) {
                Restart(0);
                break;
            }

            yield return null;
        }

        Restart(0);
    }

    public void PlayerWin() {
        /*
        DamageRatio[] ratios = new DamageRatio[5];
        int i = 0;

        foreach (DamageRatio rato in ratios) {
            rato.ratio = 0;
            rato.type = (enumDamageType)i;
            i++;
        }

        playerLife.ratios= ratios;
        */
        //win pop-up

        StartCoroutine(PlayerWinCoroutine());
    }

    System.Collections.IEnumerator PlayerWinCoroutine() {
        Time.timeScale = 0.333f;
        this.enabled = false;

        float x = 0f;
        while (x < 1) {
            x += Time.unscaledDeltaTime;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) {
                break;
            }
            //faltou keycode.ESC
            yield return null;
        }

        Time.timeScale = 0;
        fpsControl.enabled = false;

        bloodVignette.transform.parent.Find("crosshair").gameObject.SetActive(false);

        while (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter) && !Input.GetKeyDown(KeyCode.Space)) {
            yield return null;
        }

        DecalsMaster.Resetar();

        Director d=lendaLib.probeSystem.transform.GetComponent<Director>();
        int index = d.nextLevelIndex;
        if (index == -1) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
        } else {
            SceneManager.LoadScene(index);
        }
    }

    void ApplyDamage(Damage damage) {
        if (playerLife.vida <= 0f) {
            PlayerDeath();
            return;
        }

        Transform canvas = bloodVignette.transform.root;
        HitIndicator auxHitIndic = Instantiate(hitIndicatorPrefab, canvas.position, Quaternion.identity, canvas);
        auxHitIndic.Setup(damage.source.transform.position);

        float aux = Mathf.Clamp(damage.impulso / 10f, 0.5f, 15f);
        AddRecoil(aux, 0, hitSnap, hitRecover);
    }

    void PlayerDeath() {
        Time.timeScale = 1;

        GetComponentInChildren<WeaponsHolder>().gameObject.SetActive(false);

        playerBodyAnime.gameObject.SetActive(false);
        bloodVignette.transform.parent.Find("crosshair").gameObject.SetActive(false);
        fpsControl.enabled = false;
        cameraAnime.Play("Death");
        transform.Find("Capsule").GetComponent<MeshRenderer>().enabled = false;

        Restart(0.9f);
    }

    public void AddRecoil(Vector2 recoil, float snap, float speed) {
        AddRecoil(recoil.x, recoil.y, snap, speed);
    }

    public void AddRecoil(float x, float y, float snap, float speed) {
        StartCoroutine(RecoilCoroutine(x, y, snap, speed));
    }

    System.Collections.IEnumerator RecoilCoroutine(float x, float y, float snap, float recoverTime) {

        //x = Mathf.Clamp(x, 0f, 60f);
        //y = Mathf.Clamp(y, 0f, 30f);
        float timer = 0;
        y = Random.Range(-y, y);
        Vector3 targRot = Vector3.Lerp(new Vector3(x, y, 0), Vector3.zero, recoverTime * Time.deltaTime);
        Vector3 currRot = Vector3.zero;

        const float thres = 0.01f;

        while (timer < recoverTime || targRot.magnitude <= thres || currRot.magnitude <= thres) {
            Vector3 lastRot = currRot;

            targRot = Vector3.Lerp(targRot, Vector3.zero, recoverTime * Time.deltaTime);
            currRot = Vector3.Slerp(currRot, targRot, snap * Time.fixedDeltaTime);
            //currentRot.z = 0;
            recoilTarget.Rotate(lastRot - currRot);

            timer += Time.fixedDeltaTime;
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer==(int)enumLayer.Enemy) {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }
}