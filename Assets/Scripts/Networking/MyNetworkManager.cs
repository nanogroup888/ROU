using Logger = ROLikeMMO.Util.Logger;
using Mirror;
using UnityEngine;
using ROLikeMMO.Util;
using ROLikeMMO.Gameplay;
using ROLikeMMO.Persistence;

namespace ROLikeMMO.Networking
{
    public class MyNetworkManager : NetworkManager
    {
        public static MyNetworkManager Instance { get; private set; }

        [Header("Scenes")]
        public string loginSceneName = "Login";
        public string defaultFieldScene = "Field_Pronto";
        public string lasvegusScene = "City_Lasvegus";

        public override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // ใช้พฤติกรรมพื้นฐานของ Mirror: สปอว์น Player จาก PlayerPrefab ให้คอนเนกชันนี้
            base.OnServerAddPlayer(conn);
            Logger.Log("Client connected. Spawned player placeholder, waiting for CmdLogin to configure.");
        }

        // เมื่อเซิร์ฟเวอร์เปลี่ยนซีน (ทุกคนจะเปลี่ยนตาม Mirror)
        public override void OnServerSceneChanged(string sceneName)
        {
            // หาตำแหน่ง StartPosition (ถ้ามีในซีน)
            var start = GetStartPosition();
            var pos = start != null ? start.position : Vector3.zero;

            foreach (var kvp in NetworkServer.connections)
            {
                var conn = kvp.Value;
                if (conn?.identity == null) continue;

                var pc = conn.identity.GetComponent<PlayerCharacter>();
                if (pc == null) continue;

                // บันทึกข้อมูลแผนที่ล่าสุด และเทเลพอร์ตฝั่งคลายเอนต์
                if (pc.SaveData == null) pc.SaveData = new PlayerSave { accountId = pc.accountId, characterName = pc.characterName };
                pc.SaveData.mapId = sceneName;
                pc.SaveToStorage();
                pc.RpcTeleport(pos, sceneName);
            }
        }

        /// <summary>
        /// เรียกจากเซิร์ฟเวอร์เพื่อพาผู้เล่นไปแผนที่ใหม่ (ทั้งเซิร์ฟเวอร์จะเปลี่ยนซีนตามข้อจำกัดของ Mirror)
        /// </summary>
        public void ServerChangeMapForPlayer(NetworkConnectionToClient conn, string sceneName, Vector3 startPos)
        {
            if (!NetworkServer.active) return;

            // เปลี่ยนซีนทั้งเซิร์ฟเวอร์
            ServerChangeScene(sceneName);

            // อัปเดตเซฟและตำแหน่งให้ผู้เล่นที่ร้องขอ (จะมีผลหลังซีนโหลดเสร็จ)
            if (conn != null && conn.identity != null)
            {
                var pc = conn.identity.GetComponent<PlayerCharacter>();
                if (pc != null)
                {
                    if (pc.SaveData == null) pc.SaveData = new PlayerSave { accountId = pc.accountId, characterName = pc.characterName };
                    pc.SaveData.mapId = sceneName;
                    pc.SaveToStorage();
                    pc.RpcTeleport(startPos, sceneName);
                }
            }
        }
    }
}
