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

namespace wpfArVideo
{
    class NullModel : ModelVisual3D, IModelCode
    {
        private wpfArVideo.Window1.MyModel mm;

        public void Init(wpfArVideo.Window1.MyModel mm)
        {
            this.mm = mm;

        }

        public void Start()
        {

        }

        public void Stop()
        {

        }
    }
}
