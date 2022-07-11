using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelModifier
{
    public enum Mode
    {
        Paint, Add, Sub
    }

    public static Mode mode = Mode.Paint;
    public static int paintColor = 2;

    public static Transform targetTransform;
    public static Vector3 growthDirectionAxisAligned;

    public static void BuildSphereBuffer(ref Voxel [,,] voxels, int width)
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
                        voxels[x, y, z].attr = 1; // yellow
                    }
                }
            }
        }
    }

    public static void BuildPlaneBuffer(ref Voxel[,,] voxels, int width)
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
                    if (y == 0)
                    {
                        voxels[x, y, z].attr = 4; // blue
                    }
                }
            }
        }
    }

    public static void Expand(ref Voxel[,,] voxels, ref Voxel[,,] voxelsForTrim, int width)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                for (int z = 0; z < width; z++)
                {
                    voxels[x, y, z].attr = voxelsForTrim[x, y, z].attr;
                }
    }

    public static void Extrude(int x0, int y0, int z0, int radius, int width, ref Voxel[,,] voxels, ref Voxel[,,] voxelsForTrim)
    {
        int r = radius;

        for (int x = x0 - r; x <= x0 + r; x++)
            for (int y = y0 - r; y <= y0 + r; y++)
                for (int z = z0 - r; z <= z0 + r; z++)
                {
                    if (0 <= x && x < width && 0 <= y && y < width && 0 <= z && z < width)
                    {
                        if (voxelsForTrim[x, y, z].attr > 0) continue;

                        int dx = x - x0;
                        int dy = y - y0;
                        int dz = z - z0;

                        int sq = dx * dx + dy * dy + dz * dz;
                        if (sq > r * r) continue;
                        voxelsForTrim[x, y, z].attr = paintColor;
                    }
                }
    }

    static bool FillSingle(bool hit, int x, int y, int z, int width, ref Voxel[,,] voxels, ref Voxel[,,] voxelsForTrim)
    {
        if (0 <= x && x < width && 0 <= y && y < width && 0 <= z && z < width)
        {
            if (voxels[x, y, z].attr > 0)
            {
                if (mode == Mode.Paint)
                    voxels[x, y, z].attr = paintColor;
                else if (mode == Mode.Sub)
                    voxels[x, y, z].attr = 0;
                hit = true;
            }

            if (mode == Mode.Add)
            {
                int newx = x;
                int newy = y;
                int newz = z;

                if (growthDirectionAxisAligned.x > 0) newx += 1;
                if (growthDirectionAxisAligned.x < 0) newx -= 1;
                if (growthDirectionAxisAligned.y > 0) newy += 1;
                if (growthDirectionAxisAligned.y < 0) newy -= 1;
                if (growthDirectionAxisAligned.z > 0) newz += 1;
                if (growthDirectionAxisAligned.z < 0) newz -= 1;

                if (0 <= newx && newx < width && 0 <= newy && newy < width && 0 <= newz && newz < width)
                {
                    if (voxelsForTrim[newx, newy, newz].attr > 0)
                        voxels[newx, newy, newz].attr = paintColor;
                }
            }
            
        }
        return hit;
    }

    private static void AddToCamPos(int x, int y, int z, int width, Voxel[,,] voxels)
    {
        // to cam position
        var growthDirection = targetTransform.InverseTransformDirection(-Camera.main.transform.forward);
        growthDirection.Normalize();
        float maxLength = 0;

        // align to local axis
        var growthDirectionAxisAligned = Vector3.zero;

        if (Mathf.Abs(growthDirection.x) > maxLength)
        {
            growthDirectionAxisAligned = Vector3.right * Mathf.Sign(growthDirection.x);
            maxLength = Mathf.Abs(growthDirection.x);
        }
        if (Mathf.Abs(growthDirection.y) > maxLength)
        {
            growthDirectionAxisAligned = Vector3.up * Mathf.Sign(growthDirection.y);
            maxLength = Mathf.Abs(growthDirection.y);
        }
        if (Mathf.Abs(growthDirection.z) > maxLength)
        {
            growthDirectionAxisAligned = Vector3.right * Mathf.Sign(growthDirection.z);
            maxLength = Mathf.Abs(growthDirection.z);
        }

        int newx = x + (int)growthDirectionAxisAligned.x;
        int newy = y + (int)growthDirectionAxisAligned.y;
        int newz = z + (int)growthDirectionAxisAligned.z;

        if (0 <= newx && newx < width && 0 <= newy && newy < width && 0 <= newz && newz < width)
        {
            voxels[newx, newy, newz].attr = paintColor;
        }
    }

    public static bool FillDab(bool hit, int x0, int y0, int z0, int radius, int width, ref Voxel[,,] voxels, ref Voxel[,,] voxelsForTrim)
    {
        int r = radius;

        if(r == 0)
        {
            // single voxel add/sub mode
            return FillSingle(hit, x0,y0,z0,width,ref voxels, ref voxelsForTrim);
        }

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
                            if (mode == Mode.Paint)
                                voxels[x, y, z].attr = paintColor;
                            else if (mode == Mode.Sub)
                                voxels[x, y, z].attr = 0;
                            hit = true;
                        }
                        else
                        {
                            if (mode == Mode.Add)
                            {
                                if (voxelsForTrim[x, y, z].attr > 0)
                                {
                                    voxels[x, y, z].attr = paintColor;
                                }
                            }
                        }
                    }
                }

        return hit;
    }
}
