using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cm107.Grid
{
    public struct IntervalInt
    {
        public int min;
        public int max;

        public IntervalInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Contains(int value)
        {
            return value >= this.min && value <= this.max;
        }

        public static IntervalInt Union(IntervalInt a, IntervalInt b)
        {
            return new IntervalInt(
                min: (a.min <= b.min) ? a.min : b.min,
                max: (a.max >= b.max) ? a.max : b.max
            );
        }

        public static IntervalInt? Intersection(IntervalInt a, IntervalInt b)
        {
            if (a.Contains(b.min) || a.Contains(b.max) || b.Contains(a.min) || b.Contains(a.max))
            {
                int min = Mathf.Max(a.min, b.min);
                int max = Mathf.Min(a.max, b.max);
                return new IntervalInt(min: min, max: max);
            }
            else
                return null;
        }
    }

    public struct IntervalFloat
    {
        public float min;
        public float max;

        public IntervalFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Contains(float value)
        {
            return value >= this.min && value <= this.max;
        }

        public static IntervalFloat Union(IntervalFloat a, IntervalFloat b)
        {
            return new IntervalFloat(
                min: (a.min <= b.min) ? a.min : b.min,
                max: (a.max >= b.max) ? a.max : b.max
            );
        }

        public static IntervalFloat? Intersection(IntervalFloat a, IntervalFloat b)
        {
            if (a.Contains(b.min) || a.Contains(b.max) || b.Contains(a.min) || b.Contains(a.max))
            {
                float min = Mathf.Max(a.min, b.min);
                float max = Mathf.Min(a.max, b.max);
                return new IntervalFloat(min: min, max: max);
            }
            else
                return null;
        }
    }

    public struct BoundingBox
    {
        public Vector3 v0;
        public Vector3 v1;

        public override string ToString()
        {
            return $"BoundingBox({v0} ~ {v1})";
        }

        public static BoundingBox operator +(BoundingBox bbox, Vector3 other) => new BoundingBox
        {
            v0 = bbox.v0 + other,
            v1 = bbox.v1 + other
        };

        public static BoundingBox operator -(BoundingBox bbox, Vector3 other) => new BoundingBox
        {
            v0 = bbox.v0 - other,
            v1 = bbox.v1 - other
        };

        public float xmin
        {
            get { return this.v0.x; }
        }

        public float xmax
        {
            get { return this.v1.x; }
        }

        public float ymin
        {
            get { return this.v0.y; }
        }

        public float ymax
        {
            get { return this.v1.y; }
        }

        public float zmin
        {
            get { return this.v0.z; }
        }

        public float zmax
        {
            get { return this.v1.z; }
        }

        public Vector3 center
        {
            get { return 0.5f * (this.v0 + this.v1); }
        }

        public BoundingBoxInt ToInt()
        {
            Vector3Int convert(Vector3 vec, bool ceil = false)
            {
                if (!ceil)
                    return new Vector3Int(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z));
                else
                    return new Vector3Int(Mathf.CeilToInt(vec.x), Mathf.CeilToInt(vec.y), Mathf.CeilToInt(vec.z));
            }
            return new BoundingBoxInt
            {
                v0 = convert(this.v0, ceil: false),
                v1 = convert(this.v1, ceil: true)
            };
        }

        public class WorkingValues
        {
            public float? xmin = null;
            public float? xmax = null;
            public float? ymin = null;
            public float? ymax = null;
            public float? zmin = null;
            public float? zmax = null;

            public WorkingValues() { }

            public bool isNull
            {
                get
                {
                    return this.xmin == null || this.xmax == null
                        || this.ymin == null || this.ymax == null
                        || this.zmin == null || this.zmax == null;
                }
            }

            public void Update(Vector3 point)
            {
                if (this.xmin == null || point.x < this.xmin.Value) this.xmin = point.x;
                if (this.xmax == null || point.x > this.xmax.Value) this.xmax = point.x;

                if (this.ymin == null || point.y < this.ymin.Value) this.ymin = point.y;
                if (this.ymax == null || point.y > this.ymax.Value) this.ymax = point.y;

                if (this.zmin == null || point.z < this.zmin.Value) this.zmin = point.z;
                if (this.zmax == null || point.z > this.zmax.Value) this.zmax = point.z;
            }

            public BoundingBox ToBoundingBox()
            {
                if (this.isNull)
                    throw new Exception("One of the working values are still null.");
                Vector3 vmin = new Vector3(x: xmin.Value, y: ymin.Value, z: zmin.Value);
                Vector3 vmax = new Vector3(x: xmax.Value, y: ymax.Value, z: zmax.Value);
                return new BoundingBox { v0 = vmin, v1 = vmax };
            }
        }

        // static public BoundingBox FromTriangle(FEE.SDFUtil.Triangle triangle)
        // {
        //     WorkingValues workingValues = new WorkingValues();
        //     foreach (Vector3 vert in new Vector3[] { triangle.v0, triangle.v1, triangle.v2 })
        //         workingValues.Update(vert);

        //     return workingValues.ToBoundingBox();
        // }

        static public BoundingBox FromVertices(IEnumerable<Vector3> vertices, Matrix4x4? mat = null)
        {
            WorkingValues workingValues = new WorkingValues();
            if (mat == null)
            {
                foreach (Vector3 vert in vertices)
                    workingValues.Update(vert);
            }
            else
            {
                foreach (Vector3 vert in vertices)
                    workingValues.Update(mat.Value.MultiplyPoint(vert));
            }

            return workingValues.ToBoundingBox();
        }

        // static public BoundingBox[] FromVertices(Vector3[] vertices, int[] triangles) // Shouldn't need triangles.
        // {
        //     List<BoundingBox> result = new List<BoundingBox>();
        //     FEE.SDFUtil.Triangle[] triangleObjs = FEE.SDFUtil.Triangle.FromVertices(vertices: vertices, triangles: triangles);
        //     foreach (FEE.SDFUtil.Triangle triangleObj in triangleObjs)
        //         result.Add(BoundingBox.FromTriangle(triangleObj));
        //     return result.ToArray();
        // }

        public bool ContainsX(float val, bool inclusive = true)
        {
            if (inclusive)
            {
                if (val < this.v0.x || val > this.v1.x) return false;
            }
            else
            {
                if (val <= this.v0.x || val >= this.v1.x) return false;
            }
            return true;
        }

        public bool ContainsY(float val, bool inclusive = true)
        {
            if (inclusive)
            {
                if (val < this.v0.y || val > this.v1.y) return false;
            }
            else
            {
                if (val <= this.v0.y || val >= this.v1.y) return false;
            }
            return true;
        }

        public bool ContainsZ(float val, bool inclusive = true)
        {
            if (inclusive)
            {
                if (val < this.v0.z || val > this.v1.z) return false;
            }
            else
            {
                if (val <= this.v0.z || val >= this.v1.z) return false;
            }
            return true;
        }

        public bool Contains(Vector3 position, bool inclusive = true)
        {
            if (inclusive)
            {
                if (position.x < this.v0.x || position.x > this.v1.x) return false;
                if (position.y < this.v0.y || position.y > this.v1.y) return false;
                if (position.z < this.v0.z || position.z > this.v1.z) return false;
            }
            else
            {
                if (position.x <= this.v0.x || position.x >= this.v1.x) return false;
                if (position.y <= this.v0.y || position.y >= this.v1.y) return false;
                if (position.z <= this.v0.z || position.z >= this.v1.z) return false;
            }
            return true;
        }

        public static BoundingBox Union(BoundingBox bbox0, BoundingBox bbox1)
        {
            WorkingValues workingValues = new WorkingValues();
            foreach (Vector3 point in new Vector3[] { bbox0.v0, bbox0.v1, bbox1.v0, bbox1.v1 })
                workingValues.Update(point);
            return workingValues.ToBoundingBox();
        }

        public IntervalFloat xInterval
        {
            get { return new IntervalFloat(min: this.v0.x, max: this.v1.x); }
        }

        public IntervalFloat yInterval
        {
            get { return new IntervalFloat(min: this.v0.y, max: this.v1.y); }
        }

        public IntervalFloat zInterval
        {
            get { return new IntervalFloat(min: this.v0.z, max: this.v1.z); }
        }

        public static BoundingBox? Intersection(BoundingBox bbox0, BoundingBox bbox1)
        {
            IntervalFloat? xIntersection = IntervalFloat.Intersection(bbox0.xInterval, bbox1.xInterval);
            if (xIntersection == null) return null;
            IntervalFloat? yIntersection = IntervalFloat.Intersection(bbox0.yInterval, bbox1.yInterval);
            if (yIntersection == null) return null;
            IntervalFloat? zIntersection = IntervalFloat.Intersection(bbox0.zInterval, bbox1.zInterval);
            if (zIntersection == null) return null;
            return new BoundingBox
            {
                v0 = new Vector3(
                    x: xIntersection.Value.min,
                    y: yIntersection.Value.min,
                    z: zIntersection.Value.min
                ),
                v1 = new Vector3(
                    x: xIntersection.Value.max,
                    y: yIntersection.Value.max,
                    z: zIntersection.Value.max
                )
            };
        }

        public void DebugDraw(Color color, float duration = 0f, Func<Vector3, Vector3> coordTransform = null)
        {
            Vector3 v0 = this.v0;
            Vector3 v1 = this.v1;
            if (coordTransform != null)
            {
                v0 = coordTransform(v0);
                v1 = coordTransform(v1);
            }
            Debug.DrawLine(
                start: new Vector3(v0.x, v0.y, v0.z),
                end: new Vector3(v1.x, v0.y, v0.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v0.x, v0.y, v0.z),
                end: new Vector3(v0.x, v1.y, v0.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v0.x, v0.y, v0.z),
                end: new Vector3(v0.x, v0.y, v1.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v1.x, v0.y, v0.z),
                end: new Vector3(v1.x, v0.y, v1.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v1.x, v0.y, v0.z),
                end: new Vector3(v1.x, v1.y, v0.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v1.x, v0.y, v1.z),
                end: new Vector3(v1.x, v1.y, v1.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v1.x, v1.y, v0.z),
                end: new Vector3(v1.x, v1.y, v1.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v1.x, v1.y, v0.z),
                end: new Vector3(v0.x, v1.y, v0.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v0.x, v1.y, v1.z),
                end: new Vector3(v0.x, v1.y, v0.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v0.x, v1.y, v1.z),
                end: new Vector3(v1.x, v1.y, v1.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v0.x, v1.y, v1.z),
                end: new Vector3(v0.x, v0.y, v1.z),
                color: color,
                duration: duration
            );
            Debug.DrawLine(
                start: new Vector3(v1.x, v0.y, v1.z),
                end: new Vector3(v0.x, v0.y, v1.z),
                color: color,
                duration: duration
            );
        }

        public float volume
        {
            get
            {
                Vector3 diff = this.v1 - this.v0;
                return diff.x * diff.y * diff.z;
            }
        }
    }

    public struct BoundingBoxInt
    {
        public Vector3Int v0;
        public Vector3Int v1;

        public override string ToString()
        {
            return $"BoundingBoxInt({v0} ~ {v1})";
        }

        public static BoundingBoxInt operator +(BoundingBoxInt bbox, Vector3Int other) => new BoundingBoxInt
        {
            v0 = bbox.v0 + other,
            v1 = bbox.v1 + other
        };

        public static BoundingBoxInt operator -(BoundingBoxInt bbox, Vector3Int other) => new BoundingBoxInt
        {
            v0 = bbox.v0 - other,
            v1 = bbox.v1 - other
        };

        public int xmin
        {
            get { return this.v0.x; }
        }

        public int xmax
        {
            get { return this.v1.x; }
        }

        public int ymin
        {
            get { return this.v0.y; }
        }

        public int ymax
        {
            get { return this.v1.y; }
        }

        public int zmin
        {
            get { return this.v0.z; }
        }

        public int zmax
        {
            get { return this.v1.z; }
        }

        public BoundingBox ToFloat()
        {
            return new BoundingBox
            {
                v0 = (Vector3)this.v0,
                v1 = (Vector3)this.v1
            };
        }

        public class WorkingValues
        {
            public int? xmin = null;
            public int? xmax = null;
            public int? ymin = null;
            public int? ymax = null;
            public int? zmin = null;
            public int? zmax = null;

            public WorkingValues() { }

            public bool isNull
            {
                get
                {
                    return this.xmin == null || this.xmax == null
                        || this.ymin == null || this.ymax == null
                        || this.zmin == null || this.zmax == null;
                }
            }

            public void Update(Vector3Int point)
            {
                if (this.xmin == null || point.x < this.xmin.Value) this.xmin = point.x;
                if (this.xmax == null || point.x > this.xmax.Value) this.xmax = point.x;

                if (this.ymin == null || point.y < this.ymin.Value) this.ymin = point.y;
                if (this.ymax == null || point.y > this.ymax.Value) this.ymax = point.y;

                if (this.zmin == null || point.z < this.zmin.Value) this.zmin = point.z;
                if (this.zmax == null || point.z > this.zmax.Value) this.zmax = point.z;
            }

            public BoundingBoxInt ToBoundingBoxInt()
            {
                if (this.isNull)
                    throw new Exception("One of the working values are still null.");
                Vector3Int vmin = new Vector3Int(x: xmin.Value, y: ymin.Value, z: zmin.Value);
                Vector3Int vmax = new Vector3Int(x: xmax.Value, y: ymax.Value, z: zmax.Value);
                return new BoundingBoxInt { v0 = vmin, v1 = vmax };
            }
        }

        static public BoundingBoxInt FromGridCoordList(List<Vector3Int> gridCoordList)
        {
            if (gridCoordList.Count == 0)
                throw new System.Exception("Cannot create bounding box from empty list.");

            WorkingValues workingValues = new WorkingValues();
            foreach (Vector3Int coord in gridCoordList)
                workingValues.Update(coord);

            return workingValues.ToBoundingBoxInt();
        }

        public bool ContainsX(int val, bool inclusive = true)
        {
            if (inclusive)
            {
                if (val < this.v0.x || val > this.v1.x) return false;
            }
            else
            {
                if (val <= this.v0.x || val >= this.v1.x) return false;
            }
            return true;
        }

        public bool ContainsY(int val, bool inclusive = true)
        {
            if (inclusive)
            {
                if (val < this.v0.y || val > this.v1.y) return false;
            }
            else
            {
                if (val <= this.v0.y || val >= this.v1.y) return false;
            }
            return true;
        }

        public bool ContainsZ(int val, bool inclusive = true)
        {
            if (inclusive)
            {
                if (val < this.v0.z || val > this.v1.z) return false;
            }
            else
            {
                if (val <= this.v0.z || val >= this.v1.z) return false;
            }
            return true;
        }

        public bool Contains(Vector3Int coord, bool inclusive = true)
        {
            if (inclusive)
            {
                if (coord.x < this.v0.x || coord.x > this.v1.x) return false;
                if (coord.y < this.v0.y || coord.y > this.v1.y) return false;
                if (coord.z < this.v0.z || coord.z > this.v1.z) return false;
            }
            else
            {
                if (coord.x <= this.v0.x || coord.x >= this.v1.x) return false;
                if (coord.y <= this.v0.y || coord.y >= this.v1.y) return false;
                if (coord.z <= this.v0.z || coord.z >= this.v1.z) return false;
            }
            return true;
        }

        public static BoundingBoxInt Union(BoundingBoxInt bbox0, BoundingBoxInt bbox1)
        {
            WorkingValues workingValues = new WorkingValues();
            foreach (Vector3Int point in new Vector3Int[] { bbox0.v0, bbox0.v1, bbox1.v0, bbox1.v1 })
                workingValues.Update(point);
            return workingValues.ToBoundingBoxInt();
        }

        public IntervalInt xInterval
        {
            get { return new IntervalInt(min: this.v0.x, max: this.v1.x); }
        }

        public IntervalInt yInterval
        {
            get { return new IntervalInt(min: this.v0.y, max: this.v1.y); }
        }

        public IntervalInt zInterval
        {
            get { return new IntervalInt(min: this.v0.z, max: this.v1.z); }
        }

        public static BoundingBoxInt? Intersection(BoundingBoxInt bbox0, BoundingBoxInt bbox1)
        {
            IntervalInt? xIntersection = IntervalInt.Intersection(bbox0.xInterval, bbox1.xInterval);
            if (xIntersection == null) return null;
            IntervalInt? yIntersection = IntervalInt.Intersection(bbox0.yInterval, bbox1.yInterval);
            if (yIntersection == null) return null;
            IntervalInt? zIntersection = IntervalInt.Intersection(bbox0.zInterval, bbox1.zInterval);
            if (zIntersection == null) return null;
            return new BoundingBoxInt
            {
                v0 = new Vector3Int(
                    x: xIntersection.Value.min,
                    y: yIntersection.Value.min,
                    z: zIntersection.Value.min
                ),
                v1 = new Vector3Int(
                    x: xIntersection.Value.max,
                    y: yIntersection.Value.max,
                    z: zIntersection.Value.max
                )
            };
        }

        public int volume
        {
            get
            {
                Vector3Int diff = this.v1 - this.v0;
                return diff.x * diff.y * diff.z;
            }
        }
    }
}
