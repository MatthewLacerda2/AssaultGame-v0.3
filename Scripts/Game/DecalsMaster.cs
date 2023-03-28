using UnityEngine;
using System.Collections.Generic;

public static class DecalsMaster {

    public static List<Decal> decals = new(maxDecals);
    static int maxDecals = 512;

    public static void SetMaxDecals(int max) {
        
        maxDecals = Mathf.Clamp(max, 64, 4098);

        if (max < decals.Count) {
            decals.RemoveRange(0, decals.Count - max - 1);
        }
    }
    
    public static void Resetar() {
        for (int i = 0; i < decals.Count; i++) {
            GameObject.Destroy(decals[i].gameObject);
        }

        decals = new List<Decal>(maxDecals);
    }

    public static void Add(Decal decal) {
        if (decals.Count < maxDecals) {
            decals.Add(decal);
            return;
        }
        //Debug.LogError("Decal limit exceeded!");
        int j = 0;
        for (int i = 0; i < decals.Count; i++) {
            if (decals[i].isInCameraFrustum == false) {
                j = i;
                break;
            }
        }

        decals.RemoveAt(j);
        GameObject.Destroy(decals[j].transform.gameObject);        
        decals.Add(decal);
    }
}