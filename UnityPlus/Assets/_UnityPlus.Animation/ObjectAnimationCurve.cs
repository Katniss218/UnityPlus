using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Animation
{
    public class ObjectAnimationCurve
    {
        // xyz components put together for better cache hits hopefully.
        public Vector3Curve position;
        public QuaternionCurve rotation; // shouldn't be normalized.
        public Vector3Curve scale;
    }
}