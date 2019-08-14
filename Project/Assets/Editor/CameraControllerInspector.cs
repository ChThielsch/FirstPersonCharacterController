using UnityEngine;
using UnityEditor;

//Script by Christian Thielsch 2019
[CustomEditor(typeof(CameraController))]
public class CameraCotrollerInspector : Editor
{
    float minX = 90;
    float maxX = 90;

    float minY = -180;
    float maxY = 180;

    SerializedProperty m_freezeCamera;
    SerializedProperty m_invertMouseX;
    SerializedProperty m_invertMouseY;
    SerializedProperty m_sensitivityX;
    SerializedProperty m_sensitivityY;
    SerializedProperty m_minimumX;
    SerializedProperty m_maximumX;
    SerializedProperty m_clampY;
    SerializedProperty m_minimumY;
    SerializedProperty m_maximumY;
    SerializedProperty m_mouseDelay;
    SerializedProperty m_headBob;
    SerializedProperty m_headBobRage;
    SerializedProperty m_headBobSpeed;
    SerializedProperty m_headBobTransitionSpeed;

    public override void OnInspectorGUI()
    {
        minX = m_minimumX.floatValue;
        maxX = m_maximumX.floatValue;

        minY = m_minimumX.floatValue;
        maxY = m_maximumX.floatValue;

        EditorGUILayout.PropertyField(m_freezeCamera, new GUIContent("Freeze Camera", "Freezes the camera movement"));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_invertMouseX, new GUIContent("Inverse MouseX", "Inverts the mouseX input value"));
        EditorGUILayout.PropertyField(m_invertMouseY, new GUIContent("Inverse MouseY", "Inverts the mouseY input value"));

        EditorGUILayout.Space();
        EditorGUILayout.Slider(m_sensitivityX, 1, 100, new GUIContent("Sensitivity X", "The ammount of sensitivity of the cameras X axis"));
        EditorGUILayout.Slider(m_sensitivityY, 1, 100, new GUIContent("Sensitivity Y", "The ammount of sensitivity of the cameras Y axis"));
        EditorGUILayout.Slider(m_mouseDelay,0,50, new GUIContent("Mouse Delay", "The ammount of inputs nessesary for the camera to move"));

        EditorGUILayout.Space();
        EditorGUILayout.MinMaxSlider("Clamp X (" + m_minimumX.floatValue + ", " + m_maximumX.floatValue + ")", ref minX, ref maxX, -90, 90);
        m_minimumX.floatValue = (int)minX;
        m_maximumX.floatValue = (int)maxX;

        EditorGUILayout.PropertyField(m_clampY, new GUIContent("Clamp Y", "Enables to clamp the Y camera rotation"));
        if (m_clampY.boolValue)
        {
            EditorGUILayout.MinMaxSlider("Clamp Y (" + m_minimumY.floatValue + ", " + m_maximumY.floatValue + ")", ref minY, ref maxY, -180, 180);
            m_minimumY.floatValue = (int)minY;
            m_maximumY.floatValue = (int)maxY;
        }

        EditorGUILayout.PropertyField(m_headBob);
        if (m_headBob.boolValue)
        {
            EditorGUILayout.Slider(m_headBobRage,0,1, new GUIContent("Headbob Range","The distance in which the players head bobs while walking"));
            EditorGUILayout.PropertyField(m_headBobSpeed, new GUIContent("Headbob Speed","the ammount of seconds in which one full headbob happens"));
            EditorGUILayout.PropertyField(m_headBobTransitionSpeed, new GUIContent("Head Bob Transition Speed", "The ammount of time it takes to transition between the current head position and the default position"));
        }


        serializedObject.ApplyModifiedProperties();
    }


    private void OnEnable()
    {
        m_freezeCamera = serializedObject.FindProperty("m_freezeCamera");
        m_invertMouseX = serializedObject.FindProperty("m_invertMouseX");
        m_invertMouseY = serializedObject.FindProperty("m_invertMouseY");
        m_sensitivityX = serializedObject.FindProperty("m_sensitivityX");
        m_sensitivityY = serializedObject.FindProperty("m_sensitivityY");
        m_minimumX = serializedObject.FindProperty("m_minimumX");
        m_maximumX = serializedObject.FindProperty("m_maximumX");
        m_clampY = serializedObject.FindProperty("m_clampY");
        m_minimumY = serializedObject.FindProperty("m_minimumY");
        m_maximumY = serializedObject.FindProperty("m_maximumY");
        m_mouseDelay = serializedObject.FindProperty("m_mouseDelay");
        m_headBob = serializedObject.FindProperty("m_headBob");
        m_headBobRage = serializedObject.FindProperty("m_headBobRage");
        m_headBobSpeed = serializedObject.FindProperty("m_headBobSpeed");
        m_headBobTransitionSpeed = serializedObject.FindProperty("m_headBobTransitionSpeed");
    }
}
