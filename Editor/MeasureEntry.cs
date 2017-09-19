using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeasuringTool
{
    [System.Serializable]
    public class MeasureEntry
    {
        private GameObject a, b;
        private float fDistance;
        private Vector3 vec3Distance;

        public Color Color;
        public bool Shown = true;

        public GameObject B
        {
            get
            {
                return b;
            }
            set
            {
                b = value;
                UpdateDistance();
            }
        }

        public GameObject A
        {
            get
            {
                return a;
            }
            set
            {
                a = value;
                UpdateDistance();
            }
        }

        public float FDistance
        {
            get
            {
                return fDistance;
            }
        }

        public Vector3 Vec3Distance
        {
            get
            {
                return vec3Distance;
            }
        }

        public MeasureEntry()
        {
            a = null;
            b = null;
            fDistance = 0f;
            Color = Color.white;
        }

        public void UpdateDistance()
        {
            if (a != null && b != null)
            {
                fDistance = Vector3.Distance(b.transform.position, a.transform.position);

                vec3Distance = b.transform.position - a.transform.position;
                vec3Distance.x = Mathf.Abs(vec3Distance.x);
                vec3Distance.y = Mathf.Abs(vec3Distance.y);
                vec3Distance.z = Mathf.Abs(vec3Distance.z);
            }
        }
    }
}