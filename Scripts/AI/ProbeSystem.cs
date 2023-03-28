using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

[RequireComponent(typeof(Director))]
public class ProbeSystem : MonoBehaviour {

    public readonly static Vector3 up = new(0, 1.8f, 0);

    [SerializeField] Bounds area;
    [SerializeField] Bounds[] exceptions;
    [SerializeField] GameObject[] destroyOnAwake;
    
    public ProbeSample[] samples;
    public float targetTime;

    readonly float spacing = 2.0f;

    void Awake() {
        if (lendaLib.probeSystem != null) {
            Debug.LogError("ProbeSys a mais em " + transform.name);
            Destroy(this);
            return;
        }

        lendaLib.probeSystem = this;

        foreach (GameObject go in destroyOnAwake) {
            if (go != null) {
                Destroy(go);
            }
        }

        Generate();
    }

    void Generate() {

        List<ProbeSample> samplesList = new();

        float lengthX = area.extents.x;
        float lengthZ = area.extents.z;

        float startX = area.center.x-lengthX/2;
        float endX = area.center.x+lengthX/2;

        float startZ=area.center.z-lengthZ/2;
        float endZ = area.center.z+lengthZ/2;

        for (float x = startX; x < endX; x += spacing) {
            for (float z = startZ; z < endZ; z += spacing) {

                bool hasHit = NavMesh.SamplePosition(new Vector3(x, 0, z), out NavMeshHit hit, spacing, NavMesh.AllAreas);

                if (hasHit == false) {
                    continue;
                }

                NavMeshPath path = new();
                bool hasPath = NavMesh.CalculatePath(hit.position, lendaLib.playerPos, NavMesh.AllAreas, path);

                if (path.status != NavMeshPathStatus.PathComplete) {
                    continue;
                }

                ProbeSample sample = new(hit.position + up);
                samplesList.Add(sample);
            }
        }

        for(int i = 0; i < samplesList.Count-1; i++) {
            float dist = Vector3.Distance(samplesList[i].position, samplesList[i+1].position);
            if(dist<spacing) {
                samplesList.RemoveAt(i + 1);
            }
        }

        samples = samplesList.ToArray();
    }
    
    int samplesIndex=0;
    void RefreshProbes() {

        int budget = (int)(samples.Length / 15);

        for (int j = 0; j < budget; j++) {

            Vector3 target = lendaLib.cameraPos;
            Vector3 pos = samples[samplesIndex].position;

            float distToPlayer = Vector3.Distance(target, pos);
            samples[samplesIndex].isPlayerVisible = lendaLib.RaycastVisibility(samples[samplesIndex].position, lendaLib.playerPos+up, lendaLib.playerTransf, 60f);
            samples[samplesIndex].distToPlayer = distToPlayer;
            samples[samplesIndex].isInPlayerFrustum = lendaLib.isInViewFrustum(lendaLib.cameraPos, Camera.main.transform.forward, 90.0f, samples[samplesIndex].position);

            samplesIndex++;
            if (samplesIndex >= samples.Length) {
                samplesIndex= 0;
            }
        }
    }

    void RefreshDecals() {
        if (DecalsMaster.decals.Count == 0) {
            return;
        }

        foreach (Decal dc in DecalsMaster.decals) {
            if (dc == null) {
                continue;
            }

            dc.isInCameraFrustum = lendaLib.isInPlayerCameraFrustum(dc.transform.position);
            dc.fovSize = lendaLib.percentOnFov(Camera.main.fieldOfView, Vector3.Distance(lendaLib.cameraPos, dc.transform.position), dc.radius);
        }
    }

    // Update is called once per frame
    public void Refresh() {

        Profiler.BeginSample("probeSys");

        RefreshProbes();
        RefreshDecals();

        samples.OrderBy(sp => sp.isPlayerVisible).ThenBy(sp => sp.isInPlayerFrustum).ThenBy(sp => sp.distToPlayer);
        DecalsMaster.decals.OrderBy(dc => dc.isInCameraFrustum).ThenBy(dc => dc.fovSize);

        Profiler.EndSample();
    }

    public ProbeSample GetNearestProbe(Vector3 position, bool visibility) {

        ProbeSample sample = samples[0];
        float dist=Vector3.Distance(position, sample.position);

        foreach(ProbeSample sp in samples) {

            if (sp.isPlayerVisible != visibility) {
                continue;
            }

            float aux = Vector3.Distance(position, sp.position);
            if (dist > aux) {
                dist = aux;
                sample = sp;
            }
        }

        return sample;
    }

    public ProbeSample GetNearestSight(Vector3 position) {
        return GetNearestProbe(position, true);
    }

    public ProbeSample GetNearestCover(Vector3 position) {
        return GetNearestProbe(position, false);
    }

    public Vector3 GetNearestProbePos(Vector3 position, bool visibility) {
        return GetNearestProbe(position, visibility).position;
    }

    public Vector3 GetNearestSightPos(Vector3 position) {
        return GetNearestProbe(position, true).position;
    }

    public Vector3 GetNearestCoverPos(Vector3 position) {
        return GetNearestProbe(position, false).position;
    }

    void OnDrawGizmosSelected() {
        Vector3 upper = new(0, 0.05f, 0);

        Gizmos.color = new Color(0, 1f, 0, 0.25f);
        Gizmos.DrawCube(area.center + upper, area.extents);

        Gizmos.color = new Color(1f, 0, 0, 0.333f);
        foreach (Bounds except in exceptions) {
            Gizmos.DrawCube(except.center + upper, except.extents);
        }
    }
}