using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

//Script by Christian Thielsch 2019
[CustomEditor(typeof(PlayerController))]
public class PlayerControllerInspector : Editor
{
    bool m_debugFold = false;
    bool m_advancedFold = false;
    bool m_showAllComponents = false;

    SerializedProperty m_debugView;
    SerializedProperty m_crouchColliderColor;
    SerializedProperty m_groundCheckRaycastColor;
    SerializedProperty m_groundNormalRaycastColor;
    SerializedProperty m_groundSlopeRaycastColor;
    SerializedProperty m_crouchSpherecastColor;

    //General
    SerializedProperty m_health;
    SerializedProperty m_stamina;
    SerializedProperty m_breath;

    //Movement
    SerializedProperty m_freezeInputs;
    SerializedProperty m_walkSpeed;
    SerializedProperty m_sprintSpeed;
    SerializedProperty m_crouchSpeed;
    SerializedProperty m_velocityChange;

    //Advanced Settings
    SerializedProperty m_playerHeight;
    SerializedProperty m_crouchHeight;
    SerializedProperty m_playerWidth;
    SerializedProperty m_playerMass;

    SerializedProperty m_staminaFactor;
    SerializedProperty m_crouchTime;
    SerializedProperty m_speedFactor;

    SerializedProperty m_climbMinHeight;
    SerializedProperty m_climbMaxDistance;
    SerializedProperty m_climbMaxHeight;
    SerializedProperty m_timeToClimb;

    //Other
    SerializedProperty m_canJump;
    SerializedProperty m_jumpHeight;
    SerializedProperty m_gravity;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.Space();

        //Debug
        EditorGUILayout.PropertyField(m_debugView, new GUIContent("Debug View", "Shows developer objects and data"));
        if (m_debugView.boolValue)
        {
            m_debugFold = EditorGUILayout.Foldout(m_debugFold, "Debug Options", true);
            if (m_debugFold)
            {
                EditorGUILayout.PropertyField(m_crouchColliderColor, new GUIContent("Crouch Collider Color", "The color of the drawn wired sphere that indicates the colliders height when crouching"));
                EditorGUILayout.PropertyField(m_groundCheckRaycastColor, new GUIContent("Ground Check Color", "The color of the RayCast that indicates the length of the ground check"));
                EditorGUILayout.PropertyField(m_groundNormalRaycastColor, new GUIContent("Ground Normal", "The color of the RayCast that indicates the direction of the grounds normal"));
                EditorGUILayout.PropertyField(m_groundSlopeRaycastColor, new GUIContent("Ground Slope Color", "The color of the RayCast that indicates the players velocity  on the current ground"));
                EditorGUILayout.PropertyField(m_crouchSpherecastColor, new GUIContent("Crouch Spherecast Color", "The color of the Spherecast that checks if the player is beneath something"));
            }
        }

        //General
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(m_health, new GUIContent("Health", "The maximum ammount of health the player got"));
        EditorGUILayout.PropertyField(m_stamina, new GUIContent("Stamina", "The maximum ammount of stamina the player got"));
        EditorGUILayout.Slider(m_breath, 0, 60, new GUIContent("Breath", "The ammount in seconds that the player can hold its breath"));

        //Movement
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(m_freezeInputs, new GUIContent("Freeze Inputs", "Freezes the players inputs"));
        EditorGUILayout.PropertyField(m_walkSpeed, new GUIContent("Walk Speed", "The normal movement speed of the player"));
        EditorGUILayout.PropertyField(m_sprintSpeed, new GUIContent("Sprint Speed", "The maximum sprinnting movement speed of the player"));
        EditorGUILayout.PropertyField(m_crouchSpeed, new GUIContent("Crouch Speed", "The crouching movement speed of the player"));
        EditorGUILayout.PropertyField(m_velocityChange, new GUIContent("Velocity Change", "The maximum ammount of speed in which the player can move"));

        //Advanced Settings
        EditorGUILayout.Space();
        m_advancedFold = EditorGUILayout.Foldout(m_advancedFold, "Advanced Options", true);
        if (m_advancedFold)
        {
            EditorGUILayout.PropertyField(m_playerHeight, new GUIContent("Player Height", "The height of the players collider while standing"));
            EditorGUILayout.PropertyField(m_crouchHeight, new GUIContent("Crouch Height", "The height of the players collider while crouching"));
            EditorGUILayout.PropertyField(m_playerWidth, new GUIContent("Player Width", "The radius of the players collider"));
            EditorGUILayout.PropertyField(m_playerMass, new GUIContent("Player Mass", "The mass of the players gameobject"));

            EditorGUILayout.Space();
            EditorGUILayout.Slider(m_staminaFactor, 1, 5, new GUIContent("Stamina Factor", "The factor in which the stamina recharges each settings"));
            EditorGUILayout.Slider(m_crouchTime, 0, 5, new GUIContent("Crouch Time", "The time in seconds it takes for the player to change height"));
            EditorGUILayout.Slider(m_speedFactor, 1, 10, new GUIContent("Speed Factor", "The factor in which the sprinting speed is devided in when stamina gets lower"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_climbMaxHeight,new GUIContent("Climb Max Height", "The maximum height of an object to be climbed on"));
            EditorGUILayout.PropertyField(m_climbMinHeight, new GUIContent("Climb Min Height", "The minimum height of an object to be climbed on"));
            EditorGUILayout.PropertyField(m_climbMaxDistance, new GUIContent("Climb Max Distance", "The maximum distance from the player of an object to be climbed on"));
            EditorGUILayout.PropertyField(m_timeToClimb, new GUIContent("Climb Speed", "The speed in which the player climbs"));
        }


        //Other
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Other", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(m_canJump, new GUIContent("Can Jump", "Enables the player to jump"));
        if (m_canJump.boolValue)
        {
            EditorGUILayout.PropertyField(m_jumpHeight, new GUIContent("Jump Height", "The ammount of jump height added to the Y velocity"));
        }
        EditorGUILayout.PropertyField(m_gravity, new GUIContent("Gravity", "The ammount gravity added to the Y velocity"));

        bool tmp = m_showAllComponents;
        m_showAllComponents = EditorGUILayout.Toggle("Show All Components", m_showAllComponents);
        EditorGUILayout.Space();

        //Hides and shows the players CapsuleCollider and Rigidbody so no artist can break them
        var castedTarget = (target as PlayerController);
        if (tmp != m_showAllComponents)
        {
            Editor editor;
            Component[] components = castedTarget.gameObject.GetComponents<Component>();

            if (!m_showAllComponents)
            {
                castedTarget.GetComponent<CapsuleCollider>().hideFlags = HideFlags.HideInInspector;
                castedTarget.GetComponent<Rigidbody>().hideFlags = HideFlags.HideInInspector;
            }
            else
            {
                castedTarget.GetComponent<CapsuleCollider>().hideFlags = HideFlags.None;
                castedTarget.GetComponent<Rigidbody>().hideFlags = HideFlags.None;
            }

            foreach (Component component in components)
            {
                editor = null;
                Editor.CreateCachedEditor(component, null, ref editor);
                editor.Repaint();
            }
        }

        //Saves internal variables
        EditorPrefs.SetBool("m_debugFold", m_debugFold);
        EditorPrefs.SetBool("m_advancedFold", m_advancedFold);
        EditorPrefs.SetBool("showAllComponents", m_showAllComponents);

        //Apply changes
        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        //Debug
        m_debugView = serializedObject.FindProperty("m_debugView");
        m_crouchColliderColor = serializedObject.FindProperty("m_crouchColliderColor");
        m_groundCheckRaycastColor = serializedObject.FindProperty("m_groundCheckRaycastColor");
        m_groundNormalRaycastColor = serializedObject.FindProperty("m_groundNormalRaycastColor");
        m_groundSlopeRaycastColor = serializedObject.FindProperty("m_groundSlopeRaycastColor");
        m_crouchSpherecastColor = serializedObject.FindProperty("m_crouchSpherecastColor");

        //General
        m_health = serializedObject.FindProperty("m_health");
        m_stamina = serializedObject.FindProperty("m_stamina");
        m_breath = serializedObject.FindProperty("m_breath");

        //Movement
        m_freezeInputs = serializedObject.FindProperty("m_freezeInputs");
        m_walkSpeed = serializedObject.FindProperty("m_walkSpeed");
        m_sprintSpeed = serializedObject.FindProperty("m_sprintSpeed");
        m_crouchSpeed = serializedObject.FindProperty("m_crouchSpeed");
        m_velocityChange = serializedObject.FindProperty("m_velocityChange");

        //Advanced Settings
        m_playerHeight = serializedObject.FindProperty("m_playerHeight");
        m_crouchHeight = serializedObject.FindProperty("m_crouchHeight");
        m_playerWidth = serializedObject.FindProperty("m_playerWidth");
        m_playerMass = serializedObject.FindProperty("m_playerMass");
        m_staminaFactor = serializedObject.FindProperty("m_staminaFactor");
        m_crouchTime = serializedObject.FindProperty("m_crouchTime");
        m_speedFactor = serializedObject.FindProperty("m_speedFactor");
        m_climbMaxHeight = serializedObject.FindProperty("m_climbMaxHeight");
        m_climbMinHeight = serializedObject.FindProperty("m_climbMinHeight");
        m_climbMaxDistance = serializedObject.FindProperty("m_climbMaxDistance");
        m_timeToClimb = serializedObject.FindProperty("m_timeToClimb");

        //Other
        m_canJump = serializedObject.FindProperty("m_canJump");
        m_jumpHeight = serializedObject.FindProperty("m_jumpHeight");
        m_gravity = serializedObject.FindProperty("m_gravity");

        //Gets all saved internal variables
        m_debugFold = EditorPrefs.GetBool("m_debugFold", false);
        m_advancedFold = EditorPrefs.GetBool("m_advancedFold", false);
        m_showAllComponents = EditorPrefs.GetBool("showAllComponents", false);
    }
}
