using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class PropertyDescription {
    public static List<Item> list = new List<Item>();
    public static List<Item> debug_Items = new List<Item>();
    public static List<Property> debug_Property = new List<Property>();


    public static void Add(Item it, Property prop) {
        if (!list.Contains(it)) {
            list.Add(it);
        }

        prop.changed = true;
    }


    public static void Describe() {

        for (var itIndex = 0; itIndex < list.Count; itIndex++) {
            var it = list[itIndex];
            var props = it.properties.FindAll(x => x.changed);

            if (itIndex > 0 && it.containedIn(list[itIndex - 1])) {
                Debug.Log(it.debug_name + " is contained in " + list[itIndex - 1].debug_name);
                TextManager.write("(d) its &dog& is ", it);
            } else {
                TextManager.write("(d) &the dog& is ", it);
            }


            for (var i = 0; i < props.Count; i++) {
                var prop = props[i];
                TextManager.add(prop.GetDescription());
                TextManager.AddLink(itIndex, props.Count);
                prop.changed = false;
            }

        }

        list.Clear();
    }
}
