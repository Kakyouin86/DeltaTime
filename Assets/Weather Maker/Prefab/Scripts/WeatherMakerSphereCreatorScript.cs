using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WeatherMakerSphereCreatorScript : MonoBehaviour
    {

#if UNITY_EDITOR

        [Header("Generation of Sphere")]
        [Range(2, 6)]
        [Tooltip("Resolution of sphere. The higher the more triangles.")]
        public int Resolution = 4;

        [UnityEngine.HideInInspector]
        [UnityEngine.SerializeField]
        private int lastResolution = -1;

        [Tooltip("UV mode for sphere generation")]
        public UVMode UVMode = UVMode.Sphere;

        [UnityEngine.HideInInspector]
        [UnityEngine.SerializeField]
        private UVMode lastUVMode = (UVMode)int.MaxValue;

        private void DestroyMesh()
        {
            if (MeshFilter.sharedMesh != null)
            {
                GameObject.DestroyImmediate(MeshFilter.sharedMesh, true);
                MeshFilter.sharedMesh = null;
            }
        }

#endif

        protected virtual void Awake()
        {
            MeshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();

#if UNITY_EDITOR

            if (Material == null)
            {
                Debug.LogErrorFormat("Material not set on {0}", gameObject.name);
                return;
            }

#endif

        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

#if UNITY_EDITOR

            if (Resolution != lastResolution)
            {
                lastResolution = Resolution;
                DestroyMesh();
            }
            if (UVMode != lastUVMode)
            {
                lastUVMode = UVMode;
                DestroyMesh();
            }
            Mesh mesh = MeshFilter.sharedMesh;
            if (mesh == null)
            {
                MeshFilter.sharedMesh = WeatherMakerSphereCreator.Create(gameObject.name, Resolution, UVMode);
            }

#endif

        }

        protected virtual void LateUpdate()
        {

        }

        public MeshFilter MeshFilter { get; private set; }
        public MeshRenderer MeshRenderer { get; private set; }
        public Material Material { get { return MeshRenderer.sharedMaterial; } }
    }
}