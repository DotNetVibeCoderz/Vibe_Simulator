using System;
using System.Collections.Generic;
using System.Numerics;

namespace PhysicsNet.Core
{
    public class SpringJoint
    {
        public Body BodyA { get; }
        public Body BodyB { get; }
        public float Length { get; }
        public float Stiffness { get; }
        public float Damping { get; }

        public SpringJoint(Body a, Body b, float length, float stiffness, float damping)
        {
            BodyA = a;
            BodyB = b;
            Length = length;
            Stiffness = stiffness;
            Damping = damping;
        }

        public void ApplyForce()
        {
            Vector2 direction = BodyB.Position - BodyA.Position;
            float currentLength = direction.Length();
            if (currentLength == 0) return;

            Vector2 unitDir = direction / currentLength;
            float stretch = currentLength - Length;

            // Hooke's Law: F = -k * x
            Vector2 force = unitDir * (Stiffness * stretch);

            // Damping
            Vector2 relVel = BodyB.Velocity - BodyA.Velocity;
            float dampingForce = Vector2.Dot(relVel, unitDir) * Damping;
            force += unitDir * dampingForce;

            BodyA.ApplyForce(force);
            BodyB.ApplyForce(-force);
        }
    }

    public class PhysicsWorld
    {
        public List<Body> Bodies { get; } = new List<Body>();
        public List<SpringJoint> Joints { get; } = new List<SpringJoint>();
        public Vector2 Gravity { get; set; } = new Vector2(0, 9.8f * 50f); 

        public void AddBody(Body body) => Bodies.Add(body);
        public void AddJoint(SpringJoint joint) => Joints.Add(joint);
        public void Clear() { Bodies.Clear(); Joints.Clear(); }

        public void Step(float dt)
        {
            // 1. Apply Forces
            foreach (var body in Bodies)
            {
                if (body.InverseMass == 0) continue;
                body.ApplyForce(Gravity * body.Mass);
            }

            foreach (var joint in Joints)
            {
                joint.ApplyForce();
            }

            // 2. Integrate
            foreach (var body in Bodies)
            {
                body.Integrate(dt);
            }

            // 3. Detect & Resolve Collisions
            // Iterate a few times to resolve stacking and jitter
            int iterations = 4;
            for (int i = 0; i < iterations; i++)
            {
                SolveCollisions();
            }
        }

        private void SolveCollisions()
        {
            for (int i = 0; i < Bodies.Count; i++)
            {
                for (int j = i + 1; j < Bodies.Count; j++)
                {
                    Body a = Bodies[i];
                    Body b = Bodies[j];

                    if (a.InverseMass == 0 && b.InverseMass == 0) continue;
                    
                    if (!AABBCheck(a, b)) continue;

                    if (Collisions.Intersect(a, b, out Manifold m))
                    {
                        ResolveCollision(m);
                    }
                }
            }
        }
        
        private bool AABBCheck(Body a, Body b)
        {
             float sizeA = (a.Shape is CircleShape ca) ? ca.Radius : 
                           (a.Shape is BoxShape ba) ? MathF.Max(ba.Width, ba.Height) : 0;
             float sizeB = (b.Shape is CircleShape cb) ? cb.Radius : 
                           (b.Shape is BoxShape bb) ? MathF.Max(bb.Width, bb.Height) : 0;
             
             sizeA *= 1.42f; 
             sizeB *= 1.42f;

             float distX = MathF.Abs(a.Position.X - b.Position.X);
             float distY = MathF.Abs(a.Position.Y - b.Position.Y);
             float sumSize = sizeA + sizeB;

             return distX < sumSize && distY < sumSize;
        }

        private void ResolveCollision(Manifold m)
        {
            Body a = m.BodyA;
            Body b = m.BodyB;
            Vector2 normal = m.Normal;

            // --- Positional Correction (Anti-Stuck) ---
            // We apply this REGARDLESS of velocity to ensure objects are pushed out of walls.
            const float percent = 0.8f; // Stronger correction
            const float slop = 0.01f;   
            
            if (m.Depth > slop)
            {
                Vector2 correction = MathF.Max(m.Depth - slop, 0.0f) / (a.InverseMass + b.InverseMass) * percent * normal;
                if (a.InverseMass > 0) a.Position -= correction * a.InverseMass;
                if (b.InverseMass > 0) b.Position += correction * b.InverseMass;
            }

            // --- Velocity Resolution ---
            Vector2 rv = b.Velocity - a.Velocity;
            float velAlongNormal = Vector2.Dot(rv, normal);

            // If moving away, do not reflect velocity
            if (velAlongNormal > 0) return;

            float e = MathF.Min(a.Restitution, b.Restitution);

            // Velocity Threshold for bouncing (prevent micro-bouncing)
            // If relative velocity is small, we assume inelastic collision to stop jitter.
            if (MathF.Abs(velAlongNormal) < 60.0f) 
            {
                e = 0.0f;
            }

            float j = -(1 + e) * velAlongNormal;
            j /= a.InverseMass + b.InverseMass;

            Vector2 impulse = j * normal;
            
            a.Velocity -= impulse * a.InverseMass;
            b.Velocity += impulse * b.InverseMass;

            // Friction
            Vector2 tangent = rv - Vector2.Dot(rv, normal) * normal;
            if (tangent.LengthSquared() > 0.0001f)
            {
                tangent = Vector2.Normalize(tangent);
                float jt = -Vector2.Dot(rv, tangent);
                jt /= (a.InverseMass + b.InverseMass);

                float mu = (a.StaticFriction + b.StaticFriction) * 0.5f;
                Vector2 frictionImpulse;
                
                if (MathF.Abs(jt) < j * mu)
                    frictionImpulse = jt * tangent;
                else
                    frictionImpulse = -j * a.DynamicFriction * tangent;

                a.Velocity -= frictionImpulse * a.InverseMass;
                b.Velocity += frictionImpulse * b.InverseMass;
            }
        }
    }
}