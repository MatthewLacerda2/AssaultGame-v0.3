using System.Collections;
using UnityEngine;

public class BotInterface : MonoBehaviour {

    [HideInInspector] public float timePlayerSpotted;

    public Transform tracer;

    public ShooterAI shooterAI {
        get; private set;
    }

    public enumBotState state;
    public float distPlayer {
        get; private set;
    }
    public bool isInPlayerView {
        get; private set;
    }
    public float continuousTime {
        get;
        private set;
    }
    public bool canShoot {
        get;
        private set;
    }    

    Animator anime;
    SkinnedMeshRenderer skRender;

    void Awake() {
        anime = GetComponent<Animator>();
        shooterAI = GetComponent<ShooterAI>();
        skRender = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    public void Refresh() {

        if (anime.enabled == false) {
            RagdollParameters();
            return;
        }

        isInPlayerView = skRender.isVisible;
        distPlayer = Vector3.Distance(lendaLib.playerPos, transform.position);
        shooterAI.distPlayer = distPlayer;

        bool isPlayerVisible = lendaLib.RaycastVisibility(tracer.position, lendaLib.playerPos + ProbeSystem.up/2, lendaLib.playerTransf, shooterAI.range);

        if (isPlayerVisible) {
            timePlayerSpotted = Mathf.Clamp(timePlayerSpotted, 0f, 30f);
            timePlayerSpotted += shooterAI.reflexDropoff.Evaluate(distPlayer) * Time.deltaTime;
        } else {
            timePlayerSpotted -= Time.deltaTime;
        }

        shooterAI.timeViewing = timePlayerSpotted;

        if (isPlayerVisible) {
            continuousTime = Mathf.Clamp(continuousTime, 0f, 30f);
            continuousTime += Time.deltaTime;
        } else {
            continuousTime = Mathf.Clamp(continuousTime, -30f, 0f);
            continuousTime -= Time.deltaTime;
        }

        canShoot = timePlayerSpotted >= shooterAI.timeWait;

        anime.SetFloat("continuousTime", continuousTime);
        anime.SetFloat("distPlayer", distPlayer);
        anime.SetFloat("timePlayerSpotted", timePlayerSpotted);
        anime.SetBool("isInPlayerView", isInPlayerView);
        anime.SetBool("canShoot", canShoot);

        anime.SetInteger("numInView", Director.numInView);
        anime.SetInteger("numShooters", Director.numShooters);
    }
    
    void RagdollParameters() {
        timePlayerSpotted = 0;
        isInPlayerView = skRender.isVisible;
        distPlayer = Vector3.Distance(lendaLib.playerPos, transform.position);
        canShoot = false;
    }

    public bool SetState(enumBotState newState, float time) {
        if (timer > 0) {
            return false;
        }

        state = newState;

        if (time > 0) {
            timer = time;
            StartCoroutine(StateTimer());
        }

        return true;
    }

    float timer;
    IEnumerator StateTimer() {
        while (timer > 0) {
            timer -= Time.deltaTime;
            yield return null;
        }
    }

    public void TakeAction() {
        shooterAI.TakeDueAction();
    }

    public void RemoveInterfaceFromDirector() {
        lendaLib.probeSystem.GetComponent<Director>().RemoveInterface(this);
    }
}