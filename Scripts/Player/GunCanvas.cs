using TMPro;
using UnityEngine;

public class GunCanvas {

    public GunInterface gInterf;
    public Sprite icon;
    //public Image image;   //trocar cor quando selecionado
    //public GameObject crosshairPrefab;

    public TextMeshProUGUI textMesh;

    public GunCanvas() {
        gInterf = null;
        icon = null;
        textMesh = null;
        //crosshairPrefab = null;
    }

    public GunCanvas(GunInterface gunInterf, Sprite sprite, TextMeshProUGUI textMeshPro) {
        gInterf = gunInterf;
        icon = sprite;
        textMesh = textMeshPro;
        //crosshairPrefab = crossHairP;
    }
}