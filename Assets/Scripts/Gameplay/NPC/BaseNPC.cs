using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class BaseNPC : NetworkBehaviour
    {
        [TextArea] public string greeting = "Hello, traveler.";
        // Hook this with UI on client
    }
}
