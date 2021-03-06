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
using Box2D.Callbacks;
using Box2D.Common;

//UPGRADE_TODO: The type 'org.slf4j.Logger' could not be found. If it was not included in the conversion, there may be compiler issues. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1262'"
//using Logger = org.slf4j.Logger;
//UPGRADE_TODO: The type 'org.slf4j.LoggerFactory' could not be found. If it was not included in the conversion, there may be compiler issues. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1262'"
//using LoggerFactory = org.slf4j.LoggerFactory;

namespace Box2D.Collision.Broadphase
{

	/// <summary>
	/// The broad-phase is used for computing pairs and performing volume queries and ray casts. This
	/// broad-phase does not persist pairs. Instead, this reports potentially new pairs. It is up to the
	/// client to consume the new pairs and to track subsequent overlap.
	/// </summary>
	/// <author>Daniel Murphy</author>
	public class BroadPhase : ITreeCallback
	{
	    /// <summary>
	    /// Get the number of proxies.
	    /// </summary>
	    /// <returns></returns>
	    public int ProxyCount { get; private set; }

	    /// <summary>
		/// Get the height of the embedded tree.
		/// </summary>
		/// <returns></returns>
		public int TreeHeight
		{
			get
			{
				return m_tree.ComputeHeight();
			}
		}

		public int TreeBalance
		{
			get
			{
				return m_tree.MaxBalance;
			}
		}

		public float TreeQuality
		{
			get
			{
				return m_tree.AreaRatio;
			}
		}

		//private static readonly Logger log = LoggerFactory.getLogger(typeof(BroadPhase));

		public const int NULL_PROXY = -1;

		private readonly DynamicTree m_tree;

	    private int[] m_moveBuffer;
		private int m_moveCapacity;
		private int m_moveCount;

		private Pair[] m_pairBuffer;
		private int m_pairCapacity;
		private int m_pairCount;

		private int m_queryProxyId;

		public BroadPhase()
		{
			ProxyCount = 0;

			m_pairCapacity = 16;
			m_pairCount = 0;
			m_pairBuffer = new Pair[m_pairCapacity];
			for (int i = 0; i < m_pairCapacity; i++)
			{
				m_pairBuffer[i] = new Pair();
			}

			m_moveCapacity = 16;
			m_moveCount = 0;
			m_moveBuffer = new int[m_moveCapacity];

			m_tree = new DynamicTree();
			m_queryProxyId = NULL_PROXY;
		}

		/// <summary>
		/// Create a proxy with an initial AABB. Pairs are not reported until updatePairs is called.
		/// </summary>
		/// <param name="aabb"></param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public int CreateProxy(AABB aabb, object userData)
		{
			int proxyId = m_tree.CreateProxy(aabb, userData);
			++ProxyCount;
			BufferMove(proxyId);
			return proxyId;
		}

		/// <summary>
		/// Destroy a proxy. It is up to the client to remove any pairs.
		/// </summary>
		/// <param name="proxyId"></param>
		public void DestroyProxy(int proxyId)
		{
			UnbufferMove(proxyId);
			--ProxyCount;
			m_tree.DestroyProxy(proxyId);
		}

		/// <summary>
		/// Call MoveProxy as many times as you like, then when you are done call UpdatePairs to finalized
		/// the proxy pairs (for your time step).
		/// </summary>
		public void MoveProxy(int proxyId, AABB aabb, Vec2 displacement)
		{
			bool buffer = m_tree.MoveProxy(proxyId, aabb, displacement);
			if (buffer)
			{
				BufferMove(proxyId);
			}
		}

		public void TouchProxy(int proxyId)
		{
			BufferMove(proxyId);
		}

		public object GetUserData(int proxyId)
		{
			return m_tree.GetUserData(proxyId);
		}

		public AABB GetFatAABB(int proxyId)
		{
			return m_tree.GetFatAABB(proxyId);
		}

		public bool TestOverlap(int proxyIdA, int proxyIdB)
		{
			// return AABB.testOverlap(proxyA.aabb, proxyB.aabb);
			var a = m_tree.GetFatAABB(proxyIdA);
			var b = m_tree.GetFatAABB(proxyIdB);
			if (b.LowerBound.X - a.UpperBound.X > 0.0f || b.LowerBound.Y - a.UpperBound.Y > 0.0f)
			{
				return false;
			}

			if (a.LowerBound.X - b.UpperBound.X > 0.0f || a.LowerBound.Y - b.UpperBound.Y > 0.0f)
			{
				return false;
			}

			return true;
		}

		public void DrawTree(DebugDraw argDraw)
		{
			m_tree.DrawTree(argDraw);
		}

		/// <summary>
		/// Update the pairs. This results in pair callbacks. This can only add pairs.
		/// </summary>
		/// <param name="callback"></param>
		public void UpdatePairs(IPairCallback callback)
		{
			// log.debug("beginning to update pairs");
			// Reset pair buffer
			m_pairCount = 0;

			// Perform tree queries for all moving proxies.
			for (int i = 0; i < m_moveCount; ++i)
			{
				m_queryProxyId = m_moveBuffer[i];
				if (m_queryProxyId == NULL_PROXY)
				{
					continue;
				}

				// We have to query the tree with the fat AABB so that
				// we don't fail to create a pair that may touch later.
				var fatAABB = m_tree.GetFatAABB(m_queryProxyId);

				// Query tree, create pairs and add them pair buffer.
				// log.debug("quering aabb: "+m_queryProxy.aabb);
				m_tree.Query(this, fatAABB);
			}
			// log.debug("Number of pairs found: "+m_pairCount);

			// Reset move buffer
			m_moveCount = 0;

			// Sort the pair buffer to expose duplicates.
			Array.Sort(m_pairBuffer, 0, m_pairCount - 0);

			// Send the pairs back to the client.
			int i2 = 0;
			while (i2 < m_pairCount)
			{
				var primaryPair = m_pairBuffer[i2];
				var userDataA = m_tree.GetUserData(primaryPair.ProxyIdA);
				var userDataB = m_tree.GetUserData(primaryPair.ProxyIdB);

				// log.debug("returning pair: "+userDataA+", "+userDataB);
				callback.AddPair(userDataA, userDataB);
				++i2;

				// Skip any duplicate pairs.
				while (i2 < m_pairCount)
				{
					var pair = m_pairBuffer[i2];
					if (pair.ProxyIdA != primaryPair.ProxyIdA || pair.ProxyIdB != primaryPair.ProxyIdB)
					{
						break;
					}
					// log.debug("skipping duplicate");
					++i2;
				}
			}

			// Try to keep the tree balanced.
			// m_tree.rebalance(Settings.TREE_REBALANCE_STEPS);
		}

		/// <summary>
		/// Query an AABB for overlapping proxies. The callback class is called for each proxy that
		/// overlaps the supplied AABB.
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="aabb"></param>
		public void Query(ITreeCallback callback, AABB aabb)
		{
			m_tree.Query(callback, aabb);
		}

		/// <summary>
		/// Ray-cast against the proxies in the tree. This relies on the callback to perform a exact
		/// ray-cast in the case were the proxy contains a shape. The callback also performs the any
		/// collision filtering. This has performance roughly equal to k * log(n), where k is the number of
		/// collisions and n is the number of proxies in the tree.
		/// </summary>
		/// <param name="input">the ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).</param>
		/// <param name="callback">a callback class that is called for each proxy that is hit by the ray.</param>
		public void Raycast(ITreeRayCastCallback callback, RayCastInput input)
		{
			m_tree.Raycast(callback, input);
		}

		protected internal void BufferMove(int proxyId)
		{
			if (m_moveCount == m_moveCapacity)
			{
				var old = m_moveBuffer;
				m_moveCapacity *= 2;
				m_moveBuffer = new int[m_moveCapacity];
				Array.Copy(old, 0, m_moveBuffer, 0, old.Length);
			}

			m_moveBuffer[m_moveCount] = proxyId;
			++m_moveCount;
		}

		protected internal void UnbufferMove(int proxyId)
		{
			for (int i = 0; i < m_moveCount; i++)
			{
				if (m_moveBuffer[i] == proxyId)
				{
					m_moveBuffer[i] = NULL_PROXY;
				}
			}
		}

		// private final PairStack pairStack = new PairStack();
		/// <summary>
		/// This is called from DynamicTree::query when we are gathering pairs.
		/// </summary>
		public bool TreeCallback(int proxyId)
		{

			// log.debug("Got a proxy back: " + proxyId);
			// A proxy cannot form a pair with itself.
			if (proxyId == m_queryProxyId)
			{
				// log.debug("It was us...");
				return true;
			}

			// Grow the pair buffer as needed.
			if (m_pairCount == m_pairCapacity)
			{
				var oldBuffer = m_pairBuffer;
				m_pairCapacity *= 2;
				m_pairBuffer = new Pair[m_pairCapacity];
				Array.Copy(oldBuffer, 0, m_pairBuffer, 0, oldBuffer.Length);
				for (int i = oldBuffer.Length; i < m_pairCapacity; i++)
				{
					m_pairBuffer[i] = new Pair();
				}
			}

			if (proxyId < m_queryProxyId)
			{
				// log.debug("new proxy is first");
				m_pairBuffer[m_pairCount].ProxyIdA = proxyId;
				m_pairBuffer[m_pairCount].ProxyIdB = m_queryProxyId;
			}
			else
			{
				// log.debug("new proxy is second");
				m_pairBuffer[m_pairCount].ProxyIdA = m_queryProxyId;
				m_pairBuffer[m_pairCount].ProxyIdB = proxyId;
			}

			++m_pairCount;
			return true;
		}
	}
}