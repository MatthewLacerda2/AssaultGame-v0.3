using UnityEngine;

public class WeaponsHolder : MonoBehaviour {

    [HideInInspector] public int currentIndex, lastIndex;

    GunInterface[] guns;

    void Start() {
        guns = GetComponentsInChildren<GunInterface>(true);
    }

    void Update() {/*
        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            Switch((int)Input.GetAxis("Mouse ScrollWheel"));
            return;
        }*/

        if (Input.GetKeyDown(KeyCode.Q)) {
            Switch(lastIndex);
            return;
        }

        for (int i = 0; i < 9 && i<transform.childCount + 1; i++) {
            if (Input.GetKeyDown((KeyCode)(i + 48)) || Input.GetKeyDown((KeyCode)(i + 257))) {
                Switch(i-1);
                return;
            }
        }
    }

    public void Switch(int index) {

        //playerSystem.SwitchWeaponsCanvas();

        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(i == index);
        }
        
        lastIndex = currentIndex;
        currentIndex = index;
    }

    public int AddBullets(enumGun type, int bullets) {

        foreach (GunInterface gun in guns) {
            if (gun.ammo.type == type) {
                return gun.AddBullets(bullets);
            }
        }
        return bullets;
    }
}