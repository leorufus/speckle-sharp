using Objects.Geometry;
using System.Collections.Generic;
using Utilities;
using Utilities.Geometry;

namespace Objects.Converter.IApp
{
    public partial class ConverterIAppGeometric
    {
        public Punto3D PointToNative(Point pt)
        {
            var iappPoint = new Punto3D() { X = pt.x, Y = pt.y, Z = pt.z };
            return iappPoint;

        }

        public Point PointToSpeckle(Punto3D pt, string units = null)
        {
            var pointToSpeckle = new Point(pt.X, pt.Y, pt.Z);            
            return pointToSpeckle;
        }

        //RRR Crear clase Units para gestionar las unidades
        private string _modelUnits = string.Empty;
        public Mesh MeshToSpeckle(BimSurface mesh, string units = null)
        {
            var vertices = new List<double>(mesh.Nodes.Count * 3);
            foreach (var vert in mesh.Nodes)
            {
                vertices.AddRange(PointToSpeckle(new Punto3D() { X = vert.X, Y = vert.Y, Z = vert.Z}).ToList());
            }

            var faces = new List<int>(mesh.Triangles.Count * 4);
            for (int i = 0; i < mesh.Triangles.Count; i++)
            {
                var triangle = mesh.Triangles[i];
                var A = triangle.Node1;
                var B = triangle.Node2;
                var C = triangle.Node3;
                faces.Add(0);
                faces.AddRange(new int[] {A, B, C });
            }

            var u = units ?? _modelUnits;
            var speckleMesh = new Mesh(vertices, faces, units: u);

            return speckleMesh;
        }

        public Mesh SolidToSpeckle(BimSolid solid, string units = null)
        {
            var faceArr = new List<int>();
            var vertexArr = new List<double>();
            var prevVertCount = 0;

            if (solid == null) return null;

            foreach (var surf in solid.Surfaces)
            {
                foreach (var vert in surf.Nodes)
                {
                    var vertex = PointToSpeckle(new Punto3D() { X = vert.X, Y = vert.Y, Z = vert.Z });
                    vertexArr.AddRange(new double[] { vertex.x, vertex.y, vertex.z });
                }

                for (int i = 0; i < surf.Triangles.Count; i++)
                {
                    var triangle = surf.Triangles[i];

                    faceArr.Add(0); // TRIANGLE flag
                    faceArr.Add(triangle.Node1 + prevVertCount);
                    faceArr.Add(triangle.Node2 + prevVertCount);
                    faceArr.Add(triangle.Node3 + prevVertCount);
                }
                prevVertCount += surf.Nodes.Count;
            }

            var u = units ?? _modelUnits;
            return new Mesh(vertexArr, faceArr, units:u);
        }
    }
}
