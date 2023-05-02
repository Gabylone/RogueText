﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Story story;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        AdjectiveLoader.Instance.Load();

        PhraseLoader.Instance.Load();

        VerbLoader.Instance.Load();
        ItemLoader.Instance.Load();

        ItemAppearInfoLoader.Instance.Load();
        ItemSocketLoader.Instance.Load();
        TileSocketLoader.Instance.Load();

        MapTexture.Instance.CreateMapFromTexture();

        Player.Instance.Init();
    }


    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Interior interior = Interior.GetCurrent;
            interior.Genererate();

            TileSet.SetCurrent(interior.tileSet);

            MapTexture.Instance.UpdateInteriorMap();
        }
    }*/
}
