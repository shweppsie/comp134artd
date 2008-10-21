using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using ARTKPManagedWrapper;

namespace projAR
{
    public class MyMarkerInfo
    {
        //matrix of marker
        public Matrix transform;
        //identifies marker
        public ArManWrap.ARMarkerInfo markerInfo;
        //float matrix of marker from previous frame
        public float[] prevMatrix = new float[0];
        //is this marker visible now?
        public bool found = false;
        //how long has it not been visible for?
        public int notFoundCount = 0;
        //should we be drawing this marker?
        public bool draw;
    }
}
