//----------------------------------------------
// (c) 2007 by casey chesnut, brains-N-brawn LLC
//----------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Controls;

namespace wpfArVideo
{
    public interface IModelCode
    {
        void Init(wpfArVideo.Window1.MyModel m3dg);
        void Start();
        void Stop();
    }
}
