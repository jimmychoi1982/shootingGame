using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using Microsoft.Xna.Framework;

public class ConvexBody
{
    public class Polygon
    {
        List<Vector3> vertices;

        public int VertexCount
        {
            get { return vertices.Count; }
        }

        internal Polygon()
        {
            vertices = new List<Vector3>(4);
        }


        public void getVertex(int index, out Vector3 result)
        {
            result = vertices[index];
        }

        public void addVertex(Vector3 vertex)
        {
            vertices.Add(vertex);
        }

        public void clearVertices()
        {
            vertices.Clear();
        }
    }

    #region Edge

    public struct Edge
    {
        public Vector3 point0;
        public Vector3 point1;

        public Edge(Vector3 point0, Vector3 point1)
        {
            this.point0 = point0;
            this.point1 = point1;
        }

        public override string ToString()
        {
            return "{Point0: " + point0 + " Point1:" + point1 + "}";
        }
    }

    #endregion

    #region Pool

    /// <summary>
    /// オブジェクトのプーリングを管理するクラスです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Pool<T> : IEnumerable<T> where T : class
    {
        // 0 は容量制限無し。
        public const int DefaultMaxCapacity = 100;

        Func<T> createFunction;

        Queue<T> objects = new Queue<T>();

        public int Count
        {
            get { return objects.Count; }
        }

        public int InitialCapacity { get; private set; }

        public int MaxCapacity { get; set; }

        public int TotalObjectCount { get; private set; }

        /// <summary>
        /// オブジェクトの生成関数を指定してインスタンス生成します。
        /// オブジェクトの生成関数は、プールで新たなインスタンスの生成が必要となった場合に呼び出されます。
        /// </summary>
        /// <param name="createFunction">オブジェクトの生成関数。</param>
        public Pool(Func<T> createFunction)
        {
            if (createFunction == null)
                throw new ArgumentNullException("createFunction");

            this.createFunction = createFunction;

            MaxCapacity = DefaultMaxCapacity;
        }

        /// <summary>
        /// プールからオブジェクトを取得します。
        /// プールが空の場合、オブジェクトを新たに生成して返します。
        /// ただし、MaxCapacity に 0 以上を指定し、かつ、
        /// プールから生成したオブジェクトの総数が上限を越える場合には、null を返します。
        /// </summary>
        /// <returns></returns>
        public T Borrow()
        {
            while (0 < MaxCapacity && MaxCapacity < TotalObjectCount && 0 < objects.Count)
                DisposeObject(objects.Dequeue());

            if (0 < MaxCapacity && MaxCapacity <= TotalObjectCount && objects.Count == 0)
                return null;

            if (0 < objects.Count)
                return objects.Dequeue();

            return CreateObject();
        }

        /// <summary>
        /// オブジェクトをプールへ戻します。
        /// </summary>
        /// <param name="obj"></param>
        public void Return(T obj)
        {
            if (MaxCapacity == 0 || TotalObjectCount <= MaxCapacity)
            {
                objects.Enqueue(obj);
            }
            else
            {
                DisposeObject(obj);
            }
        }

        /// <summary>
        /// プール内の全てのオブジェクトを破棄します。
        /// </summary>
        public void Clear()
        {
            foreach (var obj in objects)
                DisposeObject(obj);

            objects.Clear();
        }

        T CreateObject()
        {
            if (0 < MaxCapacity && MaxCapacity < TotalObjectCount)
                return null;

            TotalObjectCount++;
            return createFunction();
        }

        void DisposeObject(T obj)
        {
            var disposable = obj as IDisposable;
            if (disposable != null)
                disposable.Dispose();

            TotalObjectCount--;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return objects.GetEnumerator();
        }
    }
    #endregion

    #region PolygonCollection

    public class PolygonCollection : Collection<Polygon>
    {
        ConvexBody convexBody;

        public PolygonCollection(ConvexBody convexBody) : base(new List<Polygon>(6))
        {
            this.convexBody = convexBody;
        }

        protected override void RemoveItem(int index)
        {
            var item = Items[index];
            convexBody.releasePolygon(item);

            base.RemoveItem(index);
        }
        protected override void ClearItems()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                convexBody.releasePolygon(item);
            }
            base.ClearItems();
        }

        internal void clearWithoutReturnToPool()
        {
            base.ClearItems();
        }
    }

    #endregion

    // Ogre3d Vector3と同じ値
    const float equalsPointTolerance = 1e-03f;

    // Degree 1 の角度差ならば等しいベクトル方向であるとする
    static readonly float directionEqualsTolerance = 1f * Mathf.Deg2Rad;
    Vector3[] corners;
    List<bool> outsides;
    List<Edge> intersectEdges;

    Pool<Polygon> polygonPool;
    PolygonCollection workingPolygons;

    public PolygonCollection Polygons { get; private set; }

    public ConvexBody()
    {
        Polygons = new PolygonCollection(this);
        corners = new Vector3[8];
        outsides = new List<bool>(6);
        intersectEdges = new List<Edge>();
        polygonPool = new Pool<Polygon>(() => { return new Polygon(); });
        workingPolygons = new PolygonCollection(this);
    }

    // 多角形プールからインスタンスを取得
    // プールが空の場合には新たなインスタンスを生成する
    // XNA CreatePolygon()
    public Polygon allocatePolygon()
    {
        Polygon result = polygonPool.Borrow();

        return result;
    }

    public void releasePolygon(Polygon polygon)
    {
        if (polygon == null)
        {
            throw new ArgumentNullException("polygon");
        }

        polygon.clearVertices();
        polygonPool.Return(polygon);
    }

    // 指定の法線を持ち、指定の点を含む平面を生成する
    // XNA PlaneHelper.CreatePlane(Vector3 normal, Vector3 point)
    public Plane redefine(Vector3 normal, Vector3 point)
    {
        float d;
        d = Vector3.Dot(normal, point);
        return new Plane(normal, -d);
    }

//    public void define(Matrix4x4 invEyeCameraProj)
//    {
//        //getFrustumCorners(ref invEyeCameraProj, ref corners);
//        EternalShadowMath.getFrustumCorners(ref invEyeCameraProj, ref corners);
//        // from ogre test
//        /*
//		corners[0] = new Vector3(249.66891f, 21.9646244f, 394.260162f);
//		corners[1] = new Vector3(244.985626f, 21.9646244f, 397.187256f);
//		corners[2] = new Vector3(245.032150f, 17.8234196f, 397.187256f);
//		corners[3] = new Vector3(249.715515f, 17.8234196f, 394.260162f);
//		corners[4] = new Vector3(-6369.64063f, 39312.5273f, -114396.297f);
//		corners[5] = new Vector3(-100036.945f, 39312.5273f, -55854.2266f);
//		corners[6] = new Vector3(-99106.3359f, -43511.5664f, -54365.2500f);
//		corners[7] = new Vector3(-5439.03125f, -43511.5664f, -112907.320f);
//*/
//        Polygons.Clear();
//        // ordering of the points:
//        // near (0-3), far (4-7); each (top-right, top-left, bottom-left, bottom-right)
//        //     5-----4
//        //    /|    /|
//        //   / |   / |
//        //  1-----0  |
//        //  |  6--|--7
//        //  | /   | /
//        //  |/    |/
//        //  2-----3

//        // LiSPSM/Ogre (CCW:反時計回り) に合わせる。
//        // Ogre : これ
//        // 0: 3: near-top-right         
//        // 1: 0: near-top-left          
//        // 2: 1: near-bottom-left       
//        // 3: 2: near-bottom-right      
//        // 4: 7: far-top-right          
//        // 5: 4: far-top-left           
//        // 6: 5: far-bottom-left        
//        // 7: 6: far-bottom-right
//        Polygon near = allocatePolygon();
//        near.addVertex(corners[0]);
//        near.addVertex(corners[1]);
//        near.addVertex(corners[2]);
//        near.addVertex(corners[3]);
//        Polygons.Add(near);

//        // far
//        Polygon far = allocatePolygon();
//        far.addVertex(corners[5]);
//        far.addVertex(corners[4]);
//        far.addVertex(corners[7]);
//        far.addVertex(corners[6]);
//        Polygons.Add(far);

//        // left
//        Polygon left = allocatePolygon();
//        left.addVertex(corners[5]);
//        left.addVertex(corners[6]);
//        left.addVertex(corners[2]);
//        left.addVertex(corners[1]);
//        Polygons.Add(left);

//        // right 
//        Polygon right = allocatePolygon();
//        right.addVertex(corners[4]);
//        right.addVertex(corners[0]);
//        right.addVertex(corners[3]);
//        right.addVertex(corners[7]);
//        Polygons.Add(right);

//        // bottom
//        Polygon bottom = allocatePolygon();
//        bottom.addVertex(corners[6]);
//        bottom.addVertex(corners[7]);
//        bottom.addVertex(corners[3]);
//        bottom.addVertex(corners[2]);
//        Polygons.Add(bottom);

//        // top
//        Polygon top = allocatePolygon();
//        top.addVertex(corners[4]);
//        top.addVertex(corners[5]);
//        top.addVertex(corners[1]);
//        top.addVertex(corners[0]);
//        Polygons.Add(top);
//    }

    public void debugDrawPolygons()
    {
        foreach (Polygon polygon in Polygons)
        {
            Vector3 start;
            Vector3 end;
            int count = polygon.VertexCount;
            for (int i = 0; i < count; i++)
            {
                // 次の頂点のインデックス 末尾だったら先頭
                int nIndex = (i + 1) % count;
                polygon.getVertex(i, out start);
                polygon.getVertex(nIndex, out end);
                Debug.DrawLine(start, end, Color.green);
            }
        }
    }

    // 境界ボックスで凸体をクリップする
    public void clip(Bounds box)
    {
        // near
        clip(redefine(new Vector3(0, 0, 1), box.max));
        // far
        clip(redefine(new Vector3(0, 0, -1), box.min));
        // left
        clip(redefine(new Vector3(-1, 0, 0), box.min));
        // right
        clip(redefine(new Vector3(1, 0, 0), box.max));
        // bottom
        clip(redefine(new Vector3(0, -1, 0), box.min));
        // top
        clip(redefine(new Vector3(0, 1, 0), box.max));
    }

    public void clip(Plane plane)
    {
        // 複製
        for (int i = 0; i < Polygons.Count; ++i)
        {
            workingPolygons.Add(Polygons[i]);
        }

        // 元を削除
        Polygons.clearWithoutReturnToPool();

        intersectEdges.Clear();
        for (int ip = 0; ip < workingPolygons.Count; ++ip)
        {
            var originalPolygon = workingPolygons[ip];
            if (originalPolygon.VertexCount < 3)
            {
                continue;
            }

            var pNew = allocatePolygon();
            var pIntersect = allocatePolygon();

            clip(ref plane, originalPolygon, pNew, pIntersect);

            if (3 <= pNew.VertexCount)
            {
                // 面がある場合
                Polygons.Add(pNew);
            }
            else
            {
                // 追加しなかった Polygon オブジェクトはリリース
                releasePolygon(pNew);
            }

            // 交差した辺を記憶
            if (pIntersect.VertexCount == 2)
            {
                Vector3 v0;
                Vector3 v1;
                pIntersect.getVertex(0, out v0);
                pIntersect.getVertex(1, out v1);

                var edge = new Edge(v0, v1);

                intersectEdges.Add(edge);
            }

            // 交差する辺についての Polygon オブジェクトをリリース
            releasePolygon(pIntersect);
        }

        // 新たな多角形の構築には、少なくとも3つの辺が必要
        if (3 <= intersectEdges.Count)
        {
            Edge lastEdge;
            lastEdge = intersectEdges[intersectEdges.Count - 1];
            intersectEdges.RemoveAt(intersectEdges.Count - 1);

            Vector3 first = lastEdge.point0;
            Vector3 second = lastEdge.point1;

            Vector3 next;
            if (findPointAndRemoveEdge(ref second, intersectEdges, out next))
            {
                var closingPolygon = allocatePolygon();
                Polygons.Add(closingPolygon);

                // 交差する二つの辺から多角形の法線を算出。
                Vector3 edge0 = first - second;
                Vector3 edge1 = next - second;

                Vector3 polygonNormal = Vector3.Cross(edge0, edge1);

                bool frontside;
                directionEquals(plane.normal, ref polygonNormal, out frontside);

                Vector3 firstVertex;
                Vector3 currentVertex;

                if (frontside)
                {
                    closingPolygon.addVertex(next);
                    closingPolygon.addVertex(second);
                    closingPolygon.addVertex(first);
                    firstVertex = next;
                    currentVertex = first;
                }
                else
                {
                    closingPolygon.addVertex(first);
                    closingPolygon.addVertex(second);
                    closingPolygon.addVertex(next);
                    firstVertex = first;
                    currentVertex = next;
                }

                while (0 < intersectEdges.Count)
                {
                    if (findPointAndRemoveEdge(ref currentVertex, intersectEdges, out next))
                    {
                        if (intersectEdges.Count != 0)
                        {
                            currentVertex = next;
                            closingPolygon.addVertex(next);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

            }
        }
        workingPolygons.Clear();
    }

    public void clip(ref Plane plane, Polygon originalPolygon, Polygon newPolygon, Polygon intersectPolygon)
    {
        // 各頂点(frustum)が面 plane の裏側にあるか否か
        outsides.Clear();
        for (int iv = 0; iv < originalPolygon.VertexCount; iv++)
        {
            Vector3 v;
            originalPolygon.getVertex(iv, out v);

            // 面 plane から頂点 v の距離
            float distance = plane.GetDistanceToPoint(v);

            // 頂点 v が面 plane の外側（表側）にあるならばtrue,さもなくばfalse
            outsides.Add(distance > 0.0f);

        }

        for (int iv0 = 0; iv0 < originalPolygon.VertexCount; iv0++)
        {
            // 二つの頂点は多角形の変を表す

            // 次の頂点のインデックス（末尾の次は先頭）
            int iv1 = (iv0 + 1) % originalPolygon.VertexCount;

            // case 2: both outside(do nothing)
            if (outsides[iv0] && outsides[iv1])
            {
                // 変が面 Plane の外側にあるならばスキップ
                continue;
            }

            // case 4: outside -> inside
            if (outsides[iv0])
            {
                // 面 plane の外側から内側へ向かう辺の場合。

                Vector3 v0;
                Vector3 v1;
                originalPolygon.getVertex(iv0, out v0);
                originalPolygon.getVertex(iv1, out v1);

                Vector3? intersect;
                intersectEdgeAndPlane(ref v0, ref v1, ref plane, out intersect);

                if (intersect != null)
                {
                    Vector3 intersectV = intersect.Value;
                    newPolygon.addVertex(intersectV);
                    intersectPolygon.addVertex(intersectV);
                }

                newPolygon.addVertex(v1);
            }
            // case 3: inside -> outside
            else if (outsides[iv1])
            {
                // 面 plane の内側から外側へ向かう辺の場合。

                Vector3 v0;
                Vector3 v1;
                originalPolygon.getVertex(iv0, out v0);
                originalPolygon.getVertex(iv1, out v1);

                Vector3? intersect;
                intersectEdgeAndPlane(ref v0, ref v1, ref plane, out intersect);

                if (intersect != null)
                {
                    Vector3 intersectV = intersect.Value;
                    newPolygon.addVertex(intersectV);
                    intersectPolygon.addVertex(intersectV);
                }
            }
            // case 1: both points inside
            else
            {
                // 辺が面の内側にある場合。

                Vector3 v1;
                originalPolygon.getVertex(iv1, out v1);

                newPolygon.addVertex(v1);
            }
        }
    }

    // 辺と平面の交差判定
    void intersectEdgeAndPlane(ref Vector3 point0, ref Vector3 point1, ref Plane plane, out Vector3? result)
    {
        // 辺の方向
        Vector3 direction = point0 - point1;
        direction.Normalize();

        Ray ray = new Ray(point1, direction);

        float intersect;
        if (plane.Raycast(ray, out intersect))
        {
            // 交点
            result = ray.GetPoint(intersect);
            return;
        }
        result = null;
    }

    void directionEquals(Vector3 v0, ref Vector3 v1, out bool result)
    {
        float dot = Vector3.Dot(v0, v1);
        float angle = Mathf.Acos(dot);

        result = (angle <= directionEqualsTolerance);
    }

    bool findPointAndRemoveEdge(ref Vector3 point, List<Edge> edges, out Vector3 another)
    {
        another = default(Vector3);
        int index = -1;

        for (int i = 0; i < edges.Count; i++)
        {
            Edge edge = edges[i];

            if (equalsPoints(ref edge.point0, ref point))
            {
                another = edge.point1;
                index = i;
                break;
            }
            else if (equalsPoints(ref edge.point1, ref point))
            {
                another = edge.point0;
                index = i;
                break;
            }
        }

        return false;
    }

    public static bool equalsPoints(ref Vector3 left, ref Vector3 right)
    {
        return ((float)Math.Abs(right.x - left.x) < equalsPointTolerance &&
            (float)Math.Abs(right.y - left.y) < equalsPointTolerance &&
            (float)Math.Abs(right.z - left.z) < equalsPointTolerance);
    }

    // 視錐台のビュー射影逆行列から8点を算出
    //public void getFrustumCorners(ref Matrix4x4 invCameraProj, ref Vector3[] corners)
    //{
    //    for (int i = 0; i < EternalShadowMath.frustumCorners.Length; ++i)
    //    {
    //        corners[i] = invCameraProj.MultiplyPoint(EternalShadowMath.frustumCorners[i]);
    //    }
    //}

    #region Debug Function

    public void debugDrawFrustum(Color color)
    {
        // near
        Debug.DrawLine(corners[0], corners[1], color);
        Debug.DrawLine(corners[1], corners[2], color);
        Debug.DrawLine(corners[2], corners[3], color);
        Debug.DrawLine(corners[3], corners[0], color);

        // far
        Debug.DrawLine(corners[4], corners[5], color);
        Debug.DrawLine(corners[5], corners[6], color);
        Debug.DrawLine(corners[6], corners[7], color);
        Debug.DrawLine(corners[7], corners[4], color);

        // near - far
        Debug.DrawLine(corners[0], corners[4], color);
        Debug.DrawLine(corners[1], corners[5], color);
        Debug.DrawLine(corners[2], corners[6], color);
        Debug.DrawLine(corners[3], corners[7], color);
    }
    #endregion

    #region For XNA Function

    //// 境界ボックスで凸体をクリップする
    //public void XNAclip(XNABoundingBox box)
    //{
    //    // near
    //    XNAclip(PlaneHelper.CreatePlane(new Vector3(0, 0, 1), box.max));
    //    // far
    //    XNAclip(PlaneHelper.CreatePlane(new Vector3(0, 0, -1), box.min));
    //    // left
    //    XNAclip(PlaneHelper.CreatePlane(new Vector3(-1, 0, 0), box.min));
    //    // right
    //    XNAclip(PlaneHelper.CreatePlane(new Vector3(1, 0, 0), box.max));
    //    // bottom
    //    XNAclip(PlaneHelper.CreatePlane(new Vector3(0, -1, 0), box.min));
    //    // top
    //    XNAclip(PlaneHelper.CreatePlane(new Vector3(0, 1, 0), box.max));
    //}

    //public void XNAclip(XNAPlane plane)
    //{
    //    // 複製
    //    for (int i = 0; i < Polygons.Count; ++i)
    //    {
    //        workingPolygons.Add(Polygons[i]);
    //    }

    //    // 元を削除
    //    Polygons.clearWithoutReturnToPool();

    //    intersectEdges.Clear();
    //    for (int ip = 0; ip < workingPolygons.Count; ++ip)
    //    {
    //        var originalPolygon = workingPolygons[ip];
    //        if (originalPolygon.VertexCount < 3)
    //        {
    //            continue;
    //        }

    //        var newPolygon = allocatePolygon();
    //        var intersectPolygon = allocatePolygon();

    //        XNAclip(ref plane, originalPolygon, newPolygon, intersectPolygon);

    //        if (3 <= newPolygon.VertexCount)
    //        {
    //            // 面がある場合
    //            Polygons.Add(newPolygon);
    //        }
    //        else
    //        {
    //            // 追加しなかった Polygon オブジェクトはリリース
    //            releasePolygon(newPolygon);
    //        }

    //        // 交差した辺を記憶
    //        if (intersectPolygon.VertexCount == 2)
    //        {
    //            Vector3 v0;
    //            Vector3 v1;
    //            intersectPolygon.getVertex(0, out v0);
    //            intersectPolygon.getVertex(1, out v1);

    //            var edge = new Edge(v0, v1);

    //            intersectEdges.Add(edge);
    //        }

    //        // 交差する辺についての Polygon オブジェクトをリリース
    //        releasePolygon(intersectPolygon);
    //    }

    //    // 新たな多角形の構築には、少なくとも3つの辺が必要
    //    if (3 <= intersectEdges.Count)
    //    {
    //        Edge lastEdge;
    //        lastEdge = intersectEdges[intersectEdges.Count - 1];
    //        intersectEdges.RemoveAt(intersectEdges.Count - 1);

    //        Vector3 first = lastEdge.point0;
    //        Vector3 second = lastEdge.point1;

    //        Vector3 next;
    //        if (findPointAndRemoveEdge(ref second, intersectEdges, out next))
    //        {
    //            var closingPolygon = allocatePolygon();
    //            Polygons.Add(closingPolygon);

    //            // 交差する二つの辺から多角形の法線を算出。
    //            Vector3 edge0 = first - second;
    //            Vector3 edge1 = next - second;

    //            Vector3 polygonNormal = Vector3.Cross(edge0, edge1);

    //            bool frontside;
    //            directionEquals(plane.Normal, ref polygonNormal, out frontside);

    //            Vector3 firstVertex;
    //            Vector3 currentVertex;

    //            if (frontside)
    //            {
    //                closingPolygon.addVertex(next);
    //                closingPolygon.addVertex(second);
    //                closingPolygon.addVertex(first);
    //                firstVertex = next;
    //                currentVertex = first;
    //            }
    //            else
    //            {
    //                closingPolygon.addVertex(first);
    //                closingPolygon.addVertex(second);
    //                closingPolygon.addVertex(next);
    //                firstVertex = first;
    //                currentVertex = next;
    //            }

    //            while (0 < intersectEdges.Count)
    //            {
    //                if (findPointAndRemoveEdge(ref currentVertex, intersectEdges, out next))
    //                {
    //                    if (intersectEdges.Count != 0)
    //                    {
    //                        currentVertex = next;
    //                        closingPolygon.addVertex(next);
    //                    }
    //                }
    //                else
    //                {
    //                    break;
    //                }
    //            }

    //        }
    //    }
    //    workingPolygons.Clear();
    //}

    //public void XNAclip(ref XNAPlane plane, Polygon originalPolygon, Polygon newPolygon, Polygon intersectPolygon)
    //{
    //    // 各頂点(frustum)が面 plane の裏側にあるか否か
    //    outsides.Clear();
    //    for (int iv = 0; iv < originalPolygon.VertexCount; iv++)
    //    {
    //        Vector3 v;
    //        originalPolygon.getVertex(iv, out v);

    //        // 面 plane から頂点 v の距離
    //        float distance;
    //        plane.DotCoordinate(ref v, out distance);

    //        // 頂点 v が面 plane の外側（表側）にあるならばtrue,さもなくばfalse
    //        outsides.Add(0.0f < distance);
    //    }

    //    for (int iv0 = 0; iv0 < originalPolygon.VertexCount; iv0++)
    //    {
    //        // 二つの頂点は多角形の変を表す

    //        // 次の頂点のインデックス（末尾の次は先頭）
    //        int iv1 = (iv0 + 1) % originalPolygon.VertexCount;

    //        // case 2: both outside(do nothing)
    //        if (outsides[iv0] && outsides[iv1])
    //        {
    //            // 変が面 Plane の外側にあるならばスキップ
    //            continue;
    //        }

    //        // case 4: outside -> inside
    //        if (outsides[iv0])
    //        {
    //            // 面 plane の外側から内側へ向かう辺の場合。

    //            Vector3 v0;
    //            Vector3 v1;
    //            originalPolygon.getVertex(iv0, out v0);
    //            originalPolygon.getVertex(iv1, out v1);

    //            Vector3? intersect;
    //            XNAintersectEdgeAndPlane(ref v0, ref v1, ref plane, out intersect);

    //            if (intersect != null)
    //            {
    //                Vector3 intersectV = intersect.Value;
    //                newPolygon.addVertex(intersectV);
    //                intersectPolygon.addVertex(intersectV);
    //            }

    //            newPolygon.addVertex(v1);
    //        }
    //        // case 3: inside -> outside
    //        else if (outsides[iv1])
    //        {
    //            // 面 plane の内側から外側へ向かう辺の場合。

    //            Vector3 v0;
    //            Vector3 v1;
    //            originalPolygon.getVertex(iv0, out v0);
    //            originalPolygon.getVertex(iv1, out v1);

    //            Vector3? intersect;
    //            XNAintersectEdgeAndPlane(ref v0, ref v1, ref plane, out intersect);

    //            if (intersect != null)
    //            {
    //                Vector3 intersectV = intersect.Value;
    //                newPolygon.addVertex(intersectV);
    //                intersectPolygon.addVertex(intersectV);
    //            }
    //        }
    //        // case 1: both points inside
    //        else
    //        {
    //            // 辺が面の内側にある場合。

    //            Vector3 v1;
    //            originalPolygon.getVertex(iv1, out v1);

    //            newPolygon.addVertex(v1);
    //        }
    //    }
    //}

    //// 辺と平面の交差判定
    //void XNAintersectEdgeAndPlane(ref Vector3 point0, ref Vector3 point1, ref XNAPlane plane, out Vector3? result)
    //{
    //    // 辺の方向
    //    Vector3 direction = point0 - point1;
    //    direction.Normalize();

    //    XNARay ray = new XNARay(point1, direction);

    //    float? intersect;
    //    ray.Intersects(ref plane, out intersect);

    //    if (intersect != null)
    //    {
    //        // 交点
    //        result = RayHelper.GetPoint(ref ray, intersect.Value);
    //    }
    //    else
    //    {
    //        result = null;
    //    }
    //}

    #endregion
}
