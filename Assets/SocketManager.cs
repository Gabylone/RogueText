using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    public static SocketManager Instance;

    public List<Socket> itemSockets = new List<Socket>();
    public List<Socket> tileSockets = new List<Socket>();


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

    public string DescribeItems(List<Item> items, Socket targetSocket)
    {
        /// mettre les socjets de c�t� pour l'instnat
        List<Socket> mSockets = new List<Socket>();

        string phrases = "";

        // get item item AND item counts
        foreach (var item in items)
        {
            // A REMETTRE
            // j'ai mis de c�t� quand j'ai enlev� la classe "ItemPhrase" et "ItemGroup" qui ne servaient � rien
            // mais important et � remettre
            if (item.stackable)
            {

            }

            Socket socket = targetSocket != null ? targetSocket : Socket.GetRandomSocket(item);
            if (mSockets.Contains(socket))
            {
                // socket already exists
                socket = mSockets.Find(x => x == socket);
            }
            else
            {

                // didn't find socket, fetching new
                mSockets.Add(socket);
                socket.itemGroups.Clear();
            }

            // see if the item is already in the socket
            Socket.ItemGroup itemGroup = socket.itemGroups.Find(x => x.item.dataIndex == item.dataIndex);

            if (itemGroup == null)
            {
                // it isn't, so add it
                itemGroup = new Socket.ItemGroup();

                itemGroup.item = item;

                itemGroup.count = 1;

                socket.itemGroups.Add(itemGroup);
            }
            else
            {

                // is already is, so add to the item group count
                itemGroup.count += 1;

            }

        }

        // return the phrases
        int i = 0;
        foreach (var socket in mSockets)
        {
            string phrase = socket.GetDescription();
            phrases += phrase;


            if (i < mSockets.Count - 1)
            {
                phrases += "\n";
            }

            ++i;
        }

        return phrases;
    }
}
