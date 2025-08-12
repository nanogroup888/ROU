using Mirror;
using UnityEngine;
using ROLikeMMO.Util;
using ROLikeMMO.Gameplay;
using ROLikeMMO.Persistence;

namespace ROLikeMMO.Networking
{
    public class PlayerAuth : NetworkBehaviour
    {
        [SyncVar] public string accountId;
        [SyncVar] public string characterName;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!isLocalPlayer) return;
            // In real UI, collect from login form. For now, use device name/time
            string acc = SystemInfo.deviceUniqueIdentifier.Substring(0, 8);
            string nick = $"Hero_{Random.Range(1000, 9999)}";
            CmdLogin(acc, nick);
        }

        [Command]
        void CmdLogin(string account, string nick)
        {
            accountId = account;
            characterName = nick;

            var pc = connectionToClient.identity.GetComponent<PlayerCharacter>();
            if (pc == null) return;

            var save = DataStorage.LoadPlayer(account) ?? new PlayerSave
            {
                accountId = account,
                characterName = string.IsNullOrWhiteSpace(nick) ? account : nick,
                mapId = MyNetworkManager.singleton is MyNetworkManager mm ?
                        (string.IsNullOrWhiteSpace(mm.defaultFieldScene) ? "Field_Pronto" : mm.defaultFieldScene)
                        : "Field_Pronto"
            };

            pc.LoadFromSave(save);

            // If current scene is not the saved scene, change scene for everyone (simple approach for now)
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!string.IsNullOrWhiteSpace(save.mapId) && currentScene != save.mapId)
            {
                // Use NetworkManager to change the scene
                (MyNetworkManager.singleton as MyNetworkManager)?.ServerChangeScene(save.mapId);
            }
        }
    }
}
