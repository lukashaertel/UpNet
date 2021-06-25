using System;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace UpNet.Graphics
{
    public class Icosphere
    {
        const float X = 0.525731112119133606f;
        const float Z = 0.850650808352039932f;
        const float N = 0.0f;

        public static readonly float[] DataPos =
        {
            -X, N, Z, X, N, Z, -X, N, -Z, X, N, -Z,
            N, Z, X, N, Z, -X, N, -Z, X, N, -Z, -X,
            Z, X, N, -Z, X, N, Z, -X, N, -Z, -X, N
        };

        public static float[] ComputeTexCoords(float[]? vertices = null)
        {
            vertices ??= DataPos;

            var result = new float[vertices.Length * 2 / 3];
            for (var i = 0; i < vertices.Length; i += 3)
            {
                var h = MathHelper.Sqrt(vertices[i + 0] * vertices[i + 0] + vertices[i + 2] * vertices[i + 2]);
                var s = 0.5 - MathHelper.Atan2(vertices[i + 2], vertices[i + 0]) / MathHelper.TwoPi;
                var t = 0.5 + MathHelper.Atan2(vertices[i + 1], h) / MathHelper.Pi;

                result[i * 2 / 3 + 0] = (float) s;
                result[i * 2 / 3 + 1] = (float) t;
            }

            return result;
        }

        // public static readonly float[] DataTexCoord =
        // {
        //     -X, N, Z, X, N, Z, -X, N,
        //     N, Z, X, N, Z, -X, N, -Z,
        //     Z, X, N, -Z, X, N, Z, -X,
        // };

        public static float[] ComputeNormals(float[]? vertices = null)
        {
            vertices ??= DataPos;

            var result = new float[vertices.Length];
            for (var i = 0; i < vertices.Length; i += 3)
            {
                var normal = Vector3.Normalize(new Vector3(vertices[i + 0], vertices[i + 1], vertices[i + 2]));
                result[i + 0] = normal.X;
                result[i + 1] = normal.Y;
                result[i + 2] = normal.Z;
            }

            return result;
        }


        public static float[] ComputeBitangents(float[]? vertices = null)
        {
            vertices ??= DataPos;

            var result = new float[vertices.Length];
            for (var i = 0; i < vertices.Length; i += 3)
            {
                var bitangent = Vector3.Normalize(new Vector3(vertices[i + 2], 0, -vertices[i + 0]));

                result[i + 0] = bitangent.X;
                result[i + 1] = bitangent.Y;
                result[i + 2] = bitangent.Z;
            }

            return result;
        }


        public static float[] ComputeTangents(float[]? vertices = null)
        {
            vertices ??= DataPos;

            var result = new float[vertices.Length];
            for (var i = 0; i < vertices.Length; i += 3)
            {
                var tangent = Vector3.Normalize(new Vector3(vertices[i + 0], vertices[i + 1], vertices[i + 2]));
                tangent = Vector3.Transform(tangent, Quaternion.FromAxisAngle(
                    new Vector3(tangent.Z, 0, -tangent.X), MathHelper.PiOver2));
                tangent.Normalize();
                result[i + 0] = tangent.X;
                result[i + 1] = tangent.Y;
                result[i + 2] = tangent.Z;
            }

            return result;
        }

        public static byte[] ComputeColors(float[]? vertices = null)
        {
            vertices ??= DataPos;

            var result = new byte[vertices.Length];
            Array.Fill(result, (byte) 255);
            return result;
        }

        public static readonly uint[] DataIndices =
        {
            0, 4, 1, 0, 9, 4, 9, 5, 4, 4, 5, 8, 4, 8, 1,
            8, 10, 1, 8, 3, 10, 5, 3, 8, 5, 2, 3, 2, 7, 3,
            7, 10, 3, 7, 6, 10, 7, 11, 6, 11, 0, 6, 0, 1, 6,
            6, 1, 10, 9, 0, 11, 9, 11, 2, 9, 2, 5, 7, 2, 11
        };

        public static (float[], uint[]) SubdivideNoReuse(float[] vertices, uint[] indices)
        {
            // TODO With reuse.

            // New vertices for all triangles.
            var newVertices = new float[indices.Length / 3 * 18];

            // Replaced existing vertices with new inner connections
            var newIndices = new uint[indices.Length / 3 * 12];

            // Run for all triangles.
            Parallel.For(0, indices.Length / 3, i =>
            {
                // Get group of three indices to get the triangle.
                var ia = indices[i * 3 + 0];
                var ib = indices[i * 3 + 1];
                var ic = indices[i * 3 + 2];

                // Get edges.
                var a = new Vector3(vertices[ia * 3 + 0], vertices[ia * 3 + 1], vertices[ia * 3 + 2]);
                var b = new Vector3(vertices[ib * 3 + 0], vertices[ib * 3 + 1], vertices[ib * 3 + 2]);
                var c = new Vector3(vertices[ic * 3 + 0], vertices[ic * 3 + 1], vertices[ic * 3 + 2]);

                // Compute the midpoints.
                var ab = ((a + b) / 2f).Normalized();
                var bc = ((b + c) / 2f).Normalized();
                var ca = ((c + a) / 2f).Normalized();

                var at = i * 18;
                var nia = at / 3;
                newVertices[at++] = a.X;
                newVertices[at++] = a.Y;
                newVertices[at++] = a.Z;

                var nib = at / 3;
                newVertices[at++] = b.X;
                newVertices[at++] = b.Y;
                newVertices[at++] = b.Z;

                var nic = at / 3;
                newVertices[at++] = c.X;
                newVertices[at++] = c.Y;
                newVertices[at++] = c.Z;

                var niab = at / 3;
                newVertices[at++] = ab.X;
                newVertices[at++] = ab.Y;
                newVertices[at++] = ab.Z;

                var nibc = at / 3;
                newVertices[at++] = bc.X;
                newVertices[at++] = bc.Y;
                newVertices[at++] = bc.Z;

                var nica = at / 3;
                newVertices[at++] = ca.X;
                newVertices[at++] = ca.Y;
                newVertices[at] = ca.Z;

                at = i * 12;
                newIndices[at++] = (uint) nica;
                newIndices[at++] = (uint) nia;
                newIndices[at++] = (uint) niab;

                newIndices[at++] = (uint) niab;
                newIndices[at++] = (uint) nib;
                newIndices[at++] = (uint) nibc;

                newIndices[at++] = (uint) nibc;
                newIndices[at++] = (uint) nic;
                newIndices[at++] = (uint) nica;

                newIndices[at++] = (uint) nica;
                newIndices[at++] = (uint) niab;
                newIndices[at] = (uint) nibc;
            });

            return (newVertices, newIndices);
        }
    }
}