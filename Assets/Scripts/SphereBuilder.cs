using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshRenderer))]
public class SphereBuilder : MonoBehaviour
{
    // components
    public Slider radiusSlider;
    public TextMeshProUGUI txtBrush;

    CameraController cameraContoller;    
    MeshFilter mf = null;
    MeshCollider mc = null;

    // enums, struct
    public enum Mode
    {
        Paint, Add, Sub
    }

    public struct Voxel
    {
        public int attr;
    }

    // tool state
    int paintcolor = 2;
    int radius = 1;
    bool picked = false;
    bool emptySpace = false;
    public Mode mode = Mode.Paint;
    public bool lookat = false;

    public Vector3 pickpos = Vector3.zero;

    public Vector2 prevMousePos = Vector2.zero;

    // data
    static int width = 64;
    Voxel[,,] voxels = new Voxel[width, width, width];
    Voxel[,,] voxelsForTrim = new Voxel[width, width, width];

    // unity method

    void Awake()
    {
        cameraContoller = Camera.main.GetComponent<CameraController>();
        mf = gameObject.AddComponent<MeshFilter>();
        BuildSphereBuffer();
        mc = gameObject.AddComponent<MeshCollider>();
        BuildQuad();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pickpos, 1);
    }

    // implementation

    private void BuildQuad()
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
        mf.mesh = mesh;
        mc.sharedMesh = mesh;
    }

    void AddUvByIdx(ref List<Vector2>  uvs, int idx)
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

    

    int lcheck(int a, int b)
    {
        if (a > b) return a - b;
        return b - a;
    }

    private void BuildSphereBuffer()
    {
        int center = width / 2;
        int radius = width / 2 - 10;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    voxels[x, y, z] = new Voxel();
                    voxels[x, y, z].attr = 0;
                    float mag = new Vector3(x - center, y - center, z - center).magnitude;
                    if (Mathf.RoundToInt(mag) <= radius)
                    {
                        voxels[x, y, z].attr = 1;// + z % 4;
                    }
                }
            }
        }
    }    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.F)) Expand();

        if (Input.mouseScrollDelta.y != 0)
        {
            Camera.main.transform.Translate(Vector3.forward * Input.mouseScrollDelta.y, Space.Self);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            CopyBuffer();
            if (ModifyByOnePoint(ray))
            {
                BuildQuad();
                picked = true;
            }
            else emptySpace = true;

            cameraContoller.MouseDown();

        }

        if (picked)
        {
            // dab-spacing 
            Ray ray0 = Camera.main.ScreenPointToRay(prevMousePos);

            if (Physics.Raycast(ray0, out var hitinfo0))
            {
                Vector3 worldPos0 = hitinfo0.point;
                if (Physics.Raycast(ray, out var hitinfo1))
                {
                    Vector3 worldPos1 = hitinfo1.point;
                }

            }

            if (ModifyByOnePoint(ray))
            {
                BuildQuad();
            }
        }
        else if(Input.GetMouseButton(0))
        {
            cameraContoller.Drag();
        }        

        if (Input.GetMouseButtonUp(0))
        {
            picked = false;
            emptySpace = false;
        }
    }

    private bool ModifyByOnePoint(Ray ray)
    {
        int center = width / 2;
        bool hit = false;

        if (Physics.Raycast(ray, out var hitinfo)) //, 1000, LayerMask.NameToLayer("Voxel"), QueryTriggerInteraction.UseGlobal))
        {
            hit = true;
            pickpos = hitinfo.point;
            var localpos = transform.InverseTransformPoint(pickpos);
            var arrIdxpos = localpos + new Vector3(center, center, center);
            int x = Mathf.RoundToInt(arrIdxpos.x);
            int y = Mathf.RoundToInt(arrIdxpos.y);
            int z = Mathf.RoundToInt(arrIdxpos.z);
            hit = FillDab(hit, x, y, z);
        }
        else
        {
            pickpos = Vector3.zero;
        }

        prevMousePos = Input.mousePosition;

        return hit;
    }

    private bool FillDab(bool hit, int x0, int y0, int z0)
    {
        int r = radius;

        for (int x = x0 - r; x <= x0 + r; x++)
            for (int y = y0 - r; y <= y0 + r; y++)
                for (int z = z0 - r; z <= z0 + r; z++)
                {
                    int dx = x - x0;
                    int dy = y - y0;
                    int dz = z - z0;

                    int sq = dx * dx + dy * dy + dz * dz;
                    if (sq > r * r) continue;

                    if (0 <= x && x < width && 0 <= y && y < width && 0 <= z && z < width)
                    {
                        if (voxels[x, y, z].attr > 0)
                        {
                            if(mode == Mode.Paint)
                                voxels[x, y, z].attr = paintcolor;
                            else if(mode == Mode.Sub)
                                voxels[x, y, z].attr = 0;
                            hit = true;
                        }
                        else
                        {
                            if (mode == Mode.Add)
                            {
                                if (voxelsForTrim[x, y, z].attr > 0)
                                {
                                    voxels[x, y, z].attr = paintcolor;
                                }                                    
                            }                                
                        }
                    }
                }

        return hit;
    }

    private bool ModifyByFullScan(Ray ray)
    {
        int center = width / 2;
        bool hit = false;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    Vector3 vCenter = new Vector3(x - center, y - center, z - center);
                    vCenter = transform.TransformPoint(vCenter);
                    Bounds bounds = new Bounds();
                    bounds.min = vCenter;
                    bounds.max = vCenter + Vector3.one;

                    if (bounds.IntersectRay(ray))
                    {
                        if (voxels[x, y, z].attr > 0)
                        {
                            voxels[x, y, z].attr = paintcolor;
                            hit = true;
                        }
                    }
                }
        return hit;
    }

    // public method
    public void SetColor(int c)
    {
        if (c >= 1 && c <= 6)
        {
            paintcolor = c;
        }
    }

    public void SetMode(int newMode)
    {
        mode = (Mode)newMode;
    }

    public void SetLookAt(bool newVal)
    {
        lookat = newVal;
    }

    public void Save()
    {
        BinaryWriter bw = new BinaryWriter(new FileStream("buffer.raw", FileMode.Create));
        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    int attr = voxels[x, y, z].attr;
                    bw.Write(attr);
                }
    }

    public void Load()
    {
        BinaryReader br = new BinaryReader(new FileStream("buffer.raw", FileMode.Open));
        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    voxels[x, y, z].attr = br.ReadInt32();                    
                }

        BuildQuad();
    }

    public void DabRadiusChanged()
    {
        radius =  (int)radiusSlider.value;
        txtBrush.text = $"R = {radius}";

    }

    public void CopyBuffer()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    voxelsForTrim[x, y, z].attr = voxels[x, y, z].attr;
                }

        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    if(voxels[x, y, z].attr != 0)
                    {
                        Extrusion(x, y, z);
                    }                    
                }
    }

    private void Extrusion(int x0, int y0, int z0)
    {
        int r = radius;

        for (int x = x0 - r; x <= x0 + r; x++)
            for (int y = y0 - r; y <= y0 + r; y++)
                for (int z = z0 - r; z <= z0 + r; z++)
                {
                    if (voxelsForTrim[x, y, z].attr > 0) continue;

                    int dx = x - x0;
                    int dy = y - y0;
                    int dz = z - z0;

                    int sq = dx * dx + dy * dy + dz * dz;
                    if (sq > r * r) continue;

                    if (0 <= x && x < width && 0 <= y && y < width && 0 <= z && z < width)
                    {
                        voxelsForTrim[x, y, z].attr = paintcolor;
                    }
                }
    }

    void Expand()
    {

        CopyBuffer();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    voxels[x, y, z].attr = voxelsForTrim[x, y, z].attr;
                }

        BuildQuad();
    }   

}
