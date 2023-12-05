using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using UnityEngine.Rendering;
using TreeEditor;
using System;
using System.Security.Cryptography;
using System.Threading;

[System.Serializable]
public class Property {

    // in data
    [System.Serializable]
    public class Part {
        public string key;
        public string text;
        public Part(string k, string v) {
            key = k;
            text = v;
            //Debug.Log($"new part(key:{k} value:{v})");
        }
    }

    public string name;
    // peut �tre tout le temps mettre "*" quand il est disabled
    // juste sauver un peu de m�moire mais bon...
    public bool enabled = false;
    public bool destroy = false;
    public List<Part> parts;

    // � revoir
    public bool changed = false;

    public Property() {

    }

    public Property(Property copy) {
        name = copy.name;
        enabled = !name.StartsWith('*');
        if (!enabled) name = name.Remove(0, 1);
        if (copy.parts == null)
            return;
        foreach (var part in copy.parts)
            AddPart(new Part(part.key, part.text));
    }

    public Property(string cell) {

        var lines = cell.Split('\n');
        name = lines[0];

        // quick way for values like "weight:20"
        if (name.Contains(':')) {
            var split = name.Split(':');
            name = split[0];
            AddPart("value", split[1]);
            return;
        }

        // no parts, only name
        if (lines.Length == 1) return;

        var partCount = cell.Split(':').Length - 1;
        if (partCount == 0) return;

        parts = new List<Part>();
        char[] chars = { '\r', '\t', '\b', '\n', ' ' };

        for (int i = 1; i < lines.Length; i++) {
            // skip empty lines
            if (string.IsNullOrEmpty(lines[i])) continue;

            if (lines[i].Contains(':')) {
                // new part
                var strs = lines[i].Split(':');
                var text = strs[1].Trim(chars);
                parts.Add(new Part(strs[0], text));
            } else {
                try {
                    // add to current part
                    var str = lines[i].Trim(chars);
                    var part = parts[parts.Count - 1];
                    part.text += string.IsNullOrEmpty(part.text) ? str : $"\n{str}";
                } catch (Exception e) {
                    Debug.Log($"error loading propety : {name}");
                    Debug.Log($"line : {lines[i]}");
                    Debug.LogException(e);
                }
            }
        }
    }

    #region parts
    public void AddPart(string key, string text) {
        AddPart(new Part(key, text));
    }
    public void AddPart(Part part) {
        if ( parts == null)
            parts = new List<Part>();
        parts.Add(part);
    }

    public bool HasPart(string str) {
        return parts != null && parts.Find(x => x.key == str) != null;
    }

    public Part GetPart(string key) {
        return parts.Find(x => x.key == key);
    }
    #endregion

    #region value
    // value can be number or text. but it's alway dynamic
    public int GetNumValue(string key = "value") {
        Part part = GetPart(key);
        if ( part == null)
        {
            Debug.LogError($"get num value : prop {name} doesn't have part with key {key}");
            return 0;
        }

        if (part.text.Contains("?")) {
            string[] prts = part.text.Split(" ? ");
            int min = int.Parse(prts[0]);
            int max = int.Parse(prts[1]);
            int i = UnityEngine.Random.Range(min, max);
            part.text = $"{i}";
            AddPart("max", max.ToString());
        }

        if (part.text.Contains('m')) {
            string[] prts = part.text.Split('m');
            int min = int.Parse(prts[0]);
            int max = int.Parse(prts[1]);
            part.text = min.ToString();
            AddPart("max", max.ToString());
        }

        int num = 0;
        if (!int.TryParse(part.text, out num)) {
            Debug.LogError($"get value error : value of {name} ({part.text}) can't be parsed");
            return -1;
        }
        return num;
    }
    public string GetTextValue() {
        var part = GetPart("name");
        if (part == null)
            return $"property {name} has no part (name)";

        if (part.text.Contains('=')) {
            var strs = part.text.Split('=');
            switch (strs[0]) {
                case "type":
                    var itemname = ItemData.GetItemData(strs[1]).name;
                    part.text = itemname;
                    Debug.Log($"changed value of {name} to {itemname}");
                    break;
                case "spec":
                    part.text = Spec.GetCat(strs[1]).GetRandomSpec();
                    break;
            }
        }

        return part.text;

    }
    public void SetValue(int num, string part = "value") {
        if (HasPart("max")) {
            num = Math.Clamp(num, 0, GetNumValue("max"));
        }
        GetPart("value").text = num.ToString();
    }
    #endregion

    #region description
    public string GetDescription() {

        if (!HasPart("description")) return "error : no item description";
        
        var description = GetPart("description").text;

        var start = "";
        if (description.Contains("/")) {
            if (description.StartsWith('(')) {
                start = TextUtils.Extract('(', description);
                description = description.Remove(0, description.IndexOf(')') + 1);
            }
            var prts = description.Split(" / ");
            int max = HasPart("max") ? GetNumValue("max") : 10;
            var lerp = (float)GetNumValue() / max * prts.Length;
            int index = Math.Clamp((int)lerp, 0, prts.Length-1);
            return $"{start} {prts[index]}";
        }

        if ( description.Contains("[value]"))
            description = description.Replace("[value]", GetNumValue().ToString());
        if ( description.Contains("[name]"))
            description = description.Replace("[name]", GetTextValue().ToString());

        if (description.Contains('/'))
            return "didn't implemant split description yet";
        return description;
    }
    public static string GetDescription(List<Property> props) {
        string str = "";
        for (int i = 0; i < props.Count; i++) {
            str += $"{props[i].GetDescription()}";
            if (i < props.Count - 1) str += $" ";
        }
        return str;
    }
    #endregion

    public void Destroy() {
        destroy = true;
    }
    #region update
    public enum UpdateType {
        None,
        Add,
        Substract,
    }
    #endregion
}


