using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(DecalProjector))]
public class Decal : MonoBehaviour {

    public float fovSize, radius;
    public bool isInCameraFrustum;

    [SerializeField] Material[] blood, normal, metal, wood;

    public void Setup(string name) {
        if (name.Contains("metal", System.StringComparison.OrdinalIgnoreCase)) {
            Setup(enumPhysicMaterial.Metal);
        } else if (name.Contains("wood", System.StringComparison.OrdinalIgnoreCase)) {
            Setup(enumPhysicMaterial.Wood);
        } else {
            Setup(enumPhysicMaterial.None);
        }
    }

    public void Setup(enumPhysicMaterial material) {
        
        DecalProjector projector = GetComponent<DecalProjector>();

        radius = Vector3.Scale(projector.size, transform.lossyScale).magnitude;
        fovSize = lendaLib.percentOnFov(Camera.main.fieldOfView, Vector3.Distance(lendaLib.cameraPos, transform.position), radius);
        isInCameraFrustum=lendaLib.isInPlayerCameraFrustum(transform.position);

        projector.material = material switch {
            enumPhysicMaterial.Flash => blood[Random.Range(0, blood.Length)],
            enumPhysicMaterial.Metal => metal[Random.Range(0, metal.Length)],
            enumPhysicMaterial.Wood => wood[Random.Range(0, wood.Length)],
            _ => normal[Random.Range(0, normal.Length)],
        };

        if(material== enumPhysicMaterial.Flash) {
            transform.localScale *= 10f;
            projector.drawDistance *= 1.5f;
        }

        //ver se isso poupa memória
        blood = null;
        normal = null;
        metal = null;
        wood=null;

        DontDestroyOnLoad(gameObject);

        DecalsMaster.Add(this);
    }
}