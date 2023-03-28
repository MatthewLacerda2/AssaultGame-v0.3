using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PowerUp : MonoBehaviour {

    public enum power {
        ammo, damage, firerate, life
    }

    public float duration = 7f;

    void OnCollisionEnter(Collision collision) {
        
    }

    IEnumerator damageRoutine() {
        Debug.LogError("Não implementado");
        yield return new WaitForSeconds(duration);
    }

    IEnumerator firerateRoutine() {
        Debug.LogError("Não implementado");
        yield return new WaitForSeconds(duration);
    }

    IEnumerator hpRoutine() {
        Debug.LogError("Não implementado");
        yield return new WaitForSeconds(duration);
    }

    IEnumerator ammoRoutine() {
        Debug.LogError("Não implementado");
        yield return new WaitForSeconds(duration);
    }
}