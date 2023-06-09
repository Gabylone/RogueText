using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyWords
{
    public enum KeyWord {
        ITEM_DESCRIPTION,
        ITEM_VERBS,
        ITEM_PROPERTIES,
        VERB_NAME,
        VERB_QUESTION,
        VERB_PREPOSITION,
        TILE_CURRENT_DESCRIPTION,
        TIME_OF_DAY,
        SOCKET_POS,

        TARGET_ORIENTATION
    }

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
            case KeyWord.TIME_OF_DAY:
                return TimeManager.Instance.GetTimeOfDayDescription();
            case KeyWord.TARGET_ORIENTATION:
                return Coords.GetOrientationText(TextManager.GetOverrideOrientations());
            case KeyWord.ITEM_DESCRIPTION:
                
                return "";
            case KeyWord.VERB_NAME:

                if ( Verb.GetCurrent == null)
                {
                    return "ERROR(NULLVERB)";
                }

                if (Verb.GetCurrent == null)
                {
                    return "ERROR(NULLVERB)";
                }
                return Verb.GetCurrent.GetName;
            case KeyWord.VERB_QUESTION:
                if (Verb.GetCurrent == null)
                {
                    return "ERROR(NULLVERB)";
                }
                return Verb.GetCurrent.question;
            case KeyWord.VERB_PREPOSITION:
                
                return Verb.GetCurrent.GetPreposition;

            default:
                Debug.LogError("no text for KEY WORD " + keyWord.ToString());
                return keyWord.ToString();
        }
    }
}
