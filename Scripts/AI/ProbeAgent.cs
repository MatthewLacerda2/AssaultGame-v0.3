using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ProbeAgent : MonoBehaviour {

    [SerializeField] AnimationCurve speedDropoff;

    NavMeshAgent agent;
    ProbeSystem probeSys;

    float stopDist {
        get {
            if (agent) {
                return agent.stoppingDistance;
            } else {
                return 0.666f;  //ksksksk
            }
        }
    }

    float agentSpeed;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        agentSpeed = agent.speed;

        probeSys = lendaLib.probeSystem;
    }

    void Update() {
        AdjustSpeed();
    }

    void AdjustSpeed() {
        int index = Mathf.Clamp(1, 0, agent.path.corners.Length-1);
        float remainDist = Vector3.Distance(agent.path.corners[index], transform.position);

        agent.speed = agentSpeed * speedDropoff.Evaluate(remainDist);
    }

    public void SetWalk(bool walk) {
        if (this.enabled == false) {
            return;
        }

        //Se ta como pediram, deixa, senao, troca
        //agent.isStopped chama uma funcao que so deve ser chamada se for trocar stopped de valor
        if (agent.isStopped == walk) {
            agent.isStopped = !walk;
        }
    }

    public void SetDestination(Vector3 pos) {
        if (this.enabled == false) {
            return;
        }

        agent.SetDestination(pos);
    }

    public void ShortestSight() {
        PathWithVisibility(lendaLib.playerPos, true);
    }

    public void Flankear() {
        PathWithVisibility(lendaLib.playerPos, false);
    }

    void PathWithVisibility(Vector3 targetPos, bool visib) {

        Vector3 target = probeSys.GetNearestProbePos(targetPos, visib);

        float distBotTarget = Vector3.Distance(transform.position, targetPos);
        float distBotPlayer = Vector3.Distance(transform.position, lendaLib.playerPos);
        float distTargetPlayer = Vector3.Distance(target, lendaLib.playerPos);

        if (distBotTarget <= stopDist * 1.5f) {
            agent.SetDestination(lendaLib.playerPos);
        }else if (distBotTarget <= distTargetPlayer) {
            agent.SetDestination(targetPos);
        } else {
            agent.SetDestination(lendaLib.playerPos);
        }
    }

    void OnEnable() {
        if (agent == null) {
            agent = GetComponent<NavMeshAgent>();
        }
        agent.enabled = true;
    }

    void OnDisable() {
        agent.enabled = false;
    }

    public void OnDrawGizmosSelected() {
        if (agent == null) {
            return;
        }

        Gizmos.color = new Color(0.5f, 0, 0.4f, 0.7f);

        foreach (Vector3 corner in agent.path.corners) {
            Gizmos.DrawSphere(corner, 0.333f);
        }

        for (int i = 0; i < agent.path.corners.Length - 1; i++) {
            Vector3 from = agent.path.corners[i];
            Vector3 to = agent.path.corners[i + 1];
            Gizmos.DrawLine(from, to);
        }
    }
}