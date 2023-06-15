using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Function_If : Function
{
    public override void TryCall()
    {
        base.TryCall();
        Call(this);
    }
    void prop()
    {
        Item targetItem = GetItem();

        string property_line = GetParam(0);

        if (property_line.StartsWith('!'))
        {
            property_line = property_line.Remove(0, 1);

            if (targetItem.HasEnabledProperty(property_line))
            {
                TextManager.Write("It's " + property_line);
                FunctionSequence.current.Break();
                return;
            }

            return;

        }

        string[] parts = property_line.Split(" / ");

        if (!targetItem.HasEnabledProperty(parts[0]))
        {
            Debug.Log(targetItem.debug_name + " id : " + targetItem.debug_randomID + " dont have the prop " + parts[0]);

            TextManager.Write("It's not " + parts[0]);
            FunctionSequence.current.Break();
            return;
        }

        Property property = targetItem.GetProperty(parts[0]);

        if (property.HasInt())
        {
            if (property.GetInt() <= int.Parse(parts[1]))
            {
                TextManager.Write("No " + property.name);
                FunctionSequence.current.Break();
                return;
            }
        }
    }

    void has()
    {
        if (CurrentItems.HasItem( GetParam(0)) )
        {
            Debug.Log("has " + GetParam(0));

            // do stuff below until "*"
        }
        else
        {
            Debug.Log("doesn't have " + GetParam(0));
            FunctionSequence.current.GoToNextNode();
            // go to next "*"
        }
    }

}
