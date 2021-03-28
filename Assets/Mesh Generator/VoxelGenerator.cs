using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGenerator : MonoBehaviour
{
    MeshFilter filter;
    Mesh mesh;
    MeshCollider collider;
    
    public int[,,] voxelArray;
    public Color[,,] colorArray;
    
    public Vector3Int chunkSize;
    
    public GameObject boundingBox;
    
    public Transform pivotPoint;
    
    public Texture3D highlightVolumeTexture;
    public Texture3D colorVolumeTexture;
    
    public Vector3Int sliceVector;

    // Start is called before the first frame update
    void Start()
    {
        filter = GetComponent<MeshFilter>();
        collider = GetComponent<MeshCollider>();
        mesh = new Mesh();
        filter.mesh = mesh;
       
        //Init(chunkSize, new Dictionary<Vector3Int, int[,]>());
    }
    
    public void Init(Vector3Int chunkSize, Texture3D indicatorVolumeTexture, bool initEmpty = false, int[,,] voxelArray = null, Color[,,] colorArray = null) {
        this.chunkSize = chunkSize;
        
        //Initialize voxelArray
        if (voxelArray == null) {
            this.voxelArray = new int[chunkSize.x,chunkSize.y,chunkSize.z]; 
            
            for (int x = 0; x < chunkSize.x; x++) {
                for (int y = 0; y < chunkSize.y; y++) {
                    for (int z = 0; z < chunkSize.z; z++) {
                        if (initEmpty) {
                            this.voxelArray[x,y,z] = 0;
                        } else {
                            this.voxelArray[x,y,z] = 1;
                        }
                    }
                }
            }
        } else {
            this.voxelArray = voxelArray;
        }
        
        //Initialize colorArray
        if (colorArray == null) {
            this.colorArray = new Color[chunkSize.x,chunkSize.y,chunkSize.z]; 
            for (int x = 0; x < chunkSize.x; x++) {
                for (int y = 0; y < chunkSize.y; y++) {
                    for (int z = 0; z < chunkSize.z; z++) {
                        this.colorArray[x,y,z] = new Color(0,0,0,0);
                    }
                }
            }
        } else {
            this.colorArray = colorArray;
        }
        this.colorVolumeTexture = new Texture3D(chunkSize.x, chunkSize.y, chunkSize.z, TextureFormat.RGBA32, 0);
        
        this.highlightVolumeTexture = indicatorVolumeTexture;
        
        /*
        highlightVolumeTexture = new Texture3D(chunkSize, chunkSize, chunkSize, TextureFormat.RGBA32, 0);
        Color[] initColors = new Color[chunkSize*chunkSize*chunkSize];
        //System.Random random = new System.Random();
        for (int i = 0; i < chunkSize*chunkSize*chunkSize; i++) {
            initColors[i] = new Color((float)random.Next(0,7)/((float)chunkSize+1.0f),(float)random.Next(0,7)/((float)chunkSize+1.0f),(float)random.Next(0,7)/((float)chunkSize+1.0f),0.0f);
        }
        highlightVolumeTexture.SetPixels(initColors, 0);
        //highlightVolumeTexture.SetPixel(0,0,0,new Color(0.0f,1.0f,1.0f,1.0f),0);
        highlightVolumeTexture.Apply();
        */
        
        Camera.main.transform.position = new Vector3(chunkSize.x/2, chunkSize.y/2, -chunkSize.z * 2.5f);
        
        sliceVector = new Vector3Int(0,0,0);
        
        //Setup bounding box
        foreach (LineRenderer r in boundingBox.GetComponentsInChildren<LineRenderer>()) {
            if (r.name == "x") {
                r.transform.localScale = new Vector3(0, chunkSize.x, 0);
            } else if (r.name == "y") {
                r.transform.localScale = new Vector3(0, chunkSize.y, 0);
            } else if (r.name == "z") {
                r.transform.localScale = new Vector3(0, chunkSize.z, 0);
            }
            
            Vector3 tempPos = r.transform.position;
            for (int i = 0; i < 3; i++) {
                if (tempPos[i] != -0.5f) {
                    tempPos[i] = chunkSize[i] - 0.5f;
                }
            }
            r.transform.position = tempPos;
        }
        
        GenerateAndApplyMesh();
    }
    
    public void UpdateColorVolumeTexture() {
        for (int x = 0; x < chunkSize.x; x++) {
            for (int y = 0; y < chunkSize.y; y++) {
                for (int z = 0; z < chunkSize.z; z++) {
                    colorVolumeTexture.SetPixel(x, y, z, colorArray[x,y,z], 0);
                }
            }
        }
        colorVolumeTexture.Apply();
    }
    
    int GetVoxel(Vector3Int pos) {
        if (sliceVector.x + sliceVector.y + sliceVector.z <= 0) {
            if (pos.x >= 0 && pos.y >= 0 && pos.z >= 0 && pos.x < chunkSize.x + sliceVector.x && pos.y < chunkSize.y + sliceVector.y && pos.z < chunkSize.z + sliceVector.z) {
                return voxelArray[pos.x,pos.y,pos.z];
            } else {
                //Out of bounds
                return -1;
            }
        } else {
            if (pos.x >= sliceVector.x && pos.y >= sliceVector.y && pos.z >= sliceVector.z && pos.x < chunkSize.x && pos.y < chunkSize.y && pos.z < chunkSize.z) {
                return voxelArray[pos.x,pos.y,pos.z];
            } else {
                //Out of bounds
                return -1;
            }
        }
    }
    
    public void RemoveVoxels(List<Vector3Int> voxels) {
        for (var i = 0; i < voxels.Count; i++) {
            Vector3Int pos = new Vector3Int((int) voxels[i].x, (int) voxels[i].y, (int) voxels[i].z);
            
            if (GetVoxel(pos) != -1) {
                voxelArray[pos.x,pos.y,pos.z] = 0;
                colorArray[pos.x,pos.y,pos.z] = new Color(0,0,0,0);
            }
        }
        
        GenerateAndApplyMesh();
    }
    
    private bool SolidInPlane(Vector3Int plane, int i) {
        var normalIndices = new Dictionary<Vector3Int, int[]>() {
            {Vector3Int.up, new int[]{0,2}},
            {Vector3Int.right, new int[]{1,2}},
            {new Vector3Int(0,0,1), new int[]{0,1}}
        };
        
        int[] normalIdx = normalIndices[plane];

        for (int x = 0; x < voxelArray.GetLength(normalIdx[0]); x++)
        {
            for (int y = 0; y < voxelArray.GetLength(normalIdx[1]); y++)
            {
                int[] idx = new int[]{0,0,0};
                idx[Array.IndexOf(new int[]{plane.x,plane.y,plane.z}, 1)] = i;
                idx[Array.IndexOf(new int[]{plane.x,plane.y,plane.z}, 0)] = x;
                idx[Array.LastIndexOf(new int[]{plane.x,plane.y,plane.z}, 0)] = y;
                
                if ((int)voxelArray.GetValue(idx) != 0) {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    public void ShiftToOrigin() {
        (Vector3Int bbLowest, Vector3Int bbHighest) = calculateBoundingBox();
        
        if (bbLowest.x + bbLowest.y + bbLowest.z > 0) {
            //Shift everything so it's at 0
            int[,,] voxelArrayShifted = new int[voxelArray.GetLength(0),voxelArray.GetLength(1),voxelArray.GetLength(2)];
            Color[,,] colorArrayShifted = new Color[colorArray.GetLength(0),colorArray.GetLength(1),colorArray.GetLength(2)];
            
            for (int x = 0; x < voxelArray.GetLength(0); x++) {
                for (int y = 0; y < voxelArray.GetLength(1); y++) {
                    for (int z = 0; z < voxelArray.GetLength(2); z++) {
                        if (GetVoxel(new Vector3Int(x,y,z)) == 1) {
                            voxelArrayShifted[x - bbLowest.x, y - bbLowest.y, z - bbLowest.z] = 1;
                            colorArrayShifted[x - bbLowest.x, y - bbLowest.y, z - bbLowest.z] = colorArray[x,y,z];
                        }
                    }
                }
            }
            voxelArray = voxelArrayShifted;
            colorArray = colorArrayShifted;
        }
        
        GenerateAndApplyMesh();
    }
    
    public bool AddVoxel(Vector3Int pos) {
        (Vector3Int bbLowest, Vector3Int bbHighest) = calculateBoundingBox();
        pos = pos - bbLowest;
        
        ShiftToOrigin();
        
        if (GetVoxel(pos) != -1) { //Not out of bounds
            voxelArray[pos.x,pos.y,pos.z] = 1;
            GenerateAndApplyMesh();
            return true;
        } else if (pos.x < chunkSize.x && pos.y < chunkSize.y && pos.z < chunkSize.z) { 
            //Out of bounds but in the negative direction
            //Shift everything if out of bounds and possible
            Vector3Int plane = Vector3Int.zero;
            if (pos.x < 0)
                plane = new Vector3Int(1,0,0);
            if (pos.y < 0)
                plane = new Vector3Int(0,1,0);
            if (pos.z < 0)
                plane = new Vector3Int(0,0,1);
            
            if (bbHighest[Array.IndexOf(new int[3]{plane.x,plane.y,plane.z},1)]
                    == chunkSize[Array.IndexOf(new int[3]{plane.x,plane.y,plane.z},1)] - 1) {
                //Cannot shift any further
                //Should have some sort of alert (return bool)
                return false;
            } else {
                //Shift everything in the direction of the plane one over, then set the pos (shifted)
                int[,,] voxelArrayShifted = new int[voxelArray.GetLength(0),voxelArray.GetLength(1),voxelArray.GetLength(2)];
                Color[,,] colorArrayShifted = new Color[colorArray.GetLength(0),colorArray.GetLength(1),colorArray.GetLength(2)];
                
                for (int x = 0; x < voxelArray.GetLength(0); x++) {
                    for (int y = 0; y < voxelArray.GetLength(1); y++) {
                        for (int z = 0; z < voxelArray.GetLength(2); z++) {
                            if (GetVoxel(new Vector3Int(x,y,z)) == 1) {
                                voxelArrayShifted[x + plane.x, y + plane.y, z + plane.z] = 1;
                                colorArrayShifted[x + plane.x, y + plane.y, z + plane.z] = colorArray[x,y,z];
                            }
                        }
                    }
                }
                voxelArray = voxelArrayShifted;
                colorArray = colorArrayShifted;
                
                voxelArray[pos.x+plane.x,pos.y+plane.y,pos.z+plane.z] = 1;
                GenerateAndApplyMesh();
                return true;
            }
        }
        
        return false;
    }
    
    public int CountVoxels() {
        int voxels = 0;
        
        for (int x = 0; x < chunkSize.x; x++) {
            for (int y = 0; y < chunkSize.y; y++) {
                for (int z = 0; z < chunkSize.z; z++) {
                    if (GetVoxel(new Vector3Int(x,y,z)) == 1) {
                        voxels += 1;
                    }
                }
            }
        }
        return voxels;
    }
    
    Vector3 calculatePivotPoint() {
        //Vector3 averagePoint = Vector3.zero;
        //int voxels = 0;
        //
        //for (int x = 0; x < chunkSize.x; x++) {
        //    for (int y = 0; y < chunkSize.y; y++) {
        //        for (int z = 0; z < chunkSize.z; z++) {
        //            if (GetVoxel(new Vector3Int(x,y,z)) > 0) {//(voxelArray[x,y,z] > 0) {
        //                averagePoint += new Vector3(x,y,z);
        //                voxels++;
        //            }
        //        }
        //    }
        //}
        //    
        //if (voxels == 0) {
        //    return Vector3.zero;
        //}
        //
        //return averagePoint/voxels;
        
        (Vector3 bbLowest, Vector3 bbHighest) = calculateBoundingBox();
        
        return (bbLowest + bbHighest)/2;
    }
        
    public (Vector3Int, Vector3Int) calculateBoundingBox() {
        //TODO SOMETHING WRONG HERE???
        Vector3Int lowestPoint = 1000*Vector3Int.one;
        Vector3Int highestPoint = Vector3Int.zero;
        int voxels = 0;
        
        for (int x = 0; x < chunkSize.x; x++) {
            for (int y = 0; y < chunkSize.y; y++) {
                for (int z = 0; z < chunkSize.z; z++) {
                    if (GetVoxel(new Vector3Int(x,y,z)) == 1) {
                        if (lowestPoint.x > x)
                            lowestPoint.x = x;
                        if (lowestPoint.y > y)
                            lowestPoint.y = y;
                        if (lowestPoint.z > z)
                            lowestPoint.z = z;
                        
                        if (highestPoint.x < x)
                            highestPoint.x = x;
                        if (highestPoint.y < y)
                            highestPoint.y = y;
                        if (highestPoint.z < z)
                            highestPoint.z = z;
                        
                        voxels += 1;
                    }
                }
            }
        }
        
        if (voxels == 0) {
            return (Vector3Int.zero, Vector3Int.zero);
        }
            
        return (lowestPoint, highestPoint);
    }
    
    public void GenerateAndApplyMesh() {
        mesh.Clear();
        
        var (vertices_, normals_, uvs_, triangles_, colors_) = GenerateMesh();
        
        mesh.SetVertices(vertices_);
        mesh.SetNormals(normals_);
        mesh.SetUVs(0, uvs_);
        mesh.SetTriangles(triangles_,0);
        mesh.SetColors(colors_);
        
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        
        collider.sharedMesh = mesh;
        
        pivotPoint.position = calculatePivotPoint();
        Vector3Int bbHighest = calculateBoundingBox().Item2;
        FindObjectOfType<CameraPanScript>().maxSize = Mathf.Max(bbHighest.x, bbHighest.y, bbHighest.z);
        
        //Debug.Log(calculateBoundingBox().Item1);
        //Debug.Log(calculateBoundingBox().Item2);
        
        GetComponent<MeshRenderer>().material.SetVector("_OutlineCenter", new Vector4(pivotPoint.position.x, pivotPoint.position.y, pivotPoint.position.z, 0.0f));
        GetComponent<MeshRenderer>().material.SetVector("_ChunkSize", new Vector4(chunkSize.x, chunkSize.y, chunkSize.z, 0.0f));
        
        GetComponent<MeshRenderer>().material.SetTexture("_HighlightVolumeTex", highlightVolumeTexture);
        
        UpdateColorVolumeTexture();
        GetComponent<MeshRenderer>().material.SetTexture("_ColorVolumeTex", colorVolumeTexture);
    }
    
    (List<Vector3>, List<Vector3>, List<Vector2>, List<int>, List<Color>) GenerateMesh() {
        List<Vector3> vertices_ = new List<Vector3>();
        List<Vector3> normals_ = new List<Vector3>();
        List<Vector2> uvs_ = new List<Vector2>();
        List<int> triangles_ = new List<int>();
        List<Color> colors_ = new List<Color>();
        
        for (int x = 0; x < voxelArray.GetLength(0); x++) {
            for (int y = 0; y < voxelArray.GetLength(1); y++) {
                for (int z = 0; z < voxelArray.GetLength(2); z++) {
                    if (GetVoxel(new Vector3Int(x,y,z)) > 0) {
                        var (tmpVertices, tmpNormals, tmpUVs, tmpTriangles, tmpColors) = GenerateVoxel(new Vector3Int(x,y,z));
                        for (var k = 0; k < tmpTriangles.Count; k++) {
                            tmpTriangles[k] += vertices_.Count;
                        }
                        vertices_.AddRange(tmpVertices);
                        triangles_.AddRange(tmpTriangles);
                        normals_.AddRange(tmpNormals);
                        uvs_.AddRange(tmpUVs);
                        colors_.AddRange(tmpColors);
                    }
                }
            }
        }
        
        return (vertices_, normals_, uvs_, triangles_, colors_);
    }
    
    (List<Vector3>, List<Vector3>, List<Vector2>, List<int>, List<Color>) GenerateVoxel(Vector3Int pos) {
        Vector3Int[] faces = new Vector3Int[] {
            Vector3Int.up, Vector3Int.down,
            Vector3Int.right, Vector3Int.left,
            new Vector3Int(0,0,1), new Vector3Int(0,0,-1)
        };
        
        List<Vector3> vertices_ = new List<Vector3>();
        List<Vector3> normals_ = new List<Vector3>();
        List<Vector2> uvs_ = new List<Vector2>();
        List<int> triangles_ = new List<int>();
        List<Color> colors_ = new List<Color>();
        
        for (var i = 0; i < faces.Length; i++) {
            if (GetVoxel(pos + faces[i]) <= 0) {
                //Set face with chunk position as vertex color
                var (tmpVertices, tmpNormals, tmpUVs, tmpTriangles, tmpColors) =
                    GenerateFace(pos + (0.5f * (Vector3) faces[i]), faces[i], 0.5f, new Color(((float) pos.x)/chunkSize.x, ((float) pos.y)/chunkSize.y, ((float) pos.z)/chunkSize.z, 1.0f));
                
                for (var k = 0; k < tmpTriangles.Length; k++) {
                    tmpTriangles[k] += vertices_.Count;
                }
                triangles_.AddRange(tmpTriangles);
                vertices_.AddRange(tmpVertices);
                normals_.AddRange(tmpNormals);
                uvs_.AddRange(tmpUVs);
                colors_.AddRange(tmpColors);
            }
        }
        
        return (vertices_, normals_, uvs_, triangles_, colors_);
    }
    
    // Vert, Norm, UV, Triangles, Colors
    (Vector3[], Vector3[], Vector2[], int[], Color[]) GenerateFace(Vector3 centerPos, Vector3 normal, float size, Color vertexColor) {
        //                                          
        //  3______0                                
        //   |\   |                                 
        //   | \  |                                 
        //   |  \ |                                 
        //   |___\|                                 
        //  1      2                                
        //                                          
        
        //By default normal = (0,1,0)
        Vector3[] vertices_ = new Vector3[] {
            new Vector3(  size,0,  size),
            new Vector3( -size,0, -size),
            new Vector3(  size,0, -size),
            new Vector3( -size,0,  size)
        };
        int[] triangles_ = new int[] {3,0,2,3,2,1};
        
        Quaternion vecRot = Quaternion.FromToRotation(Vector3.up, normal);
        Quaternion vecRotUp = Quaternion.LookRotation(Vector3.up, normal);
        
        for (var i = 0; i < vertices_.Length; i++) {
            if (Mathf.Abs(normal.y) == 0) {
                vertices_[i] = (vecRotUp * vertices_[i]);
            } else {
                vertices_[i] = (vecRot * vertices_[i]);
            }
            vertices_[i] = vertices_[i] + centerPos;
        }
        
        Vector3[] normals_ = new Vector3[] {
            normal, normal, normal, normal
        };
        
        Vector2[] uvs_ = new Vector2[] {
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1)
        };
        
        Color[] colors_ = new Color[] {
            vertexColor,
            vertexColor,
            vertexColor,
            vertexColor
        };
        
        return (vertices_, normals_, uvs_, triangles_, colors_);
    }
}
