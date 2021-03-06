// ****************************************************************************
// Copyright (c) 2011, Daniel Murphy
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// * Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation
// and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
// ****************************************************************************

using System;
using System.Diagnostics;
using Box2D.Collision;
using Box2D.Collision.Broadphase;
using Box2D.Collision.Shapes;
using Box2D.Common;
using Box2D.Dynamics.Contacts;

namespace Box2D.Dynamics
{

    /// <summary>
    /// A fixture is used to attach a shape to a body for collision detection. A fixture inherits its
    /// transform from its parent. Fixtures hold additional non-geometric data such as friction,
    /// collision filters, etc. Fixtures are created via Body::CreateFixture.
    /// </summary>
    /// <warning>you cannot reuse fixtures.</warning>
    /// <author>daniel</author>
    public class Fixture
    {
        private float m_density;

        public FixtureProxy[] Proxies;
        public int ProxyCount;

        public readonly Filter Filter;

        public bool IsSensor;

        public Fixture()
        {
            UserData = null;
            Body = null;
            Next = null;
            Proxies = null;
            ProxyCount = 0;
            Shape = null;
            Filter = new Filter();
        }


        /// <summary>
        /// Get the type of the child shape. You can use this to down cast to the concrete shape.
        /// </summary>
        /// <returns>the shape type.</returns>
        public ShapeType Type
        {
            get
            {
                return Shape.Type;
            }
        }

        /// <summary>
        /// Get the child shape. You can modify the child shape, however you should not change the number
        /// of vertices because this will crash some collision caching mechanisms.
        /// </summary>
        /// <returns></returns>
        public Shape Shape { get; set; }

        /// <summary>
        /// Is this fixture a sensor (non-solid)?
        /// </summary>
        public bool Sensor
        {
            get
            {
                return IsSensor;
            }
            set
            {
                if (value != IsSensor)
                {
                    Body.Awake = true;
                    IsSensor = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the contact filtering data.
        /// Setter is an expensive operation and should not be called
        /// frequently. This will not update contacts until the next time step when either parent body is
        /// awake. This automatically calls refilter.
        /// </summary>
        public Filter FilterData
        {
            get
            {
                return Filter;
            }
            set
            {
                Filter.Set(value);

                Refilter();
            }
        }

        /// <summary>
        /// Get the parent body of this fixture. This is NULL if the fixture is not attached.
        /// </summary>
        /// <returns>the parent body.</returns>
        public Body Body { get; set; }

        /// <summary>
        /// Get the next fixture in the parent body's fixture list.
        /// </summary>
        /// <returns>the next shape.</returns>
        public Fixture Next { get; set; }

        public float Density
        {
            get
            {
                return m_density;
            }
            set
            {
                Debug.Assert(value >= 0f);
                m_density = value;
            }
        }

        /// <summary>
        /// Gets or sets the user data that was assigned in the fixture definition. Use this to store your
        /// application specific data.
        /// </summary>
        public object UserData { get; set; }

        /// <summary>
        /// Gets or sets the coefficient of friction.
        /// Setter will not change the friction of existing contacts.
        /// </summary>
        public float Friction { get; set; }

        /// <summary>
        /// Gets or sets the coefficient of restitution.
        /// Setter will not change the restitution of existing
        /// contacts.
        /// </summary>
        public float Restitution { get; set; }

        /// <summary>
        /// Call this if you want to establish collision that was previously disabled by
        /// ContactFilter::ShouldCollide.
        /// </summary>
        public void Refilter()
        {
            if (Body == null)
            {
                return;
            }

            // Flag associated contacts for filtering.
            ContactEdge edge = Body.ContactList;
            while (edge != null)
            {
                Contact contact = edge.Contact;
                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                if (fixtureA == this || fixtureB == this)
                {
                    contact.SetFlagForFiltering();
                }
                edge = edge.Next;
            }

            World world = Body.World;

            if (world == null)
            {
                return;
            }

            // Touch each proxy so that new pairs may be created
            BroadPhase broadPhase = world.ContactManager.BroadPhase;
            for (int i = 0; i < ProxyCount; ++i)
            {
                broadPhase.TouchProxy(Proxies[i].ProxyId);
            }
        }

        /// <summary>
        /// Test a point for containment in this fixture. This only works for convex shapes.
        /// </summary>
        /// <param name="p">a point in world coordinates.</param>
        /// <returns></returns>
        public bool TestPoint(Vec2 p)
        {
            return Shape.TestPoint(Body.Xf, p);
        }

        /// <summary>
        /// Cast a ray against this shape.
        /// </summary>
        /// <param name="output">the ray-cast results.</param>
        /// <param name="input">the ray-cast input parameters.</param>
        /// <param name="childIndex"></param>
        public bool Raycast(RayCastOutput output, RayCastInput input, int childIndex)
        {
            return Shape.Raycast(output, input, Body.Xf, childIndex);
        }

        /// <summary>
        /// Get the mass data for this fixture. The mass data is based on the density and the shape. The
        /// rotational inertia is about the shape's origin.
        /// </summary>
        /// <returns></returns>
        public void GetMassData(MassData massData)
        {
            Shape.ComputeMass(massData, m_density);
        }

        /// <summary>
        /// Get the fixture's AABB. This AABB may be enlarge and/or stale. If you need a more accurate
        /// AABB, compute it using the shape and the body transform.
        /// </summary>
        /// <returns></returns>
        public AABB GetAABB(int childIndex)
        {
            Debug.Assert(childIndex >= 0 && childIndex < ProxyCount);
            return Proxies[childIndex].AABB;
        }

        /// <summary>
        /// Dump this fixture to the log file.
        /// </summary>
        /// <param name="bodyIndex"></param>
        public void Dump(int bodyIndex)
        {

        }


        // We need separation create/destroy functions from the constructor/destructor because
        // the destructor cannot access the allocator (no destructor arguments allowed by C++).

        public void Create(Body body, FixtureDef def)
        {
            UserData = def.UserData;
            Friction = def.Friction;
            Restitution = def.Restitution;

            Body = body;
            Next = null;


            Filter.Set(def.Filter);

            IsSensor = def.IsSensor;

            Shape = def.Shape.Clone();

            // Reserve proxy space
            int childCount = Shape.ChildCount;
            if (Proxies == null)
            {
                Proxies = new FixtureProxy[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    Proxies[i] = new FixtureProxy {Fixture = null, ProxyId = BroadPhase.NULL_PROXY};
                }
            }

            if (Proxies.Length < childCount)
            {
                FixtureProxy[] old = Proxies;
                int newLen = MathUtils.Max(old.Length * 2, childCount);
                Proxies = new FixtureProxy[newLen];
                Array.Copy(old, 0, Proxies, 0, old.Length);
                for (int i = 0; i < newLen; i++)
                {
                    if (i >= old.Length)
                    {
                        Proxies[i] = new FixtureProxy();
                    }
                    Proxies[i].Fixture = null;
                    Proxies[i].ProxyId = BroadPhase.NULL_PROXY;
                }
            }
            ProxyCount = 0;

            m_density = def.Density;
        }

        public void Destroy()
        {
            // The proxies must be destroyed before calling this.
            Debug.Assert(ProxyCount == 0);

            // Free the child shape.
            Shape = null;
            Proxies = null;
            Next = null;

            // TODO pool shapes
            // TODO pool fixtures
        }

        // These support body activation/deactivation.
        public void CreateProxies(BroadPhase broadPhase, Transform xf)
        {
            Debug.Assert(ProxyCount == 0);

            // Create proxies in the broad-phase.
            ProxyCount = Shape.ChildCount;

            for (int i = 0; i < ProxyCount; ++i)
            {
                FixtureProxy proxy = Proxies[i];
                Shape.ComputeAABB(proxy.AABB, xf, i);
                proxy.ProxyId = broadPhase.CreateProxy(proxy.AABB, proxy);
                proxy.Fixture = this;
                proxy.ChildIndex = i;
            }
        }

        /// <summary>
        /// Internal method
        /// </summary>
        /// <param name="broadPhase"></param>
        public void DestroyProxies(BroadPhase broadPhase)
        {
            // Destroy proxies in the broad-phase.
            for (int i = 0; i < ProxyCount; ++i)
            {
                FixtureProxy proxy = Proxies[i];
                broadPhase.DestroyProxy(proxy.ProxyId);
                proxy.ProxyId = BroadPhase.NULL_PROXY;
            }

            ProxyCount = 0;
        }

        private readonly AABB pool1 = new AABB();
        private readonly AABB pool2 = new AABB();
        private readonly Vec2 displacement = new Vec2();

        /// <summary>
        /// Internal method
        /// </summary>
        /// <param name="broadPhase"></param>
        /// <param name="transform1"></param>
        /// <param name="transform2"></param>
        protected internal void Synchronize(BroadPhase broadPhase, Transform transform1, Transform transform2)
        {
            if (ProxyCount == 0)
            {
                return;
            }

            for (int i = 0; i < ProxyCount; ++i)
            {
                FixtureProxy proxy = Proxies[i];

                // Compute an AABB that covers the swept shape (may miss some rotation effect).
                AABB aabb1 = pool1;
                AABB aab = pool2;
                Shape.ComputeAABB(aabb1, transform1, proxy.ChildIndex);
                Shape.ComputeAABB(aab, transform2, proxy.ChildIndex);

                proxy.AABB.LowerBound.X = aabb1.LowerBound.X < aab.LowerBound.X ? aabb1.LowerBound.X : aab.LowerBound.X;
                proxy.AABB.LowerBound.Y = aabb1.LowerBound.Y < aab.LowerBound.Y ? aabb1.LowerBound.Y : aab.LowerBound.Y;
                proxy.AABB.UpperBound.X = aabb1.UpperBound.X > aab.UpperBound.X ? aabb1.UpperBound.X : aab.UpperBound.X;
                proxy.AABB.UpperBound.Y = aabb1.UpperBound.Y > aab.UpperBound.Y ? aabb1.UpperBound.Y : aab.UpperBound.Y;
                displacement.X = transform2.P.X - transform1.P.X;
                displacement.Y = transform2.P.Y - transform1.P.Y;

                broadPhase.MoveProxy(proxy.ProxyId, proxy.AABB, displacement);
            }
        }
    }
}