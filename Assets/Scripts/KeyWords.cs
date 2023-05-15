using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyWords
{
    public enum KeyWord {
        CONTAINER_LIST,
        ITEM_DESCRIPTION,
        ITEM_VERBS,
        ITEM_PROPERTIES,
        VERB_NAME,
        VERB_QUESTION,
        VERB_PREPOSITION,
        TILE_CURRENT_DESCRIPTION,
        TIME_OF_DAY,
        SOCKET_POS,
        SOCKET_ITEMS,

        TARGET_ORIENTATION
    }

    public static Socket socket;

    public static string ReplaceKeyWords(string str)
    {
        foreach (KeyWord keyWord in Enum.GetValues(typeof(KeyWord)))
        {
            if (str.Contains(keyWord.ToString()))
            {
                str = str.Replace(keyWord.ToString(), GetKeyWord(keyWord));
            }
        }

        return str;
    }

    public static string GetKeyWord(KeyWord keyWord)
    {
        switch (keyWord)
        {
            case KeyWord.CONTAINER_LIST:
                return Item.ItemListString(Item.OpenedItem.GetContainedItems, Item.ListSeparator.Commas, true);
            case KeyWord.TIME_OF_DAY:
                return TimeManager.GetInstance().GetTimeOfDayDescription();
            case KeyWord.TARGET_ORIENTATION:
                return Coords.GetOrientationText(TextManager.GetOverrideOrientations());
            case KeyWord.ITEM_DESCRIPTION:
                InputInfo.Instance.GetItem(0).WriteDescription();
                return "";
            case KeyWord.VERB_NAME:
                return InputInfo.Instance.verb.GetName;
            case KeyWord.VERB_QUESTION:
                return InputInfo.Instance.verb.question;
            case KeyWord.VERB_PREPOSITION:
                return InputInfo.Instance.verb.GetPreposition;
            case KeyWord.SOCKET_POS:
                return socket.GetPositionText();

            default:
                Debug.LogError("no text for KEY WORD " + keyWord.ToString());
                return keyWord.ToString();
        }
    }
}
