using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phrase
{
    public string key = "";
    public List<string> values = new List<string>();

    public static Player.Orientation orientation;
    public static List<Player.Orientation> orientations = new List<Player.Orientation>();

    static Item overrideItem = null;

    // PARAMS

    public static List<Phrase> phrases = new List<Phrase>();

    // override c'est vraiment pas bien, il faut trouver une façon de faire ("&le chien sage (surrounding tile)&")
    public static string GetPhrase(string key, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        return GetPhrase(key);
    }

    public static void SetOverrideItem(Item item)
    {
        overrideItem = item;
    }

    public static string GetPhrase(string key)
    {
        Phrase phrase = phrases.Find(x => x.key == key);

        if (phrase == null)
        {
            Debug.LogError("phrase <color=red>" + key + "</color> does not exist");
            return null;
        }

        // get random
        string str = phrase.values[Random.Range(0, phrase.values.Count)];

        // ITEM ( il faut le faire ici AUSSI, pour la compil du display description
        str = Replace(str);

        // ORIENTATIONS
        str = str.Replace("ORIENTATIONS", Coords.GetOrientationText(orientations));
        // 

        // ORIENTATION
        str = str.Replace("ORIENTATION", Coords.GetOrientationText(orientation));
        //

        return str;
    }

    public static string Replace(string text)
    {
        int safetyBreak = 0;

        // each "&le chien sage (itemcode)& iteration
        while (text.Contains("&"))
        {
            // bonjour je suis &le chien sage (main item)& => &le chien sage (main item)&
            string targetPart = IsolatePart(text);

            // "&le chien sage (main item)& => le chien sage (main item)
            string wordCode = TrimPart(targetPart);

            // get target item
            string itemCode = "";
            wordCode = GetItemCode(wordCode, out itemCode);
            Item targetItem = GetItemFromCode(itemCode);

            // get word from item
            string word = targetItem.word.GetContent(wordCode);

            // replace target part with word, and continu
            text = text.Replace(targetPart, word);
            
            // safety break
            ++safetyBreak;

            if ( safetyBreak >= 10)
            {
                Debug.LogError("item word detection reached safety break");
                break;
            }

        }

        return text;
        //
    }

    private static string GetItemCode(string wordCode, out string itemCode)
    {
        // word code = le chien sage (main item)

        // check if there tags
        if (!wordCode.Contains("("))
        {
            Debug.LogError("word code : " + wordCode + " doesn't contain item code");
            itemCode = "main item";
            return wordCode;
        }

        int startIndex = wordCode.IndexOf('(');

        string tmpItemCode = wordCode.Remove(0, startIndex+1);
        tmpItemCode = tmpItemCode.Remove(tmpItemCode.Length - 1);

        wordCode = wordCode.Remove(startIndex - 1);

        // assign
        itemCode = tmpItemCode;
        return wordCode;
    }

    // isolate "&" du texte
    static string IsolatePart(string str)
    {
        // remove &s
        int startIndex = str.IndexOf('&');

        string targetPart = str.Remove(0, startIndex);
        int endIndex = targetPart.Remove(0, 1).IndexOf('&') + 2;

        if (endIndex < targetPart.Length)
        {
            targetPart = targetPart.Remove(endIndex);
        }

        return targetPart;
    }

    static string TrimPart( string str)
    {
        string wordCode = str.Remove(0, 1);
        wordCode = wordCode.Remove(wordCode.Length - 1);

        return wordCode;
    }

    static Item GetItemFromCode (string itemCode)
    {
        switch (itemCode)
        {
            case "main item":
                return InputInfo.GetCurrent.MainItem;
            case "second item":
                return InputInfo.GetCurrent.GetSecondItem;
            case "tile item":
                return Tile.GetCurrent.tileItem;
            case "override item":
                return overrideItem;
            default:
                Debug.LogError(itemCode + " doesnt go in any item category, returning input main item");
                return InputInfo.GetCurrent.MainItem;
        }
    }



    public static void Write( string str)
    {
        DisplayDescription.Instance.AddToDescription(str);
    }

    public static void Space()
    {
        Phrase.Write("\n__________\n");
    }

    public static void Renew()
    {
        DisplayDescription.Instance.Renew();
    }

}
