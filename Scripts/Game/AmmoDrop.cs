using System.Collections.Generic;
using UnityEngine;

public class AmmoDrop : MonoBehaviour {

    [SerializeField] AudioClip clip;
    [SerializeField] Material[] mats;
    [SerializeField] List<AmmoPack> ammoPacks;
    [SerializeField] Vector3 offset;
    [SerializeField] float range = 2.2f;
    [SerializeField] bool locRot;

    void OnEnable() {
        Iniciar();
    }

    void Iniciar() {
        if (mats.Length > 0) {
            GetComponentInChildren<MeshRenderer>().materials = mats;
        }

        Vector3 pos = transform.position;
        pos.y = 0.5f;

        transform.SetPositionAndRotation(pos, Quaternion.Euler(0,0,0));

        foreach(Collider col in GetComponentsInChildren<Collider>()) {
            col.enabled = false;
        }
    }

    // Update is called once per frame
    void Update() {

        if (locRot == false) {
            transform.Rotate(Time.deltaTime * 0.4f * new Vector3(0, 360f, 0));
        }

        Vector3 pos = lendaLib.playerPos + offset;
        pos.y = transform.position.y;
        float dist = Vector3.Distance(pos, transform.position);

        if (dist <= range) {
            GiveBullets();
            return;
        }
    }

    void GiveBullets() {

        foreach(AmmoPack pack in ammoPacks) {
            int aux =  lendaLib.playerTransf.GetComponentInChildren<WeaponsHolder>().AddBullets(pack.type, pack.numBullets);

            pack.numBullets -= (pack.numBullets-aux);
        }/*
        foreach(AmmoPack pack in ammoPacks) {
            Debug.Log(pack.numBullets);
        }
        
        for(int i=0;i<ammoPacks.Count;i++) {
            if (ammoPacks[i].numBullets== 0) {
                ammoPacks.RemoveAt(i);
            }
        }
        */
        if (clip != null) {
            AudioSource.PlayClipAtPoint(clip, transform.position, 1f);
        }

        //if (ammoPacks.Count == 0) {
            Destroy(gameObject);
        //}
    }

    void OnDrawGizmosSelected() {
        if (this.enabled) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + offset, range);
        }        
    }
}