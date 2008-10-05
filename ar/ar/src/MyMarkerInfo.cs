using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using ARTKPManagedWrapper;

namespace projAR
{
    public class MyMarkerInfo
    {
        public Matrix transform;
        public ArManWrap.ARMarkerInfo markerInfo;
        public float[] prevMatrix = new float[0];
        public bool found = false;
        public int notFoundCount = 0;
    }
}
