using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadGenerator
{
    public static Mesh VoxelToMesh(Voxel[,,] voxels, int width)
    {
        int center = width / 2;

        var mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        List<int> indicies = new List<int>();
        int idx = 0;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    // 6-faces
                    int attr = voxels[x, y, z].attr;
                    if (attr > 0)
                    {
                        Vector3 vCenter = new Vector3(x - center, y - center, z - center);
                        // top
                        if (y == width - 1 || voxels[x, y + 1, z].attr == 0)
                        {
                            vertices.Add(new Vector3(0, 1, 0) + vCenter);
                            vertices.Add(new Vector3(0, 1, 1) + vCenter);
                            vertices.Add(new Vector3(1, 1, 1) + vCenter);
                            vertices.Add(new Vector3(1, 1, 0) + vCenter);
                            normals.Add(Vector3.up);
                            normals.Add(Vector3.up);
                            normals.Add(Vector3.up);
                            normals.Add(Vector3.up);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            AddUvByIdx(ref uvs, attr);

                        }
                        // bottom
                        if (y == 0 || voxels[x, y - 1, z].attr == 0)
                        {
                            vertices.Add(new Vector3(0, 0, 0) + vCenter);
                            vertices.Add(new Vector3(1, 0, 0) + vCenter);
                            vertices.Add(new Vector3(1, 0, 1) + vCenter);
                            vertices.Add(new Vector3(0, 0, 1) + vCenter);
                            normals.Add(-Vector3.up);
                            normals.Add(-Vector3.up);
                            normals.Add(-Vector3.up);
                            normals.Add(-Vector3.up);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            AddUvByIdx(ref uvs, attr);
                        }
                        // left
                        if (x == 0 || voxels[x - 1, y, z].attr == 0)
                        {
                            vertices.Add(new Vector3(0, 0, 0) + vCenter);
                            vertices.Add(new Vector3(0, 0, 1) + vCenter);
                            vertices.Add(new Vector3(0, 1, 1) + vCenter);
                            vertices.Add(new Vector3(0, 1, 0) + vCenter);
                            normals.Add(-Vector3.right);
                            normals.Add(-Vector3.right);
                            normals.Add(-Vector3.right);
                            normals.Add(-Vector3.right);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            AddUvByIdx(ref uvs, attr);
                        }
                        // right
                        if (x == width - 1 || voxels[x + 1, y, z].attr == 0)
                        {
                            vertices.Add(new Vector3(1, 0, 0) + vCenter);
                            vertices.Add(new Vector3(1, 1, 0) + vCenter);
                            vertices.Add(new Vector3(1, 1, 1) + vCenter);
                            vertices.Add(new Vector3(1, 0, 1) + vCenter);
                            normals.Add(Vector3.right);
                            normals.Add(Vector3.right);
                            normals.Add(Vector3.right);
                            normals.Add(Vector3.right);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            AddUvByIdx(ref uvs, attr);
                        }
                        // front
                        if (z == 0 || voxels[x, y, z - 1].attr == 0)
                        {
                            vertices.Add(new Vector3(0, 0, 0) + vCenter);
                            vertices.Add(new Vector3(0, 1, 0) + vCenter);
                            vertices.Add(new Vector3(1, 1, 0) + vCenter);
                            vertices.Add(new Vector3(1, 0, 0) + vCenter);
                            normals.Add(-Vector3.forward);
                            normals.Add(-Vector3.forward);
                            normals.Add(-Vector3.forward);
                            normals.Add(-Vector3.forward);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            AddUvByIdx(ref uvs, attr);
                        }
                        // back
                        if (z == width - 1 || voxels[x, y, z + 1].attr == 0)
                        {
                            vertices.Add(new Vector3(0, 0, 1) + vCenter);
                            vertices.Add(new Vector3(1, 0, 1) + vCenter);
                            vertices.Add(new Vector3(1, 1, 1) + vCenter);
                            vertices.Add(new Vector3(0, 1, 1) + vCenter);
                            normals.Add(Vector3.forward);
                            normals.Add(Vector3.forward);
                            normals.Add(Vector3.forward);
                            normals.Add(Vector3.forward);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            indicies.Add(idx++);
                            AddUvByIdx(ref uvs, attr);

                        }
                    }

                }

        mesh.name = "sphere";
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.SetUVs(0, uvs);
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetIndices(indicies, MeshTopology.Quads, 0);
        mesh.RecalculateBounds();

        return mesh;
    }

    static void AddUvByIdx(ref List<Vector2> uvs, int idx)
    {
        float trim = 1.0f / 32;
        float interval = 1 / 4.0f;
        Vector2 uvbase = new Vector2(0, 0);

        if (idx == 2) uvbase = new Vector2(interval, 0);
        if (idx == 3) uvbase = new Vector2(0, interval);
        if (idx == 4) uvbase = new Vector2(interval, interval);
        if (idx == 5) uvbase = new Vector2(0, interval * 2);
        if (idx == 6) uvbase = new Vector2(interval, interval * 2);

        uvs.Add(new Vector2(0 + trim, 0 + trim) + uvbase);
        uvs.Add(new Vector2(0 + trim, interval - trim) + uvbase);
        uvs.Add(new Vector2(interval - trim, interval - trim) + uvbase);
        uvs.Add(new Vector2(interval - trim, 0 + trim) + uvbase);
    }

}
