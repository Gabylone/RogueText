﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public static List<Item> items = new List<Item>();

    /// <summary>
    /// declaration
    /// </summary>
    public int index;
    public int weight = 0;
    public int value = 0;
    public bool usableAnytime = false;

    public string inputToFind = "str";

    public List<AppearInfo> appearInfos = new List<AppearInfo>();

    /// <summary>
    /// exemples :
    /// une carrote ou bouteille d'eau se trouverait souvent sur une table ou quoi
    /// une flaque se trouverait souvent par terre
    /// </summary>
    private Socket socket;

    /// <summary>
    /// exemples :
    /// dans une forêt : "derrière l'arbre", ou "dans un buisson"
    /// dans un sac : (donc autre objet) au fond du sac, etc...
    /// </summary>
    public List<Socket> sockets = new List<Socket>();

    public List<Item> containedItems;

    public Word word;
    public bool stackable = false;

    /// <summary>
    /// container
    /// </summary>
    public bool emptied = false;

    /// <summary>
    /// properties
    /// </summary>
    public List<Property> properties = new List<Property>();

    public Item()
    {

    }

    #region container
    public void GenerateItems()
    {
        if (containedItems != null)
        {
            return;
        }

        Debug.Log("first time open");
        containedItems = new List<Item>();

        foreach (var appearInfo in appearInfos)
        {
            for (int i = 0; i < appearInfo.amount; i++)
            {
                if (Random.value * 100f < appearInfo.rate)
                {
                    Item newItem = Item.CreateNewItem(appearInfo.GetItem());

                    Debug.Log("new item hash code : " + newItem.GetHashCode());

                    containedItems.Add(newItem);
                }
            }
        }

    }


    public void Open()
    {
        Item item = InputInfo.GetCurrent.MainItem;
        item.GenerateItems();

        Container.opened = true;
        Container.CurrentItem = item;

        Container.Describe();
    }

    public void Close()
    {
        // toujours dans la classe inventory.cs pour l'intsant
    }

    public string GetContainedItemsDescription()
    {
        string word_str = word.GetContent("le chien");

        if (containedItems.Count == 0)
        {
            return "Il n'y a rien dans " + word_str;
        }

        string str = "Dans " + word_str + " : vous voyez : \n" +
            "" + Item.ItemListString(containedItems, true, true);

        return str;
    }

    public void RemoveItem(Item item)
    {
        containedItems.Remove(item);
    }
    public bool SameTypeAs(Item otherItem)
    {
        return otherItem.index == index;
    }
    public bool ExactSameAs(Item otherItem)
    {
        return otherItem == this;
    }
    #endregion

    #region properties
    public void AddProperty(string name, string value)
    {
        Property newProperty = new Property();
        newProperty.name = name;
        newProperty.SetValue(value);
        AddProperty(newProperty);
    }

    public void AddProperty(Property property)
    {
        properties.Add(property);
    }

    public bool HasProperty(string name)
    {
        return properties.Find(x => x.name == name) != null;
    }

    public Property GetProperty(string name)
    {
        Property property = properties.Find(x => x.name == name);

        if (property == null)
        {
            Debug.LogError("property : " + name + " doesn't exist in item " + word.text);
            return null;
        }

        return property;

    }

    public class Property
    {
        public string name;
        private string value;
        public string param;
        public Item item;

        public void SetValue(string _value)
        {
            value = _value;
        }

        public int GetNumericValue()
        {
            int i = 0;

            if (int.TryParse(value, out i))
            {
                return i;
            }
            else
            {
                Debug.LogError("couldn't parse : " + value + " in property : " + name);
                return i;
            }
        }

        public string GetValue()
        {
            return value;
        }

        public string GetDebugText()
        {
            return "property of : " + item.word.text + " / " + name + " : " + value;
        }

        public void HandleOnNextHour()
        {
            switch (name)
            {
                case "grow":
                    Debug.Log("item : " + item.word.text + " is growing...");
                    Item newItem = Item.FindByName("carrote");
                    newItem = CreateNewItem(newItem);
                    item = newItem;
                    break;
                default:
                    Debug.LogError("timer property : " + name + " is dead end");
                    break;
            }
        }
    }
    #endregion

    #region appear info
    public class AppearInfo
    {
        public int itemIndex = 0;
        public int rate = 0;
        public int amount = 0;

        public Item GetItem()
        {
            return Item.items[itemIndex];
        }

        public bool CanAppear()
        {
            return false;
        }
    }

    public Socket GetSocket()
    {
        if (socket == null)
        {
            socket = Socket.GetRandomSocket(this);
        }

        return socket;
    }
    #endregion

    ///
    /// <summary>
    /// TOOLS
    /// </summary>
    ///

    #region tools
    public static Item CreateNewItem(string name)
    {
        Item item = FindByName(name);
        return CreateNewItem(item);
    }

    public static Item CreateNewItem(Item copy)
    {
        Item newItem = new Item();

        newItem.index = copy.index;
        newItem.weight = copy.weight;
        newItem.value = copy.value;
        newItem.usableAnytime = copy.usableAnytime;
        newItem.appearInfos = copy.appearInfos;
        newItem.socket = copy.socket;
        newItem.sockets = copy.sockets;
        newItem.word = copy.word;
        newItem.stackable = copy.stackable;
        newItem.properties = copy.properties;

        foreach (var _property in newItem.properties)
        {
            if (!string.IsNullOrEmpty(_property.param))
            {
                if (_property.param == "timed")
                {
                    Debug.LogError("timing item : " + newItem.word.text);

                    TimeManager.GetInstance().onNextHour += _property.HandleOnNextHour;
                }
                else
                {
                    // quand la case est juste un nombre

                    int chance = int.Parse(_property.param);
                    _property.SetValue(Random.value * 100 < chance ? "true" : "false");
                }
            }
        }

        return newItem;
    }
    #endregion

    #region remove
    public static void Remove(Item targetItem)
    {
        if (Container.opened)
        {
            if (Container.CurrentItem.containedItems.Contains(targetItem))
            {
                Container.CurrentItem.RemoveItem(targetItem);
            }

            Container.Describe();

            return;
        }

        if (Tile.current.items.Contains(targetItem))
        {
            Tile.current.RemoveItem(targetItem);

            DisplayDescription.Instance.UpdateDescription();
            return;
        }

        if (Inventory.Instance.items.Contains(targetItem))
        {
            Inventory.Instance.RemoveItem(targetItem);

            Inventory.Instance.Describe();
            return;
        }
    }
    #endregion

    #region search
    public static Item FindInWorld(string str)
    {
        Item item = null;

        // dans un container
        if (Container.opened)
        {
            Debug.Log("looking in container");

            item = Container.CurrentItem.containedItems.Find(x => x.word.Compare(str));
        }

        // chercher une premiere fois dans l'inventaire s'il est ouvert
        if (Inventory.Instance.opened)
        {
            item = FindInInventory(str);
            if (item != null)
            {
                return item;
            }
        }

        // is the item one of the surrounding tiles ?
        if (item == null)
        {
            item = FindInTile(str);
        }

        if (item == null)
        {
            item = FindUsableAnytime(str);
        }

        // et en dernier s'il est fermé
        if (item == null)
        {
            item = FindInInventory(str);
        }

        if (item == null)
        {

        }

        return item;
    }

    public static Item FindInTile(string str)
    {
        // is the item the exact same tile as the one we're in ?
        if (Tile.current.tileItem.word.Compare(str))
        {
            return Tile.current.tileItem;
        }

        // is the item one of the surrounding tiles ?
        foreach (var tileGroup in TileGroupDescription.tileGroups)
        {
            if (tileGroup.tile.tileItem.word.Compare(str))
            {
                return tileGroup.tile.tileItem;
            }
        }

        List<Item> items = Tile.current.items.FindAll(x => x.word.Compare(str));

        if (items.Count == 0)
        {
            items = Tile.current.items.FindAll(x => x.word.Compare(str));
        }

        /// ADJECTIVES ///

        /// chercher les adjectifs pour différencier les objets ( porte bleu, porte rouge )
        if (items.Count > 0)
        {

            foreach (var inputPart in InputInfo.parts)
            {
                foreach (var item in items)
                {
                    string adjSTR = item.word.GetAdjective.GetContent(item.word.genre, false);

                    if (adjSTR == inputPart)
                    {
                        return item;
                    }
                }
            }

            return items[0];

        }

        return null;
    }

    public static Item FindUsableAnytime(string str)
    {
        return items.Find(x => x.word.Compare(str) && x.usableAnytime);
    }

    public static Item FindInInventory(string str)
    {
        Item item = Inventory.Instance.items.Find(x => x.word.Compare(str));

        if (item == null)
            return null;

        return item;
    }

    public static Item FindByName(string _name)
    {
        _name = _name.ToLower();

        Item item = items.Find(x => x.word.text.ToLower() == _name);

        if (item == null)
        {
            // find plural
            item = items.Find(x => x.word.GetPlural() == _name);

            if (item != null)
            {
                //Debug.Log("found plural");
                InputInfo.GetCurrent.actionOnAll = true;
                return item;
            }
        }

        if (item == null)
        {
            Debug.LogError("couldn't find item : " + _name);
        }

        return item;
    }
    public static List<Item> FindAllByName(string str)
    {
        str = str.ToLower();

        return items.FindAll(x => x.word.text.StartsWith(str));
    }
    #endregion

    #region list
    public static string ItemListString(List<Item> _items, bool separate, bool displayWeight)
    {
        string text = "";

        int i = 0;

        foreach (var item in _items)
        {
            text += item.word.GetContent("chien");

            if (displayWeight)
            {
                text += " (w:" + (item.weight) + ")";
            }

            if (_items.Count > 1 && i < _items.Count - 1)
            {
                if (separate)
                {
                    text += "\n";
                }
                else
                {
                    if (_items.Count > 2)
                    {
                        if (i == _items.Count - 2)
                        {
                            text += " et ";
                        }
                        else
                        {
                            text += ", ";
                        }
                    }
                    else
                    {
                        text += " et ";
                    }
                }

            }

            ++i;
        }

        return text;
    }
    public static string ItemListString(List<ItemGroup> _itemSockets, bool separateWithLigns, bool displayWeight)
    {
        string text = "";

        int i = 0;

        foreach (var itemSocket in _itemSockets)
        {
            text += itemSocket.GetWordGroup();

            if (displayWeight)
            {
                text += " (w:" + (itemSocket.item.weight * itemSocket.count) + ")";
            }

            if (_itemSockets.Count > 1 && i < _itemSockets.Count - 1)
            {
                if (separateWithLigns)
                {
                    text += "\n";
                }
                else
                {
                    if (_itemSockets.Count > 2)
                    {
                        if (i == _itemSockets.Count - 2)
                        {
                            text += " et ";
                        }
                        else
                        {
                            text += ", ";
                        }
                    }
                    else
                    {
                        text += " et ";
                    }
                }

            }

            ++i;
        }

        return text;
    }
    #endregion

}