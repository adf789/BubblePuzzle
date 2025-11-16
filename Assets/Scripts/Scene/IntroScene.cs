using BubblePuzzle.Core;
using BubblePuzzle.UI;
using UnityEngine;

public class IntroScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var introUI = GameManager.Instance.UIManager.OpenUI<IntroUI>(UIType.IntroUI);

        introUI.SetEventLoadScene(GameManager.Instance.UIManager.CloseAllUI);
    }
}
