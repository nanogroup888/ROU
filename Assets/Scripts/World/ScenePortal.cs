using UnityEngine;
using UnityEngine.SceneManagement;
public class ScenePortal : MonoBehaviour
{
    public string targetScene;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!string.IsNullOrEmpty(targetScene))
            SceneManager.LoadScene(targetScene);
    }
}