/// <summary>****************************************************************************
/// Copyright (c) 2011, Daniel Murphy
/// All rights reserved.
/// 
/// Redistribution and use in source and binary forms, with or without modification,
/// are permitted provided that the following conditions are met:
/// * Redistributions of source code must retain the above copyright notice,
/// this list of conditions and the following disclaimer.
/// * Redistributions in binary form must reproduce the above copyright notice,
/// this list of conditions and the following disclaimer in the documentation
/// and/or other materials provided with the distribution.
/// 
/// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
/// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
/// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
/// IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
/// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
/// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
/// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
/// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
/// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
/// POSSIBILITY OF SUCH DAMAGE.
/// ****************************************************************************
/// </summary>
using System;
namespace org.jbox2d.common
{
	
	/// <summary> Orientated bounding box viewport transform
	/// 
	/// </summary>
	/// <author>  Daniel Murphy
	/// </author>
	public class OBBViewportTransform : IViewportTransform
	{
		private void  InitBlock()
		{
			box = new OBB();
			yFlipMatInv = yFlipMat.invert();
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <summary> gets the transform of the viewport, transforms around the center. Not a copy.
		/// 
		/// </summary>
		/// <returns>
		/// </returns>
		/// <summary> Sets the transform of the viewport. Transforms about the center.
		/// 
		/// </summary>
		/// <param name="transform">
		/// </param>
		virtual public Mat22 Transform
		{
			get
			{
				return box.R;
			}
			
			set
			{
				box.R.set_Renamed(value);
			}
			
		}
		//UPGRADE_NOTE: Respective javadoc comments were merged.  It should be changed in order to comply with .NET documentation conventions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1199'"
		/// <seealso cref="IViewportTransform.isYFlip()">
		/// </seealso>
		/// <seealso cref="IViewportTransform.setYFlip(boolean)">
		/// </seealso>
		virtual public bool YFlip
		{
			get
			{
				return yFlip;
			}
			
			set
			{
				this.yFlip = value;
			}
			
		}
		
		public class OBB
		{
			//UPGRADE_NOTE: Final was removed from the declaration of 'R '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
			public Mat22 R = new Mat22();
			//UPGRADE_NOTE: Final was removed from the declaration of 'center '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
			public Vec2 center = new Vec2();
			//UPGRADE_NOTE: Final was removed from the declaration of 'extents '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
			public Vec2 extents = new Vec2();
		}
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'box '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'box' was moved to method 'InitBlock'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		protected internal OBB box;
		private bool yFlip = false;
		//UPGRADE_NOTE: Final was removed from the declaration of 'yFlipMat '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private Mat22 yFlipMat = new Mat22(1, 0, 0, - 1);
		//UPGRADE_NOTE: Final was removed from the declaration of 'yFlipMatInv '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		//UPGRADE_NOTE: The initialization of  'yFlipMatInv' was moved to method 'InitBlock'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
		private Mat22 yFlipMatInv;
		
		public OBBViewportTransform()
		{
			InitBlock();
			box.R.setIdentity();
		}
		
		public virtual void  set_Renamed(OBBViewportTransform vpt)
		{
			box.center.set_Renamed(vpt.box.center);
			box.extents.set_Renamed(vpt.box.extents);
			box.R.set_Renamed(vpt.box.R);
			yFlip = vpt.yFlip;
		}
		
		/// <seealso cref="IViewportTransform.setCamera(float, float, float)">
		/// </seealso>
		public virtual void  setCamera(float x, float y, float scale)
		{
			box.center.set_Renamed(x, y);
			Mat22.createScaleTransform(scale, box.R);
		}
		
		/// <seealso cref="IViewportTransform.getExtents()">
		/// </seealso>
		public virtual Vec2 getExtents()
		{
			return box.extents;
		}
		
		/// <seealso cref="IViewportTransform.setExtents(Vec2)">
		/// </seealso>
		public virtual void  setExtents(Vec2 argExtents)
		{
			box.extents.set_Renamed(argExtents);
		}
		
		/// <seealso cref="IViewportTransform.setExtents(float, float)">
		/// </seealso>
		public virtual void  setExtents(float argHalfWidth, float argHalfHeight)
		{
			box.extents.set_Renamed(argHalfWidth, argHalfHeight);
		}
		
		/// <seealso cref="IViewportTransform.getCenter()">
		/// </seealso>
		public virtual Vec2 getCenter()
		{
			return box.center;
		}
		
		/// <seealso cref="IViewportTransform.setCenter(Vec2)">
		/// </seealso>
		public virtual void  setCenter(Vec2 argPos)
		{
			box.center.set_Renamed(argPos);
		}
		
		/// <seealso cref="IViewportTransform.setCenter(float, float)">
		/// </seealso>
		public virtual void  setCenter(float x, float y)
		{
			box.center.set_Renamed(x, y);
		}
		
		/// <summary> Multiplies the obb transform by the given transform
		/// 
		/// </summary>
		/// <param name="argTransform">
		/// </param>
		public virtual void  mulByTransform(Mat22 argTransform)
		{
			box.R.mulLocal(argTransform);
		}
		
		// djm pooling
		//UPGRADE_NOTE: Final was removed from the declaration of 'inv '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private Mat22 inv = new Mat22();
		
		/// <seealso cref="IViewportTransform.getScreenVectorToWorld(Vec2, Vec2)">
		/// </seealso>
		public virtual void  getScreenVectorToWorld(Vec2 argScreen, Vec2 argWorld)
		{
			inv.set_Renamed(box.R);
			inv.invertLocal();
			inv.mulToOut(argScreen, argWorld);
			if (yFlip)
			{
				yFlipMatInv.mulToOut(argWorld, argWorld);
			}
		}
		
		/// <seealso cref="IViewportTransform.getWorldVectorToScreen(Vec2, Vec2)">
		/// </seealso>
		public virtual void  getWorldVectorToScreen(Vec2 argWorld, Vec2 argScreen)
		{
			box.R.mulToOut(argWorld, argScreen);
			if (yFlip)
			{
				yFlipMatInv.mulToOut(argScreen, argScreen);
			}
		}
		
		/// <seealso cref="IViewportTransform.getWorldToScreen(Vec2, Vec2)">
		/// </seealso>
		public virtual void  getWorldToScreen(Vec2 argWorld, Vec2 argScreen)
		{
			argScreen.set_Renamed(argWorld);
			argScreen.subLocal(box.center);
			box.R.mulToOut(argScreen, argScreen);
			if (yFlip)
			{
				yFlipMat.mulToOut(argScreen, argScreen);
			}
			argScreen.addLocal(box.extents);
		}
		
		//UPGRADE_NOTE: Final was removed from the declaration of 'inv2 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		private Mat22 inv2 = new Mat22();
		
		/// <seealso cref="IViewportTransform.getScreenToWorld(Vec2, Vec2)">
		/// </seealso>
		public virtual void  getScreenToWorld(Vec2 argScreen, Vec2 argWorld)
		{
			argWorld.set_Renamed(argScreen);
			argWorld.subLocal(box.extents);
			box.R.invertToOut(inv2);
			inv2.mulToOut(argWorld, argWorld);
			if (yFlip)
			{
				yFlipMatInv.mulToOut(argWorld, argWorld);
			}
			argWorld.addLocal(box.center);
		}
	}
}