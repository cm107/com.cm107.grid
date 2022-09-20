using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Cm107.Grid
{
    public class Grid3D<TGridObject>
    {
        private int m_width;
        public int width
        {
            get { return this.m_width; }
        }
        private int m_depth;
        public int depth
        {
            get { return this.m_depth; }
        }
        private int m_height;
        public int height
        {
            get { return this.m_height; }
        }
        private float m_cellWidth;
        public float cellWidth
        {
            get { return this.m_cellWidth; }
        }
        private float m_cellDepth;
        public float cellDepth
        {
            get { return this.m_cellDepth; }
        }
        private float m_cellHeight;
        public float cellHeight
        {
            get { return this.m_cellHeight; }
        }

        public float maxCellSide
        {
            get { return Mathf.Max(this.cellWidth, this.cellHeight, this.cellDepth); }
        }

        protected int size_offset = 0;
        private Vector3 m_origin;
        public Vector3 origin
        {
            get { return this.m_origin; }
        }
        public TGridObject[,,] gridArray;
        // private TextMesh[,,] debugTextArray;
        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
            public int z;
        }

        private void Setup(
            int width, int depth, int height, float cellWidth, float cellDepth, float cellHeight,
            Vector3 origin = default(Vector3),
            Func<Grid3D<TGridObject>, int, int, int, TGridObject> initGrid = null,
            int size_offset = 0, bool drawDebugGrid = false
        )
        {
            this.m_width = width; this.m_depth = depth; this.m_height = height;
            this.m_cellWidth = cellWidth; this.m_cellDepth = cellDepth; this.m_cellHeight = cellHeight;
            this.m_origin = origin;
            this.size_offset = size_offset;

            this.gridArray = new TGridObject[width + size_offset + 1, depth + size_offset + 1, height + size_offset + 1];
            if (initGrid != null)
            {
                for (int x = 0; x <= width; x++)
                {
                    for (int z = 0; z <= depth; z++)
                    {
                        for (int y = 0; y <= height; y++)
                        {
                            this.gridArray[x, z, y] = initGrid(this, x, z, y);
                        }
                    }
                }
            }
            if (drawDebugGrid)
            {
                this.DrawDebugGrid(color: Color.white);
            }
        }

        public Grid3D(
            int width, int depth, int height, float cellWidth, float cellDepth, float cellHeight,
            Vector3 origin = default(Vector3),
            Func<Grid3D<TGridObject>, int, int, int, TGridObject> initGrid = null,
            int size_offset = 0, bool drawDebugGrid = false
        )
        {
            this.Setup(
                width: width, depth: depth, height: height,
                cellWidth: cellWidth, cellDepth: cellDepth, cellHeight: cellHeight,
                origin: origin, initGrid: initGrid, size_offset: size_offset,
                drawDebugGrid: drawDebugGrid
            );
        }

        public Grid3D(
            int width, float cellWidth,
            Vector3 origin = default(Vector3),
            Func<Grid3D<TGridObject>, int, int, int, TGridObject> initGrid = null,
            int size_offset = 0, bool drawDebugGrid = false
        )
        {
            this.Setup(
                width: width, depth: width, height: width,
                cellWidth: cellWidth, cellDepth: cellWidth, cellHeight: cellWidth,
                origin: origin, initGrid: initGrid, size_offset: size_offset,
                drawDebugGrid: drawDebugGrid
            );
        }

        private void DrawDebugGrid(Color color, bool drawLines = true)
        {
            // this.debugTextArray = new TextMesh[this.width + 1, this.depth + 1, this.height + 1];

            for (int i = 0; i <= this.width + this.size_offset; i++)
            {
                float x0 = this.origin.x + this.cellWidth * i;
                float x1 = this.origin.x + this.cellWidth * (i + 1);
                for (int j = 0; j <= this.depth + this.size_offset; j++)
                {
                    float z0 = this.origin.z + this.cellDepth * j;
                    float z1 = this.origin.z + this.cellDepth * (j + 1);
                    for (int k = 0; k <= this.height + this.size_offset; k++)
                    {
                        float y0 = this.origin.y + this.cellHeight * k;
                        float y1 = this.origin.y + this.cellHeight * (k + 1);

                        if (drawLines)
                        {
                            if (i < this.width + this.size_offset)
                                Debug.DrawLine(
                                    start: new Vector3(x: (float)x0, y: (float)y0, z: (float)z0),
                                    end: new Vector3(x: (float)x1, y: (float)y0, z: (float)z0),
                                    color: color,
                                    duration: Mathf.Infinity
                                );
                            if (j < this.depth + this.size_offset)
                                Debug.DrawLine(
                                    start: new Vector3(x: (float)x0, y: (float)y0, z: (float)z0),
                                    end: new Vector3(x: (float)x0, y: (float)y0, z: (float)z1),
                                    color: color,
                                    duration: Mathf.Infinity
                                );
                            if (k < this.height + this.size_offset)
                                Debug.DrawLine(
                                    start: new Vector3(x: (float)x0, y: (float)y0, z: (float)z0),
                                    end: new Vector3(x: (float)x0, y: (float)y1, z: (float)z0),
                                    color: color,
                                    duration: Mathf.Infinity
                                );
                        }
                        // this.debugTextArray[i, j, k] = CreateWorldText(
                        //     text: this.gridArray[i, j, k]?.ToString(),
                        //     parent: null,
                        //     localPosition: new Vector3(x0, y0, z0),
                        //     fontSize: 8,
                        //     color: Color.red,
                        //     textAnchor: TextAnchor.MiddleCenter,
                        //     textAlignment: TextAlignment.Center,
                        //     sortingOrder: 5000
                        // );
                        // // this.OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                        // //     debugTextArray[eventArgs.x, eventArgs.z, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.z, eventArgs.y]?.ToString();
                        // //     Debug.Log("Changed text.");
                        // // };
                    }
                }
            }
        }

        // Note: It seems like RoundToInt is faster than FloorToInt and CeilToInt.
        public Vector3Int World2GridCoord(Vector3 coord)
        {
            Vector3 relCoord = coord - this.origin;
            return new Vector3Int(
                x: Mathf.RoundToInt(relCoord.x / this.m_cellWidth),
                y: Mathf.RoundToInt(relCoord.y / this.m_cellHeight),
                z: Mathf.RoundToInt(relCoord.z / this.m_cellDepth)
            );
        }

        public Vector3Int World2GridCoord(Vector3 coord, bool max = false)
        {
            Vector3 relCoord = coord - this.origin;
            if (!max)
                return new Vector3Int(
                    x: Mathf.FloorToInt(relCoord.x / this.m_cellWidth),
                    y: Mathf.FloorToInt(relCoord.y / this.m_cellHeight),
                    z: Mathf.FloorToInt(relCoord.z / this.m_cellDepth)
                );
            else
                return new Vector3Int(
                    x: Mathf.CeilToInt(relCoord.x / this.m_cellWidth),
                    y: Mathf.CeilToInt(relCoord.y / this.m_cellHeight),
                    z: Mathf.CeilToInt(relCoord.z / this.m_cellDepth)
                );
        }

        public Vector3Int ClampGridCoord(Vector3Int gridCoord)
        {
            return new Vector3Int(
                x: Mathf.Clamp(gridCoord.x, min: 0, max: this.width + size_offset),
                y: Mathf.Clamp(gridCoord.y, min: 0, max: this.height + size_offset),
                z: Mathf.Clamp(gridCoord.z, min: 0, max: this.depth + size_offset)
            );
        }

        // public Vector3 GetWorldPosition(int x, int z, int y)
        // {
        //     return new Vector3(
        //         x: this.origin.x + x * this.cellWidth,
        //         y: this.origin.y + y * this.cellHeight,
        //         z: this.origin.z + z * this.cellDepth
        //     );
        // }

        public Vector3 Grid2WorldPosition(Vector3Int gridCoord)
        {
            return new Vector3(
                x: this.origin.x + gridCoord.x * this.cellWidth,
                y: this.origin.y + gridCoord.y * this.cellHeight,
                z: this.origin.z + gridCoord.z * this.cellDepth
            );
        }

        public Vector3? Grid2WorldPositionIfValid(Vector3Int gridCoord)
        {
            if (this.gridBoundingBox.Contains(gridCoord))
                return this.Grid2WorldPosition(gridCoord);
            else
                return null;
        }

        public BoundingBox Grid2WorldBBox(BoundingBoxInt gridBBox)
        {
            return new BoundingBox
            {
                v0 = this.Grid2WorldPosition(gridBBox.v0),
                v1 = this.Grid2WorldPosition(gridBBox.v1)
            };
        }

        public BoundingBoxInt World2GridBBox(BoundingBox worldBBox)
        {
            return new BoundingBoxInt
            {
                v0 = this.World2GridCoord(worldBBox.v0, max: false),
                v1 = this.World2GridCoord(worldBBox.v1, max: true)
            };
        }

        public TGridObject GetGridObject(int x, int z, int y)
        {
            return this.gridArray[x, z, y];
        }

        public TGridObject GetGridObject(Vector3Int coord)
        {
            return this.GetGridObject(x: coord.x, z: coord.z, y: coord.y);
        }

        public TGridObject GetGridObject(Vector3 position)
        {
            Vector3Int intVector = this.World2GridCoord(position);
            return this.GetGridObject(x: intVector.x, z: intVector.z, y: intVector.y);
        }

        public TGridObject GetGridObject(float x, float z, float y)
        {
            return this.GetGridObject(position: new Vector3(x: x, y: y, z: z));
        }

        public void SetGridObject(int x, int z, int y, TGridObject obj)
        {
            this.gridArray[x, z, y] = obj;
            this.OnGridObjectChanged?.Invoke(sender: this, e: new OnGridObjectChangedEventArgs { x = x, y = y, z = z });
        }

        public void SetGridObject(Vector3Int coord, TGridObject obj)
        {
            this.SetGridObject(x: coord.x, z: coord.z, y: coord.y, obj: obj);
        }

        public void SetGridObject(Vector3 position, TGridObject obj)
        {
            Vector3Int coord = this.World2GridCoord(position);
            this.SetGridObject(coord: coord, obj: obj);
        }

        public void SetGridObject(float x, float z, float y, TGridObject obj)
        {
            this.SetGridObject(position: new Vector3(x: x, y: y, z: z), obj: obj);
        }

        public void TriggerGridObjectChanged(int x, int z, int y)
        {
            this.OnGridObjectChanged?.Invoke(sender: this, e: new OnGridObjectChangedEventArgs { x = x, y = y, z = z });
        }

        private static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }

        // Warning: LoopCoords and ParallelLoopCoords suffer from overhead when working with variables defined out of scope.
        // https://stackoverflow.com/questions/63230014/why-are-func-delegates-so-much-slower
        public void LoopCoords(
            Action<Grid3D<TGridObject>, Vector3Int> loopFunc, BoundingBoxInt? bbox = null
        )
        {
            if (bbox != null)
            {
                bbox = BoundingBoxInt.Intersection(bbox.Value, this.gridBoundingBox);
                if (bbox == null) return; // No intersection. Bbox is completely outside of grid.
            }

            int xmin = (bbox != null) ? bbox.Value.v0.x : 0;
            int xmax = (bbox != null) ? bbox.Value.v1.x : this.width + this.size_offset;
            int ymin = (bbox != null) ? bbox.Value.v0.y : 0;
            int ymax = (bbox != null) ? bbox.Value.v1.y : this.height + this.size_offset;
            int zmin = (bbox != null) ? bbox.Value.v0.z : 0;
            int zmax = (bbox != null) ? bbox.Value.v1.z : this.depth + this.size_offset;

            for (int y = ymin; y <= ymax; y++)
            {
                for (int z = zmin; z <= zmax; z++)
                {
                    for (int x = xmin; x <= xmax; x++)
                    {
                        Vector3Int gridCoord = new Vector3Int(x, y, z);
                        loopFunc(this, gridCoord);
                    }
                }
            }
        }

        public void LoopCoords(
            Action<Vector3Int> loopFunc, BoundingBoxInt? bbox = null
        )
        {
            if (bbox != null)
            {
                bbox = BoundingBoxInt.Intersection(bbox.Value, this.gridBoundingBox);
                if (bbox == null) return; // No intersection. Bbox is completely outside of grid.
            }

            int xmin = (bbox != null) ? bbox.Value.v0.x : 0;
            int xmax = (bbox != null) ? bbox.Value.v1.x : this.width + this.size_offset;
            int ymin = (bbox != null) ? bbox.Value.v0.y : 0;
            int ymax = (bbox != null) ? bbox.Value.v1.y : this.height + this.size_offset;
            int zmin = (bbox != null) ? bbox.Value.v0.z : 0;
            int zmax = (bbox != null) ? bbox.Value.v1.z : this.depth + this.size_offset;

            for (int y = ymin; y <= ymax; y++)
            {
                for (int z = zmin; z <= zmax; z++)
                {
                    for (int x = xmin; x <= xmax; x++)
                    {
                        Vector3Int gridCoord = new Vector3Int(x, y, z);
                        loopFunc(gridCoord);
                    }
                }
            }
        }

        public void ParallelLoopCoords(
            Action<Grid3D<TGridObject>, Vector3Int> loopFunc, BoundingBoxInt? bbox = null
        )
        {
            if (bbox != null)
            {
                bbox = BoundingBoxInt.Intersection(bbox.Value, this.gridBoundingBox);
                if (bbox == null) return; // No intersection. Bbox is completely outside of grid.
            }

            int xmin = (bbox != null) ? bbox.Value.v0.x : 0;
            int xmax = (bbox != null) ? bbox.Value.v1.x : this.width + this.size_offset;
            int ymin = (bbox != null) ? bbox.Value.v0.y : 0;
            int ymax = (bbox != null) ? bbox.Value.v1.y : this.height + this.size_offset;
            int zmin = (bbox != null) ? bbox.Value.v0.z : 0;
            int zmax = (bbox != null) ? bbox.Value.v1.z : this.depth + this.size_offset;

            List<Vector3Int> gridCoordList = new List<Vector3Int>();
            for (int y = ymin; y <= ymax; y++)
            {
                for (int z = zmin; z <= zmax; z++)
                {
                    for (int x = xmin; x <= xmax; x++)
                    {
                        // Vector3Int gridCoord = new Vector3Int(x, y, z);
                        // loopFunc(this, gridCoord);
                        gridCoordList.Add(new Vector3Int(x, y, z));
                    }
                }
            }
            Parallel.ForEach<Vector3Int>(
                source: gridCoordList, body: gridCoord =>
                {
                    loopFunc(this, gridCoord);
                }

            );
        }

        public void ParallelLoopCoords(
            Action<Vector3Int> loopFunc, BoundingBoxInt? bbox = null
        )
        {
            if (bbox != null)
            {
                bbox = BoundingBoxInt.Intersection(bbox.Value, this.gridBoundingBox);
                if (bbox == null) return; // No intersection. Bbox is completely outside of grid.
            }

            int xmin = (bbox != null) ? bbox.Value.v0.x : 0;
            int xmax = (bbox != null) ? bbox.Value.v1.x : this.width + this.size_offset;
            int ymin = (bbox != null) ? bbox.Value.v0.y : 0;
            int ymax = (bbox != null) ? bbox.Value.v1.y : this.height + this.size_offset;
            int zmin = (bbox != null) ? bbox.Value.v0.z : 0;
            int zmax = (bbox != null) ? bbox.Value.v1.z : this.depth + this.size_offset;

            List<Vector3Int> gridCoordList = new List<Vector3Int>();
            for (int y = ymin; y <= ymax; y++)
            {
                for (int z = zmin; z <= zmax; z++)
                {
                    for (int x = xmin; x <= xmax; x++)
                    {
                        // Vector3Int gridCoord = new Vector3Int(x, y, z);
                        // loopFunc(this, gridCoord);
                        gridCoordList.Add(new Vector3Int(x, y, z));
                    }
                }
            }
            Parallel.ForEach<Vector3Int>(
                source: gridCoordList, body: gridCoord =>
                {
                    loopFunc(gridCoord);
                }

            );
        }

        public BoundingBoxInt gridBoundingBox
        {
            get
            {
                return new BoundingBoxInt
                {
                    v0 = Vector3Int.zero,
                    v1 = new Vector3Int(
                        this.width + this.size_offset,
                        this.height + this.size_offset,
                        this.depth + this.size_offset
                    )
                };
            }
        }

        public BoundingBoxInt marchingCubesBbox
        {
            get
            {
                return new BoundingBoxInt
                {
                    v0 = Vector3Int.zero,
                    v1 = new Vector3Int(
                        this.width + this.size_offset - 1,
                        this.height + this.size_offset - 1,
                        this.depth + this.size_offset - 1
                    )
                };
            }
        }

        public bool VoxelContainsPoint(Vector3Int gridCoord, Vector3 p)
        {
            Vector3Int v0 = gridCoord; Vector3Int v1 = gridCoord + Vector3Int.one;
            Vector3? worldV0 = this.Grid2WorldPositionIfValid(v0); Vector3? worldV1 = this.Grid2WorldPositionIfValid(v1);
            if (worldV0 == null || worldV1 == null) return false;
            else return new BoundingBox { v0 = worldV0.Value, v1 = worldV1.Value }.Contains(p);
        }

        // Copied From MarchingCubesData
        private static Vector3Int[] CornerTable = new Vector3Int[8] {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(1, 0, 1),
            new Vector3Int(1, 1, 1),
            new Vector3Int(0, 1, 1)
        };

        // Copied From MarchingCubesData
        private static int[,] EdgeIndexes = new int[12, 2] {
            {0, 1}, {1, 2}, {3, 2}, {0, 3}, {4, 5}, {5, 6}, {7, 6}, {4, 7}, {0, 4}, {1, 5}, {2, 6}, {3, 7}
        };

        private static Vector3Int[] GetCorners(Vector3Int gridCoord)
        {
            Vector3Int[] corners = new Vector3Int[MarchingCubesTables.CornerTable.Length];
            for (int i = 0; i < corners.Length; i++)
                corners[i] = gridCoord + CornerTable[i];
            return corners;
        }

        public void DebugDrawVoxel(
            Vector3Int gridCoord,
            Color color, float duration
        )
        {
            Vector3Int[] corners = GetCorners(gridCoord);
            for (int i = 0; i < EdgeIndexes.GetLength(0); i++)
            {
                int start = EdgeIndexes[i, 0];
                int end = EdgeIndexes[i, 1];
                Vector3Int startCoord = corners[start];
                Vector3Int endCoord = corners[end];
                Vector3? startPosition = this.Grid2WorldPositionIfValid(startCoord);
                Vector3? endPosition = this.Grid2WorldPositionIfValid(endCoord);
                if (startPosition != null && endPosition != null)
                {
                    Debug.DrawLine(
                        start: startPosition.Value,
                        end: endPosition.Value,
                        color: color, duration: duration,
                        depthTest: true
                    );
                }
            }
        }
    }
}