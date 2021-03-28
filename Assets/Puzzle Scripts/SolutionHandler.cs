using System;
using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

using UnityEngine;
using UnityEngine.Profiling;

public class Block {
    public int Length;
    public int Idx;
    
    public Block(int Idx = 0, int Length = 1) {
        this.Idx = Idx;
        this.Length = Length;
    }
    
    public bool CheckValidity(int[] lineArray) {
        for (int i = this.Idx; i < this.Idx + this.Length; i++) {
            if (i > lineArray.Length - 1) {
                //Out of bounds
                return false;
            }
            if (lineArray[i] == 0) {
                //If an index has been determined to be empty, then this position isn't valid
                return false;
            }
        }
        return true;
    }
    
    public bool CheckIntersection(Block toCheck) {
        //Basically checks if the start or end is inside toCheck, and then does the same but reversed
        //For an intersection to occur one of these has to be true
        if (this.Idx >= toCheck.Idx && this.Idx < toCheck.Idx + toCheck.Length) {
            return true;
        } else if (this.Idx+this.Length-1 >= toCheck.Idx && this.Idx+this.Length-1 < toCheck.Idx + toCheck.Length) {
            return true;
        } else if (toCheck.Idx >= this.Idx && toCheck.Idx < this.Idx + this.Length) {
            return true;
        } else if (toCheck.Idx+toCheck.Length-1 >= this.Idx && toCheck.Idx+toCheck.Length-1 < this.Idx + this.Length) {
            return true;
        }
        return false;
    }
    
    public bool CheckCompletelyInside(Block toCheck) {
        //Check if this block is completely inside of toCheck
        if (this.Idx >= toCheck.Idx && this.Idx + (this.Length - 1) < toCheck.Idx + toCheck.Length) {
            return true;
        }
        return false;
    }
    
    public bool OutOfBounds(int lineLength) {
        if (this.Idx < 0) {
            return true;
        } else if (this.Idx + this.Length - 1 > lineLength - 1) {
            return true;
        }
        return false;
    }
    
    public override string ToString() {
        return "Block - Idx: " + this.Idx + ", Length: " + this.Length;
    }
}

public class Hint2D {
    public int[] groupArray;

    public Hint2D(int[] groupArray) {
        this.groupArray = groupArray;
    }
    
    public Block[] BlockArrayFromCellArray(int[] cellArray) {
        List<Block> outArray = new List<Block>();
        
        bool inBlock = false;
        for (int i = 0; i < cellArray.Length; i++) {
            if (cellArray[i] == 1) {
                if (!inBlock) {
                    outArray.Add(new Block(i,1));
                } else {
                    outArray[outArray.Count-1].Length += 1;
                }
                inBlock = true;
            } else if (cellArray[i] <= 0) {
                inBlock = false;
            }
        }
        
        return outArray.ToArray();
    }
    
    public Block[] BlockArrayRepresentation(bool reversed) {
        Block[] outArray = new Block[groupArray.Length];
        int[] tempGroupArray = (int[])groupArray.Clone();
        if (reversed) {
            tempGroupArray = tempGroupArray.Reverse().ToArray();
        }
        
        for (int i = 0; i < outArray.Length; i++) {
            outArray[i] = new Block(0, tempGroupArray[i]);
            if (i>0) {
                outArray[i].Idx = tempGroupArray.Take(i).Sum() + i;
            } else {
                outArray[i].Idx = 0;
            }
        }
        
        return outArray;
    }

    public int MinLength() {
        // Ex. [6,2,3]
        // 6 + 2 + 3 = 11
        // Length = 3 => - 1 => 2
        // 11 + 2 = 13
        // OOOOOO OO OOO
        // 123456789ABCD (hex D=13)
        return groupArray.Sum() + (groupArray.Length - 1);
    }

    public int[] MinLineArrayRepresentation() {
        //Ex. groupArray = [3,1]
        //Returns: [1,1,1,0,1]
        int[] outArray = new int[this.MinLength()];

        int outIdx = 0;
        for (int i = 0; i < groupArray.Length; i++) {
            for (int k = 0; k < groupArray[i]; k++) {
                outArray[outIdx] = 1;
                outIdx++;
            }
            outIdx++;
        }
        //Debug.Assert(outIdx == outArray.Length-1);

        return outArray;
    }

    //Returns null if unsolvable
    public (int[],Block[]) LeftMostLineArrayRepresentation(int[] currentLine, bool reverseBlocks = false) {
        //Stub
        //Ex. currentLine = [-1,-1,-1,0,-1], groupArray = [2,1]
        //Returns: [1,1,0,0,1]
        //Returns: [1,1,0,0,2] maybe index the contiguous groups?
        //Should be obvious but the length will not be the same as MinLineArrayRepresentation...
        //It will be the length of the currentLine, since hints like (2) can occur in longer lines
        
        //Actually POTENTIALLY unassigned blocks
        Block[] unassignedBlocks = this.BlockArrayFromCellArray(currentLine);
        
        int[] outLine = currentLine; //currentLine by default
        Block[] blocks = this.BlockArrayRepresentation(reverseBlocks);
        
        //First, make sure no blocks are intersecting with determined empty cells (CheckValidity)
        //This gives the simple leftmost
        for (int i = 0; i < blocks.Length; i++) {
            //Move it 1 empty space after the previous block
            if (i > 0) {
                blocks[i].Idx = blocks[i - 1].Idx + blocks[i - 1].Length + 1;
            }
            //Move it right until a valid space
            while (!blocks[i].CheckValidity(currentLine)) {
                blocks[i].Idx += 1;
                if (blocks[i].OutOfBounds(currentLine.Length)) {
                    //Unsolvable
                    //Debug.Log("Out of bounds while constructing left most array representation!");
                    return (null,null);
                }
            }
        }
        
        for (int l = 0; l < 10; l++) { //Iteration limit
            //Iterate through all potentially unassigned blocks and make sure they are assigned
            bool allAssigned = true;
            for (int i = unassignedBlocks.Length - 1; i >= 0; i--) {
                //Debug.Log("Potentially unassigned block with length: " + unassignedBlocks[i].Length + ", and idx: " + unassignedBlocks[i].Idx);
                bool isAssigned = false;
                for (int k = 0; k < blocks.Length; k++) {
                    isAssigned = isAssigned || unassignedBlocks[i].CheckCompletelyInside(blocks[k]);
                    //Debug.Log(blocks[k].Length);
                    //Debug.Log(unassignedBlocks[i].CheckCompletelyInside(blocks[k]));
                }
                //if (isAssigned)
                //    Debug.Log("Nevermind, it's assigned...");
                
                //If it isn't assigned, readjust all blocks so it becomes assigned
                if (!isAssigned) {
                    allAssigned = false;
                    //Iterate backwards through all blocks to the left of the unassigned block
                    //i.e., find the first valid block to the left of the unassigned block
                    Block toTheLeft = null; //Reference
                    for (int k = blocks.Length - 1; k >= 0; k--) { //Iterate end to beginning
                        if (blocks[k].Idx < unassignedBlocks[i].Idx && blocks[k].Length >= unassignedBlocks[i].Length) {
                            //Debug.Log("Found valid block to the left of the unassigned block!");
                            
                            toTheLeft = blocks[k];
                            toTheLeft.Idx = unassignedBlocks[i].Idx + (unassignedBlocks[i].Length - 1) - (toTheLeft.Length - 1);
                            
                            //Move it to the first intersecting position
                            //Keep moving it to the right if it isn't valid
                            while (true) {
                                //if (!toTheLeft.CheckIntersection(unassignedBlocks[i])) {
                                if (!unassignedBlocks[i].CheckCompletelyInside(toTheLeft)) {
                                    //Means that there are no valid positions for toTheLeft on this unassigned block
                                    //This means that we should check the next block to the left
                                    //Debug.Log("No valid way to cover this unassigned block!");
                                    break; //Breaks out of the while loop and grabs another valid block to the left
                                }
                                if (toTheLeft.CheckValidity(currentLine)) {
                                    //Valid position covering unassigned block
                                    //Now we need to rearrange the blocks following toTheLeft
                                    //Debug.Log("Unassigned block has been successfully covered! Now It is necessary to rearrange all following blocks");
                                    
                                    if (k == blocks.Length - 1) { //No need to rearrange following blocks
                                        goto BlockAssigned;
                                    }
                                    
                                    //Iterate through all blocks following toTheLeft
                                    for (int j = k + 1; j < blocks.Length; j++) {
                                        //Move it 1 empty space after the previous block
                                        blocks[j].Idx = blocks[j - 1].Idx + blocks[j - 1].Length + 1;
                                        //Move it right until a valid space
                                        while (!blocks[j].CheckValidity(currentLine)) {
                                            blocks[j].Idx += 1;
                                            if (blocks[j].OutOfBounds(currentLine.Length)) {
                                                //Unsolvable
                                                return (null,null);
                                            }
                                        }
                                    }
                                    
                                    //Instead of break, exit the nested loop
                                    goto BlockAssigned;
                                } else {
                                    //Move it one to the right
                                    toTheLeft.Idx += 1;
                                }
                            }
                        }
                    }
                    
                    BlockAssigned:
                        {}
                        //Debug.Log("Block Assigned!");
                }
            }
            
            if (allAssigned)
                break; //Exit loop if there is nothing more to assign
        }
            
        //Convert blocks to outLine
        for (int i = 0; i < blocks.Length; i++) {
            for (int k = blocks[i].Idx; k < blocks[i].Idx + blocks[i].Length; k++) {
                
                if (blocks[i].Idx + blocks[i].Length > outLine.Length) {
                    //Blocks have been placed outside of the line
                    //I believe this means that this is unsolvable?
                    return (null,null);
                }
                
                //Index each block group
                if (reverseBlocks) {
                    outLine[k] = i+1;
                } else {
                    outLine[k] = blocks.Length - i;
                }
            }
        }
        
        return (outLine, blocks);
    }
    
    public (int[],Block[]) RightMostLineArrayRepresentation(int[] currentLine) {
        //Calls LeftMost, but flipped (flips on return to retain orientation)
        int[] reversedCurrentLine = currentLine.Reverse().ToArray();
        (int[] leftMost, Block[] leftMostBlocks) = LeftMostLineArrayRepresentation(reversedCurrentLine, true);
        if (leftMost == null) {
            return (null,null);
        }
        leftMostBlocks = leftMostBlocks.Reverse().ToArray(); //Reverse to leftMost order
        for (int i = 0; i < leftMostBlocks.Length; i++) { //Fix Idx's
            leftMostBlocks[i].Idx = (leftMost.Length - 1)  - leftMostBlocks[i].Idx;
            leftMostBlocks[i].Idx -= leftMostBlocks[i].Length - 1;
        }
        return (leftMost.Reverse().ToArray(), leftMostBlocks); //TODO: Reverse leftMostBlocks
    }
}

//3D Hint, i.e. [5] or (2) or 8
public class Hint {
    public int tempCount;
    public int count;
    public int groups;

    public Hint() {}

    public Hint(int count, int groups) {
        this.count = count;
        this.groups = groups;
    }

    public int CalculateDifficulty(bool orderReversed) {
        if (this.count == -1 || this.count == 0) {
            if (orderReversed)
                return 1000000;
            return -1;
        } else {
            //return this.Hints2D().Length + this.count;
            //return this.groups;
            return (100 * this.groups) + this.count;
        }
    }
    
    private static void FindSets(ref List<int[]> allSets, int[] set, int startIdx) {
        if (set[set.Length - 1] != 1 || set.Length <= 3)
            return;
        set = set.Take(set.Length - 1).ToArray();
        for (int i = startIdx; i < set.Length; i++) {
            set[i] += 1;
            allSets.Add(set);
            FindSets(ref allSets, set, i);
            set[i] -= 1;
        }
    }

    private static int[][] Partitions(int n, int g) {
        //See: https://www.geeksforgeeks.org/generate-unique-partitions-of-an-integer/
        //Doesn't work?
        List<int[]> outArr = new List<int[]>();

        if (g == 2) {
            int right = 1;
            int left = n - 1;
            while (left > 0) {
                outArr.Add(new int[2] {left, right});
                left -= 1;
                right += 1;
            }
        } else if (g >= 3) {
            outArr.Add(Enumerable.Repeat(1, n).ToArray());
        }
        
        return outArr.ToArray();
    }

    public Hint2D[] Hints2D() {
        //See: file:///C:/Users/tesseractcat/Downloads/dissertation.pdf 
        //See also: https://stackoverflow.com/questions/4588429/number-of-ways-to-add-up-to-a-sum-s-with-n-numbers 

        //Only need one hint2d if it is in a single group
        if (this.groups == 1) {
            return new Hint2D[1]{new Hint2D(new int[1]{this.count})};
        }

        //Find unique permutations and convert to all permutations (non-unique included)
        int[][] PartitionsArr = Partitions(this.count, this.groups);
        //List<int[]> PartitionsList = new List<int[]>(); //All partitions
        //for (int i = 0; i < UniquePartitionsArr.Length; i++) {
        //    //Only add permutations to PartitionsList if they are in line with the amount of groups in hint (circle 2, square 3+)
        //    if ((this.groups == 2 && UniquePartitionsArr[i].Length == 2) || (this.groups > 2 && UniquePartitionsArr[i].Length > 2)) {
        //        PartitionsList = PartitionsList.Concat(ArrayPermutations(UniquePartitionsArr[i])).ToList();
        //    }
        //}

        //Find all, then convert to Hint2D's
        Hint2D[] PartitionHints = PartitionsArr.Select(i => new Hint2D(i)).ToArray();

        return PartitionHints;
    }
}

public class Line {
    public int[] lineArray;
    public int height;
    public int x;
    public int y;
    public Vector3Int side;

    public Line(Vector3Int side, int x, int y, int height, int[] lineArray) {
        this.lineArray = lineArray;
        this.height = height;
        this.x = x;
        this.y = y;
        this.side = side;
    }

    //inclusive, only for completely solid groups
    public bool IsSolidGroupValid(int startIdx, int endIdx) {
        //Not valid if outside bounds
        if (startIdx < 0 || endIdx > lineArray.Length - 1) {
            return false;
        }

        int i = 0;
        for (int k = startIdx; k <= endIdx; k++) {
            if (lineArray[k] != -1) {
                return false;
            }
            i++;
        }
        return true;
    }

    public Line LeftMostValidSolidGroup() {
        //Not implemented
        int[] solidLineArray = Enumerable.Repeat(0, this.height).ToArray();

        if (solidLineArray != null) {
            return new Line(this.side,this.x,this.y,this.height,solidLineArray);
        } else {
            //If null is returned, there is no valid left most solid group
            return null;
        }
    }

    public Line FindSolid(Hint hint) {
        //int[] solidLineArray = (int[])this.lineArray.Clone();
        int[] solidLineArray = Enumerable.Repeat(0, this.height).ToArray();

        return new Line(this.side,this.x,this.y,this.height,solidLineArray);
    }
}

public class Solver {
    //-1 = Unknown, 0 = Empty, 1 = Full
    public int[,,] voxelArray; // "Guess" array, solver buffer
    public int[,,] solutionArray;
    
    public int[,,] prepassArray;

    public Vector3Int chunkSize;
    
    public Dictionary<Vector3Int, Hint[,]> hintDict;
    
    public Solver(Dictionary<Vector3Int, Hint[,]> hintDict, Vector3Int chunkSize, int[,,] solutionArray) {
        this.hintDict = hintDict;
        this.chunkSize = chunkSize;
        this.solutionArray = solutionArray;
        
        //Initializes local guess array
        ResetVoxelArray();
    }
    
    public void ResetVoxelArray() {
        if (voxelArray == null) {
            voxelArray = new int[chunkSize.x,chunkSize.y,chunkSize.z];
        }
        
        for (int x = 0; x < chunkSize.x; x++) {
            for (int y = 0; y < chunkSize.y; y++) {
                for (int z = 0; z < chunkSize.z; z++) {
                    if (prepassArray == null) {
                        voxelArray[x,y,z] = -1;
                    } else {
                        voxelArray[x,y,z] = prepassArray[x,y,z];
                    }
                }
            }
        }
    }
    
    public void SetLine(int[,,] setArray, Line line) {
        int height = setArray.GetLength(Array.IndexOf(new int[]{line.side.x,line.side.y,line.side.z}, 1));
        
        for (int i = 0; i < height; i++) {
            int[] idx = new int[]{0,0,0};
            idx[Array.IndexOf(new int[]{line.side.x,line.side.y,line.side.z}, 1)] = i;
            idx[Array.IndexOf(new int[]{line.side.x,line.side.y,line.side.z}, 0)] = line.x;
            idx[Array.LastIndexOf(new int[]{line.side.x,line.side.y,line.side.z}, 0)] = line.y;
            
            //if ((int)setArray.GetValue(idx) == -1) {
            setArray.SetValue(line.lineArray[i], idx);
            //}
        }
    }

    //private void SolveLineOneGroup(ref Line line, Hint hint) {
    //    Hint2D hint2d = hint.Hints2D()[0]; //There should only be one if there is only one group
    //    
    //    //Debug.Log("SolveLineOneGroup: " + hint.count + " - hint count");
    //    
    //    int[] leftMost = hint2d.LeftMostLineArrayRepresentation(line.lineArray);
    //    int[] rightMost = hint2d.RightMostLineArrayRepresentation(line.lineArray);
    //    Debug.Assert(leftMost.Length == rightMost.Length);
    //    
    //    //Debug.Log("LeftMost: " + String.Join(",",leftMost));
    //    //Debug.Log("RightMost: " + String.Join(",",rightMost));
    //    
    //    int[] overlappedArray = Enumerable.Repeat(-1, leftMost.Length).ToArray();
    //    
    //    for (var i = 0; i < leftMost.Length; i++) {
    //        //At the places where leftMost and rightMost are equal, set overlappedArray to their shared value
    //        //Otherwise keep lineArray's values
    //        if (leftMost[i] == rightMost[i]) {
    //            overlappedArray[i] = leftMost[i];
    //            if (overlappedArray[i] >= 1) {
    //                overlappedArray[i] = 1;
    //            }
    //        } else {
    //            overlappedArray[i] = line.lineArray[i];
    //        }
    //    }
    //    
    //    //Debug.Log("OverlappedArray: " + String.Join(",",overlappedArray));
    //    
    //    //ENABLE THIS ONCE LeftMostLineArrayRepresentation is unstubbed
    //    line.lineArray = overlappedArray;
    //    
    //    return;
    //}

    private void SolveLineTwoGroups(ref Line line, Hint hint) {
        return;
    }
    
    private void SolveLineThreePlusGroups(ref Line line, Hint hint) {
        return;
    }
    
    private void SolveLineMultipleGroups(ref Line line, Hint hint) {
        //Debug.Log(" !!! ----- SOLVING LINE MULTIPLE GROUPS: " + hint.count + " - " + hint.groups);
        //Debug.Log("Line Array: " + String.Join(",",line.lineArray));
        
        Hint2D[] hint2dArr = hint.Hints2D();
        
        List<int[]> overlappedArrays = new List<int[]>();
        
        //Debug.Log(hint2dArr.Length);
        //Debug.Log(String.Join(",",hint2dArr[0].groupArray));

        for (int i = 0; i < hint2dArr.Length; i++) {
            //Debug.Log("Analyzing Hint2D: " + String.Join(",",hint2dArr[i].groupArray));
            
            (int[] leftMost, Block[] leftMostBlocks) = hint2dArr[i].LeftMostLineArrayRepresentation((int[])line.lineArray.Clone());
            (int[] rightMost, Block[] rightMostBlocks) = hint2dArr[i].RightMostLineArrayRepresentation((int[])line.lineArray.Clone());
            if (leftMost == null || rightMost == null) {
                continue;
            }
            //Debug.Log("LeftMost: " + String.Join(",",leftMost));
            //Debug.Log("RightMost: " + String.Join(",",rightMost));
            
            Debug.Assert(leftMost.Length == rightMost.Length);
            Debug.Assert(leftMostBlocks.Length == rightMostBlocks.Length);
            
            overlappedArrays.Add(Enumerable.Repeat(-1, leftMost.Length).ToArray());
            
            //Solve solid cells
            for (var k = 0; k < leftMost.Length; k++) {
                //At the places where leftMost and rightMost are equal, set overlappedArray to their shared value
                //Otherwise keep lineArray's values
                if (leftMost[k] == rightMost[k]) {
                    overlappedArrays[overlappedArrays.Count-1][k] = leftMost[k];
                    if (overlappedArrays[overlappedArrays.Count-1][k] >= 1) {
                        overlappedArrays[overlappedArrays.Count-1][k] = 1;
                    }
                } else {
                    overlappedArrays[overlappedArrays.Count-1][k] = line.lineArray[k];
                }
            }
            
            //Solve empty cells
            for (var k = 0; k < leftMost.Length; k++) {
                bool canBeSetToEmpty = true;
                for (var j = 0; j < leftMostBlocks.Length; j++) {
                    //Test if cell is in range between leftMost and rightMost variant
                    if (k >= leftMostBlocks[j].Idx && k < rightMostBlocks[j].Idx + rightMostBlocks[j].Length) {
                        canBeSetToEmpty = false;
                    }
                }
                if (canBeSetToEmpty) {
                    overlappedArrays[overlappedArrays.Count-1][k] = 0;
                }
            }
        }
        
        if (overlappedArrays.Count == 0) {
            return;
        }
        
        for (int i = 0; i < overlappedArrays.Count; i++) {
            if (overlappedArrays[0].SequenceEqual(overlappedArrays[i])) {
                if (i == overlappedArrays.Count - 1) {
                    //Debug.Log("Solution Found: " + String.Join(",",overlappedArrays[0]));
                    //for (var k = 0; k < overlappedArrays[0].Length; k++) {
                    //    if (overlappedArrays[0][k] > 1) {
                    //        overlappedArrays[0][k] = 1;
                    //    }
                    //}
                    line.lineArray = overlappedArrays[0];
                    return;
                }
                continue;
            } else {
                break;
            }
        }
        
        return;
    }
    
    public void SolveLine(Line line, Hint hint) {
        //Simple cases
        if (hint.count == -1) { //Line with no hint, do nothing
            return;
        }
        if (hint.count == 0) { //Empty line
            //Set line to be completely empty
            line.lineArray = Enumerable.Repeat(0, line.height).ToArray();
            SetLine(voxelArray, line);
            return;
        }
        if (hint.count == line.height) { //Full line
            //Set line to be completely full
            line.lineArray = Enumerable.Repeat(1, line.height).ToArray();
            SetLine(voxelArray, line);
            return;
        }
        if (!line.lineArray.Contains(-1)) { //Line with no unknowns, nothing to solve
            return;
        }
        //if (hint.Hints2D().Length == 1 && line.height == hint.Hints2D()[0].MinLength() && hint.count == hint.groups) { //Simple complete definition
        //    //Basically, if there is only one Hint2D and it's
        //    //    minlength is the same as the line height, then it fills the line
        //    // i.e.
        //    //    (2) => [1,0,1] = (len) > 3
        //    line.lineArray = hint.Hints2D()[0].MinLineArrayRepresentation();
        //    SetLine(voxelArray, line);
        //    return;
        //}

        //Solve lines based on group count
        if (hint.groups == 1) {
            //SolveLineOneGroup(ref line, hint);
            SolveLineMultipleGroups(ref line, hint);
        } else if (hint.groups == 2) {
            //SolveLineTwoGroups(ref line, hint);
            SolveLineMultipleGroups(ref line, hint);
        } else if (hint.groups >= 3) {
            //SolveLineThreePlusGroups(ref line, hint);
            SolveLineMultipleGroups(ref line, hint);
        }
        
        SetLine(voxelArray, line);
        return;
    }
    
    void PrepassLine(Line line, Hint hint) {
        if (hint.count == 0) { //Empty line
            //Set line to be completely empty
            line.lineArray = Enumerable.Repeat(0, line.height).ToArray();
            SetLine(prepassArray, line);
            return;
        }
        if (hint.count == line.height) { //Full line
            //Set line to be completely full
            line.lineArray = Enumerable.Repeat(1, line.height).ToArray();
            SetLine(prepassArray, line);
            return;
        }
        line.lineArray = Enumerable.Repeat(-1, line.height).ToArray();
        SetLine(prepassArray, line);
    }

    public Line GetLine(int[,,] getArray, Vector3Int side, int x, int y) {
        int height = getArray.GetLength(Array.IndexOf(new int[]{side.x,side.y,side.z}, 1));
        int[] lineArray = new int[height];

        for (int i = 0; i < height; i++) {
            int[] idx = new int[]{0,0,0};
            idx[Array.IndexOf(new int[]{side.x,side.y,side.z}, 1)] = i;
            idx[Array.IndexOf(new int[]{side.x,side.y,side.z}, 0)] = x;
            idx[Array.LastIndexOf(new int[]{side.x,side.y,side.z}, 0)] = y;
            
            lineArray[i] = (int)getArray.GetValue(idx);
        }

        return new Line(side, x, y, height, lineArray);
    }
    
    public void DoPrepass() {
        prepassArray = new int[chunkSize.x, chunkSize.y, chunkSize.z];
        
        var normalIndices = new Dictionary<Vector3Int, int[]>() {
            {Vector3Int.up, new int[]{0,2}},
            {Vector3Int.right, new int[]{1,2}},
            {new Vector3Int(0,0,1), new int[]{0,1}}
        };
        
        foreach (KeyValuePair<Vector3Int, int[]> normalIdx in normalIndices) {
            for (int x = 0; x < voxelArray.GetLength(normalIdx.Value[0]); x++)
            {
                for (int y = 0; y < voxelArray.GetLength(normalIdx.Value[1]); y++)
                {
                    PrepassLine(GetLine(prepassArray, normalIdx.Key, x, y), hintDict[normalIdx.Key][x, y]);
                }
            }
        }
    }

    public void BruteForceSolveAllLines() {
        var normalIndices = new Dictionary<Vector3Int, int[]>() {
            {Vector3Int.up, new int[]{0,2}},
            {Vector3Int.right, new int[]{1,2}},
            {new Vector3Int(0,0,1), new int[]{0,1}}
        };

        //Do it multiple times?
        for (int i = 0; i < 10; i++) {
            //Iterate through each side, follow by each line (2D array)
            bool noUnknowns = true;
            bool nothingChanged = true;
            foreach (KeyValuePair<Vector3Int, int[]> normalIdx in normalIndices) {
                for (int x = 0; x < voxelArray.GetLength(normalIdx.Value[0]); x++)
                {
                    for (int y = 0; y < voxelArray.GetLength(normalIdx.Value[1]); y++)
                    {
                        //Solve line
                        int[] tempLineArray = GetLine(voxelArray, normalIdx.Key, x, y).lineArray;
                        SolveLine(GetLine(voxelArray, normalIdx.Key, x, y), hintDict[normalIdx.Key][x, y]);
                        //Check if something changed
                        if (!tempLineArray.SequenceEqual(GetLine(voxelArray, normalIdx.Key, x, y).lineArray))
                            nothingChanged = false;
                        //Check if the line has any unknowns
                        noUnknowns = noUnknowns && !GetLine(voxelArray, normalIdx.Key, x, y).lineArray.Contains(-1);
                    }
                }
            }
            if (noUnknowns) {
                //Debug.Log("Solved All Lines! Required iterations: " + i.ToString());
                return;
            }
            if (nothingChanged) {
                //Debug.Log("BruteForceSolveAllLines: Nothing changed this iteration (" + i + ")! Returning...");
                return;
            }
        }
        //Debug.Log("BruteForceSolveAllLines: Reached iteration limit but there are still unknowns!");
        return;
    }
    
    Line[] PerpendicularLines(Line line, int i) {
        var normalIndices = new Dictionary<Vector3Int, int[]>() {
            {Vector3Int.up, new int[]{0,2}},
            {Vector3Int.right, new int[]{1,2}},
            {new Vector3Int(0,0,1), new int[]{0,1}}
        };
        
        Vector3Int pos = new Vector3Int();
        pos[normalIndices[line.side][0]] = line.x;
        pos[normalIndices[line.side][1]] = line.y;
        pos[Array.IndexOf(new int[]{line.side.x,line.side.y,line.side.z}, 1)] = i;
        
        normalIndices.Remove(line.side);
        
        Line lineOne = GetLine(voxelArray, normalIndices.Keys.ElementAt(0),
               pos[normalIndices.ElementAt(0).Value[0]],pos[normalIndices.ElementAt(0).Value[1]]);
        
        Line lineTwo = GetLine(voxelArray, normalIndices.Keys.ElementAt(1),
               pos[normalIndices.ElementAt(1).Value[0]],pos[normalIndices.ElementAt(1).Value[1]]);
        
        if (!lineOne.lineArray.Contains(-1) && !lineTwo.lineArray.Contains(-1)) {
            return new Line[0]{};
        }
        
        if (lineOne.lineArray.Contains(-1) && !lineTwo.lineArray.Contains(-1)) {
            return new Line[1]{lineOne};
        }
        if (!lineOne.lineArray.Contains(-1) && lineTwo.lineArray.Contains(-1)) {
            return new Line[1]{lineTwo};
        }
        
        return new Line[2]{ lineOne, lineTwo };
    }
    
    public bool HierarchicalSolveAllLines() {
        var normalIndices = new Dictionary<Vector3Int, int[]>() {
            {Vector3Int.up, new int[]{0,2}},
            {Vector3Int.right, new int[]{1,2}},
            {new Vector3Int(0,0,1), new int[]{0,1}}
        };
        
        List<Line> lineQueue = new List<Line>();
        
        //For each line
        foreach (KeyValuePair<Vector3Int, int[]> normalIdx in normalIndices) {
            for (int x = 0; x < voxelArray.GetLength(normalIdx.Value[0]); x++)
            {
                for (int y = 0; y < voxelArray.GetLength(normalIdx.Value[1]); y++)
                {
                    //Add line to queue
                    lineQueue.Add(GetLine(voxelArray, normalIdx.Key, x, y));
                    
                    int[] tempLineArray;
                    Line tempLine;
                    //Empty queue
                    while (lineQueue.Count > 0) {
                        //Take the last line
                        tempLine = lineQueue[lineQueue.Count - 1];
                        lineQueue.RemoveAt(lineQueue.Count - 1);

                        //Record the lines previous line array
                        tempLineArray = tempLine.lineArray;
                        
                        //Solve the line
                        SolveLine(tempLine, hintDict[tempLine.side][tempLine.x, tempLine.y]);
                        
                        //Check if there are any changes
                        for (int i = 0; i < tempLineArray.Length; i++) {
                            if (tempLineArray[i] != tempLine.lineArray[i]) {
                                //Add perpendicular lines to queue
                                lineQueue.AddRange(PerpendicularLines(tempLine, i));
                            }
                        }
                    }
                }
            }
            //Check if solved
            bool isEqual = true;
            for (int x = 0; x < voxelArray.GetLength(0); x++)
            {
                for (int y = 0; y < voxelArray.GetLength(1); y++)
                {
                    for (int z = 0; z < voxelArray.GetLength(2); z++)
                    {
                        // Voxel Array and Solution Array should be the same size.
                        
                        // This means that there should be NO unknowns, if there are,
                        // then it means it is not fully solved.
                        isEqual = isEqual && (voxelArray[x,y,z] == solutionArray[x,y,z]);
                    }
                }
            }
            if (isEqual)
                return true;
        }
        return false;
    }
    
    public bool IsSolvable() {
        ResetVoxelArray();
        BruteForceSolveAllLines();
        //return HierarchicalSolveAllLines();
        //Debug.Log(voxelArray.Cast<int>());
        bool isEqual = true;
        for (int x = 0; x < voxelArray.GetLength(0); x++)
        {
            for (int y = 0; y < voxelArray.GetLength(1); y++)
            {
                for (int z = 0; z < voxelArray.GetLength(2); z++)
                {
                    // Voxel Array and Solution Array should be the same size.
                    
                    // This means that there should be NO unknowns, if there are,
                    // then it means it is not fully solved.
                    isEqual = isEqual && (voxelArray[x,y,z] == solutionArray[x,y,z]);
                }
            }
        }
        //Debug.Log(isEqual);
        return isEqual;
    }
    
    public void BinaryOrderedHintRemoval() {
        var normalIndices = new Dictionary<Vector3Int, int[]>() {
            {Vector3Int.up, new int[]{0,2}},
            {Vector3Int.right, new int[]{1,2}},
            {new Vector3Int(0,0,1)/*Forwards*/, new int[]{0,1}}
        };
        
        //Initialize ordered reference list
        List<KeyValuePair<Vector3Int, Tuple<int,int>>> sortedHints = new List<KeyValuePair<Vector3Int, Tuple<int, int>>>();
        //Iterate through each side, follow by each line (2D array)
        foreach (KeyValuePair<Vector3Int, int[]> normalIdx in normalIndices) {
            for (int x = 0; x < voxelArray.GetLength(normalIdx.Value[0]); x++)
            {
                for (int y = 0; y < voxelArray.GetLength(normalIdx.Value[1]); y++)
                {
                    sortedHints.Add(new KeyValuePair<Vector3Int, Tuple<int,int>>(normalIdx.Key, new Tuple<int, int>(x, y)));
                    
                    //Initialize tempCount
                    hintDict[normalIdx.Key][x, y].tempCount = hintDict[normalIdx.Key][x, y].count;
                }
            }
        }
        
        //Sort hints by CalculateDifficulty, to adjust sorting order, adjust CalculateDifficulty
        List<KeyValuePair<Vector3Int, Tuple<int,int>>> hintRef =
            sortedHints.OrderBy(
                    x => hintDict[x.Key][x.Value.Item1, x.Value.Item2].CalculateDifficulty(false)).ToList();
        
        //System.Random r = new System.Random();
        //List<KeyValuePair<Vector3Int, Tuple<int,int>>> hintRef =
        //    sortedHints.OrderBy(x => r.Next()).ToList();
        
        int left = 0;
        int right = hintRef.Count;
        int middle = 0;
        
        while (right > left + 1) {
            middle = Mathf.CeilToInt( (left + right)/2 );
            
            //Disable up till middle
            for (int i = 0; i <= middle; i++) {
                hintDict[hintRef[i].Key][hintRef[i].Value.Item1, hintRef[i].Value.Item2].count = -1;
            }
            if (IsSolvable()) {
                //Split to the right, remove harder hints
                left = middle;
            } else {
                //Split to the left, undo hard hint removal
                right = middle;
            }
            //Restore hint counts
            for (int i = 0; i < hintRef.Count; i++) {
                hintDict[hintRef[i].Key][hintRef[i].Value.Item1, hintRef[i].Value.Item2].count =
                    hintDict[hintRef[i].Key][hintRef[i].Value.Item1, hintRef[i].Value.Item2].tempCount;
            }
        }
        
        int hintRemovalCount = 0;
        
        //Binary result found, disable up till middle for the final time
        for (int i = 0; i < middle; i++) {
            hintDict[hintRef[i].Key][hintRef[i].Value.Item1, hintRef[i].Value.Item2].count = -1;
            hintRemovalCount += 1;
        }
        Debug.Log("Binary hint removal completed! Middle found: " + middle + ", Total hints: " + hintRef.Count);
        
        System.Random random = new System.Random(7355);
        
        //Hybrid approach, iterate through the rest
        for (int i = middle - 1; i < hintRef.Count; i++) {
            hintDict[hintRef[i].Key][hintRef[i].Value.Item1, hintRef[i].Value.Item2].count = -1;
            if (!IsSolvable()) {// || random.Next(1,11) > 9) {
                hintDict[hintRef[i].Key][hintRef[i].Value.Item1, hintRef[i].Value.Item2].count =
                    hintDict[hintRef[i].Key][hintRef[i].Value.Item1, hintRef[i].Value.Item2].tempCount;
            } else {
                hintRemovalCount += 1;
            }
        }

        Debug.Log("Hint removal completed! " + hintRemovalCount + " hint(s) removed!");
        
        //Disabled because unnecessary
        
        //int isSolvableExecutions = 0;
        //int groupSize = 20;
        //int tempGroupSize = 20;
        //Hybrid approach, iterate through the rest
        //Grouped approach, speed everything up by disabling hints in groups
        //for (int i = middle - 1; i < hintRef.Count; i+=groupSize) {
        //    //if (hintDict[hintRef[i].Key][hintRef[i].Value.Item1, hintRef[i].Value.Item2].count == -1)
        //    //    continue;
        //    
        //    Debug.Log("Processed hint groups: " + i + "/" + hintRef.Count);
        //    
        //    //Adjust group size
        //    if (i + groupSize >= hintRef.Count)
        //        groupSize = 1;
        //    
        //    //Reset tempGroupSize
        //    tempGroupSize = groupSize;
        //    
        //    while (true) {
        //        bool succesfullyRemovedHints = false;
        //        Debug.Log("tempGroupSize: " + tempGroupSize);
        //        for (int j = 0; j < groupSize/tempGroupSize; j++) {
        //            int offset = j * tempGroupSize;
        //            //Disable all hints in group
        //            for (int k = i + offset; k < i + tempGroupSize + offset; k++) {
        //                hintDict[hintRef[k].Key][hintRef[k].Value.Item1, hintRef[k].Value.Item2].count = -1;
        //            }
        //            
        //            //Check if solvable when removing hint group, if not, undo hint removal
        //            bool solvable = IsSolvable();
        //            
        //            isSolvableExecutions++;
        //            if (!solvable) {
        //                //Debug.Log("Removing hint group... NOT SOLVABLE!");
        //                for (int k = i + offset; k < i + tempGroupSize + offset; k++) {
        //                    hintDict[hintRef[k].Key][hintRef[k].Value.Item1, hintRef[k].Value.Item2].count = 
        //                        hintDict[hintRef[k].Key][hintRef[k].Value.Item1, hintRef[k].Value.Item2].tempCount;
        //                }
        //            } else {
        //                succesfullyRemovedHints = true;
        //            }
        //        }
        //        
        //        if (tempGroupSize <= 2) {
        //            //Debug.Log("Reached min size <2!!!");
        //            break;
        //        }
        //        
        //        if (!succesfullyRemovedHints) {
        //            //Halve group size and restart
        //            tempGroupSize = Mathf.FloorToInt((float)tempGroupSize/2);
        //            //Debug.Log("Halved group!");
        //            continue;
        //        } else {
        //            //Debug.Log("Successfully removed some hints! tempGroupSize: " + tempGroupSize);
        //            break;
        //        }
        //    }
        //    
        //}
        
        //Debug.Log("BinaryOrderedHintRemoval: Completed! isSolvableExecutions: " + isSolvableExecutions);
    }
    
    public void OrderedHintRemoval() {
        var normalIndices = new Dictionary<Vector3Int, int[]>() {
            {Vector3Int.up, new int[]{0,2}},
            {Vector3Int.right, new int[]{1,2}},
            {new Vector3Int(0,0,1)/*Forwards*/, new int[]{0,1}}
        };
        
        //Initialize ordered reference list
        List<KeyValuePair<Vector3Int, Tuple<int,int>>> sortedHints = new List<KeyValuePair<Vector3Int, Tuple<int, int>>>();
        //Iterate through each side, follow by each line (2D array)
        foreach (KeyValuePair<Vector3Int, int[]> normalIdx in normalIndices) {
            for (int x = 0; x < voxelArray.GetLength(normalIdx.Value[0]); x++)
            {
                for (int y = 0; y < voxelArray.GetLength(normalIdx.Value[1]); y++)
                {
                    sortedHints.Add(new KeyValuePair<Vector3Int, Tuple<int,int>>(normalIdx.Key, new Tuple<int, int>(x, y)));
                }
            }
        }
        //Sort hints by CalculateDifficulty, to adjust sorting order, adjust CalculateDifficulty
        IEnumerable<KeyValuePair<Vector3Int, Tuple<int,int>>> sortedHintsEnumerable = sortedHints.OrderBy(x => hintDict[x.Key][x.Value.Item1, x.Value.Item2].CalculateDifficulty(false));
        
        //Iterate through sorted hints
        foreach (KeyValuePair<Vector3Int, Tuple<int,int>> hintRef in sortedHintsEnumerable) {
            //Check if solveable when removing hint, if not, undo hint removal
            int tempCount = hintDict[hintRef.Key][hintRef.Value.Item1, hintRef.Value.Item2].count;
            hintDict[hintRef.Key][hintRef.Value.Item1, hintRef.Value.Item2].count = -1;
            if (!IsSolvable()) {
                hintDict[hintRef.Key][hintRef.Value.Item1, hintRef.Value.Item2].count = tempCount;
            }
        }
    }
    
    public void BruteForceHintRemoval() {
        //Follows normalIndices order, which means that forwards hints will be most likely to be preserved
        //Better way to do it will be to sort all hints by difficulty (possible Hint2D's, potentially)

        var normalIndices = new Dictionary<Vector3Int, int[]>() {
            {Vector3Int.up, new int[]{0,2}},
            {Vector3Int.right, new int[]{1,2}},
            {new Vector3Int(0,0,1)/*Forwards*/, new int[]{0,1}}
        };

        //Iterate through each side, follow by each line (2D array)
        foreach (KeyValuePair<Vector3Int, int[]> normalIdx in normalIndices) {
            for (int x = 0; x < voxelArray.GetLength(normalIdx.Value[0]); x++)
            {
                for (int y = 0; y < voxelArray.GetLength(normalIdx.Value[1]); y++)
                {
                    //Check if solveable when removing hint, if not, undo hint removal
                    int tempCount = hintDict[normalIdx.Key][x, y].count;
                    hintDict[normalIdx.Key][x, y].count = -1;
                    if (!IsSolvable()) {
                        hintDict[normalIdx.Key][x, y].count = tempCount;
                    }
                }
            }
        }
    }

    public int[,,] CarvedArray() {
        int[,,] outArray = (int[,,])voxelArray.Clone();
        for (int x = 0; x < outArray.GetLength(0); x++)
        {
            for (int y = 0; y < outArray.GetLength(1); y++)
            {
                for (int z = 0; z < outArray.GetLength(2); z++)
                {
                    if (outArray[x,y,z] == -1) {
                        outArray[x,y,z] = 1;
                    }
                }
            }
        }
        return outArray;
    }
}



public class SolutionHandler : MonoBehaviour
{
    Vector3Int chunkSize;
    
    Puzzle currentPuzzle;
    public int[,,] solutionArray;
    
    public GameObject boundingBox;
    
    public RectTransform selectPanel;
    public RectTransform scrobblePanel;
    
    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI errorsText;
    
    public VoxelGenerator voxelGenerator;
    
    public DialogHandler loadingDialog;
    
    public void SetTimer(bool setEnabled) {timerEnabled = setEnabled;}
    bool timerEnabled = false;
    public float startTime = 0.0f;
    public int errors;
    
    public bool isSolved = false;
    
    public bool initializing = false;
    
    Stopwatch debugStopwatch;
    
    // Start is called before the first frame update
    public void Init(Puzzle puzzle)
    {
        if (initializing) {
            Debug.Log("Attempted to initalize solver, while solving!");
            return;
        }
        
        FindObjectOfType<ModeHandler>().Reset();
        FindObjectOfType<ModeHandler>().designMode = false;
        FindObjectOfType<ModeHandler>().selectPanel = selectPanel;
        FindObjectOfType<ModeHandler>().scrobblePanel = scrobblePanel;
        
        currentPuzzle = puzzle;
        chunkSize = puzzle.puzzleSize;
        
        solutionArray = SaveLoadHandler.ExpandFlattened3DArray(puzzle.flattenedVoxelArray, puzzle.puzzleSize);
        
        voxelGenerator.Init(chunkSize, DesignHandler.EmptyTexture3D(chunkSize));
        boundingBox.SetActive(true);
        
        loadingDialog.Show();
        initializing = true;
        
        timerEnabled = false;
        timerText.SetText("Loading...");
        errorsText.SetText("");
        
        debugStopwatch = new Stopwatch();
        debugStopwatch.Start();
        
        StartCoroutine(WaitForHintRemoval());
        //WaitForHintRemoval();
    }
    
    IEnumerator WaitForHintRemoval() {
        Dictionary<Vector3Int, Hint[,]> indicatorDict = populateIndicatorDict(solutionArray, chunkSize);
        
        Solver s = new Solver(indicatorDict, chunkSize, solutionArray);
        bool hintRemovalDone = false;
        
        Thread solverThread = new Thread(() => {
            //s.DoPrepass();
            s.BinaryOrderedHintRemoval();
            hintRemovalDone = true;
        });
        
        //Debug.Break();
        
        solverThread.Start();
        
        while (!hintRemovalDone)
            yield return new WaitForSeconds(0.1f);
        
        Texture3D indicatorVolumeTexture = indicatorDictToTex3D(s.hintDict, chunkSize);
        voxelGenerator.Init(chunkSize, indicatorVolumeTexture);
        
        initializing = false;
        isSolved = false;
        timerEnabled = true;
        startTime = 0.0f;
        errors = 0;
        
        loadingDialog.Hide();
        
        debugStopwatch.Stop();
        Debug.Log("Elapsed milliseconds: " + debugStopwatch.ElapsedMilliseconds);
    }
    
    void Update() {
        if (timerEnabled) {
            startTime += Time.deltaTime;
        }
        if (timerText.text != String.Format("<b>Timer:</b> {0}s", Mathf.FloorToInt(startTime).ToString()) && timerEnabled) {
            timerText.SetText(String.Format("<b>Timer:</b> {0}s", Mathf.FloorToInt(startTime).ToString()));
        }
        if (errorsText.text != String.Format("<b>Errors:</b> {0}", errors.ToString())) {
            errorsText.SetText(String.Format("<b>Errors:</b> {0}", errors.ToString()));
        }
    }
    
    public void CheckIfSolved() {
        bool unsolved = false;
        
        for (int x = 0; x < chunkSize.x; x++) {
            for (int y = 0; y < chunkSize.y; y++) {
                for (int z = 0; z < chunkSize.z; z++) {
                    if (solutionArray[x,y,z] != voxelGenerator.voxelArray[x,y,z]) {
                        unsolved = true;
                    }
                }
            }
        }
        
        if (!unsolved) { //Solved!!
            timerEnabled = false;
            isSolved = true;
            
            voxelGenerator.colorArray = SaveLoadHandler.ExpandFlattened3DArray(currentPuzzle.flattenedColorArray, currentPuzzle.puzzleSize);
            voxelGenerator.highlightVolumeTexture = DesignHandler.EmptyTexture3D(currentPuzzle.puzzleSize);
            
            voxelGenerator.GenerateAndApplyMesh();
            
            boundingBox.SetActive(false);
        }
    }
    
    Texture3D indicatorDictToTex3D(Dictionary<Vector3Int, Hint[,]> indicatorDict, Vector3Int chunkSize) {
        Texture3D volumeTexture = new Texture3D(chunkSize.x, chunkSize.y, chunkSize.z, TextureFormat.RGBA32, 0);
        
        for (int x = 0; x < chunkSize.x; x++) {
            for (int y = 0; y < chunkSize.y; y++) {
                for (int z = 0; z < chunkSize.z; z++) {
                    Color32 color = new Color(0.0f,0.0f,0.0f,0.0f);
                    // Two four bit groups, the count and the group count, making the max 16 (15?) values
                    byte r = (byte)(((byte)indicatorDict[Vector3Int.right][y,z].count+1) << (byte)4);
                    byte g = (byte)(((byte)indicatorDict[Vector3Int.up][x,z].count+1) << (byte)4);
                    byte b = (byte)(((byte)indicatorDict[new Vector3Int(0,0,1)][x,y].count+1) << (byte)4);
                    r = (byte)(r | (byte)indicatorDict[Vector3Int.right][y,z].groups+1);
                    g = (byte)(g | (byte)indicatorDict[Vector3Int.up][x,z].groups+1);
                    b = (byte)(b | (byte)indicatorDict[new Vector3Int(0,0,1)][x,y].groups+1);
                    color.r = r;
                    color.g = g;
                    color.b = b;
                    volumeTexture.SetPixel(x, y, z, color, 0);
                }
            }
        }
        
        volumeTexture.Apply();
        
        return volumeTexture;
    }
    
    Hint GetColumnHint(int[,,] voxels, Vector3Int normal, int x, int y) {
        int height = voxels.GetLength(Array.IndexOf(new int[]{normal.x,normal.y,normal.z}, 1));
        
        Hint outHint = new Hint();
        bool breaking = true;
        for (int i = 0; i < height; i++) {
            int[] idx = new int[]{0,0,0};
            idx[Array.IndexOf(new int[]{normal.x,normal.y,normal.z}, 1)] = i;
            idx[Array.IndexOf(new int[]{normal.x,normal.y,normal.z}, 0)] = x;
            idx[Array.LastIndexOf(new int[]{normal.x,normal.y,normal.z}, 0)] = y;
            
            if ((int)voxels.GetValue(idx) > 0) {
                outHint.count++;
                if (breaking) {
                    outHint.groups++;
                    breaking = false;
                }
            } else {
                breaking = true;
            }
        }
        
        return outHint;
    }
    
    Dictionary<Vector3Int, Hint[,]> populateIndicatorDict(int[,,] voxels, Vector3Int chunkSize) {
        var outDict = new Dictionary<Vector3Int, Hint[,]>(){
            {Vector3Int.up, new Hint[chunkSize.x, chunkSize.z]},
            {Vector3Int.right, new Hint[chunkSize.y, chunkSize.z]},
            {new Vector3Int(0,0,1), new Hint[chunkSize.x, chunkSize.y]},
        };
        var normalIndices = new Dictionary<Vector3Int, int[]>() {
            {Vector3Int.up, new int[]{0,2}},
            {Vector3Int.right, new int[]{1,2}},
            {new Vector3Int(0,0,1), new int[]{0,1}}
        };
        
        for (int i = 0; i < outDict.Count; i++) {
            var key = outDict.Keys.ElementAt(i);
            var indices = normalIndices[key];
            
            for (int x = 0; x < voxels.GetLength(indices[0]); x++) {
                for (int y = 0; y < voxels.GetLength(indices[1]); y++) {
                    outDict[key][x,y] = GetColumnHint(voxels, key, x, y);
                }
            }
        }
        
        return outDict;
    }
}
