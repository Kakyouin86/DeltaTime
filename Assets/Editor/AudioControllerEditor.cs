using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioController))]
public class AudioControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AudioController controller = (AudioController)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Play"))
        {
            controller.PlayAudio();
        }

        if (GUILayout.Button("Pause"))
        {
            controller.PauseAudio();
        }

        if (GUILayout.Button("Stop"))
        {
            controller.StopAudio();
        }

        if (GUILayout.Button("Restart"))
        {
            controller.RestartAudio();
        }
    }
}