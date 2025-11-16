using UnityEngine;

namespace BubblePuzzle.UI
{
    public class IntroUI : MonoBehaviour
    {
        private System.Action onEventLoadScene = null;

        public void SetEventLoadScene(System.Action onEvent)
        {
            onEventLoadScene = onEvent;
        }

        public void OnClickStartGame()
        {
            onEventLoadScene?.Invoke();

            UnityEngine.SceneManagement.SceneManager.LoadScene($"Scenes/MainScene");
        }
    }
}
