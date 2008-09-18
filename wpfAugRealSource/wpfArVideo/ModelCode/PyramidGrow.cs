//----------------------------------------------
// (c) 2007 by casey chesnut, brains-N-brawn LLC
//----------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace wpfArVideo
{
    class PyramidGrow : ModelVisual3D, IModelCode
    {
        private wpfArVideo.Window1.MyModel mm;
        AnimationClock clock;

        public void Init(wpfArVideo.Window1.MyModel mm)
        {
            this.mm = mm;

            Transform3DGroup t3dg = (Transform3DGroup)this.mm.m3dg.Transform;
            ScaleTransform3D st3d = (ScaleTransform3D)t3dg.Children[0];
            st3d.ScaleX = 0.01;
            st3d.ScaleY = 0.01;
            st3d.ScaleZ = 0.01;

            DoubleAnimation dax = new DoubleAnimation(0.01, 0.6, new Duration(new TimeSpan(0, 0, 3)));
            clock = dax.CreateClock();

            st3d.ApplyAnimationClock(ScaleTransform3D.ScaleXProperty, clock);
            st3d.ApplyAnimationClock(ScaleTransform3D.ScaleYProperty, clock);
            st3d.ApplyAnimationClock(ScaleTransform3D.ScaleZProperty, clock);
            clock.Controller.Begin();
        }

        public void Start()
        {
            clock.Controller.Resume();
        }

        public void Stop()
        {
            clock.Controller.Pause();
        }
    }
}
