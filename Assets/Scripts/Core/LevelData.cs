using UnityEngine;
using BubblePuzzle.Bubble;
using BubblePuzzle.Core;

namespace BubblePuzzle.Core
{
    /// <summary>
    /// Level data structure
    /// </summary>
    [CreateAssetMenu(fileName = "Level", menuName = "BubblePuzzle/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public int LevelNumber = 1;
        public string LevelName = "Level 1";

        [Header("Grid Setup")]
        public int Rows = 5;
        public int ColumnsPerRow = 7;

        [Header("Bubble Colors")]
        public BubbleType[] AvailableColors = new BubbleType[]
        {
            BubbleType.Red,
            BubbleType.Blue,
            BubbleType.Green
        };

        [Header("Win Condition")]
        public int TargetScore = 1000;
        public int MaxShots = 30;

        [Header("Initial Pattern")]
        [Tooltip("Leave empty for random generation")]
        public LevelPattern InitialPattern;

        /// <summary>
        /// Get random bubble type from available colors
        /// </summary>
        public BubbleType GetRandomBubbleType()
        {
            if (AvailableColors.Length == 0)
                return BubbleType.Red;

            int index = Random.Range(0, AvailableColors.Length);
            return AvailableColors[index];
        }
    }

    /// <summary>
    /// Level pattern definition
    /// </summary>
    [System.Serializable]
    public class LevelPattern
    {
        [Tooltip("Each string represents a row. Use R=Red, B=Blue, G=Green, Y=Yellow, P=Purple, -=Empty")]
        public string[] rows;

        /// <summary>
        /// Parse character to bubble type
        /// </summary>
        public static BubbleType? ParseBubbleType(char c)
        {
            return c switch
            {
                'R' or 'r' => BubbleType.Red,
                'B' or 'b' => BubbleType.Blue,
                'G' or 'g' => BubbleType.Green,
                'Y' or 'y' => BubbleType.Yellow,
                'P' or 'p' => BubbleType.Purple,
                '-' or ' ' => null,
                _ => null
            };
        }
    }
}
