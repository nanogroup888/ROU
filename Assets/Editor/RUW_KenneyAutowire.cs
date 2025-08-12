#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class RUW_KenneyAutowire
{
    const string SkillsJson = "Assets/Resources/Data/Skills/skills.json";
    const string MonstersJson = "Assets/Resources/Data/Monsters/monsters.json";
    const string ItemsJson = "Assets/Resources/Data/Items/items.json";

<<<<<<< HEAD
    [MenuItem("ROLike Tools/Kenney/Autowire %#k")]
=======
    [MenuItem("Ragnarok Underground World/Kenney/Autowire %#k")]
>>>>>>> 8b2444b85c97f9eb6e9b77e045fbd2cb48deee6a
    public static void Run()
    {
        string root = "Assets/Art/Kenney";
        if (!AssetDatabase.IsValidFolder(root))
        {
            EditorUtility.DisplayDialog("Kenney Autowire", "ไม่พบโฟลเดอร์ " + root + "\\nโปรดแตกไฟล์ Kenney ลงในโฟลเดอร์นี้ก่อน", "OK");
            return;
        }

        var spriteGUIDs = AssetDatabase.FindAssets("t:Sprite", new[]{root});
        var spriteIndex = new Dictionary<string, string>();
        foreach (var guid in spriteGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (!spriteIndex.ContainsKey(name))
                spriteIndex[name] = path;
        }

        EnsureDir("Assets/Resources/Data/Monsters");
        EnsureDir("Assets/Resources/Data/Items");

        PatchSkillsJson(spriteIndex);
        CreateMonstersJsonIfMissing(spriteIndex);
        CreateItemsJsonIfMissing(spriteIndex);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Kenney Autowire", "เชื่อม Kenney assets เข้าระบบเรียบร้อย", "OK");
    }

    static void PatchSkillsJson(Dictionary<string,string> idx)
    {
        if (!File.Exists(SkillsJson)) return;
        var text = File.ReadAllText(SkillsJson);
        var root = JsonUtility.FromJson<SkillRoot>(text);
        if (root == null || root.skills == null) return;
        File.Copy(SkillsJson, SkillsJson + ".bak", true);

        foreach (var s in root.skills)
        {
            if (!string.IsNullOrEmpty(s.icon)) continue;
            var keys = new[]{ s.code, s.name.Replace(" ","_"), s.name.Replace(" ","") }.Select(k=>k.ToLowerInvariant());
            foreach (var k in keys)
            {
                if (idx.TryGetValue(k, out var path)) { s.icon = path; break; }
            }
            if (string.IsNullOrEmpty(s.icon))
            {
                if (idx.TryGetValue("sword", out var sw)) s.icon = sw;
                else if (idx.TryGetValue("bolt", out var bt)) s.icon = bt;
                else if (idx.TryGetValue("heart", out var ht)) s.icon = ht;
            }
        }
        File.WriteAllText(SkillsJson, JsonUtility.ToJson(root, true));
    }

    static void CreateMonstersJsonIfMissing(Dictionary<string,string> idx)
    {
        if (File.Exists(MonstersJson)) return;
        var list = new List<MonsterDef>();
        list.Add(new MonsterDef{ code="picky", name="Picky", level=3, hp=80, atk=12, sprite=Find(idx, new[]{"picky","bird","chicken"}) });
        list.Add(new MonsterDef{ code="pecopeco", name="Peco Peco", level=10, hp=220, atk=28, sprite=Find(idx, new[]{"peco","ostrich","bird"}) });
        list.Add(new MonsterDef{ code="mummy", name="Mummy", level=20, hp=480, atk=55, sprite=Find(idx, new[]{"mummy","undead","skeleton"}) });
        var root = new MonsterRoot{ monsters = list.ToArray() };
        Directory.CreateDirectory(Path.GetDirectoryName(MonstersJson));
        File.WriteAllText(MonstersJson, JsonUtility.ToJson(root, true));
    }

    static void CreateItemsJsonIfMissing(Dictionary<string,string> idx)
    {
        if (File.Exists(ItemsJson)) return;
        var list = new List<ItemDef>();
        list.Add(new ItemDef{ code="red_potion", name="Red Potion", type="consumable", sprite=Find(idx, new[]{"potion","flask","bottle","heart"}) });
        list.Add(new ItemDef{ code="knife", name="Knife", type="weapon", sprite=Find(idx, new[]{"knife","dagger","sword"}) });
        var root = new ItemRoot{ items = list.ToArray() };
        Directory.CreateDirectory(Path.GetDirectoryName(ItemsJson));
        File.WriteAllText(ItemsJson, JsonUtility.ToJson(root, true));
    }

    static string Find(Dictionary<string,string> idx, IEnumerable<string> keys)
    {
        foreach (var k in keys) { if (idx.TryGetValue(k.ToLowerInvariant(), out var p)) return p; }
        return idx.Values.FirstOrDefault() ?? "";
    }

    static void EnsureDir(string path) { if (!Directory.Exists(path)) Directory.CreateDirectory(path); }

    [System.Serializable] public class SkillRoot { public List<SkillDefJSON> skills; }
    [System.Serializable] public class SkillDefJSON { public string code,name,job; public int maxLv,sp; public float cooldown; public string type; public int power; public float duration; public float chance; public int amount; public string icon; }
    [System.Serializable] public class MonsterRoot { public MonsterDef[] monsters; }
    [System.Serializable] public class MonsterDef { public string code,name; public int level,hp,atk; public string sprite; }
    [System.Serializable] public class ItemRoot { public ItemDef[] items; }
    [System.Serializable] public class ItemDef { public string code,name,type; public string sprite; }
}
#endif