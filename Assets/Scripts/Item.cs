﻿using DG.Tweening.Core.Easing;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor.XR;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UIElements;

[System.Serializable]
public class Item
{
    public int count = 1;

    public string debug_name = "debug name";
    public int debug_randomID;
    public string className;

    // interior
    public Interior interior = null;

    public AppearInfo GetAppearInfo()
    {
        return AppearInfo.GetAppearInfo(dataIndex);
    }

    public List<string> infos;
    public void AddInfo(string str)
    {
        if (infos.Contains(str))
        {
            return;
            //Debug.Log("info already contains " + str);
        }

        infos.Add(str);
    }

    public bool HasInfo(string str)
    {
        return infos.Contains(str);
    }

    public bool visible = false;

    // pas encore utilisé mais à faire 

    /// <summary>
    /// declaration
    /// </summary>
    public int dataIndex;

    /// <summary>
    /// WORD
    /// </summary>
    public int currentWordIndex = 0;
    public List<Word> words = new List<Word>();

    /// <summary>
    /// properties
    /// </summary>
    public List<Property> properties = new List<Property>();


    public virtual void Init()
    {

    }

    #region contained items
    public bool ContainsItems()
    {
        bool b = mContainedItems != null && mContainedItems.Count > 0;
        return b;
    }

    
    public virtual void TryGenerateItems()
    {
        if (HasInfo("generated items"))
        {
            return;
        }

        GenerateItems();
    }

    public virtual void GenerateItems()
    {
        AddInfo("generated items");

        foreach (var itemInfo in GetAppearInfo().itemInfos)
        {
            for (int i = 0; i < itemInfo.amount; i++)
            {
                float f = UnityEngine.Random.value * 100f;
                if (f < itemInfo.chanceAppear)
                {
                    AddItem(itemInfo.name);
                }
            }
        }
    }
    public Item AddItem(string name)
    {
        Item newItem = Item.CreateFromData(name);
        return AddItem(newItem);
    }

    public Item AddItem(Item item)
    {
        if (mContainedItems == null)
        {
            mContainedItems = new List<Item>();
        }

        mContainedItems.Add(item);
        item.TryGenerateItems();

        return item;
    }

    // remove item
    public void RemoveItem(Item item)
    {
        mContainedItems.Remove(item);
    }
    public Item GetItem(string itemName)
    {
       if (mContainedItems == null)
            return null;

        Item tmpItem = mContainedItems.Find(x => x.HasWord(itemName));

        return tmpItem;
    }

    public bool HasItem(string itemName)
    {
        if (mContainedItems == null)
            return false;

        return mContainedItems.Find(x => x.word.text == itemName) != null;
    }

    public bool HasItem(Item item)
    {
        if (mContainedItems == null)
            return false;

        return mContainedItems.Contains(item);
    }

    public bool SameTypeAs(Item otherItem)
    {
        if ( otherItem == null)
        {
            return false;
        }

        return otherItem.dataIndex == dataIndex;
    }
    public bool ExactSameAs(Item otherItem)
    {
        if (otherItem == null)
        {
            Debug.Log("no previous tile");
            return false;
        }

        return otherItem == this;
    }
    #endregion

    #region socket
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

    public bool IsTile()
    {
        return this as Tile != null;
    }

    public bool IsHumanoid()
    {
        return this as Humanoid != null;
    }

    public List<Item> GetUnanimatedItems()
    {
        return mContainedItems.FindAll(x =>
            
            !x.IsHumanoid()

        );
    }
    public List<Item> GetHumanoids()
    {
        return mContainedItems.FindAll(x => x.IsHumanoid());
    }
    public virtual void WriteContainedItems(bool describeContainers =false)
    {
        visible = true;
        // Get Groups for description
        List<Group> allGroups = new List<Group>();
        foreach (var item in GetUnanimatedItems())
        {
            Group group = allGroups.Find(x => x.item.dataIndex == item.dataIndex);
            item.visible = true;

            if (group == null)
            {
                // Create new group
                Group newGroup = new Group();
                newGroup.amount = 1;
                newGroup.item = item;
                allGroups.Add(newGroup);

                item.word.defaultDefined = false;
                continue;

            }

            // Found group, adding amount
            ++group.amount;

        }

        List<Group> containerGroups = new List<Group>();
        List<Group> itemGroups = new List<Group>();

        if (describeContainers)
        {
            containerGroups = allGroups.FindAll(x => x.item.ContainsItems() && x.item.HasInfo("invisible"));
        }
        itemGroups = allGroups.FindAll(x => !x.item.HasInfo("invisible"));


        if (containerGroups.Count > 0 && !containerGroups.First().item.HasInfo("invisible"))
        {
            itemGroups.Insert(0, containerGroups.First());
        }


        if (itemGroups.Count > 0 )
        {
            if (describeContainers)
            {
                TextManager.Write("there's ", this);
            }
            else
            {
                TextManager.Write("");
            }

            // Describe groups
            int index = 0;
            foreach (var group in itemGroups)
            {
                group.item.word.currentInfo.amount = group.amount;
                if (group.item.word.defaultDefined)
                {
                    TextManager.Add("&the dog&", group.item);
                }
                else
                {
                    TextManager.Add("&a dog&", group.item);
                    group.item.word.defaultDefined = true;
                }
                TextManager.AddLink(index, itemGroups.Count);

                ++index;
            }

            if (this != Tile.GetCurrent && itemGroups.Count > 0)
            {
                if (word.defaultDefined)
                {
                    TextManager.Add(" &on the dog&", this);
                }
                else
                {
                    TextManager.Add(" &on a dog&", this);
                    word.defaultDefined = true;
                }
            }
        }

        foreach (var group in containerGroups)
        {
            if (group.item.ContainsItems())
            {
                group.item.WriteContainedItems();
            }
        }

        AddInfo("discovered");

    }

    [System.Serializable]
    public class Group
    {
        public Item item;
        public int amount;
    }
    // description depth here
    public List<Item> GetAllItems()
    {
        List<Item> items = new List<Item>
        {
            this
        };

        if (mContainedItems == null)
        {
            return items;
        }

        foreach (var item in mContainedItems)
        {
            items.AddRange(item.GetAllItems());

            /*if ( item.info.available)
            {
                items.AddRange(item.GetAllItems());
            }*/
        }

        return items;
    }
    #endregion


    #region search
    public Word word
    {
        get
        {
            return words[currentWordIndex];
        }
    }
    public bool HasWord(string _text)
    {
        // find singular
        int index = words.FindIndex(x => x.text == _text);

        if (index >= 0)
        {
            currentWordIndex = index;
            return true;
        }

        // find plural
        /*index = words.FindIndex(x => x.GetPlural() == _text);

        if ( index >= 0)
        {
            currentWordIndex = index;
            return true;
        }*/


        return false;
    }
    public bool ContainedInText(string text)
    {
        foreach (var word in words)
        {
            // find singular
            string bound = @$"\b{word.text}\b";
            if (Regex.IsMatch(text, bound))
            {
                word.currentNumber = Word.Number.Singular;
                return true;
            }

            bound = @$"\b{word.GetPlural()}\b";
            if (Regex.IsMatch(text, bound))
            {
                word.currentNumber = Word.Number.Plural;
                return true;
            }
        }


        return false;
    }
    #endregion


    #region properties
    /// properties /// <summary>
    /// properties ///
    /// </summary>
    public virtual void WriteDescription()
    {
        if (HasProperties())
        {
            WriteProperties();
        }

        if (ContainsItems())
        {
            WriteContainedItems(true);
        }

        if (!HasProperties() && !ContainsItems())
        {
            if (GetAppearInfo().CanContainItems())
            {
                TextManager.Write("It's empty");
            }
            else
            {
                TextManager.Write("It's just &a dog&", this);
            }
        }
    }

    // adds whole new, simple property
    public Property AddProperty(string line, bool describe = false)
    {
        Property newProperty = new Property();
        string[] parts = line.Split(" / ");
        newProperty.type = parts[0];
        newProperty.name = parts[1];
        if (parts.Length > 2)
        {
            newProperty.SetValue(parts[2]);
        }

        newProperty.Init();

        if (describe)
        {
            newProperty.Describe();
        }

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

        if (newProperty.eventDatas != null)
        {
            foreach (var eventData in newProperty.eventDatas)
            {
                // juste un objet par evetn en fait
                ItemEvent itemEvent = ItemEvent.list.Find(x => x.item == this);

                if (itemEvent == null)
                {
                    itemEvent = ItemEvent.New(this, Tile.GetCurrent);
                }
            }
        }

        properties.Add(newProperty);

        return newProperty;
    }
    public void DeleteProperty(string propertyName)
    {
        Property property = GetProperty(propertyName);
        if (property == null)
        {
            Debug.LogError("DeleteProperty : property " + propertyName + " does not exist in " + debug_name);
            return;
        }
        properties.Remove(property);
    }
    public bool HasProperties()
    {
        return properties.FindAll(x => x.enabled).Count > 0;
    }
    public bool HasVisibleProperties()
    {
        return properties.FindAll(x => x.enabled && x.type != "hidden").Count > 0;
    }
    public bool HasEnabledProperty(string name)
    {
        Property property = properties.Find(x => x.name == name && x.enabled);

        return property != null;
    }
    public bool HasProperty(string name)
    {
        Property property = properties.Find(x => x.name == name);

        return property != null;
    }

    public bool HasPropertyOfType(string type)
    {
        Property property = properties.Find(x => x.type == type);

        return property != null;
    }

    public List<Property> GetEnabledProperties()
    {
        // ici "type == container" veut dire que la battery ou water des items est décrite meme si le truc est disabled,
        // à terme, mettre un parametre qui fait que la prop est décrite meme quand l'item est disabled
        // ou un autre truc mieux
        return properties.FindAll(x => x.enabled || x.type == "container");
    }

    public void EnableProperty(string propertyName)
    {
        properties.Find(x => x.name == propertyName).enabled = true;
    }

    public void DisableProperty(string propertyName)
    {
        properties.Find(x => x.name == propertyName).enabled = false;
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
    public List<Property> GetPropertiesOfType(string type)
    {
        return properties.FindAll(x => x.type == type);
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
    public bool HasItemWithProperty(string propertyName)
    {
        return GetContainedItems.Find(x => x.HasProperty(propertyName)) != null;
    }

    public void WriteProperties()
    {
        int enabledPropCount = GetEnabledProperties().Count;

        if (enabledPropCount > 0)
        {
            TextManager.Write("&the dog& is ", this);

            for (int i = 0; i < enabledPropCount; i++)
            {
                Property property = GetEnabledProperties()[i];

                TextManager.Add(property.GetDescription());
                TextManager.AddLink(i, enabledPropCount);

            }
        }
    }
    #endregion

    ///
    /// <summary>
    /// TOOLS
    /// </summary>
    ///

    #region remove & destroy
    public static void Destroy(Item item)
    {

        ItemEvent.Remove(item);

        foreach (var prop in item.properties)
        {
            prop.Destroy();
        }

        RemoveFromContainer(item);
    }
    public void TransferTo(Item item)
    {
        RemoveFromContainer(this);
        item.AddItem(this);
    }
    public static void RemoveFromContainer(Item targetItem)
    {

        // then in tile
        foreach (var item in AvailableItems.Get)
        {
            if (item.HasItem(targetItem))
            {
                item.RemoveItem(targetItem);
                return;
            }
        }
        Debug.LogError("removing item : " + targetItem.word.text + " failed : not in container, tile or inventory");
    }
    #endregion

    #region tools
    public static List<Item> dataItems = new List<Item>();

    public static Item GetDataItem(string key)
    {
        Item item;
        if (key.StartsWith("type:"))
        {
            key = key.Remove(0, 5);
            List<Item> items = dataItems.FindAll(x => x.HasInfo(key));

            item = items[UnityEngine.Random.Range(0, items.Count)];
        }
        else
        {
            item = dataItems.Find(x => x.HasWord(key));
        }

        if (item == null)
        {
            Debug.LogError("no " + key + " in item datas");
            return null;
        }

        return item;
    }

    public static List<AppearInfo> appearInfos = new List<AppearInfo>();

    public static Item CreateFromData(string name)
    {
        Item copy = GetDataItem(name);

        copy.Init();

        return CreateFromData(copy);
    }

    public static object CreateFromDataSpecial(string name)
    {
        Item item = CreateFromData(name);

        if (string.IsNullOrEmpty(item.className))
        {
            Debug.Log("request " + name + " : no class name in data");
            return null;
        }

        Type ItemType = Type.GetType(item.className);
        if (ItemType == null)
        {
            Debug.LogError("pas d'item type pour " + item.debug_name);
            return null;
        }

        var serializedParent = JsonConvert.SerializeObject(item);
        object obj = JsonConvert.DeserializeObject(serializedParent, ItemType);

        ((Item)obj).Init();

        return obj;
    }

    /// create a new item by name
    public static Item CreateFromData(Item copy)
    {
        Item newItem = new Item();

        // common to all
        newItem.debug_name = copy.debug_name;
        newItem.debug_randomID = UnityEngine.Random.Range(0, 10000);
        newItem.dataIndex = copy.dataIndex;
        newItem.className = copy.className;

        newItem.infos = copy.infos;

        // the word never changes, non ? pourquoi en copy
        newItem.words = copy.words;

        // unique
        foreach (var prop_copy in copy.properties)
        {
            newItem.CreateProperty(prop_copy);
        }

        return newItem;
    }

    public static List<Item> FindItemsWithProperty(string propName)
    {
        return dataItems.FindAll(x => x.HasProperty(propName));
    }
    #endregion



}