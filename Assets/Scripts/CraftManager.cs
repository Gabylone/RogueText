﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftManager : MonoBehaviour {

    public static CraftManager Instance;

    public CraftInfo[] craftInfos;

    public struct CraftInfo
    {
        public int itemRow;
        public int[] requiredItemRows;
        public int[] requiredItemAmounts;
        public int[] requiredToolItemRows;
    }

    private void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        //LoadCraftables();

        ActionManager.onAction += HandleOnAction;
    }

    private void HandleOnAction(Action action)
    {
        switch (action.type)
        {
            case Action.Type.Craft:
                Craft();
                break;
            case Action.Type.ReadRecipe:
                ReadRecipe();
                break;
            default:
                break;
        }
    }

    private void ReadRecipe()
    {
        List<int> itemRows = new List<int>();

        foreach (var craftInfo in craftInfos)
        {
            if (craftInfo.requiredToolItemRows != null)
            {
                Debug.Log("craft info : " + Item.items[craftInfo.itemRow].word.text + " is not empty");
                itemRows.Add(craftInfo.itemRow);
            }
        }

        CraftInfo targetCraftInfo;

        Debug.Log("item vaklue : "+ InputInfo.GetCurrent.MainItem.value);
        if (InputInfo.GetCurrent.MainItem.value < 0)
        {


            int i = itemRows[Random.Range(0, itemRows.Count)];
            targetCraftInfo = craftInfos[i];

            InputInfo.GetCurrent.MainItem.value = i;

                Debug.Log("craft info : " + Item.items[i].word.text + " choisi");

        }
        else
        {
            targetCraftInfo = craftInfos[InputInfo.GetCurrent.MainItem.value];
        }

        List<Item> requiredItems = new List<Item>();

        foreach (var requiredToolrow in targetCraftInfo.requiredToolItemRows)
            requiredItems.Add(Item.items[requiredToolrow]);

        foreach (var requiredItemRow in targetCraftInfo.requiredItemRows)
            requiredItems.Add(Item.items[requiredItemRow]);

        string text = Item.ItemListString(requiredItems, false, false);
        DisplayFeedback.Instance.Display("" +
            "Pour fabriquer " + Item.items[targetCraftInfo.itemRow].word.GetContent(Word.ContentType.JustWord,Word.Definition.Undefined, Word.Preposition.None , Word.Number.None) + "\n" +
        "Il vous faut " + text);
    }

    private void Craft()
    {
        CraftInfo craftInfo = craftInfos[InputInfo.GetCurrent.MainItem.index];
        if ( craftInfo.requiredToolItemRows == null)
        {
            //DisplayFeedback.Instance.Display("vous ne pouvez pas fabriquer de " + Action.last.primaryItem.word.GetDescription(Word.Def.Undefined,Word.Preposition.De));
            DisplayFeedback.Instance.Display("vous ne pouvez pas fabriquer de " + InputInfo.GetCurrent.MainItem.word.text);
        }
        else
        {
            List<Item> missingItems = new List<Item>();
            bool hasAllItems = true;

            foreach (var requiredToolrow in craftInfo.requiredToolItemRows)
            {
                if (Inventory.Instance.GetItem(Item.items[requiredToolrow].word.text) == null)
                {
                    hasAllItems = false;
                    missingItems.Add(Item.items[requiredToolrow]);
                }
            }

            foreach (var requiredItemRow in craftInfo.requiredItemRows)
            {
                if (Inventory.Instance.GetItem(Item.items[requiredItemRow].word.text) == null)
                {
                    hasAllItems = false;
                    missingItems.Add(Item.items[requiredItemRow]);
                }
            }

            if (hasAllItems)
            {
                for (int i = 0; i < craftInfo.requiredItemRows.Length; i++)
                {
                    for (int a = 0; a < craftInfo.requiredItemAmounts[i]; a++)
                    {
                        Inventory.Instance.RemoveItem(craftInfo.requiredItemRows[i]);
                    }
                }

                Inventory.Instance.AddItem(InputInfo.GetCurrent.MainItem);

                DisplayFeedback.Instance.Display("Vous avez fabriqué " + InputInfo.GetCurrent.MainItem.word.GetContent(Word.ContentType.ArticleAndWord, Word.Definition.Undefined, Word.Preposition.None, Word.Number.None));

            }
            else
            {
                DisplayFeedback.Instance.Display("Vous n'avez pas de quoi fabriquer de " + InputInfo.GetCurrent.MainItem.word.text);
            }
        }
        
    }

    private void LoadCraftables()
    {
        TextAsset textAsset = Resources.Load("Craft") as TextAsset;

        string[] rows = textAsset.text.Split('&');

        int itemIndex = 0;

        craftInfos = new CraftInfo[Item.items.Count];

        for (int rowIndex = 1; rowIndex < rows.Length - 1; rowIndex++)
        {
            string[] cells = rows[rowIndex].Split(';');

            CraftInfo newCraftInfo = new CraftInfo();

            newCraftInfo.itemRow = Item.items[itemIndex].index;

            if (cells[1].Length > 1)
            {
                string[] toolCellParts = cells[1].Split('\n');

                newCraftInfo.requiredToolItemRows = new int[toolCellParts.Length];
                int requiredToolIndex = 0;
                foreach (var cellPart in toolCellParts)
                {
                    string itemName = cellPart.Trim('"');

                    Item requiredToolItem = Item.items.Find(x => x.word.text == itemName);

                    if (requiredToolItem == null)
                    {
                        Debug.LogError("couldn't find tool for item " + Item.items[itemIndex].word.text + " / content : " + itemName);
                        break;
                    }

                    newCraftInfo.requiredToolItemRows[requiredToolIndex] = requiredToolItem.index;

                    ++requiredToolIndex;
                }


                string[] itemCellParts = cells[2].Split('\n');

                newCraftInfo.requiredItemRows = new int[itemCellParts.Length];
                newCraftInfo.requiredItemAmounts = new int[itemCellParts.Length];

                int requiredItemIndex = 0;
                foreach (var cellPart in itemCellParts)
                {
                    string itemName = cellPart.Trim('"');
                    int itemAmount = 1;

                    if (itemName.Contains(","))
                    {
                        string[] str = itemName.Split(',');

                        itemName = str[0];

                        itemAmount = int.Parse(str[1].Remove(0, 1));
                    }

                    Item requiredItem = Item.items.Find(x => x.word.text == itemName);

                    if (requiredItem == null)
                    {
                        Debug.LogError("couldn't find required item for item " + Item.items[itemIndex].word.text + " / content : " + itemName);
                        break;
                    }

                    newCraftInfo.requiredItemRows[requiredItemIndex] = requiredItem.index;
                    newCraftInfo.requiredItemAmounts[requiredItemIndex] = itemAmount;

                    ++requiredItemIndex;
                }

                craftInfos[itemIndex] = newCraftInfo;

            }

            itemIndex++;
        }
    }
}
