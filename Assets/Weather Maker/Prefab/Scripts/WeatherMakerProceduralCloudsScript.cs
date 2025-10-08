//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerProceduralCloudsScript : MonoBehaviour
    {

/*

        public Material CloudMaterial;
        public Light Sun;

        private CommandBuffer commandBuffer;

        private void UpdateMaterial()
        {
            CloudMaterial.SetVector("_WeatherMakerSunDirection", -Sun.transform.forward);
            Vector4 sunColor = new Vector4(Sun.color.r, Sun.color.g, Sun.color.b, Sun.intensity);
            CloudMaterial.SetVector("_WeatherMakerSunColor", sunColor);
            CloudMaterial.SetMatrix("_CameraInverseMVP", Camera.main.cameraToWorldMatrix * Camera.main.projectionMatrix.inverse);
            CloudMaterial.SetMatrix("_CameraInverseMV", Camera.main.cameraToWorldMatrix);
            CloudMaterial.SetMatrix("_CameraWorldToCamera", Camera.main.worldToCameraMatrix);
        }

        private void CreateCommandBuffer()
        {
            RemoveCommandBuffer();
            commandBuffer = new CommandBuffer();
            Camera.main.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
            commandBuffer.Blit((Texture2D)null, BuiltinRenderTextureType.CameraTarget, CloudMaterial);
        }

        private void RemoveCommandBuffer()
        {
            if (commandBuffer != null)
            {
                commandBuffer.Clear();
                Camera.main.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
                commandBuffer = null;
            }
        }

        private void OnDestroy()
        {
            RemoveCommandBuffer();
        }

        private void OnDisable()
        {
            RemoveCommandBuffer();
        }

        private void OnEnable()
        {
            CreateCommandBuffer();
        }

        private void Awake()
        {
            CloudMaterial = new Material(CloudMaterial);
        }

        private void Update()
        {
            UpdateMaterial();
        }

*/

    }
}
