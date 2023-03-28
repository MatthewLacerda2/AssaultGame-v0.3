using UnityEngine;

public class voxelizer : MonoBehaviour {

    [Range(2, 16)] public int resolution = 8;

    void Start() {
        InstanciarVoxel(transform);
    }

    public void InstanciarVoxel(Transform transf) {
        MeshCollider mexico = transf.GetComponentInChildren<MeshCollider>();

        Mesh remesh = voxelize(mexico.sharedMesh);

        GameObject go = new GameObject(transf.name + "Voxelized");
        go.transform.position = transf.position;
        go.transform.rotation = transf.rotation;
        go.transform.localScale = transf.localScale;

        MeshFilter mFilter = go.AddComponent<MeshFilter>();
        mFilter.mesh = remesh;

        go.AddComponent<MeshRenderer>();

        //Fé no pai que o Mesh sai
    }

    public Mesh voxelize(Mesh mexe) {
        return voxelize(mexe, resolution);
    }

    public static Mesh voxelize(Mesh mexe, int res) {

        res = Mathf.Clamp(res, 2, 16);   //Alguém mais sente tesão por Potencia de base 2?

        Mesh cubeMesh = lendaLib.cubeMesh;

        float minX = 0, maxX = 0;
        float minY = 0, maxY = 0;
        float minZ = 0, maxZ = 0;
        foreach(Vector3 vertice in mexe.vertices) {
            if (minX > vertice.x) {
                minX = vertice.x;
            } else if (maxX < vertice.x) {
                maxX = vertice.x;
            }

            if (minY > vertice.y) {
                minY = vertice.y;
            } else if (maxY < vertice.y) {
                maxY = vertice.y;
            }

            if (minZ > vertice.z) {
                minZ = vertice.z;
            } else if (maxZ < vertice.z) {
                maxZ = vertice.z;
            }
        }
        float xLength = Mathf.Abs(maxX - minX);
        float yLength = Mathf.Abs(maxY - minY);
        float zLength = Mathf.Abs(maxZ - minZ);

        float tam = xLength;
        if (tam < yLength) {
            tam = yLength;
        }
        if (tam < zLength) {
            tam = zLength;
        }
        tam /= res;

        bool[,,] matrix = new bool[res, res, res];

        foreach (Vector3 vertice in mexe.vertices) {
            int a = Mathf.FloorToInt((Mathf.Abs(vertice.x - minX) / xLength) * res);
            int b = Mathf.FloorToInt((Mathf.Abs(vertice.y - minY) / yLength) * res);
            int c = Mathf.FloorToInt((Mathf.Abs(vertice.z - minZ) / zLength) * res);

            a = Mathf.Clamp(a, 0, res - 1);
            b = Mathf.Clamp(b, 0, res - 1);
            c = Mathf.Clamp(c, 0, res - 1);

            matrix[a, b, c] = true;
        }

        int numPos = 0;
        foreach (bool b in matrix) {
            if (b == true) {
                numPos++;
            }
        }

        Vector3[] vertices = new Vector3[numPos * cubeMesh.vertices.Length];
        int[] triangles = new int[(int)(4.5f * vertices.Length)];

        int index = 0;
        for(int i = 0; i < res; i++) {
            for(int j = 0; j < res; j++) {
                for(int k = 0; k < res; k++) {
                    if(matrix[i, j, k] == false) {
                        continue;
                    }

                    foreach(Vector3 v in cubeMesh.vertices) {
                        Vector3 aux = v * tam;

                        aux.x += (tam * i);
                        aux.y += (tam * j);
                        aux.z += (tam * k);

                        vertices[index] = aux;

                        for (int a = 0; a < cubeMesh.triangles.Length; a++) {
                            triangles[index + a] = cubeMesh.triangles[a];
                        }

                        index++;
                    }
                }
            }
        }

        //GG
        Mesh meshVoxelado = lendaLib.CreateMesh(new Mesh(), mexe.name + "Voxelado", vertices, triangles);
        Debug.Log(vertices.Length + " vertexes. " + triangles.Length + " triangles.");
        return meshVoxelado;
    }
}