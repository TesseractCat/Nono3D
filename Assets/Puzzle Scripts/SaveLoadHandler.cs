using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.SerializableAttribute]
public class Puzzle {
    public string name = "untitled";
    public string author = "me";
    public long datetime;
    
    public Vector3Int puzzleSize;
    public int[] flattenedVoxelArray;
    public Color[] flattenedColorArray;
    
    public Color[] colorPalette;
}

public class SaveLoadHandler : MonoBehaviour
{
    static int To1D(Vector3Int idx, Vector3Int dimensions) {
        return (idx.z * dimensions.x * dimensions.y) + (idx.y * dimensions.x) + idx.x;
    }
    static Vector3Int To3D(int idx, Vector3Int dimensions) {
        int z = idx / (dimensions.x * dimensions.y);
        idx -= (z * dimensions.x * dimensions.y);
        int y = idx / dimensions.x;
        int x = idx % dimensions.x;
        return new Vector3Int(x, y, z);
    }
    
    public static T[] Flatten3DArray<T>(T[,,] originalArray, Vector3Int dimensions) {
        T[] outArray = new T[dimensions[0] * dimensions[1] * dimensions[2]];
        
        Debug.Assert(dimensions.x <= originalArray.GetLength(0)
                && dimensions.y <= originalArray.GetLength(1)
                && dimensions.z <= originalArray.GetLength(2));
        
        for (int x = 0; x < dimensions[0]; x++) {
            for (int y = 0; y < dimensions[1]; y++) {
                for (int z = 0; z < dimensions[2]; z++) {
                    outArray[To1D(new Vector3Int(x,y,z), dimensions)] = originalArray[x,y,z];
                }
            }
        }
        
        return outArray;
    }
    public static T[,,] ExpandFlattened3DArray<T>(T[] originalArray, Vector3Int dimensions) {
        T[,,] outArray = new T[dimensions.x, dimensions.y, dimensions.z];
        
        for (int i = 0; i < originalArray.Length; i++) {
            Vector3Int pos = To3D(i, dimensions);
            outArray[pos.x,pos.y,pos.z] = originalArray[i];
        }
        
        return outArray;
    }
    public static T[,,] Pad3DArray<T>(T[,,] originalArray, Vector3Int targetDimensions) {
        //Too big!
        if (originalArray.GetLength(0) > targetDimensions.x || originalArray.GetLength(1) > targetDimensions.y || originalArray.GetLength(2) > targetDimensions.z)
            return null;
        
        T[,,] outArray = new T[targetDimensions.x, targetDimensions.y, targetDimensions.z];
        for (int x = 0; x < originalArray.GetLength(0); x++) {
            for (int y = 0; y < originalArray.GetLength(1); y++) {
                for (int z = 0; z < originalArray.GetLength(2); z++) {
                    outArray[x,y,z] = originalArray[x,y,z];
                }
            }
        }
        
        return outArray;
    }
    
    public static void SavePuzzle(Puzzle puzzle) {
        if (!Directory.Exists(Application.persistentDataPath + "/MyPuzzles")) {
            Directory.CreateDirectory(Application.persistentDataPath + "/MyPuzzles");
        }
        string writePath = Application.persistentDataPath + "/MyPuzzles/" + puzzle.name + ".json";
        Debug.Log("Saving puzzle to persistentDataPath: " + writePath);
        
        string jsonString = JsonUtility.ToJson(puzzle);
        
        File.WriteAllText(writePath, jsonString);
    }
    
    public static Puzzle LoadPuzzleJson(string puzzleJson) {
        return JsonUtility.FromJson<Puzzle>(puzzleJson);
    }
}
