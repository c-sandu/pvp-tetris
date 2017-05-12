using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour {

    public GameObject[] availableBlocks; // array of all types of blocks
    public GameObject nextBlock; // reference used to instantiate the next block
    public GameObject nextBlock_HUD;

	// Use this for initialization
	void Start () {
        pickNextBlock();
        spawnNextBlock();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // Randomly selects the next block 
    private void pickNextBlock()
    {
        int i = Random.Range(0, availableBlocks.Length);
;
        nextBlock = availableBlocks[i];

        if (nextBlock_HUD != null)
            Destroy(nextBlock_HUD);

        Vector3 offset = new Vector3(5, 8);
        if (transform.position.x > 0)
            offset.x = -offset.x;

        nextBlock_HUD = Instantiate(nextBlock, transform.position + offset, Quaternion.identity);
        nextBlock_HUD.GetComponent<BlockController>().enabled = false;
    }

    // Spawns the previously selected block.
    public void spawnNextBlock()
    {
        // Offset is used to prevent unwanted collisions with the spawner itself.
        Vector3 offset = new Vector3(5, 0);

        if (transform.position.x > 0)
            offset = -offset;

        if (!Game.isGameOver)
        {
            Instantiate(nextBlock, transform.position + offset, Quaternion.identity);
            pickNextBlock();
        }
    }
}
