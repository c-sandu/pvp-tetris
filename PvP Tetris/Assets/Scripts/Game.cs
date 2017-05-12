using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;


public class Game : MonoBehaviour
{

    public static int width = 20; // width of one half of the playing area
    public static int height = 11; // height of the playing area

    // The array holding references to gameObjects in the playing area, defined below,
    // will be indexed starting from [0, 0], but the playing area itself is centered
    // in the origin. The following offsets are used to easily compute the proper
    // indices for the Grid.
    public static int vOffset = -7; // vertical offset of the playing area(which is centered in origin)
    public static int hOffset = -21; // horizontal offset of the playing area

    public static int midBorderPos = 0; // position of the middle border
    public static float gameSpeed = 0.75f;
    public static int pointsPerCol = 50; // points per column cleared

    public static GameObject midBorder; // reference to the midBorder Game object

    public static bool isGameOver = false;

    // UI elements:
    public Text textScoreP1;
    public Text textScoreP2;
    public Text textGameOver;
    public Text textReset;
    private static int scoreP1 = 0;
    private static int scoreP2 = 0;

    // Grid which holds the playing area for more efficient memory usage.
    // It's of Transform type because it's all I really use for my game objects.
    public static Transform[,] grid = new Transform[width * 2 + 6, height];

    // Use this for initialization.
    void Start()
    {
        // Obtain reference to midBorder Object.
        midBorder = GameObject.FindGameObjectWithTag("MidBorder");

        // Clear UI texts.
        textGameOver.text = "";
        textReset.text = "";
    }

    // Update is called once per frame.
    void Update()
    {
        UpdateUI();

        // "main loop" at the end of the game
        if (isGameOver)
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Reset all static variables.
                SceneManager.LoadScene("Game");
                isGameOver = false;
                textGameOver.text = "";
                textReset.text = "";
                gameSpeed = 0.75f;
                midBorderPos = 0;
                Destroy(midBorder);
                Instantiate(midBorder, new Vector3(0, 0), Quaternion.identity);
                scoreP1 = 0;
                scoreP2 = 0;
            }
    }

    // Updates the UI text elements.
    void UpdateUI()
    {
        textScoreP1.text = "Player1: " + scoreP1.ToString();
        textScoreP2.text = "Player2: " + scoreP2.ToString();

        if (isGameOver)
        {
            textGameOver.text = "Game Over!";
            textReset.text = "Press R to restart game";
        }
    }

    // Checks if given position is inside playing area.
    public static bool IsInBounds(Vector2 pos)
    {
        // vOffset would be the position of the bottom border
        // vOffset + height would be the position of the top border

        return (Mathf.RoundToInt(pos.x) != midBorderPos)
            && (Mathf.RoundToInt(pos.y) > vOffset)
            && (Mathf.RoundToInt(pos.y) < vOffset + height);
    }

    // Deletes the specified column.
    private static void DeleteCol(int x)
    {
        // move through each cell from bottom to top
        for (int y = vOffset + 1; y < vOffset + height; ++y)
        {
            // Destroy the tile.
            Destroy(grid[x - hOffset, y - vOffset].gameObject);

            // Clear the space inside the playing area.
            grid[x - hOffset, y - vOffset] = null;
        }
    }

    // Shift column to the right.
    // Overwrites the column to the right.
    private static void MoveColRight(int x)
    {
        for (int y = vOffset + 1; y < vOffset + height; ++y)
        {
            if (grid[x - hOffset, y - vOffset] != null)
            {
                // Copy the old reference to the tile.
                grid[x - hOffset + 1, y - vOffset] = grid[x - hOffset, y - vOffset];
                // Translate the tile to the right.
                grid[x - hOffset + 1, y - vOffset].position += new Vector3(1, 0, 0);

                // Delete old reference.
                grid[x - hOffset, y - vOffset] = null;
            }
        }
    }

    // Shift column to the left.
    // Overwrites the column to the left.
    private static void MoveColLeft(int x)
    {
        for (int y = vOffset + 1; y < vOffset + height; ++y)
        {
            if (grid[x - hOffset, y - vOffset] != null)
            {
                // Copy the old reference to the tile.
                grid[x - hOffset - 1, y - vOffset] = grid[x - hOffset, y - vOffset];
                // Translate the tile to the left.
                grid[x - hOffset - 1, y - vOffset].position += new Vector3(-1, 0, 0);

                // Delete old reference.
                grid[x - hOffset, y - vOffset] = null;
            }
        }
    }

    // Collapse columns in one half of the playing area.
    private static void MoveOuterCols(int semiPlane)
    {
        if (semiPlane < midBorderPos)
            for (int i = semiPlane; i >= -width; --i)
                MoveColRight(i);
        else
            for (int i = semiPlane; i <= width; ++i)
                MoveColLeft(i);
    }

    // Moves the midBorder one unit in the specified direction.
    // Also shifts every column in the same direction.
    // This function is called every time a full column is cleared.
    private static void MoveMidBorder(int direction)
    {
        if (direction > 0)
        {
            for (int x = width - 1; x > -width; x--)
            {
                MoveColRight(x);
            }
            midBorderPos++;
            midBorder.transform.Translate(new Vector3(1, 0));
        }
        else
        {
            for (int x = -width + 1; x < width; x++)
                MoveColLeft(x);
            midBorderPos--;
            midBorder.transform.Translate(new Vector3(-1, 0));
        }

        // This Function is called every time a full column is cleared,
        // so I may aswell speed up the game to increase difficulty.
        Game.gameSpeed = Mathf.Max(0.25f, Game.gameSpeed * 0.9f);
    }

    // Checks if the specified column is full.
    private static bool IsFullCol(int x)
    {
        if (x == midBorderPos)
            return false;

        for (int y = vOffset + 1; y < vOffset + height; ++y)
            if (grid[x - hOffset, y - vOffset] == null)
                return false;

        return true;
    }

    // Checks for full columns and deletes them, moving from the midpoint.
    // This function also updates the mid border, by calling MoveMidBorder().
    public static void DeleteFullCols()
    {
        // left side first
        for (int x = midBorderPos - 1; x > -width; x--)
            if (IsFullCol(x))
            {
                DeleteCol(x);
                MoveOuterCols(x - 1);
                MoveMidBorder(1);
                UpdateScore(-1);
                // move back one step
                x++;
            }

        // right side second
        for (int x = midBorderPos + 1; x < width; x++)
            if (IsFullCol(x))
            {
                DeleteCol(x);
                MoveOuterCols(x + 1);
                MoveMidBorder(-1);
                UpdateScore(1);
                // move back one step
                x--;
            }
    }

    // Utility function to obtain the corresponding spawner when a block gets stuck. 
    public static GameObject GetSpawner(float block_position)
    {
        // Get all objects tagged with "Spawner"
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner") as GameObject[];

        int leftIndex = 0; // will be the index of the left spawner
        int rightIndex = 1; // will be the index of the right spawner

        for (int i = 0; i < spawners.Length; ++i)
            if (spawners[i].transform.position.x < 0)
                leftIndex = i;
            else
                rightIndex = i;

        // Return the appropriate spawner.
        if (block_position < midBorderPos)
            return spawners[leftIndex];
        else
            return spawners[rightIndex];
    }

    // Function to update the score of a player.
    // Player will be identified by the sign of the argument.
    private static void UpdateScore(int player)
    {
        if (player < 0)
            scoreP1 += pointsPerCol;
        else
            scoreP2 += pointsPerCol;
    }
}
