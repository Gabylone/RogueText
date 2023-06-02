using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;
public class TextManager
{
    static Item overrideItem = null;
    static List<Movable.Orientation> overrideOrientations = new List<Movable.Orientation>();
    // PARAMS
    public static List<PhraseKey> phraseKeys = new List<PhraseKey>();
    // override c'est vraiment pas bien, il faut trouver une fa�on de faire ("&le chien sage (surrounding tile)&")

    public static void SetOverrideOrientation(Movable.Orientation orientation)
    {
        overrideOrientations = new List<Movable.Orientation> { orientation };
    }
    public static void SetOverrideOrientation(List<Movable.Orientation> _orientations)
    {
        overrideOrientations = _orientations;
    }
    public static string GetPhrase(string key, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        return GetPhrase(key);
    }
    public static string GetPhrase(string key)
    {
        // get random if no phrase return the key
        string str = GetPhraseKey(key);
        
        // ITEM ( il faut le faire ici AUSSI, pour la compil du display description
        str = KeyWords.ReplaceKeyWords(str);

        // &a good dog& => a charred road
        // &some dogs (main)& => some seeds
        str = ExtractItemWords(str);
        return str;
    }
    private static string GetPhraseKey(string key)
    {
        PhraseKey phraseKey = phraseKeys.Find(x => x.key == key);
        if (phraseKey == null)
        {
            //Debug.LogError("phrase <color=red>" + key + "</color> does not exist, returning key");
            return key;
        }
        return phraseKey.values[Random.Range(0, phraseKey.values.Count)];
    }
    public static string ExtractItemWords(string text)
    {
        int safetyBreak = 0;
        if (text == null)
        {
            Debug.LogError("extract item words : le text est null ?");
        }
        // each "&le chien sage (itemcode)& iteration
        while (text.Contains("&"))
        {
            // bonjour je suis &le chien sage (main)& => &le chien sage (main)&
            string targetPart = IsolatePart(text);
            // "&le chien sage (main)& => le chien sage (main)
            string wordInfo = TrimPart(targetPart);
            // le chien sage (main) => main
            string itemInfo = GetKey(wordInfo);

            // getting the item
            Item targetItem = GetItemFromCode(itemInfo);

            if (targetItem == null)
            {
                Debug.LogError("target item is null " + itemInfo + " text : " + text);
            }

            // get word from item
            string word = targetItem.word.GetInfo(wordInfo);
            // replace target part with word, and continue
            text = text.Replace(targetPart, word);

            // safety break
            ++safetyBreak;
            if (safetyBreak >= 10)
            {
                Debug.LogError("item word detection reached safety break");
                break;
            }
        }
        return text;
        //
    }
    private static string GetKey(string _key)
    {
        // word code = le chien sage (main)
        // check if there tags
        if (!_key.Contains("("))
        {
            // no key specified, so returning the main
            // not a bug, just flemme

            //Debug.LogError("word code : " + wordCode + " doesn't contain item code");
            return "main";
        }
        int startIndex = _key.IndexOf('(');
        string tmpItemCode = _key.Remove(0, startIndex + 1);
        tmpItemCode = tmpItemCode.Remove(tmpItemCode.Length - 1);
        _key = _key.Remove(startIndex - 1);
        // assign
        return tmpItemCode;
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
    static string TrimPart(string str)
    {
        string wordCode = str.Remove(0, 1);
        wordCode = wordCode.Remove(wordCode.Length - 1);
        return wordCode;
    }
    static Item GetItemFromCode(string itemCode)
    {
        if ( overrideItem == null)
        {
            Debug.LogError("override item is null");
        }

        return overrideItem;
    }
    public static List<Movable.Orientation> GetOverrideOrientations()
    {
        return overrideOrientations;
    }
    #region write phrase
    public static void Write(string str, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        Write(str);
    }
    public static void Write(string str)
    {
        DisplayDescription.Instance.useAIForNextText = true;
        string text = GetPhrase(str);
        DisplayDescription.Instance.AddToDescription(text, true);
    }

    public static void Add(string str, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        Add(str);
    }
    public static void Add (string str)
    {
        DisplayDescription.Instance.useAIForNextText = true;
        string text = GetPhrase(str);
        DisplayDescription.Instance.AddToDescription(text, false);
    }

    // STRAIGHT FORWARD
    public static void WriteST(string str, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        WriteST(str);
    }
    public static void WriteST(string str)
    {
        DisplayDescription.Instance.useAIForNextText = false;
        string text = GetPhrase(str);
        DisplayDescription.Instance.AddToDescription(text, true);
    }

    public static void AddST(string str, Item _overrideItem)
    {
        overrideItem = _overrideItem;
        AddST(str);
    }
    public static void AddST(string str)
    {
        DisplayDescription.Instance.useAIForNextText = false;
        string text = GetPhrase(str);
        DisplayDescription.Instance.AddToDescription(text, false);
    }

    #endregion
    public static void Renew()
    {
        
    }
}