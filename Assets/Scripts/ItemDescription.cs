using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class ItemDescription {

    public static List<ItemDescription> archive = new List<ItemDescription>();
    public static List<ItemDescription> itemDescriptions = new List<ItemDescription>();

    public string name;
    public Options options;
    public List<ItemGroup> groups;

    public ItemDescription (string name, string opts) {
        this.name = name;
        groups = new List<ItemGroup>();
        options = new Options(opts);
    }

    // static describe
    public static void DescribeAll() {
        if (itemDescriptions.Count == 0)
            return;
        foreach (var itDes in itemDescriptions)
            TextManager.Write($"{itDes.GetDescription()}\n");
        archive.AddRange(itemDescriptions);
        itemDescriptions.Clear();
    }

    public static void AddItems(string descriptionName, List<Item> items, string opts = "") {
        // find item description
        var itDes = itemDescriptions.Find(x=> x.name == descriptionName);
        if (itDes == null) {
            itDes = new ItemDescription(descriptionName, opts);
            itemDescriptions.Add(itDes);
        }

        for (int i = 0; i < items.Count; ++i) {
            var item = items[i];
            var group = itDes.groups.Find(x => x.dataIndex == item.dataIndex);
            if (group == null) {
                group = new ItemGroup($"{item.debug_name}", item.dataIndex);
                itDes.groups.Add(group);
            }
            if ( group.itemSlots.Count == 0) {
                group.itemSlots.Add(new ItemSlot($"{item.debug_name}"));
            }
            group.itemSlots[0].items.Add(item);
        }

    }
    public static void AddProperties(string descriptionName, Item item, List<Property> props, string opts = "") {

        // find /create item description
        var itDes = itemDescriptions.Find(x => x.name == descriptionName);
        if (itDes == null) {
            itDes = new ItemDescription(descriptionName, opts);
            itemDescriptions.Add(itDes);
        }

        if (itDes.options.filterEvents) {
            int c = props.RemoveAll(x => !x.CanBeDescribed());
            if ( props.Count == 0)
                return;
        }

        // find / create group
        var group = itDes.groups.Find(x => x.dataIndex == item.dataIndex);
        if ( group == null) {
            group = new ItemGroup(item.debug_name, item.dataIndex);
            itDes.groups.Add(group);
        }
        
        // find / create slot
        var slot = group.itemSlots.Find(x=> x.items.First().debug_name == item.debug_name);
        if (slot == null) {
            slot = new ItemSlot("all");
            group.itemSlots.Add(slot);
        }
        if (!slot.items.Contains(item))
            slot.items.Add(item);

        foreach (var prop in props) {
            if (slot.describeProps.Find(x => x.GetCurrentDescription() == prop.GetCurrentDescription()) == null)
                slot.describeProps.Add(prop);
        }
    }

    public string GetDescription() {

        // group properties
        SplitSlotByPropertyName();

        // merge groups with same properties
        MergeSlotsWithSameProperties();

        groups.RemoveAll(x=> x.itemSlots.Count == 0);
        foreach (var gr in groups)
            gr.itemSlots.RemoveAll(x => x.items.Count == 0);


        var description = "";
        var tmp_slots = new List<ItemSlot>();
        foreach (var group in groups) {

            // PLACEHOLDER. SINON LES ITEMS APPARAISSENT SANS PROPERTIES POUR LES EVENTS (The rain is.)
            if (options.list)
                group.itemSlots.RemoveAll(x => x.DescribePropsToString() == " ");

            tmp_slots.AddRange(group.itemSlots);
        }
        while (tmp_slots.Count > 0)
            description += $"{Phrase.GetPhrase(tmp_slots, out tmp_slots, options)}\n";
        return $"{options.start}{description}";
    }

    private void SplitSlotByPropertyName() {
        foreach (var group in groups) {
            var confName = "";
            var newSlots = new List<ItemSlot>();
            foreach (var slot in group.itemSlots) {
                foreach (var prop in slot.describeProps) {
                    var similarProp = slot.describeProps.Find(x => x.name == prop.name && x.GetCurrentDescription() != prop.GetCurrentDescription());
                    if (similarProp != null) {
                        // delete all properties that don't have the same name
                        confName = similarProp.name;
                        break;
                    }
                }
                if (confName != "") {
                    slot.describeProps.RemoveAll(x => x.name != confName);
                    var d = slot.describeProps.Select(x => x.GetCurrentDescription()).Distinct();
                    foreach (var s in d) {
                        var newSlot = new ItemSlot(s);
                        newSlot.items.AddRange(slot.items.FindAll(x => x.GetVisibleProps().Find(p => p.GetCurrentDescription() == s) != null));
                        newSlot.describeProps.Add(slot.describeProps.Find(x => x.GetCurrentDescription() == s));
                        newSlots.Add(newSlot);
                    }
                }
            }
            if (newSlots.Count > 0) {
                group.itemSlots.RemoveAt(0);
                group.itemSlots.AddRange(newSlots);
            }
        }
    }

    void MergeSlotsWithSameProperties() {
        var allSlots = new List<ItemSlot>();
        foreach (var group in groups)
            allSlots.AddRange(group.itemSlots);

        for (int i = 0; i < groups.Count; i++) {
            var baseGroup = groups[i];
            for (int j = 0; j < baseGroup.itemSlots.Count; j++) {
                var baseSlot = baseGroup.itemSlots[j];
                if (baseSlot.describeProps.Count == 0) continue;
                var sameSlots = SlotsWithSameProperties(baseSlot, allSlots);
                if (sameSlots.Count > 0) {
                    foreach (var slt in sameSlots) {
                        var targetGroup = groups.Find(x => x.itemSlots.Find(s => s == slt) != null);
                        slt.describeProps.Clear();
                        baseGroup.itemSlots.Add(slt);
                        targetGroup.itemSlots.RemoveAll(x=> sameSlots.Find(s=> s == slt) != null);
                    }
                    // setting base slot to last so the properties appear last
                    baseGroup.itemSlots.Add(baseSlot);
                    baseGroup.itemSlots.RemoveAt(0);
                    options.groupedSlots = true;
                }
            }
        }
    }
    List<ItemSlot> SlotsWithSameProperties(ItemSlot baseSlot, List<ItemSlot> slots) {
        var result = new List<ItemSlot>();
        foreach (var slot in slots) {
            if (baseSlot == slot) continue;
            if ( baseSlot.describeProps.Count == slot.describeProps.Count) {
                bool same = true;
                for (int i = 0; i < baseSlot.describeProps.Count; i++) {
                    if (baseSlot.describeProps[i].GetCurrentDescription() != slot.describeProps[i].GetCurrentDescription())
                        same = false;
                }
                if (same)
                    result.Add(slot);
            }

        }
        return result;
    }

    public struct Options {
        public Options(string txt) {
            var lines = txt.Split('/');
            start = "";
            definite= false;
            list = false;
            groupedSlots = false;
            filterEvents = false;
            foreach (var line in lines) {
                var l = line.Trim(' ');
                if (l.StartsWith("start"))
                    start = $"{l.Split(':')[1]} ";
                else {
                    switch (l) {
                        case "definite":
                            definite = true;
                            break;
                        case "list":
                            list = true;
                            break;
                        case "filter events":
                            filterEvents = true;
                            break;
                    }
                }
            }
        }

        public string start;
        public bool definite;
        public bool list;
        public bool groupedSlots;
        public bool filterEvents;
    }

    public static string log = "";
}
