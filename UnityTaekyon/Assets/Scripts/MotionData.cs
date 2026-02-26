using System;
using System.Collections.Generic;
using UnityEngine;

namespace Taekyon
{
    [Serializable]
    public class MotionClip
    {
        public int fps;
        public MotionFrame[] frames;
    }

    [Serializable]
    public class MotionFrame
    {
        public int t;
        public Dictionary<string, Vector3> joints;
    }
}
