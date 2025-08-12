using UnityEditor;
using UnityEngine;

public class AnimatorBuilderWindow : EditorWindow
{
    [MenuItem("ROLike Tools/Build Animator")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorBuilderWindow>("Animator Builder");
    }

    void OnGUI()
    {
        GUILayout.Label("Animator Builder Tool", EditorStyles.boldLabel);
        GUILayout.Label("This will help you auto-create an Animator Controller with Idle, Walk, Attack, Die states.");
        if (GUILayout.Button("Create Example Animator"))
        {
            CreateAnimator();
        }
    }

    void CreateAnimator()
    {
        var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/SlimeAC.controller");
        Debug.Log("Animator created at Assets/SlimeAC.controller");
    }
}
