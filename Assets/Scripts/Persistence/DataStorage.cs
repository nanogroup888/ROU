using Logger = ROLikeMMO.Util.Logger;
using System.IO;
using UnityEngine;
using ROLikeMMO.Util;

namespace ROLikeMMO.Persistence
{
    public static class DataStorage
    {
        static string Root => Application.persistentDataPath + "/ROLikeMMO";
        static string PlayerPath(string accountId) => $"{Root}/players/{accountId}.json";
        static string WorldPath() => $"{Root}/world.json";

        public static void SavePlayer(PlayerSave data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PlayerPath(data.accountId)));
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(PlayerPath(data.accountId), json);
            Logger.Log($"Saved player {data.accountId}");
        }
        public static PlayerSave LoadPlayer(string accountId)
        {
            var path = PlayerPath(accountId);
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerSave>(json);
        }

        public static void SaveWorld(WorldSave data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(WorldPath()));
            File.WriteAllText(WorldPath(), JsonUtility.ToJson(data, true));
        }
        public static WorldSave LoadWorld()
        {
            var path = WorldPath();
            if (!File.Exists(path)) return new WorldSave();
            return JsonUtility.FromJson<WorldSave>(File.ReadAllText(path));
        }
    }
}
