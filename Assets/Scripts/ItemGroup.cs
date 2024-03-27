using System.Collections.Generic;

[System.Serializable]
public class ItemGroup {
    public ItemGroup(string key, int dataIndex) {
        this.key = key;
        this.dataIndex = dataIndex;
    }

    public string key;
    public int dataIndex;
    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    [System.Serializable]
    public class PropertySlot {
        public bool skip = false;
        public string key;
        public List<Property> properties = new List<Property>();
        public bool IsValid() {
            return properties.Count < 2;
        }
    }

    public void SplitByProp(string key) {
        var newSlots = new List<ItemSlot>();
        foreach (var slot in itemSlots) {
            foreach (var item in slot.items) {
                var newSlot = newSlots.Find(x => x.key == item.GetProp(key).GetCurrentDescription());
                if (newSlot == null) {
                    newSlot = new ItemSlot(item.GetProp(key).GetCurrentDescription());
                    newSlot.describeProps.Add(item.GetProp(key));
                    newSlots.Add(newSlot);
                }
                newSlot.items.Add(item);
            }
        }

        itemSlots.Clear();
        itemSlots = new List<ItemSlot>(newSlots);
    }

}