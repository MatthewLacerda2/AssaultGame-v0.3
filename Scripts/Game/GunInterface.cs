using UnityEngine;
using UnityEngine.UI;

public class GunInterface : MonoBehaviour {

    public AmmoPack ammo;

    public int maxBullets;

    public bool TakeBullet() {
        if (ammo.numBullets > 0) {
            ammo.numBullets--;
            return true;
        } else {
            return false;
        }
    }

    public int AddBullets(int add) {

        int aux = ammo.numBullets + add;

        if (aux > maxBullets) {

            ammo.numBullets = maxBullets;
            return aux - maxBullets;

        } else {
            ammo.numBullets += add;
            return add;
        }
    }
}