using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Director : MonoBehaviour {

    public int nextLevelIndex = -1;

    public GameObject[] weaponsPrefabs;
    public int starterIndex;

    public static int numShooters {
        get; private set;
    }
    public static int numInView {
        get; private set;
    }

    List<BotInterface> botInterfaces = new();
    List<BotWait> botWaits = new();

    ProbeSystem probeSys;

    readonly float maxPeaceTimer = 5;
    float pazTimer;

    void Awake() {

        Random.InitState(69069);
        
        SetupPlayerStuff();

        numShooters = 0;
        numInView = 0;

        lendaLib.playerTransf.GetComponent<Life>().delegados += OnPlayerDamage;
        probeSys = GetComponent<ProbeSystem>();

        botInterfaces = GetComponentsInChildren<BotInterface>().Where(bt => bt.gameObject.activeSelf == true).ToList();
        botWaits = GetComponentsInChildren<BotWait>().Where(bt => bt.gameObject.activeSelf == true).ToList();
    }

    void SetupPlayerStuff() {
        WeaponsHolder wph = lendaLib.playerTransf.GetComponentInChildren<WeaponsHolder>();

        foreach (GameObject weapon in weaponsPrefabs) {
            PlayerGun pg = Instantiate(weapon, wph.transform.position, wph.transform.rotation, wph.transform).GetComponent<PlayerGun>();
            pg.transform.localPosition = pg.offsetPos;
        }

        wph.currentIndex = starterIndex;

        if (starterIndex >= weaponsPrefabs.Length) {
            wph.lastIndex = starterIndex - 1;
        } else {
            wph.lastIndex = starterIndex + 1;
        }

        wph.Switch(wph.currentIndex);
    }

    // Update is called once per frame
    void Update() {

        if (Time.timeScale == 0) {
            return;
        }

        StickAnnoyingPlayer();

        PreArrange();

        RefreshBotInterfaces();
    }

    void PreArrange() {
        TrackPlayerSpeed();

        probeSys.Refresh();
    }

    void StickAnnoyingPlayer() {
        pazTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            pazTimer = 0;
        }

        if (pazTimer > maxPeaceTimer) {
            WakeClosestBot();
            WakeClosestBot();
            pazTimer = 0;
        }
    }

    Vector3 lastPlayPos;
    void TrackPlayerSpeed() {

        Vector3 speed = lendaLib.playerPos - lastPlayPos;
        speed *= 50;
        speed = Vector3.Slerp(lendaLib.playerSpeed, speed, Time.deltaTime*3);

        lendaLib.playerSpeed = speed;
        lastPlayPos = lendaLib.playerPos;
    }

    void OnPlayerDamage(Damage dano) {
        if (dano.killed) {
            this.enabled = false;
        } else if (dano.amount > 0) {
            pazTimer = 0;
        }
    }

    void WakeClosestBot() {
        if (botWaits.Count == 0) {
            return;
        }

        int index = 0, i = 0;
        float dist = 999;

        foreach (BotWait bw in botWaits) {

            if (bw.waitTimer >= pazTimer) {
                continue;
            }

            float aux = Vector3.Distance(lendaLib.playerPos, bw.transform.position);

            if (dist > aux) {
                dist = aux;
                index = i;
            }

            i++;
        }

        if (dist <= 10f) {
            botWaits[index].Attack("moscow");
        }
    }

    void RefreshBotInterfaces() {

        botInterfaces.OrderBy(bt => bt.isInPlayerView).ThenBy(bt => bt.timePlayerSpotted).ThenBy(bt => bt.state);

        foreach (BotInterface bt in botInterfaces) {
            bt.Refresh();
        }

        botInterfaces.OrderBy(bt => bt.isInPlayerView).ThenBy(bt => bt.timePlayerSpotted).ThenBy(bt => bt.state);

        numInView = 0;
        numShooters = 0;
        foreach (BotInterface bt in botInterfaces) {

            if (bt.isInPlayerView) {
                numInView++;
            }
            if (bt.state == enumBotState.Aim) {
                numShooters++;
            }
        }

        foreach (BotInterface bt in botInterfaces) {

            if(bt.state==enumBotState.Idle) {
                continue;
            }

            bool waiting = false;

            foreach (BotWait bw in botWaits) {
                if (bw.transform == bt.transform) {
                    waiting = true;
                    break;
                }
            }

            if (waiting == false) {
                bt.TakeAction();
            }
        }
    }

    public void RemoveWaiter(BotWait bw) {
        botWaits.Remove(bw);
    }

    public void RemoveInterface(BotInterface bt) {
        botInterfaces.Remove(bt);

        if (botInterfaces.Count == 0) {
            lendaLib.playerTransf.GetComponent<PlayerSystem>().PlayerWin();
        }
    }
}