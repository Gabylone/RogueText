using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance;

    public List<SocketGroup> socketGroups = new List<SocketGroup>();
    [System.Serializable]
    public class SocketGroup
    {
        public List<Socket> sockets = new List<Socket>();
    }


    private void Awake()
    {
        Instance = this;
    }

    public void SetSockets(Socket socket )
    {
        SetSockets(new List<Socket> { socket });
    }

    public void SetSockets(List<Socket> sockets)
    {
        
    }

    public void SetRandomSockets()
    {

    }

    public void DescribeItems(List<Item> items, Socket targetSocket)
    {
        SocketGroup socketGroup = new SocketGroup();

        // get item item AND item counts
        foreach (var item in items)
        {
            Socket socket;
            if (item.HasProperty("direction"))
            {
                // look for socket that share the same item, to put them in the same socket
                // only for direction now, because would also be like "on the counter and on the floor, 3 apples" which doesnt make sense
                socket = socketGroup.sockets.Find(x => x.GetItem().SameTypeAs(item));

                // if found a socket which is not the current one
                if (socket != null)
                {
                    socket.AddPosition(item.GetRelativePosition());
                    socket.AddItem(item, true);
                    continue;
                }

                socket = new Socket();
                socket.SetPosition(item.GetRelativePosition());
            }
            else if ( targetSocket == null)
            {
                socket = GetRandomSocket(item);
            }
            else
            {
                socket = targetSocket;
            }
            

            if (socketGroup.sockets.Contains(socket))
            {
                // socket already exists
                socket = socketGroup.sockets.Find(x => x == socket);
            }
            else
            {

                // didn't find socket, fetching new
                socketGroup.sockets.Add(socket);
                //socket.itemGroups.Clear();
            }

            socket.AddItem(item);
        }


        // WRITE DESCRIPTION
        foreach (var socket in socketGroup.sockets)
        {
            KeyWords.socket = socket;
            //TextManager.WritePhrase(GetTextType(socket));
        }

        socketGroups.Add(socketGroup);
    }


    void WriteDescription()
    {
        // Socket Group ?
        // Socket Group has items and sockets. 
        // and the phrase writes it self

        // return the phrases
        
        
    }
    string GetTextType(Socket socket)
    {
        Item targetItem = socket.GetItem();

        Tile tmpTile = targetItem as Tile;

        if (tmpTile != null)
        {

            if (Tile.GetCurrent.SameTypeAs(targetItem))
            {
                if (Tile.GetCurrent.info.stackable)
                {
                    // tu es dans une for�t, la for�t continue
                    return "socket_tile_continue";
                }
                else
                {
                    if (Interior.InsideInterior())
                    {

                        // tu es dans la cuisine, et tu vois LE couloir ( dans un int�rieur, les articles d�finis ont plus de sens )
                        return "socket_tile_visited";
                    }
                    else
                    {
                        // tu es pr�s d'une maison, tu vois une maison que tu connais pas
                        return "socket_tile_discover";
                    }
                }
            }
            else
            {
                // ici
                if (targetItem.info.discovered)
                {
                    // tu vois es pr�s d'une maison
                    return "socket_tile_visited";
                }
                else
                {
                    return "socket_tile_discover";
                }
            }

        }
        else
        {
            return "socket_default";
        }

        
    }


    // STATIC //
    public Socket GetRandomSocket(Item item)
    {
        Socket tmpSocket = new Socket();
        tmpSocket.SetPosition("&on the dog (tile)&"); ;
        return tmpSocket;
    }

    public Item CheckSpecificInput(Item targetItem, string targetText)
    {

        foreach (var item in AvailableItems.List)
        {
            if (targetText.Contains(item.word.text))
            {
                Debug.Log("found item " + item.debug_name + " for " + targetText);
                Item tmpItem = item.GetItem(targetItem.word.text);
                if (tmpItem != null)
                {
                    Debug.LogError("found the item "+tmpItem.debug_name +" in : " + targetText);
                    return tmpItem;
                }
                else
                {
                    Debug.Log("but no specification");
                }
            }
        }

        Debug.Log("found nothing for " + targetText);

        return null;
    }

}
