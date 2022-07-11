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
        mc = gameObject.AddComponent<MeshCollider>();
        VoxelModifier.BuildSphereBuffer(ref voxels, width);
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
        var mesh = QuadGenerator.VoxelToMesh(voxels, width);
        mf.mesh = mesh;
        mc.sharedMesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
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
            cameraContoller.Drag();
        }

        // up
        if (Input.GetMouseButtonUp(0))
        {
            picked = false;
            emptySpace = false;
        }

        // expand test 
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

        if (Physics.Raycast(ray, out var hitinfo)) //, 1000, LayerMask.NameToLayer("Voxel"), QueryTriggerInteraction.UseGlobal))
        {
            hit = true;
            pickpos = hitinfo.point;
            var localpos = transform.InverseTransformPoint(pickpos);
            var arrIdxpos = localpos + new Vector3(center, center, center);
            int x = Mathf.RoundToInt(arrIdxpos.x);
            int y = Mathf.RoundToInt(arrIdxpos.y);
            int z = Mathf.RoundToInt(arrIdxpos.z);
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

        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    if (voxels[x, y, z].attr != 0)
                    {
                        VoxelModifier.Extrude(x, y, z, radius, width, ref voxels, ref voxelsForTrim);
                    }
                }
    }


}
