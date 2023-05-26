﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

[System.Serializable]
public class Item
{
    public string debug_name = "debug name";

    // pas encore utilisé mais à faire 
    public Info info;
    public struct Info
    {
        public Info(Info copy)
        {
            generatedItems = copy.generatedItems;
            discovered= copy.discovered;
            usableAnytime= copy.usableAnytime;
            stackable = copy.stackable;
            hide = copy.hide;
        }

        public bool hide;
        public bool generatedItems;
        public bool discovered;
        public bool usableAnytime;
        public bool stackable;
    }

    // va rendre les liste d'items obsolètes
    // void "no item list" dans la feuille de routes
    //public List<Socket> sockets = new List<Socket>();

    /// <summary>
    /// declaration
    /// </summary>
    public int dataIndex;

    /// <summary>
    /// WORD
    /// </summary>
    public int currentWordIndex = 0;
    public Word word
    {
        get
        {
            return words[currentWordIndex];
        }
    }
    public List<Word> words = new List<Word>();

    /// <summary>
    /// properties
    /// </summary>
    public List<Property> properties = new List<Property>();

    public Item()
    {

    }

    #region container
    public bool ContainsItems()
    {
        bool b = GetContainedItems != null && GetContainedItems.Count > 0;
        return b;
    }
    public virtual void TryGenerateItems()
    {
        if ( info.generatedItems)
        {
            return;
        }

        info.generatedItems = true;

        foreach (var itemInfo in AppearInfo.GetAppearInfo(dataIndex).itemInfos)
        {
            for (int i = 0; i < itemInfo.amount; i++)
            {
                float f = Random.value * 100f;
                if (f < itemInfo.chanceAppear)
                {
                    string itemName = itemInfo.GetItemName();
                    CreateInItem(itemName);
                    //Debug.Log("there's a " + itemInfo.GetItemName() + " in " + debug_name);
                }
            }
        }

    }

    public Item CreateInItem(string itemName)
    {
        Item item = ItemManager.Instance.CreateFromData(itemName);
        AddItem(item);
        item.TryGenerateItems();
        return item;
    }


    public void AddItem(Item item)
    {
        GetContainedItems.Add(item);
    }

    // remove item
    public void RemoveItem(Item item)
    {
        GetContainedItems.Remove(item);
        AvailableItems.Remove(item);
    }
    // item row : wtf ? le craft system est pas ouf
    public void RemoveItem(int itemRow)
    {
        RemoveItem(GetContainedItems.Find(x => x.dataIndex == itemRow));
    }
    //

    public Item GetItem(string itemName)
    {
        Item tmpItem = GetContainedItems.Find(x => x.word.text == itemName);

        return tmpItem;
    }

    public bool HasItem(string itemName)
    {
        return GetContainedItems.Find(x => x.word.text == itemName) != null;
    }

    public bool HasItem( Item item)
    {
        return GetContainedItems.Contains(item);
    }

    public bool SameTypeAs(Item otherItem)
    {
        return otherItem.dataIndex == dataIndex;
    }
    public bool ExactSameAs(Item otherItem)
    {
        return otherItem == this;
    }
    #endregion

    
    ///
    /// <summary>
    /// TOOLS
    /// </summary>
    ///


    public void PickUp()
    {
        // weight paused for clarity and because may become a property
        /*if (Inventory.Instance.GetContainedItemWeight() + weight > Inventory.Instance.maxWeight)
        {
            TextManager.WritePhrase("inventory_TooHeavy");
            return;
        }*/

        Item.Remove(this);

        Inventory.Instance.AddItem(this);

        TextManager.Write("inventory_PickUp", this);
    }

    #region remove & destroy
    public static void Destroy(Item item){
        
        Remove(item);
    }
    public static void Remove(Item targetItem)
    {
        // then in tile
        foreach (var item in AvailableItems.List)
        {
            if (item.HasItem(targetItem))
            {
                item.RemoveItem(targetItem);
                Debug.Log("removed item from " + item.debug_name);
                return;
            }
        }

        Debug.LogError("removing item : " + targetItem.word.text + " failed : not in container, tile or inventory");
    }
    #endregion

    #region search
    public bool HasWord (string _text)
    {
        //currentWordIndex = words.FindIndex(x => x.Compare(_text));
        currentWordIndex = words.FindIndex(x => x.text == _text);

        if ( currentWordIndex < 0)
        {
            currentWordIndex = 0;
            return false;
        }

        return true;
    }
    
    public bool IsAnItem(string item_name)
    {
        return ItemManager.Instance.TryGetItem(item_name) != null;
    }
    
    #endregion

   
    #region actions
    public bool CanBeDescribed()
    {
        int count = 0;

        if (HasProperties())
        {
            ++count;
        }

        foreach (var verb in Verb.GetVerbs)
        {
            foreach (var combination in verb.cellEvents)
            {
                if (combination.itemIndex == dataIndex)
                    ++count;

            }

        }
        return count != 0;
    }

    public void WriteActions()
    {
        string str = "You can ";

        List<Verb> verbList = new List<Verb>();

        foreach (var verb in Verb.GetVerbs)
        {
            foreach (var combination in verb.cellEvents)
            {
                if (combination.itemIndex == dataIndex)
                {
                    verbList.Add(verb);
                }

            }
        }

        for (int i = 0; i < verbList.Count; i++)
        {
            str += verbList[i].GetName;
            if (!string.IsNullOrEmpty(verbList[i].GetPreposition))
            {
                str += " " + verbList[i].GetPreposition;
            }

            str += " it";

            str += TextUtils.GetLink(i, verbList.Count);

        }


        TextManager.Write(str);
    }

    public void WriteDescription()
    {
        if (HasProperties())
        {
            WriteProperties();
        }

        //TryGenerateItems();

        WriteContainedItems();

        WriteActions();
    }
    public void WriteProperties()
    {
        string str = "";

        int enabledPropCount = GetEnabledProperties().Count;

        if (enabledPropCount > 0)
        {
            str += "&the dog (override)& is ";

            for (int i = 0; i < enabledPropCount; i++)
            {
                Property property = GetEnabledProperties()[i];

                str += property.GetDescription();

                str += TextUtils.GetLink(i, enabledPropCount);
            }

            TextManager.Write(str, this);

        }
    }
    #endregion

    /// properties ///
    
    #region properties
    // adds whole new, simple property
    public Property CreateProperty(string line)
    {
        Property newProperty = new Property();
        string[] parts = line.Split(" / ");
        newProperty.type = parts[0];
        newProperty.name = parts[1];
        if ( parts.Length > 2)
        {
            newProperty.value = parts[2];
        }

        newProperty.Init();

        properties.Add(newProperty);
        return newProperty;
    }

    /// <summary>
    ///  WHAT THE FUCK JAI FAIS ça a 3 heures c'est pas bon mais je l'ai mis parce que ça permet
    ///  de réfléchir à une autre solution
    /// </summary>
    // add proper property, from the data, with events and all
    public Property CreateProperty(Property property_data)
    {
        Property newProperty = new Property(property_data);

        newProperty.Init();

        if ( newProperty.events != null)
        {
            EventManager.instance.AddPropertyEvent(newProperty, this);
        }

        properties.Add(newProperty);

        return newProperty;
    }
    public void DeleteProperty(string propertyName)
    {
        Property property = GetProperty(propertyName);
        if ( property == null)
        {
            Debug.LogError("DeleteProperty : property " + propertyName + " does not exist in " + debug_name);
            return;
        }
        properties.Remove(property);
    }
    public bool HasProperties()
    {
        return properties.FindAll(x=>x.enabled).Count > 0;
    }
    public bool HasVisibleProperties()
    {
        return properties.FindAll(x => x.enabled && x.type != "hidden").Count > 0;
    }
    public bool HasEnabledProperty(string name){
        Property property = properties.Find(x => x.name == name && x.enabled);

        return property != null;
    }
    public bool HasProperty(string name)
    {
        Property property = properties.Find(x => x.name == name);

        return property != null;
    }

    public List<Property> GetEnabledProperties()
    {
        // ici "type == container" veut dire que la battery ou water des items est décrite meme si le truc est disabled,
        // à terme, mettre un parametre qui fait que la prop est décrite meme quand l'item est disabled
        // ou un autre truc mieux
        return properties.FindAll(x => x.enabled||x.type == "container");
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
    public Property GetPropertyOfType(string type)
    {
        Property property = properties.Find(x => x.type == type);

        if (property == null)
        {
            Debug.LogError("property if type : " + type + " doesn't exist in item " + word.text);
            return null;
        }

        return property;
    }
    public bool HasItemWithProperty( string propertyName)
    {
        return GetContainedItems.Find(x=> x.HasProperty(propertyName)) != null;
    }
    #endregion

    public static Item GetItemOfType(string type)
    {
        Item[] items = ItemManager.Instance.dataItems.FindAll(x => x.HasProperty(type)).ToArray();

        Item item = items[Random.Range(0, items.Length)];

        return item;
    }

    #region socket
    public string GetRelativePosition()
    {
        Cardinal cardinal = Coords.GetCardinalFromString(GetProperty("direction").value);

        Movable.Orientation orientation = Movable.CardinalToOrientation(cardinal);

        return Coords.GetOrientationWord(orientation);
    }


    /// <summary>
    /// DESCRIPTION
    /// </summary>

    [SerializeField]
    private List<Item> mContainedItems;
    public List<Item> GetContainedItems
    {
        get
        {
            if (mContainedItems == null)
            {
                mContainedItems = new List<Item>();
            }

            return mContainedItems;
        }
    }

    public virtual void WriteContainedItems(bool describeContainers = false)
    {
        AvailableItems.Add(this);

        if (!ContainsItems())
        {
            return;
        }

        if (info.discovered)
        {
            TextManager.Write("&on the dog&, ", this);
        }
        else
        {
            TextManager.Write("&on a dog&, ", this);
        }

        // Get Groups for description
        List<Group> groups = new List<Group>();
        foreach (var item in GetContainedItems)
        {
            AvailableItems.Add(item);
        
            Group group = groups.Find(x => x.item.dataIndex == item.dataIndex);

            if ( group != null )
            {
                // Found group, adding amount
                ++group.amount;
            }
            else
            {
                // Create new group
                Group newGroup = new Group();
                newGroup.amount = 1;
                newGroup.item = item;
                groups.Add(newGroup);
            }

        }

        // Describe groups
        int index = 0;
        foreach (var group in groups)
        {
            if (group.item.info.hide)
            {
                continue;
            }

            if (describeContainers && group.item.ContainsItems())
            {
                group.item.WriteContainedItems();
            }
            else
            {
                group.item.word.currentInfo.amount = group.amount;
                TextManager.Add("&a dog&", group.item);
            }

                TextManager.Add(TextUtils.GetLink(index, groups.Count));
            ++index;
        }

        info.discovered = true;

        //SocketManager.Instance.DescribeItems(GetContainedItems, socket);
    }

    public class Group
    {
        public Item item;
        public int amount;
    }

    public List<Item> GetItemsRecursive()
    {
        return null;
        /*List<Item> tmpItems = new List<Item>();

        int b = 0;

        foreach (var item in GetContainedItems)
        {
            tmpItems.Add(item);
            if (item.ContainsItems())
            {
                tmpItems.AddRange(item.GetItemsRecursive());
            }
        }

        return tmpItems;*/
    }

    /// <summary>
    /// exemples :
    /// dans une forêt : "derrière l'arbre", ou "dans un buisson"
    /// dans un sac : (donc autre objet) au fond du sac, etc...
    /// </summary>
    /// Je crois qu'il vaut mieux que ces trucs soient dans les sockets
    /// ( liste d'int dans le socket parce quand y'a pas de copy dans les sockets )
    ///c'est déjàle cas maisc'estpar rapport aux tiles
    // pareil
    #endregion

}