#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// ตรงกับสคริปต์ Readme ดั้งเดิมของ Unity Tutorial
// ถ้าโปรเจกต์คุณไม่มี Readme.cs ก็สามารถลบไฟล์นี้ทิ้งได้เช่นกัน
[CustomEditor(typeof(Readme))]
public class ReadmeEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // แสดง Inspector ปกติของ Readme (หรือจะใส่ GUI สวย ๆ เพิ่มก็ได้)
        DrawDefaultInspector();

        EditorGUILayout.Space();
        if (GUILayout.Button("Open Project Page"))
        {
            Application.OpenURL("https://example.com"); // จะแก้เป็นลิงก์โปรเจกต์จริง ๆ ก็ได้
        }
    }
}
#endif
