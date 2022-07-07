using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshRenderer))]
public class SphereBuilder : MonoBehaviour
{
    public Slider radiusSlider;

    int paintcolor = 2;
    int radius = 1;

    public void SetColor(int c)
    {
        if(c >= 1 && c <= 6)
        {
            paintcolor = c;
        }        
    }

    public enum Mode
    {
        Paint, Add, Sub
    }

    public Mode mode = Mode.Paint;

    public void SetMode(int newMode)
    {
        mode = (Mode)newMode;
    }

    public bool lookat = false;

    public void SetLookAt(bool newVal)
    {
        lookat = newVal;
    }


    public struct Voxel
    {
        public int attr;
    }

    static int width = 64;
    Voxel[,,] voxels = new Voxel[width, width, width];
    Voxel[,,] voxelsExpand = new Voxel[width, width, width];

    MeshFilter mf = null;
    MeshCollider mc = null;

    // Start is called before the first frame update
    void Awake()
    {
        mf = gameObject.AddComponent<MeshFilter>();
        BuildSphereBuffer();
        mc = gameObject.AddComponent<MeshCollider>();
        BuildQuad();        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pickpos, 1);
        return;

        int center = width / 2;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    Vector3 vCenter = new Vector3(x - center, y - center, z - center);
                    // 6-faces
                    if (voxels[x, y, z].attr > 0)
                    {
                        Gizmos.DrawCube(vCenter + transform.position, Vector3.one * 0.8f);
                    }
                }
    }

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
                            adduvbyidx(ref uvs, attr);

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
                            adduvbyidx(ref uvs, attr);
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
                            adduvbyidx(ref uvs, attr);
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
                            adduvbyidx(ref uvs, attr);
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
                            adduvbyidx(ref uvs, attr);
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
                            adduvbyidx(ref uvs, attr);

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
        //mesh.RecalculateTangents();
        //mesh.UploadMeshData(true);       

        mf.mesh = mesh;

        mc.sharedMesh = mesh;
    }

    void adduvbyidx(ref List<Vector2>  uvs, int idx)
    {
        //float trim = float.Epsilon;
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
        //int radius = 4;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    voxels[x, y, z] = new Voxel();
                    voxels[x, y, z].attr = 0;
                    float mag = new Vector3(x - center, y - center, z - center).magnitude;
                    if (Mathf.RoundToInt(mag) <= radius)
                    {
                        voxels[x, y, z].attr = 1;// + z % 4;
                    }

                    //if (lcheck(x, center) <= radius && lcheck(y , center) <= radius && lcheck(z , center) <= radius)
                    //if (x >= center - radius && x <= center + radius && y >= center - radius && y <= center + radius && z >= center - radius && z <= center + radius)
                    {
                        //voxels[x, y, z].attr = 1;
                    }
                }
    }

    bool picked = false;
    bool emptySpace = false;

    Vector3 dragStartPos = Vector3.zero;
    Quaternion cameraOriginRotation = Quaternion.identity;
    Vector3 axisCamRight = Vector3.right;
    Vector3 axisCamUp = Vector3.forward;

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

            dragStartPos = Input.mousePosition;
            cameraOriginRotation = Camera.main.transform.rotation;

            axisCamUp = Camera.main.transform.up;
            axisCamRight = Camera.main.transform.right;
        }

        if (picked)
        {
            //var mouseWorldPos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Ray ray = new Ray(mouseWorldPos, Camera.main.transform.forward);
            //if(ModifyByFullScan(ray))

            if (ModifyByOnePoint(ray))
            {
                BuildQuad();
            }

            //if(Physics.Raycast(mouseWorldPos, Camera.main.transform.forward, out RaycastHit hit))
        }
        else if(Input.GetMouseButton(0))
        {
            var dragDelta = dragStartPos - Input.mousePosition;

            var euler = cameraOriginRotation.eulerAngles;

            //var q1 = Quaternion.AngleAxis(-dragDelta.y / Screen.width * 180, axisCamRight);
            //var q2 = Quaternion.AngleAxis(dragDelta.x / Screen.width * 180, axisCamUp);

            euler.x = euler.x + dragDelta.y / Screen.width * 180;
            euler.y = euler.y - dragDelta.x / Screen.width * 180;

            //Camera.main.transform.rotation = q1 * q2 * cameraOriginRotation;

            Camera.main.transform.rotation = Quaternion.Euler(euler);

            float axisX = 0;
            float axisY = 0;
            float axisZ = 0;

            if (Input.GetKey(KeyCode.A)) axisX = -1;
            if (Input.GetKey(KeyCode.D)) axisX = 1;
            if (Input.GetKey(KeyCode.Q)) axisY = -1;
            if (Input.GetKey(KeyCode.E)) axisY = 1;
            if (Input.GetKey(KeyCode.S)) axisZ = -1;
            if (Input.GetKey(KeyCode.W)) axisZ = 1;

            

            if (
                Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.Q) ||
                Input.GetKeyDown(KeyCode.E) ||
                Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.W)
            )
            {
                cameraSpeed = 1.0f;
            }                

            float delta = Time.deltaTime * cameraSpeed;

            if (axisX != 0 || axisY != 0 || axisZ != 0)
            {
                cameraSpeed += Time.deltaTime;
            }

            Camera.main.transform.Translate(Vector3.right * axisX * delta, Space.Self);
            Camera.main.transform.Translate(Vector3.up * axisY * delta, Space.Self);
            Camera.main.transform.Translate(Vector3.forward * axisZ * delta, Space.Self);
        }

        

        if (Input.GetMouseButtonUp(0))
        {
            picked = false;
            emptySpace = false;
        }

    }


    float cameraSpeed = 1.0f;
    public Vector3 pickpos = Vector3.zero;

    private bool ModifyByOnePoint(Ray ray)
    {
        int center = width / 2;
        bool hit = false;

        //if (Physics.Raycast(ray, out var hitinfo, LayerMask.NameToLayer("Voxel")))
        if (Physics.Raycast(ray, out var hitinfo))
        {
            hit = true;
            pickpos = hitinfo.point;
            //Debug.Log("HIT");

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
            //Debug.Log("0");
        }

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
                                if (voxelsExpand[x, y, z].attr > 0)
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
                    voxelsExpand[x, y, z].attr = voxels[x, y, z].attr;
                }

        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    if(voxels[x, y, z].attr != 0)
                    {
                        F(x, y, z);
                    }                    
                }
    }

    private void F(int x0, int y0, int z0)
    {
        int r = radius;

        for (int x = x0 - r; x <= x0 + r; x++)
            for (int y = y0 - r; y <= y0 + r; y++)
                for (int z = z0 - r; z <= z0 + r; z++)
                {
                    if (voxelsExpand[x, y, z].attr > 0) continue;

                    int dx = x - x0;
                    int dy = y - y0;
                    int dz = z - z0;

                    int sq = dx * dx + dy * dy + dz * dz;
                    if (sq > r * r) continue;

                    if (0 <= x && x < width && 0 <= y && y < width && 0 <= z && z < width)
                    {
                        voxelsExpand[x, y, z].attr = paintcolor;
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
                    voxels[x, y, z].attr = voxelsExpand[x, y, z].attr;
                }

        BuildQuad();
    }

    public TextMeshProUGUI txtBrush;
}
