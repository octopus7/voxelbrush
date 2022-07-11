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
                    if (voxelsForTrim[x, y, z].attr > 0) continue;

                    int dx = x - x0;
                    int dy = y - y0;
                    int dz = z - z0;

                    int sq = dx * dx + dy * dy + dz * dz;
                    if (sq > r * r) continue;

                    if (0 <= x && x < width && 0 <= y && y < width && 0 <= z && z < width)
                    {
                        voxelsForTrim[x, y, z].attr = paintColor;
                    }
                }
    }

    public static bool FillDab(bool hit, int x0, int y0, int z0, int radius, int width, ref Voxel[,,] voxels, ref Voxel[,,] voxelsForTrim)
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
