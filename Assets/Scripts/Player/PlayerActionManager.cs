﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionManager : MonoBehaviour
{
    // singleton
    public static PlayerActionManager Instance;

    // action event
    public delegate void OnPlayerAction(PlayerAction action);
    /// <summary>
    /// POURQUOI CEST STATIC WEH SA VA CREER DES BRICOLES SI JE CHANGE DE SCENE
    /// </summary>
    public static OnPlayerAction onPlayerAction;

    public bool debug = false;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // CENTRALISER LES ACTIONS !
        onPlayerAction += HandleOnPlayerAction;
    }

    private void HandleOnPlayerAction(PlayerAction action)
    {
        switch (action.type)
        {
            case PlayerAction.Type.DescribeExterior:
                Interior.DescribeExterior();
                break;
            case PlayerAction.Type.DisplayTimeOfDay:
                TimeManager.GetInstance().WriteTimeOfDay();
                break;
            case PlayerAction.Type.DescribeItem:
                InputInfo.GetCurrent.MainItem.Describe();
                break;
            case PlayerAction.Type.PointNorth:
                Coords.WriteDirectionToNorth();
                break;
            default:
                break;
        }
    }



    public void DisplayInputFeedback()
    {
        InputInfo inputInfo = InputInfo.GetCurrent;

            // check if ANYTHING has been recognized
        if (!inputInfo.HasItems() && !inputInfo.HasVerb())
        {
            if (debug)
            {
                Debug.LogError("no verb, no items");
            }

            PhraseKey.WritePhrase("input_nothingRecognized");
            return;
        }

            // check if verb has been recognized
        if (!inputInfo.HasVerb())
        {
            if (debug)
            {
                Debug.LogError("no verb");
            }

            PhraseKey.WritePhrase("input_noVerb");
            return;
        }

            // check if item has been recognized
        if (inputInfo.HasVerb() && !inputInfo.HasItems())
        {
            if (debug)
            {
                Debug.LogError("only verb, no item");
            }

            // get verb only action
            Item verbItem = Item.GetDataItem("verbe seul");
            inputInfo.AddItem(verbItem);
            inputInfo.FindCombination();

            // no verb, displaying thing
            if (inputInfo.combination == null)
            {
                PhraseKey.WritePhrase("input_noItem");
                return;
            }
        }

        // verb / item combinaison doesn't exist
        if (inputInfo.combination == null)
        {
            if (debug)
            {
                Debug.LogError("Fail : no combination between verb : " + inputInfo.verb.names[0] + " and item : " + inputInfo.MainItem.word.text);
            }

            PhraseKey.WritePhrase("input_noCombination");
            return;
        }

        if (debug)
        {
            Debug.LogError("no action");
        }
        // get cell content
        string[] lines = inputInfo.combination.content.Split('\n');

        // separate all actions
        foreach (var line in lines)
        {
            // parse action
            PlayerAction action = GetAction(line);

            if (action != null)
            {
                action.Call();
                onPlayerAction(action);

                if (breakActions)
                {
                    Debug.Log(" !!!!! ACTION SEARCH IS STOPPED DUE TO THING !!!! ");
                    breakActions = false;
                    break;
                }

            }
        }
    }

    public PlayerAction GetAction(string line)
    {
        PlayerAction.Type[] actionTypes = System.Enum.GetValues(typeof(PlayerAction.Type)) as PlayerAction.Type[];
        
            // check parameters
        bool hasParameters = line.Contains("(");
        string function_str = line;
        if (hasParameters)
        {
            function_str = line.Remove(line.IndexOf('('));
        }

            // get action type
        PlayerAction.Type actionType = System.Array.Find(actionTypes, x => function_str.ToLower() == x.ToString().ToLower());

        if (actionType == PlayerAction.Type.None)
        {
            Debug.LogError("Couldn't find action type : " + function_str);
            return null;
        }

            // create action 
        PlayerAction newAction = new PlayerAction();
        newAction.type = actionType;

            // check parameters
        if (hasParameters)
        {
            string parameters_str = line.Remove(0, actionType.ToString().Length);

            // remove parentheses
            parameters_str = parameters_str.Remove(0, 1);
            parameters_str = parameters_str.Remove(parameters_str.Length - 1);

            string[] stringSeparators = new string[] { ", " };
            string[] args = parameters_str.Split(stringSeparators, StringSplitOptions.None);

            foreach (var arg in args)
            {
                newAction.AddContent(arg);
            }
        }

        return newAction;
    }


    #region action breaking
    bool breakActions = false;

    public void BreakAction()
    {
        breakActions = true;
    }
    #endregion


}
