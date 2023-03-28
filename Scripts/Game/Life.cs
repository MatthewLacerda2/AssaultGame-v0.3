using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Life : MonoBehaviour {

    public float vida = 50;
    public bool hasBlood = true;

    public delegate void listener(Damage dano);
    public event listener delegados;

    public DamageRatio[] ratios;

    //6 parametros, novo recorde!
    public void AddDamage(float damage, float impulso, GameObject source, Vector3 direcao, Collider col, enumDamageType danType) {
        AddDamage(new Damage(damage, impulso, source, direcao, col, danType));
    }

    public void AddDamage(Damage dano) {
        dano.direction.Normalize(); //redundancia

        foreach (DamageRatio ratio in ratios) {
            if (ratio.type == dano.damageType) {
                dano.amount *= ratio.ratio;
                dano.impulso *= ratio.ratio;
                break;
            }
        }

        vida -= dano.amount;

        if (vida <= 0 && vida + dano.amount > 0) {
            dano.killed = true;
        }

        delegados.Invoke(dano);
    }
}