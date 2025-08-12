#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.IO;

public class AnimatorQuickBuild : EditorWindow
{
    public string savePath = "Assets/Anim/SlimeAC.controller";
    public AnimationClip idle, walk, attack, hit, die;

    [MenuItem("ROLike Tools/Animator Quick Build")]
    public static void Open() => GetWindow<AnimatorQuickBuild>("Animator Quick Build");

    void OnGUI()
    {
        savePath = EditorGUILayout.TextField("Controller Path", savePath);
        idle = (AnimationClip)EditorGUILayout.ObjectField("Idle", idle, typeof(AnimationClip), false);
        walk = (AnimationClip)EditorGUILayout.ObjectField("Walk", walk, typeof(AnimationClip), false);
        attack = (AnimationClip)EditorGUILayout.ObjectField("Attack", attack, typeof(AnimationClip), false);
        hit = (AnimationClip)EditorGUILayout.ObjectField("Hit", hit, typeof(AnimationClip), false);
        die = (AnimationClip)EditorGUILayout.ObjectField("Die", die, typeof(AnimationClip), false);
        if (GUILayout.Button("Build Controller")) Build();
    }

    void Build()
    {
        var dir = Path.GetDirectoryName(savePath).Replace("\\","/");
        if (!AssetDatabase.IsValidFolder(dir))
        {
            var parts = dir.Split('/');
            for (int i=1;i<parts.Length;i++)
            {
                var parent = string.Join("/", parts, 0, i);
                var current = string.Join("/", parts, 0, i+1);
                if (!AssetDatabase.IsValidFolder(current))
                    AssetDatabase.CreateFolder(parent, parts[i]);
            }
        }

        var ac = new AnimatorController();
        AssetDatabase.CreateAsset(ac, savePath);

        ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ac.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        ac.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        ac.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        var sm = ac.layers[0].stateMachine;
        var sIdle = sm.AddState("Idle");  sIdle.motion  = idle;
        var sWalk = sm.AddState("Walk");  sWalk.motion  = walk;
        var sAtk  = sm.AddState("Attack"); sAtk.motion  = attack;
        var sHit  = sm.AddState("Hit");    sHit.motion  = hit;
        var sDie  = sm.AddState("Die");    sDie.motion  = die;
        sm.defaultState = sIdle;

        // Idle <-> Walk
        var t1 = sIdle.AddTransition(sWalk); t1.hasExitTime=false; t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        var t2 = sWalk.AddTransition(sIdle); t2.hasExitTime=false; t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Attack
        var ta1 = sIdle.AddTransition(sAtk); ta1.hasExitTime=false; ta1.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        var ta2 = sWalk.AddTransition(sAtk); ta2.hasExitTime=false; ta2.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        var taR = sAtk.AddTransition(sIdle); taR.hasExitTime=true; taR.exitTime=0.95f;

        // Hit
        var th1 = sIdle.AddTransition(sHit); th1.hasExitTime=false; th1.AddCondition(AnimatorConditionMode.If, 0, "Hit");
        var th2 = sWalk.AddTransition(sHit); th2.hasExitTime=false; th2.AddCondition(AnimatorConditionMode.If, 0, "Hit");
        var thR = sHit.AddTransition(sIdle); thR.hasExitTime=true; thR.exitTime=0.9f;

        // Die
        var td1 = sIdle.AddTransition(sDie); td1.hasExitTime=false; td1.AddCondition(AnimatorConditionMode.If, 0, "Die");
        var td2 = sWalk.AddTransition(sDie); td2.hasExitTime=false; td2.AddCondition(AnimatorConditionMode.If, 0, "Die");

        EditorUtility.SetDirty(ac);
        AssetDatabase.SaveAssets();
        Selection.activeObject = ac;
        Debug.Log($"[AnimatorQuickBuild] Created: {savePath}");
    }
}
#endif
