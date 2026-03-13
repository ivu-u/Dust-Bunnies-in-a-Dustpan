using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds extra debug keys to the player controls
/// </summary>
public class PlayerDEBUG : MonoBehaviour
{
    [SerializeField] private SceneFader sceneFader;


    public void NextScene() {
        sceneFader.FadeTo(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
