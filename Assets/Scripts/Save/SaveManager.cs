﻿using UnityEngine;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;

    GameData gameData;

    public GameData GameData => gameData;

    void Awake() {
        Instance = this;
    }

    void Start() {

        if (SaveTool.Instance.FileExists("PlayerInfo", "player info")) {

        }

        gameData = new GameData();

    }

    private void Update() {

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S)) {
            Debug.Log("saving...");
            Save();
        }

    }

    void Save() {

    }

    public void LoadGame() {


        /*for (int x = 0; x < WorldGeneration.Instance.mapScale; x++)
        {
            for (int y = 0; y < WorldGeneration.Instance.mapScale; y++)
            {
                string targetTileID = "tileset" + tileSetID + "(" + x + "/y" + y + ")";

                string tileID = PlayerPrefs.GetString(targetTileID, "null");

                if ( tileID == "null")
                {
                    
                }
                else
                {
                    Debug.Log("trouvé tile : x" + x + ";y" + y);

                }
            }
        }*/

    }

}

//[System.Serializable]
public class GameData {

}