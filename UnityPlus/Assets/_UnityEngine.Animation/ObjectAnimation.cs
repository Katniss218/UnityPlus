using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Animation
{
    public class ObjectAnimation : ScriptableObject
    {
        public (string objName, ObjectAnimationCurve keyframes)[] Curves { get; set; }

        public float TimeScale { get; set; } // multiplier when sampling time in curves.
    }
}