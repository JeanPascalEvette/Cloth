using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshCreator : MonoBehaviour
{

    [SerializeField]
    private Cloth cloth;

    private List<int> indices = new List<int>();
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();

    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    // Use this for initialization
    void Start()
    {
        if (cloth == null)
            cloth = GetComponent<Cloth>();

        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";

        if (viewMeshFilter == null)
        {
            GameObject meshChild = new GameObject("Mesh");
            meshChild.transform.parent = transform;
            meshChild.transform.localPosition = Vector3.zero;
            meshChild.transform.localRotation = Quaternion.identity;
            var vmf = meshChild.AddComponent<MeshFilter>();
            var mr = meshChild.AddComponent<MeshRenderer>();
            var col = meshChild.AddComponent<MeshCollider>();
            mr.receiveShadows = false;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.sharedMaterial = (Material)Resources.Load("Materials/Cloth");
            viewMeshFilter = vmf;
        }

        viewMeshFilter.mesh = viewMesh;

    }

    // Update is called once per frame
    void Update()
    {
        vertices.Clear();
        normals.Clear();
        indices.Clear();

        foreach (var particle in cloth.particles)
        {
            particle.resetNormal();
        }

        for (int x = 0; x < cloth.num_particles_width - 1; x++)
        {
            for (int y = 0; y < cloth.num_particles_height - 1; y++)
            {
                Vector3 normal = calcTriangleNormal(cloth.getParticle(x + 1, y), cloth.getParticle(x, y), cloth.getParticle(x, y + 1));
                cloth.getParticle(x + 1, y).addToNormal(normal);
                cloth.getParticle(x, y).addToNormal(normal);
                cloth.getParticle(x, y + 1).addToNormal(normal);

                normal = calcTriangleNormal(cloth.getParticle(x + 1, y + 1), cloth.getParticle(x + 1, y), cloth.getParticle(x, y + 1));
                cloth.getParticle(x + 1, y + 1).addToNormal(normal);
                cloth.getParticle(x + 1, y).addToNormal(normal);
                cloth.getParticle(x, y + 1).addToNormal(normal);
            }
        }

        int index = 0;
        for (int x = 0; x < cloth.num_particles_width - 1; x++)
        {
            for (int y = 0; y < cloth.num_particles_height - 1; y++)
            {
                Particle p1 = cloth.getParticle(x + 1, y);
                Particle p2 = cloth.getParticle(x, y);
                Particle p3 = cloth.getParticle(x, y + 1);
                Particle p4 = cloth.getParticle(x + 1, y + 1);

                if (!p1.getIsConstraintTorn(p2) && !p1.getIsConstraintTorn(p3) && !p3.getIsConstraintTorn(p2))
                {
                    vertices.Add(p1.getPos());
                    normals.Add(p1.getNormal().normalized);
                    indices.Add(index++);
                    vertices.Add(p2.getPos());
                    normals.Add(p2.getNormal().normalized);
                    indices.Add(index++);
                    vertices.Add(p3.getPos());
                    normals.Add(p3.getNormal().normalized);
                    indices.Add(index++);
                }

                if (!p1.getIsConstraintTorn(p4) && !p1.getIsConstraintTorn(p3) && !p4.getIsConstraintTorn(p3))
                {
                    vertices.Add(p4.getPos());
                    normals.Add(p4.getNormal().normalized);
                    indices.Add(index++);
                    vertices.Add(p1.getPos());
                    normals.Add(p1.getNormal().normalized);
                    indices.Add(index++); // p1
                    vertices.Add(p3.getPos());
                    normals.Add(p3.getNormal().normalized);
                    indices.Add(index++); // p3
                }

                //index += 2;
                
            }
        }


        viewMesh.Clear();
        viewMesh.vertices = vertices.ToArray();
        viewMesh.triangles = indices.ToArray();
        viewMesh.normals = normals.ToArray();


    }


    /* A private method used by drawShaded() and addWindForcesForTriangle() to retrieve the  
    normal vector of the triangle defined by the position of the particles p1, p2, and p3.
    The magnitude of the normal vector is equal to the area of the parallelogram defined by p1, p2 and p3
    */
    Vector3 calcTriangleNormal(Particle p1, Particle p2, Particle p3)
    {
        Vector3 pos1 = p1.getPos();
        Vector3 pos2 = p2.getPos();
        Vector3 pos3 = p3.getPos();

        Vector3 v1 = pos2 - pos1;
        Vector3 v2 = pos3 - pos1;

        return Vector3.Cross(v1, v2);
    }
}

