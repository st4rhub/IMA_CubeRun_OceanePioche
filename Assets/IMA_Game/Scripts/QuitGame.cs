// CMS26_IMA_Game · a Quit button. Button → On Click () → QuitGame.Quit.
// In a built game it closes the game. In the editor it stops Play Mode.
using UnityEngine;

[AddComponentMenu("IMA/Quit Game")]
public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
