using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour {

    private float timeLastFall = 0; // time of the last "fall"
    public float fallDirection; // fall direction(-1 for blocks on the left and 1 for those on the right)

    public bool canRotate = true; // Allows rotating(square blocks should not rotate)
    public bool restrictRotation = false; // Restricts rotating(I, S and Z blocks should have only 2 orientations)

    public KeyCode up; // Key to move the block up.
    public KeyCode down; // Key to move the block down.
    public KeyCode fastFall; // Key to make the block "fall".
    public KeyCode rotate; // Key to rotate the block.

	// Use this for initialization
	void Start () {

        // If a block is spawned in an invalid position, then it's game over.
        if (!isValidPos())
        {
            Destroy(gameObject);
            Game.isGameOver = true;
        }

        // If it's spawned on the left side, set controls and falldirection for player1.
        if (transform.position.x < Game.midBorderPos)
        {
            fallDirection = 1;
            up = KeyCode.W;
            down = KeyCode.S;
            fastFall = KeyCode.D;
            rotate = KeyCode.A;
        }
        else // player2
        {
            fallDirection = -1;
            up = KeyCode.UpArrow;
            down = KeyCode.DownArrow;
            fastFall = KeyCode.LeftArrow;
            rotate = KeyCode.RightArrow;
        }
    }
	
	// Update is called once per frame
	void Update () {

        // Look for input updates and act accordingly.
        CheckUserInput();
	}

    // Handles user input and acts upon the block.
    // Only one block per user will be active at a given time.
    private void CheckUserInput ()
    {
        if (Input.GetKeyDown(up)) {
            // try changing the position
            transform.position += new Vector3(0,  1, 0);

            if (isValidPos())
                // valid position: commit changes
                updateBlock();
            else
                // invalid position: roll back
                transform.position -= new Vector3(0, 1, 0);
        }
        else if (Input.GetKeyDown(down)) {
            transform.position += new Vector3(0, -1, 0);

            if (isValidPos())
                updateBlock();
            else
                transform.position -= new Vector3(0, -1, 0);
        }
        else if (Input.GetKeyDown(fastFall) || (Time.time - timeLastFall > Game.gameSpeed)) {
            // second if condition ^ handles automatic falling

            // try a new position
            transform.position += new Vector3(fallDirection, 0, 0);

            if (isValidPos())
                // commit the new position
                updateBlock();
            else // not a valid position: a new block will spawn and the old one will be stuck
            {
                // revert change to position
                transform.position -= new Vector3(fallDirection, 0, 0);

                Game.DeleteFullCols();

                // spawn next block
                BlockSpawner spawner = Game.GetSpawner(transform.position.x).GetComponent<BlockSpawner>();
                spawner.spawnNextBlock();

                // disable the script
                enabled = false;
            }
            timeLastFall = Time.time;
        }
        else if (Input.GetKeyDown(rotate) && canRotate) {
            if (restrictRotation)
                // I, S and Z Blocks
            {
                if (transform.rotation.eulerAngles.z < 90)
                {
                    transform.Rotate(0, 0, 90);
                    if (isValidPos())
                        updateBlock();
                    else
                        transform.Rotate(0, 0, -90);
                }
                else
                {
                    transform.Rotate(0, 0, -90);
                    if (isValidPos())
                        updateBlock();
                    else
                        transform.Rotate(0, 0, 90);
                }
            }
            else
            // L, J, T blocks
            {
                transform.Rotate(0, 0, 90);
                if (isValidPos())
                    updateBlock();
                else
                    transform.Rotate(0, 0, -90);
            }    
        }
    }

    // Checks if the whole block is in a valid position,
    // calling Game.IsInBounds() for each tile inside the block.
    private bool isValidPos ()
    {
        foreach (Transform tile in transform)
        {
            Vector2 v = new Vector2(tile.position.x, tile.position.y);

            if (!Game.IsInBounds(v))
            {
                return false;
            }

            // check if it collides with any other block
            if (Game.grid[Mathf.RoundToInt(v.x) - Game.hOffset, Mathf.RoundToInt(v.y) - Game.vOffset] != null &&
                Game.grid[Mathf.RoundToInt(v.x) - Game.hOffset, Mathf.RoundToInt(v.y) - Game.vOffset].parent != transform)
                return false;
        }

        return true;
    }

    private void updateBlock()
    {
        // delete old tiles from this block from grid
        for (int x = -Game.width; x < Game.width + 1; ++x)
            for (int y = Game.vOffset + 1; y < Game.vOffset + Game.height; ++y)
                if (Game.grid[x - Game.hOffset, y - Game.vOffset] != null)
                    if (Game.grid[x - Game.hOffset, y - Game.vOffset].parent == transform)
                        Game.grid[x - Game.hOffset, y - Game.vOffset] = null;

        // add the tiles after transformations to the grid
        foreach (Transform tile in transform)
        {
            Vector2 v = new Vector2(tile.position.x, tile.position.y);

            Game.grid[Mathf.RoundToInt(v.x) - Game.hOffset, Mathf.RoundToInt(v.y) - Game.vOffset] = tile;
        }
    }
}
