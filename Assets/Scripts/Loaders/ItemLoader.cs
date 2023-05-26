﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Analytics;

public class ItemLoader : TextParser {

    // singleton
    public static ItemLoader Instance;

    int itemIndex = 0;

    public Item[] items_debug;

    private void Awake()
    {
        Instance = this;
    }

    public override void FinishLoading()
    {
        base.FinishLoading();

        // debug list to explore items
        items_debug = ItemManager.Instance.dataItems.ToArray();
        foreach (var item in items_debug)
        {
            item.debug_name = item.word.text;
        }
        //
    }

    public override void GetCell(int rowIndex, List<string> cells)
    {
        base.GetCell(rowIndex, cells);

        // name row
        if (rowIndex < 1)
        {
            return;
        }

        // skip empty
        if (cells.Count > 0 && string.IsNullOrEmpty(cells[0]))
        {
            return;
        }

        // * est pour passer les lignes vides,
        // car on ne peut pas supprimer dans lignes ( elle ont des liens dans les autres DATAs )
        // quand on rajouter un nouveau mot, il faut utiliser les * d'abord
        if ( cells[0][0] == '*')
        {
            // can't continue, parce que sinon ça décale tout donc juste on fait rien
        }

        // create new item
        Item newItem = new Item();

        string nameCell = cells[0];
        string[] synonyms = nameCell.Split('\n');

        // SYNONYMS HERE
        // new word
        for (int i = 0; i < synonyms.Length; i++)
        {
            // pour l'instant le mot peut être reconnu mais il va toujours chercher le premier
            // de la liste words
            Word newWord = new Word();
            newWord.SetText(synonyms[i]);

            // word
            newItem.words.Add(newWord);
            // pour synonym autre ligne aussi dans les genre
            newWord.UpdateNumber(cells[1]);

            if (cells[2].Length > 1)
            {
                // ainsi que les locations peut être
                newItem.word.SetLocationPrep(cells[2]);
            }
        }

        //newItem.index = rowIndex-1;
        newItem.dataIndex = itemIndex;
        // add to item list
        ItemManager.Instance.dataItems.Add(newItem);

        newItem.info.stackable = cells[3].Contains("stackable");

        if ( cells.Count > 4)
        {
            InitProperties(newItem, cells[4]);
        }

        ++itemIndex;


    }

    void InitProperties(Item item, string cell)
    {
        /// PROPERTIES
        string[] property_lines = cell.Split('\n');

        if (string.IsNullOrEmpty(property_lines[0]))
        {

        }
        else
        {
            foreach (var property_line in property_lines)
            {
                if (string.IsNullOrEmpty(property_line))
                {
                    // just an empty line, pour aérer la cellule
                    continue;
                }

                if (property_line.StartsWith('%'))
                {
                    string eventName = property_line;
                    Property.Event propertyEvent = new Property.Event();
                    propertyEvent.name = eventName.Remove(0, 1);
                    Property lastProp = item.properties[item.properties.Count - 1];
                    lastProp.AddEvent(propertyEvent);
                    continue;
                }

                if (property_line.StartsWith('#'))
                {
                    string _event = property_line;
                    Property lastProp = item.properties[item.properties.Count - 1];

                    Property.Event.Action newAction = new Property.Event.Action();
                    int contentIndex = _event.IndexOf('(');

                    string function = _event.Remove(contentIndex).Remove(0, 1);
                    newAction.function = function;

                    string content = _event.Remove(0, contentIndex + 1);
                    content = content.Remove(content.Length - 1);
                    newAction.content = content;

                    // it's an event, 
                    lastProp.events[lastProp.events.Count - 1].AddAction(newAction);
                    continue;
                }


                Property newProperty = new Property();

                List<string> parts = property_line.Split(" / ").ToList();
                newProperty.type = parts[0];
                if (parts.Count < 2)
                {
                    Debug.LogError(property_line + " is not well formated");
                }
                newProperty.name = parts[1];
                if (parts.Count > 2)
                {
                    newProperty.value = parts[2];
                }

                item.properties.Add(newProperty);

                //newItem.CreateProperty(property_line);
            }
        }
        //
    }
}