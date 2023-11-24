using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class DebugManager : MonoBehaviour {

    // TESTS //
    // 1) plates (10x, tester les piles)
    // 2) flashlight (flashlight + battery)
    // 3) gardening (graine)
    // 4) doors & keys
    // 5) boat (� voir)

    [Space]
    [Header("[TILE]")]
    public Tile TILE;
    public List<Item> adjacentTiles;
    public List<ItemGroup> debug_groups;


    [Space]
    [Header("[PARSER]")]
    public ItemParser previousParser;
    public ItemParser currentParser;

    [Space]
    [Header("[FUNCTION]")]
    public FunctionSequence currentFunctionList;

    [Space]
    [Header("[TEXT]")]
    public bool colorWords = true;

    [Space]
    [Header("[AVAILABLE ITEMS]")]
    public List<Item> availableItems;

    [Space]
    [Header("[ITEM EVENTS]")]
    public List<WorldAction> EVENTS = new List<WorldAction>();

    public List<ItemDescription.DescriptionGroup_Debug> debugDescriptions;

    [Space]
    [Header("[PLAYER]")]
    public Player PLAYER;

    private void Start() {
        currentParser = ItemParser.GetCurrent;
        previousParser = ItemParser.GetPrevious;

        availableItems = AvailableItems.currItems;

        PLAYER = Player.Instance;
    }
    private static DebugManager _instance;
    public static DebugManager Instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType<DebugManager>().GetComponent<DebugManager>();
            }

            return _instance;
        }
    }
}
