using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Cm107.Grid
{
    public class Grid2D<TGridObject>
    {
        private int m_width;
        public int width
        {
            get { return this.m_width; }
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
        private float m_cellHeight;
        public float cellHeight
        {
            get { return this.m_cellHeight; }
        }

        public float maxCellSide
        {
            get { return Mathf.Max(this.cellWidth, this.cellHeight); }
        }

        protected int size_offset = 0;
        private Vector2 m_origin;
        public Vector2 origin
        {
            get { return this.m_origin; }
        }
        public TGridObject[,] gridArray;
        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
        public class OnGridObjectChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        private void Setup(
            int width, int height, float cellWidth, float cellHeight,
            Vector2 origin = default(Vector2),
            Func<Grid2D<TGridObject>, int, int, TGridObject> initGrid = null,
            int size_offset = 0
        )
        {
            this.m_width = width; this.m_height = height;
            this.m_cellWidth = cellWidth; this.m_cellHeight = cellHeight;
            this.m_origin = origin;
            this.size_offset = size_offset;

            this.gridArray = new TGridObject[width + size_offset + 1, height + size_offset + 1];
            if (initGrid != null)
            {
                for (int x = 0; x <= width; x++)
                {
                    for (int y = 0; y <= height; y++)
                    {
                        this.gridArray[x, y] = initGrid(this, x, y);
                    }
                }
            }
        }

        public Grid2D(
            int width, int height, float cellWidth, float cellHeight,
            Vector2 origin = default(Vector2),
            Func<Grid2D<TGridObject>, int, int, TGridObject> initGrid = null,
            int size_offset = 0
        )
        {
            this.Setup(
                width: width, height: height,
                cellWidth: cellWidth, cellHeight: cellHeight,
                origin: origin, initGrid: initGrid, size_offset: size_offset
            );
        }

        public Grid2D(
            int width, float cellWidth,
            Vector2 origin = default(Vector2),
            Func<Grid2D<TGridObject>, int, int, TGridObject> initGrid = null,
            int size_offset = 0
        )
        {
            this.Setup(
                width: width, height: width,
                cellWidth: cellWidth, cellHeight: cellWidth,
                origin: origin, initGrid: initGrid, size_offset: size_offset
            );
        }

        // Note: It seems like RoundToInt is faster than FloorToInt and CeilToInt.
        public Vector2Int World2GridCoord(Vector2 coord)
        {
            Vector2 relCoord = coord - this.origin;
            return new Vector2Int(
                x: Mathf.RoundToInt(relCoord.x / this.m_cellWidth),
                y: Mathf.RoundToInt(relCoord.y / this.m_cellHeight)
            );
        }

        public Vector2Int World2GridCoord(Vector2 coord, bool max = false)
        {
            Vector2 relCoord = coord - this.origin;
            if (!max)
                return new Vector2Int(
                    x: Mathf.FloorToInt(relCoord.x / this.m_cellWidth),
                    y: Mathf.FloorToInt(relCoord.y / this.m_cellHeight)
                );
            else
                return new Vector2Int(
                    x: Mathf.CeilToInt(relCoord.x / this.m_cellWidth),
                    y: Mathf.CeilToInt(relCoord.y / this.m_cellHeight)
                );
        }

        public Vector2Int ClampGridCoord(Vector2Int gridCoord)
        {
            return new Vector2Int(
                x: Mathf.Clamp(gridCoord.x, min: 0, max: this.width + size_offset),
                y: Mathf.Clamp(gridCoord.y, min: 0, max: this.height + size_offset)
            );
        }

        // public Vector2 GetWorldPosition(int x, int z, int y)
        // {
        //     return new Vector2(
        //         x: this.origin.x + x * this.cellWidth,
        //         y: this.origin.y + y * this.cellHeight,
        //         z: this.origin.z + z * this.cellDepth
        //     );
        // }

        public Vector2 Grid2WorldPosition(Vector2Int gridCoord)
        {
            return new Vector2(
                x: this.origin.x + gridCoord.x * this.cellWidth,
                y: this.origin.y + gridCoord.y * this.cellHeight
            );
        }

        public Vector2? Grid2WorldPositionIfValid(Vector2Int gridCoord)
        {
            if (this.gridBoundingBox.Contains(gridCoord))
                return this.Grid2WorldPosition(gridCoord);
            else
                return null;
        }

        public BoundingBox2D Grid2WorldBBox(BoundingBoxInt2D gridBBox)
        {
            return new BoundingBox2D
            {
                v0 = this.Grid2WorldPosition(gridBBox.v0),
                v1 = this.Grid2WorldPosition(gridBBox.v1)
            };
        }

        public BoundingBoxInt2D World2GridBBox(BoundingBox2D worldBBox)
        {
            return new BoundingBoxInt2D
            {
                v0 = this.World2GridCoord(worldBBox.v0, max: false),
                v1 = this.World2GridCoord(worldBBox.v1, max: true)
            };
        }

        public TGridObject GetGridObject(int x, int y)
        {
            return this.gridArray[x, y];
        }

        public TGridObject GetGridObject(Vector2Int coord)
        {
            return this.GetGridObject(x: coord.x, y: coord.y);
        }

        public TGridObject GetGridObject(Vector2 position)
        {
            Vector2Int intVector = this.World2GridCoord(position);
            return this.GetGridObject(x: intVector.x, y: intVector.y);
        }

        public TGridObject GetGridObject(float x, float y)
        {
            return this.GetGridObject(position: new Vector2(x: x, y: y));
        }

        public void SetGridObject(int x, int y, TGridObject obj)
        {
            this.gridArray[x, y] = obj;
            this.OnGridObjectChanged?.Invoke(sender: this, e: new OnGridObjectChangedEventArgs { x = x, y = y });
        }

        public void SetGridObject(Vector2Int coord, TGridObject obj)
        {
            this.SetGridObject(x: coord.x, y: coord.y, obj: obj);
        }

        public void SetGridObject(Vector2 position, TGridObject obj)
        {
            Vector2Int coord = this.World2GridCoord(position);
            this.SetGridObject(coord: coord, obj: obj);
        }

        public void SetGridObject(float x, float y, TGridObject obj)
        {
            this.SetGridObject(position: new Vector2(x: x, y: y), obj: obj);
        }

        public void TriggerGridObjectChanged(int x, int z, int y)
        {
            this.OnGridObjectChanged?.Invoke(sender: this, e: new OnGridObjectChangedEventArgs { x = x, y = y });
        }

        private static TextMesh CreateWorldText(Transform parent, string text, Vector2 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
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
            Action<Grid2D<TGridObject>, Vector2Int> loopFunc, BoundingBoxInt2D? bbox = null
        )
        {
            if (bbox != null)
            {
                bbox = BoundingBoxInt2D.Intersection(bbox.Value, this.gridBoundingBox);
                if (bbox == null) return; // No intersection. Bbox is completely outside of grid.
            }

            int xmin = (bbox != null) ? bbox.Value.v0.x : 0;
            int xmax = (bbox != null) ? bbox.Value.v1.x : this.width + this.size_offset;
            int ymin = (bbox != null) ? bbox.Value.v0.y : 0;
            int ymax = (bbox != null) ? bbox.Value.v1.y : this.height + this.size_offset;

            for (int y = ymin; y <= ymax; y++)
            {
                for (int x = xmin; x <= xmax; x++)
                {
                    Vector2Int gridCoord = new Vector2Int(x, y);
                    loopFunc(this, gridCoord);
                }
            }
        }

        public void LoopCoords(
            Action<Vector2Int> loopFunc, BoundingBoxInt2D? bbox = null
        )
        {
            if (bbox != null)
            {
                bbox = BoundingBoxInt2D.Intersection(bbox.Value, this.gridBoundingBox);
                if (bbox == null) return; // No intersection. Bbox is completely outside of grid.
            }

            int xmin = (bbox != null) ? bbox.Value.v0.x : 0;
            int xmax = (bbox != null) ? bbox.Value.v1.x : this.width + this.size_offset;
            int ymin = (bbox != null) ? bbox.Value.v0.y : 0;
            int ymax = (bbox != null) ? bbox.Value.v1.y : this.height + this.size_offset;

            for (int y = ymin; y <= ymax; y++)
            {
                for (int x = xmin; x <= xmax; x++)
                {
                    Vector2Int gridCoord = new Vector2Int(x, y);
                    loopFunc(gridCoord);
                }
            }
        }

        public void ParallelLoopCoords(
            Action<Grid2D<TGridObject>, Vector2Int> loopFunc, BoundingBoxInt2D? bbox = null
        )
        {
            if (bbox != null)
            {
                bbox = BoundingBoxInt2D.Intersection(bbox.Value, this.gridBoundingBox);
                if (bbox == null) return; // No intersection. Bbox is completely outside of grid.
            }

            int xmin = (bbox != null) ? bbox.Value.v0.x : 0;
            int xmax = (bbox != null) ? bbox.Value.v1.x : this.width + this.size_offset;
            int ymin = (bbox != null) ? bbox.Value.v0.y : 0;
            int ymax = (bbox != null) ? bbox.Value.v1.y : this.height + this.size_offset;

            List<Vector2Int> gridCoordList = new List<Vector2Int>();
            for (int y = ymin; y <= ymax; y++)
            {
                for (int x = xmin; x <= xmax; x++)
                {
                    gridCoordList.Add(new Vector2Int(x, y));
                }
            }
            Parallel.ForEach<Vector2Int>(
                source: gridCoordList, body: gridCoord =>
                {
                    loopFunc(this, gridCoord);
                }

            );
        }

        public void ParallelLoopCoords(
            Action<Vector2Int> loopFunc, BoundingBoxInt2D? bbox = null
        )
        {
            if (bbox != null)
            {
                bbox = BoundingBoxInt2D.Intersection(bbox.Value, this.gridBoundingBox);
                if (bbox == null) return; // No intersection. Bbox is completely outside of grid.
            }

            int xmin = (bbox != null) ? bbox.Value.v0.x : 0;
            int xmax = (bbox != null) ? bbox.Value.v1.x : this.width + this.size_offset;
            int ymin = (bbox != null) ? bbox.Value.v0.y : 0;
            int ymax = (bbox != null) ? bbox.Value.v1.y : this.height + this.size_offset;

            List<Vector2Int> gridCoordList = new List<Vector2Int>();
            for (int y = ymin; y <= ymax; y++)
            {
                for (int x = xmin; x <= xmax; x++)
                {
                    // Vector2Int gridCoord = new Vector2Int(x, y, z);
                    // loopFunc(this, gridCoord);
                    gridCoordList.Add(new Vector2Int(x, y));
                }
            }
            Parallel.ForEach<Vector2Int>(
                source: gridCoordList, body: gridCoord =>
                {
                    loopFunc(gridCoord);
                }

            );
        }

        public BoundingBoxInt2D gridBoundingBox
        {
            get
            {
                return new BoundingBoxInt2D
                {
                    v0 = Vector2Int.zero,
                    v1 = new Vector2Int(
                        this.width + this.size_offset,
                        this.height + this.size_offset
                    )
                };
            }
        }

        public BoundingBoxInt2D marchingCubesBbox
        {
            get
            {
                return new BoundingBoxInt2D
                {
                    v0 = Vector2Int.zero,
                    v1 = new Vector2Int(
                        this.width + this.size_offset - 1,
                        this.height + this.size_offset - 1
                    )
                };
            }
        }

        public bool VoxelContainsPoint(Vector2Int gridCoord, Vector2 p)
        {
            Vector2Int v0 = gridCoord; Vector2Int v1 = gridCoord + Vector2Int.one;
            Vector2? worldV0 = this.Grid2WorldPositionIfValid(v0); Vector2? worldV1 = this.Grid2WorldPositionIfValid(v1);
            if (worldV0 == null || worldV1 == null) return false;
            else return new BoundingBox2D { v0 = worldV0.Value, v1 = worldV1.Value }.Contains(p);
        }

        // Copied From MarchingCubesData
        private static Vector2Int[] CornerTable = new Vector2Int[4] {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1)
        };

        // Copied From MarchingCubesData
        private static int[,] EdgeIndexes = new int[4, 2] {
            {0, 1}, {1, 2}, {3, 2}, {0, 3}
        };

        private static Vector2Int[] GetCorners(Vector2Int gridCoord)
        {
            Vector2Int[] corners = new Vector2Int[MarchingCubesTables.CornerTable.Length];
            for (int i = 0; i < corners.Length; i++)
                corners[i] = gridCoord + CornerTable[i];
            return corners;
        }

        public void DebugDrawVoxel(
            Vector2Int gridCoord,
            Color color, float duration
        )
        {
            Vector2Int[] corners = GetCorners(gridCoord);
            for (int i = 0; i < EdgeIndexes.GetLength(0); i++)
            {
                int start = EdgeIndexes[i, 0];
                int end = EdgeIndexes[i, 1];
                Vector2Int startCoord = corners[start];
                Vector2Int endCoord = corners[end];
                Vector2? startPosition = this.Grid2WorldPositionIfValid(startCoord);
                Vector2? endPosition = this.Grid2WorldPositionIfValid(endCoord);
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