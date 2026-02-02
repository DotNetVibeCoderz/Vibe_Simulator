using System;
using System.Numerics;

namespace PhysicsNet.Core
{
    public struct Manifold
    {
        public Body BodyA;
        public Body BodyB;
        public Vector2 Normal;
        public float Depth;
        public Vector2 Contact1;
        public Vector2 Contact2;
        public int ContactCount;
    }

    public static class Collisions
    {
        public static bool Intersect(Body a, Body b, out Manifold manifold)
        {
            manifold = new Manifold();

            if (a.Shape is CircleShape cA && b.Shape is CircleShape cB)
            {
                return CircleVsCircle(a, cA, b, cB, out manifold);
            }
            else if (a.Shape is CircleShape cA2 && b.Shape is BoxShape bB)
            {
                return CircleVsPolygon(a, cA2, b, bB, out manifold);
            }
            else if (a.Shape is BoxShape bA && b.Shape is CircleShape cB2)
            {
                bool result = CircleVsPolygon(b, cB2, a, bA, out manifold);
                if (result)
                {
                    manifold.Normal = -manifold.Normal;
                    manifold.BodyA = a;
                    manifold.BodyB = b;
                }
                return result;
            }
            else if (a.Shape is BoxShape bA2 && b.Shape is BoxShape bB2)
            {
                return PolygonVsPolygon(a, bA2, b, bB2, out manifold);
            }
            
            return false;
        }

        private static bool CircleVsCircle(Body a, CircleShape cA, Body b, CircleShape cB, out Manifold m)
        {
            m = new Manifold();
            
            Vector2 normal = b.Position - a.Position;
            float distSq = normal.LengthSquared();
            float radiusSum = cA.Radius + cB.Radius;

            if (distSq >= radiusSum * radiusSum)
                return false;

            float distance = MathF.Sqrt(distSq);

            if (distance == 0)
            {
                m.Normal = new Vector2(0, 1);
                m.Depth = radiusSum;
                m.Contact1 = a.Position;
            }
            else
            {
                m.Normal = normal / distance;
                m.Depth = radiusSum - distance;
                m.Contact1 = a.Position + m.Normal * cA.Radius;
            }

            m.BodyA = a;
            m.BodyB = b;
            m.ContactCount = 1;
            return true;
        }

        private static bool CircleVsPolygon(Body a, CircleShape circle, Body b, BoxShape box, out Manifold m)
        {
            m = new Manifold();
            
            // Transform circle center to box local space
            Vector2 localCenter = Transform(a.Position, -b.Position, -b.Rotation);
            
            float halfW = box.Width / 2f;
            float halfH = box.Height / 2f;

            // Clamp point to box extents
            Vector2 closest = new Vector2(
                Math.Clamp(localCenter.X, -halfW, halfW),
                Math.Clamp(localCenter.Y, -halfH, halfH)
            );

            bool inside = false;
            
            if (localCenter == closest)
            {
                inside = true;
                if (Math.Abs(localCenter.X) > Math.Abs(localCenter.Y))
                {
                     if (closest.X > 0) closest.X = halfW; else closest.X = -halfW;
                }
                else
                {
                    if (closest.Y > 0) closest.Y = halfH; else closest.Y = -halfH;
                }
            }

            Vector2 normal = localCenter - closest;
            float dSq = normal.LengthSquared();
            float r = circle.Radius;

            if (dSq > r * r && !inside)
                return false;

            float d = MathF.Sqrt(dSq);
            
            Vector2 worldNormal;
            
            if (inside)
            {
                m.Depth = r + d;
                // FIX: Removed negation (-Rotate) because if inside, normal (Center-Edge) points inward.
                // We want to push the circle OUT, so we need a Normal that points towards the Edge?
                // Wait, if normal points Inward (Center - Edge), and we use Pos -= Normal, we move Outward.
                // Previous code: -Rotate(...) meant normal points Outward. Pos -= Outward -> Inward (Deeper).
                // New code: Rotate(...) means normal points Inward. Pos -= Inward -> Outward. Correct.
                worldNormal = Rotate(normal / d, b.Rotation);
            }
            else
            {
                m.Depth = r - d;
                worldNormal = Rotate(normal / d, b.Rotation);
            }

            m.Normal = worldNormal;
            m.Contact1 = Rotate(closest, b.Rotation) + b.Position;
            m.BodyA = a;
            m.BodyB = b;
            m.ContactCount = 1;

            return true;
        }

        private static bool PolygonVsPolygon(Body a, BoxShape boxA, Body b, BoxShape boxB, out Manifold m)
        {
            m = new Manifold();
            m.BodyA = a;
            m.BodyB = b;

            Vector2[] verticesA = GetVertices(a, boxA);
            Vector2[] verticesB = GetVertices(b, boxB);

            float depth = float.MaxValue;
            Vector2 normal = Vector2.Zero;

            // Check axes of A
            for (int i = 0; i < verticesA.Length; i++)
            {
                Vector2 p1 = verticesA[i];
                Vector2 p2 = verticesA[(i + 1) % verticesA.Length];
                
                Vector2 edge = p2 - p1;
                Vector2 axis = new Vector2(-edge.Y, edge.X); // Normal
                axis = Vector2.Normalize(axis);

                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA) return false;

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            // Check axes of B
            for (int i = 0; i < verticesB.Length; i++)
            {
                Vector2 p1 = verticesB[i];
                Vector2 p2 = verticesB[(i + 1) % verticesB.Length];
                
                Vector2 edge = p2 - p1;
                Vector2 axis = new Vector2(-edge.Y, edge.X);
                axis = Vector2.Normalize(axis);

                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA) return false;

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            Vector2 direction = b.Position - a.Position;
            if (Vector2.Dot(direction, normal) < 0)
            {
                normal = -normal;
            }

            m.Normal = normal;
            m.Depth = depth;
            m.ContactCount = 1; 
            
            return true;
        }

        private static Vector2[] GetVertices(Body b, BoxShape shape)
        {
            float hw = shape.Width / 2f;
            float hh = shape.Height / 2f;

            Vector2[] v = new Vector2[4];
            v[0] = b.Position + Rotate(new Vector2(-hw, -hh), b.Rotation);
            v[1] = b.Position + Rotate(new Vector2(hw, -hh), b.Rotation);
            v[2] = b.Position + Rotate(new Vector2(hw, hh), b.Rotation);
            v[3] = b.Position + Rotate(new Vector2(-hw, hh), b.Rotation);
            return v;
        }

        private static void ProjectVertices(Vector2[] vertices, Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                float proj = Vector2.Dot(vertices[i], axis);
                if (proj < min) min = proj;
                if (proj > max) max = proj;
            }
        }

        public static Vector2 Rotate(Vector2 v, float angle)
        {
            float c = MathF.Cos(angle);
            float s = MathF.Sin(angle);
            return new Vector2(v.X * c - v.Y * s, v.X * s + v.Y * c);
        }

        public static Vector2 Transform(Vector2 v, Vector2 translation, float rotation)
        {
            Vector2 translated = v + translation;
            return Rotate(translated, rotation);
        }
    }
}