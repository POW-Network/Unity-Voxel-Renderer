using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMap
{
    private Voxel[,,] map;

    public VoxelMap(Vector3Int size)
    {
        map = new Voxel[size.x, size.y, size.z];
    }

    public VoxelMap(int width, int height, int depth) : this(new Vector3Int(width, height, depth)) { }

    public Vector3Int Size
    {
        get
        {
            return new Vector3Int(map.GetLength(0), map.GetLength(1), map.GetLength(2));
        }
    }

    public Voxel this[int x, int y, int z]
    {
        get
        {
            return map[x, y, z];
        }
        set
        {
            map[x, y, z] = value;
        }
    }

    public Voxel this[Vector3Int coord]
    {
        get
        {
            return this[coord.x, coord.y, coord.z];
        }
        set
        {
            this[coord.x, coord.y, coord.z] = value;
        }
    }
    
    public bool IsSolid(int x, int y, int z)
    {
        return this[x, y, z] != null;
    }

    public bool IsSolid(Vector3Int coord)
    {
        return this[coord] != null;
    }

    public Voxel CreateVoxel(int x, int y, int z)
    {
        return CreateVoxel(new Vector3Int(x, y, z));
    }

    public Voxel CreateVoxel(Vector3Int coord)
    {
        this[coord] = new Voxel(this, coord);
        return this[coord];
    }

    public void RemoveVoxel(int x, int y, int z)
    {
        this[x, y, z] = null;
    }

    public void RemoveVoxel(Vector3Int coord)
    {
        this[coord] = null;
    }

    public void CreateMesh(Mesh mesh, float voxelSize)
    {
        mesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        for (int y = 0; y <= Size.y; y++)
        {
            for (int z = 0; z <= Size.z; z++)
            {
                for (int x = 0; x <= Size.x; x++)
                {
                    vertices.Add(new Vector3(x, y, z) * voxelSize);
                }
            }
        }

        List<int> triangles = new List<int>();
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                for (int z = 0; z < Size.z; z++)
                {
                    Voxel vox = this[x, y, z];
                    if (vox == null)
                    {
                        continue;
                    }
                    if (!vox.VoxelTowards(Vector3.up.ConvertToInt()))
                    {
                        triangles.AddRange(new int[] { GetVertexNumber(x, y + 1, z), GetVertexNumber(x, y + 1, z + 1), GetVertexNumber(x + 1, y + 1, z) });
                        triangles.AddRange(new int[] { GetVertexNumber(x + 1, y + 1, z), GetVertexNumber(x, y + 1, z + 1), GetVertexNumber(x + 1, y + 1, z + 1) });
                    }
                    if (!vox.VoxelTowards(Vector3.down.ConvertToInt()))
                    {
                        triangles.AddRange(new int[] { GetVertexNumber(x, y, z), GetVertexNumber(x + 1, y, z), GetVertexNumber(x, y, z + 1) });
                        triangles.AddRange(new int[] { GetVertexNumber(x + 1, y, z), GetVertexNumber(x + 1, y, z + 1), GetVertexNumber(x, y, z + 1) });
                    }
                    if (!vox.VoxelTowards(Vector3.forward.ConvertToInt()))
                    {
                        triangles.AddRange(new int[] { GetVertexNumber(x, y, z + 1), GetVertexNumber(x + 1, y + 1, z + 1), GetVertexNumber(x, y + 1, z + 1) });
                        triangles.AddRange(new int[] { GetVertexNumber(x + 1, y, z + 1), GetVertexNumber(x + 1, y + 1, z + 1), GetVertexNumber(x, y, z + 1) });
                    }
                    if (!vox.VoxelTowards(Vector3.back.ConvertToInt()))
                    {
                        triangles.AddRange(new int[] { GetVertexNumber(x, y, z), GetVertexNumber(x, y + 1, z), GetVertexNumber(x + 1, y + 1, z) });
                        triangles.AddRange(new int[] { GetVertexNumber(x + 1, y + 1, z), GetVertexNumber(x + 1, y, z), GetVertexNumber(x, y, z) });
                    }
                    if (!vox.VoxelTowards(Vector3.right.ConvertToInt()))
                    {
                        triangles.AddRange(new int[] { GetVertexNumber(x + 1, y, z), GetVertexNumber(x + 1, y + 1, z), GetVertexNumber(x + 1, y + 1, z + 1) });
                        triangles.AddRange(new int[] { GetVertexNumber(x + 1, y, z + 1), GetVertexNumber(x + 1, y, z), GetVertexNumber(x + 1, y + 1, z + 1) });
                    }
                    if (!vox.VoxelTowards(Vector3.left.ConvertToInt()))
                    {
                        triangles.AddRange(new int[] { GetVertexNumber(x, y, z), GetVertexNumber(x, y + 1, z + 1), GetVertexNumber(x, y + 1, z) });
                        triangles.AddRange(new int[] { GetVertexNumber(x, y + 1, z + 1), GetVertexNumber(x, y, z), GetVertexNumber(x, y, z + 1) });
                    }
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public Mesh CreateMesh(float voxelSize)
    {
        Mesh mesh = new Mesh();
        CreateMesh(mesh, voxelSize);
        return mesh;
    }

    public void CreateMesh(Mesh mesh)
    {
        CreateMesh(mesh, 1f);
    }

    public Mesh CreateMesh()
    {
        return CreateMesh(1f);
    }

    private int GetVertexNumber(int x, int y, int z)
    {
        return y * (Size.z + 1) * (Size.x + 1) + z * (Size.x + 1) + x;
    }
}

public class Voxel
{
    private VoxelMap map;
    private Vector3Int position;

    public Voxel(VoxelMap map, Vector3Int position)
    {
        this.map = map;
        this.position = position;
    }

    public bool VoxelTowards(Vector3Int axis)
    {
        if (axis == Vector3.up.ConvertToInt())
        {
            if (position.y >= map.Size.y - 1)
            {
                return false;
            }
            else
            {
                return map.IsSolid(position + axis);
            }
        }
        else if (axis == Vector3.forward.ConvertToInt())
        {
            if (position.z >= map.Size.z - 1)
            {
                return false;
            }
            else
            {
                return map.IsSolid(position + axis);
            }
        }
        else if (axis == Vector3.right.ConvertToInt())
        {
            if (position.x >= map.Size.x - 1)
            {
                return false;
            }
            else
            {
                return map.IsSolid(position + axis);
            }
        }
        else if (axis == Vector3.down.ConvertToInt())
        {
            if (position.y <= 0)
            {
                return false;
            }
            else
            {
                return map.IsSolid(position + axis);
            }
        }
        else if (axis == Vector3.back.ConvertToInt())
        {
            if (position.z <= 0)
            {
                return false;
            }
            else
            {
                return map.IsSolid(position + axis);
            }
        }
        else if (axis == Vector3.left.ConvertToInt())
        {
            if (position.x <= 0)
            {
                return false;
            }
            else
            {
                return map.IsSolid(position + axis);
            }
        }
        Debug.LogError("Unsupported VoxelTowards axis: " + axis);
        return false;
    }
}

public class VoxelRenderer : MonoBehaviour
{
    public Vector3Int size = new Vector3Int(20, 20, 20);
    public float voxelSize = 1f;
    public float solidThreshold = 0.5f;

    [Header("Perlin Noise")]
    public Vector3 perlinMultiplier = new Vector3(1f, 1f, 1f);
    public Vector3 perlinPosition;

    VoxelMap voxelMap;

    // Start is called before the first frame update
    public void GenerateMap(Vector3 position)
    {

        voxelMap = new VoxelMap(size);
        for (int x = 0; x < size.x; x ++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    Vector3 currentPosition = new Vector3(x, y, z);
                    currentPosition.Scale(perlinMultiplier);
                    //float perlinValue = Perlin.Noise(perlinPosition + currentPosition);
                    Vector3 usePosition = position + currentPosition;
                    float perlinValue1 = Mathf.PerlinNoise(usePosition.x, usePosition.y);
                    float perlinValue2 = Mathf.PerlinNoise(usePosition.z, usePosition.y);
                    float perlinValue = (perlinValue1 + perlinValue2) / 2;
                    if (perlinValue >= solidThreshold)
                    {
                        voxelMap.CreateVoxel(x, y, z);
                    }
                }
            }
        }

        Mesh mesh = voxelMap.CreateMesh(voxelSize);
        mesh.name = "VoxelMesh";
        //mesh.Optimize();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    // Update is called once per frame
    void OnValidate()
    {
        GenerateMap(perlinPosition);
    }
}
