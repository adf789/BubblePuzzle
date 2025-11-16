using BubblePuzzle.Bubble;
using BubblePuzzle.Core;
using BubblePuzzle.Grid;
using BubblePuzzle.UI;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    [SerializeField] private BubbleGrid bubbleGrid;
    [SerializeField] private BubbleReadyPool bubbleReadyPool;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance?.UIManager.OpenUI<GameUI>(UIType.GameUI);
        GameManager.Instance?.SetBubbleGrid(bubbleGrid);
        GameManager.Instance?.SetupGame();

        bubbleReadyPool.Reload();
    }
}
