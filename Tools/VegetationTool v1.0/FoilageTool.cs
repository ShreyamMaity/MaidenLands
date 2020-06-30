using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Location_Tool;
using System;

namespace FoilageSystem
{
    public static class Utils
    {
        public static bool CheckSplatmap(Texture2D mapA = null, Texture2D mapB = null)
        {
            if (mapA == null) return false;

            FixTextureSettings(mapA);
            if (mapA.height != mapA.width || Mathf.ClosestPowerOfTwo(mapA.width) != mapA.width)
            {
                Debug.Log("Splat A: -> height and width must be equal and a power of two");
                return false;
            }

            if (mapB == null) return false;
            FixTextureSettings(mapB);
            if (mapB.height != mapB.width || Mathf.ClosestPowerOfTwo(mapB.width) != mapB.width)
            {
                Debug.Log("Splat B: -> height and width must be equal and a power of two");
                return false;
            }
            return true;
        }

        public static void FixTextureSettings(Texture2D texture)
        {
            if (texture == null) { Debug.LogError("FixFormat failed - Texture is null"); return; }
            string path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) { Debug.LogError("FixFormat failed - Texture path is null"); return; }

            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (!textureImporter.isReadable)
            {
                Debug.Log("File:" + path + " needs fixing: wrong texture format or not marked as read/write allowed");
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }

    [System.Serializable]
    public class GrassAndGroundCover
    {
        // timer for timing method durations
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        Terrain myTerrain;

        public Texture2D grassMap;
        public Texture2D treeMap;

        public float grassDensity = 0.5f;
        public float grassClumping = 0.5f;
        public float bushDensity = 0.5f;

        public void Generate()
        {
            if (Utils.CheckSplatmap(grassMap))
            {
                ApplyGrassmap();
            }
        }

        void ApplyGrassmap()
        {
            StartTimer();

            if (!myTerrain) myTerrain = Terrain.activeTerrain;
            if (myTerrain == null) { Debug.LogError("No terrain selected"); return; }

            TerrainData terrainData = myTerrain.terrainData;

            if (grassMap != null)
            {
                if (SetDetailmap(grassMap, grassDensity, 0, grassClumping, "Grass map")) Debug.Log("Grass map applied " + GetTimerTime());
            }

            //if (bushmap != null)
            //{
            //    //if (SetDetailmap(bushmap, bushmod, 3, 0.0f, "Bush map")) Debug.Log("Bush map applied.");
            //    if (SetDetailmap(bushmap, bushDensity, grassmap == null ? 0 : 3, 0.0f, "Bush map")) Debug.Log("Bush map applied " + GetTimerTime());
            //}
            EditorUtility.ClearProgressBar();
        }

        private bool SetDetailmap(Texture2D map, float mod, int firstlayer, float clumping, string MapName)
        {
            if (!myTerrain) myTerrain = Terrain.activeTerrain;
            TerrainData terrainData = myTerrain.terrainData;

            if (terrainData.detailPrototypes.Length < 3)
            {
                if (terrainData.detailPrototypes.Length < 1)
                {
                    Debug.LogError("You need to add at least 1 detail textures or 1 detail meshes to Terrain (at Paint Details)");
                    return false;
                }
                Debug.LogWarning("You should add 3 detail textures or 3 detail meshes to Terrain to use all splat map color channels (at Paint Details)");
            }

            // validate terrain details count
            int detailTextureCount = 0;
            int detailMeshCount = 0;
            int maxDetailMeshes = terrainData.detailPrototypes.Length;

            for (int nn = 0; nn < terrainData.detailPrototypes.Length; nn++)
            {
                if (terrainData.detailPrototypes[nn].usePrototypeMesh)
                {
                    detailMeshCount++;
                }
                else
                {
                    detailTextureCount++;
                }
            }

            // check if there are any details for terrain
            if (MapName == "Grass map")
            {
                if (detailTextureCount < 1)
                {
                    Debug.LogError("Grass map needs at least 1 detail texture at Terrain - Paint Details tab");
                    return false;
                }
            }

            if (MapName == "Bush map")
            {
                if (detailMeshCount < 1)
                {
                    Debug.LogError("Bush map at least 1 detail meshes at Terrain - Paint Details tab");
                    return false;
                }
            }

            Color[] detailColors = map.GetPixels();
            int width = map.width;
            int res = terrainData.detailResolution;

            int[,] detail_a = new int[res, res];
            int[,] detail_b = new int[res, res];
            int[,] detail_c = new int[res, res];

            float scale = (float)width / (float)res;

            for (int y = 0; y < res; y++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Applying " + MapName, "Calculating...", Mathf.InverseLerp(0.0f, (float)res, (float)y)))
                {
                    EditorUtility.ClearProgressBar();
                    return false;
                }

                for (int x = 0; x < res; x++)
                {
                    // place detail, depending on colours in map image
                    int sx = Mathf.FloorToInt((float)(x) * scale);
                    int sy = Mathf.FloorToInt((float)(y) * scale);

                    Color col = detailColors[sx * width + sy];

                    detail_a[x, y] = DetailValue(col.r * mod);
                    if (maxDetailMeshes > 2) detail_b[x, y] = DetailValue(col.g * mod);
                    if (maxDetailMeshes > 3) detail_c[x, y] = DetailValue(col.b * mod);
                }
            }

            if (clumping > 0.01f)
            {
                detail_a = MakeClumps(detail_a, clumping);
                if (maxDetailMeshes > 2) detail_b = MakeClumps(detail_b, clumping);
                if (maxDetailMeshes > 3) detail_c = MakeClumps(detail_c, clumping);
            }

            terrainData.SetDetailLayer(0, 0, firstlayer + 0, detail_a);
            if (maxDetailMeshes > 2) terrainData.SetDetailLayer(0, 0, firstlayer + 1, detail_b);
            if (maxDetailMeshes > 3) terrainData.SetDetailLayer(0, 0, firstlayer + 2, detail_c);

            return true;
        }

        int DetailValue(float col)
        {
            // linearly translates a 0.0-1.0 number to a 0-16 integer
            int c = Mathf.FloorToInt(col * 16);
            float r = col * 16 - Mathf.FloorToInt(col * 16);

            if (r > UnityEngine.Random.Range(0.0f, 1.0f)) c++;
            return Mathf.Clamp(c, 0, 16);
        }

        int[,] MakeClumps(int[,] map, float clumping)
        {
            int res = map.GetLength(0);
            int[,] clumpmap = new int[res, res];

            // init - there's probably a better way to do this in C# that I just don't know
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    clumpmap[x, y] = 0;
                }
            }

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    clumpmap[x, y] += map[x, y];
                    if (map[x, y] > 0)
                    {
                        // there's grass here, we might want to raise the grass value of our neighbours
                        for (int a = -1; a <= 1; a++) for (int b = -1; b <= 1; b++)
                            {
                                if (x + a < 0 || x + a >= res || y + b < 0 || y + b >= res) continue;
                                if (a != 0 || b != 0 && UnityEngine.Random.Range(0.0f, 1.0f) < clumping) clumpmap[x + a, y + b]++;
                            }
                    }
                }
            }

            // values above 9 yield strange results
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    if (clumpmap[x, y] > 9) clumpmap[x, y] = 9;
                }
            }

            return clumpmap;
        }

        void StartTimer()
        {
            stopwatch.Reset();
            stopwatch.Start();
        }

        string GetTimerTime()
        {
            stopwatch.Stop();
            return " (" + stopwatch.Elapsed.Milliseconds + "ms)";
        }
    }
}
