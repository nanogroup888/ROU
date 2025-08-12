using Mirror;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace ROLikeMMO.Gameplay
{
    /// Networked Health (server-authoritative)
    /// - SyncVar: current, max (with hooks)
    /// - Events: onChanged/onDied (UnityEvent) + OnChanged/OnDied (C#)
    /// - API: ServerTakeDamage/ServerHeal/SetMax
    [DisallowMultipleComponent]
    public class Health : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnHpSync))]   public int current = 1;
        [SyncVar(hook = nameof(OnMaxSync))]  public int max     = 30;

        public UnityEvent<int,int> onChanged; // (current, max)
        public UnityEvent onDied;

        public event Action<int,int> OnChanged;
        public event Action OnDied;

        public bool IsAlive => current > 0;

        public override void OnStartServer()
        {
            if (max <= 0) max = 1;
            if (current <= 0 || current > max) current = max;
        }

        #region Server API
        [Server]
        public void ServerTakeDamage(int dmg, NetworkIdentity attacker = null)
        {
            if (dmg <= 0 || current <= 0) return;
            current = Mathf.Max(0, current - dmg);
            if (current == 0)
            {
                onDied?.Invoke();
                OnDied?.Invoke();
                Invoke(nameof(ServerRespawn), 3f);
            }
        }

        [Server]
        public void ServerHeal(int amount)
        {
            if (amount <= 0 || current <= 0) return;
            current = Mathf.Min(max, current + amount);
        }

        [Server] public void SetMax(int newMax) => SetMax(newMax, true);

        [Server]
        public void SetMax(int newMax, bool refill)
        {
            max = Mathf.Max(1, newMax);
            current = refill ? max : Mathf.Min(current, max);
            OnHpSync(current, current);
            OnMaxSync(max, max);
        }

        [Server] void ServerRespawn() { current = max; }
        #endregion

        #region SyncVar hooks
        void OnHpSync(int oldValue, int newValue)
        {
            onChanged?.Invoke(newValue, max);
            OnChanged?.Invoke(newValue, max);
        }

        void OnMaxSync(int oldValue, int newValue)
        {
            onChanged?.Invoke(current, newValue);
            OnChanged?.Invoke(current, newValue);
        }
        #endregion

        [Command] public void CmdRequestSetMax(int newMax, bool refill) => SetMax(newMax, refill);
    }
}
