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
    public CanvasRaycast canvasRaycast;

    CameraController cameraContoller;
    MeshFilter mf = null;
    MeshCollider mc = null;

    // tool state
    int radius = 1;
    bool picked = false;
    bool emptySpace = false;
    public bool lookat = false;

    public Vector3 pickpos = Vector3.zero;
    public Vector3 pickPosIndexAligned = Vector3.zero;

    public Vector2 prevMousePos = Vector2.zero;

    // data
    static int width = 64;
    Voxel[,,] voxels = new Voxel[width, width, width];
    Voxel[,,] voxelsForTrim = new Voxel[width, width, width];

    // unity method

    void Awake()
    {
        VoxelModifier.targetTransform = transform;

        cameraContoller = Camera.main.GetComponent<CameraController>();
        mf = gameObject.AddComponent<MeshFilter>();
        mc = gameObject.AddComponent<MeshCollider>();
        VoxelModifier.BuildSphereBuffer(ref voxels, width);
        CreateTrimBuffer();
        BuildQuad();
    }

    private void CreateTrimBuffer()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    voxelsForTrim[x, y, z] = new Voxel();
                    voxelsForTrim[x, y, z].attr = 0;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pickpos, 0.25f);
        Gizmos.DrawWireCube(pickPosIndexAligned + (Vector3.one * 0.5f), Vector3.one * 0.5f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(pickpos + VoxelModifier.growthDirectionAxisAligned, Vector3.one);
    }

    // implementation

    private void BuildQuad()
    {
        var mesh = QuadGenerator.VoxelToMesh(voxels, width);
        mf.mesh = mesh;
        mc.sharedMesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        // ignore by UI
        if (canvasRaycast.Hit()) return;

        // get screen-space ray
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // down
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

        // drag
        if (picked)
        {
            MouseDrag(ray);
        }
        else if (Input.GetMouseButton(0))
        {
            if (emptySpace) cameraContoller.Drag();
        }

        // up
        if (Input.GetMouseButtonUp(0))
        {
            picked = false;
            emptySpace = false;
        }

        RotateMesh();
        //TestExpand();

        if (Input.GetKey(KeyCode.P))
        {
            VoxelModifier.BuildPlaneBuffer(ref voxels, width);
            BuildQuad();
        }
    }

    private void RotateMesh()
    {
        float delta = 50 * Time.deltaTime;
        var camRight = Camera.main.transform.right;

        if (Input.GetKey(KeyCode.DownArrow)) transform.Rotate(camRight, -1.0f * delta);
        if (Input.GetKey(KeyCode.UpArrow)) transform.Rotate(camRight, 1.0f * delta);
        if (Input.GetKey(KeyCode.LeftArrow)) transform.Rotate(0, -1.0f * delta, 0, Space.World);
        if (Input.GetKey(KeyCode.RightArrow)) transform.Rotate(0, 1.0f * delta, 0, Space.World);
    }

    private void TestExpand()
    {
        if (Input.GetKey(KeyCode.F))
        {
            CopyBuffer();
            VoxelModifier.Expand(ref voxels, ref voxelsForTrim, width);
            BuildQuad();
        }
    }

    private void MouseDrag(Ray ray)
    {
        Ray ray0 = Camera.main.ScreenPointToRay(prevMousePos);

        if (Physics.Raycast(ray0, out var hitinfo0))
        {
            Vector3 worldPos0 = hitinfo0.point;
            if (Physics.Raycast(ray, out var hitinfo1))
            {
                // dab-spacing 100% interpolate
                Vector3 worldPos1 = hitinfo1.point;
                int repeatCount = (int)(Vector3.Distance(worldPos0, worldPos1) / (radius + 1));

                if (repeatCount > 1)
                {
                    for (int r = 1; r < repeatCount; r++)
                    {
                        Vector3 worldPosRepeat = Vector3.Lerp(worldPos0, worldPos1, (float)r / repeatCount);
                        Ray rayRepeat = new Ray(ray0.origin, (worldPosRepeat - ray0.origin).normalized);
                        ModifyByOnePoint(rayRepeat);
                    }
                }
            }
        }

        if (ModifyByOnePoint(ray))
        {
            BuildQuad();
        }
    }

    private bool ModifyByOnePoint(Ray ray)
    {
        int center = width / 2;
        bool hit = false;

        if (Physics.Raycast(ray, out var hitinfo))
        {
            hit = true;
            pickpos = hitinfo.point;
            VoxelModifier.growthDirectionAxisAligned = transform.InverseTransformDirection(hitinfo.normal) * 0.5f;
            var localpos = transform.InverseTransformPoint(pickpos) - VoxelModifier.growthDirectionAxisAligned;
            var arrIdxpos = localpos + new Vector3(center, center, center) - (Vector3.one * 0.5f);
            int x = Mathf.RoundToInt(arrIdxpos.x);
            int y = Mathf.RoundToInt(arrIdxpos.y);
            int z = Mathf.RoundToInt(arrIdxpos.z);

            pickPosIndexAligned = transform.TransformPoint(new Vector3(x - center, y - center, z - center));

            hit = VoxelModifier.FillDab(hit, x, y, z, radius, width, ref voxels, ref voxelsForTrim);
        }
        else
        {
            pickpos = Vector3.zero;
        }

        prevMousePos = Input.mousePosition;

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
                            voxels[x, y, z].attr = VoxelModifier.paintColor;
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
            VoxelModifier.paintColor = c;
        }
    }

    public void SetMode(int newMode)
    {
        VoxelModifier.mode = (VoxelModifier.Mode)newMode;
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
        radius = (int)radiusSlider.value;
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

        int r = radius;
        if (r == 0) r = 1;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    if (voxels[x, y, z].attr != 0)
                    {
                        VoxelModifier.Extrude(x, y, z, r, width, ref voxels, ref voxelsForTrim);
                    }
                }
    }


}
