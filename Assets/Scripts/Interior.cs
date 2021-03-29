﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Interior {

	public Coords coords;

    public static Interior current;

	public static Dictionary<Coords, Interior> interiors= new Dictionary<Coords, Interior>();

    public TileSet tileSet;

    public static float chanceLockedDoor = 0.2f;
    public static float chanceClosedDoor = 0.2f;
    public static float chanceCreateRoom = 0.35f;

    public static bool InsideInterior()
    {
        return current != null;
    }

    public static void NewInterior ( Tile tile)
    {
        tile.SetType(tile.type);

        Interior newInterior = new Interior();

        newInterior.coords = tile.coords;
        interiors.Add(tile.coords, newInterior);

        if (Random.value < chanceClosedDoor)
        {
            tile.AddItem(Item.FindByName("porte d’entrée (f)"));
        }
        else
        {
            tile.AddItem(Item.FindByName("porte d’entrée (o)"));
        }
    }

    public static void Add(Tile tile, Tile.Type type)
    {
        tile.SetType(type);

        Interior newInterior = new Interior();

        newInterior.coords = tile.coords;
        interiors.Add(tile.coords, newInterior);

        if (Random.value < chanceLockedDoor)
        {
            tile.AddItem(Item.FindByName("porte d’entrée (f)"));
        }
        else
        {
            tile.AddItem(Item.FindByName("porte d’entrée (o)"));
        }

    }

	public static Interior Get (Coords coords)
	{
		return interiors[coords];
	}

    #region enter / exit
    public void Enter()
    {
        TileSet.map.playerCoords = Player.Instance.coords;

        if (tileSet == null)
        {
            Genererate();
        }

        TileSet.SetCurrent(tileSet);

        Player.Instance.coords = tileSet.Center;
        Player.Instance.Move(Direction.None);
        //DisplayDescription.Instance.UpdateDescription();

        TimeManager.Instance.ChangeMovesPerHour(4);

    }

    public void ExitByWindow()
    {
        //Coords tCoords = TileSet.map.playerCoords + (Coords)Player.Instance.direction;
        Coords tCoords = TileSet.map.playerCoords;

        Tile tile = TileSet.map.GetTile(tCoords);

        if ( tile!= null)
        {
            Player.Instance.coords = tCoords;
            Exit();
        }
        else
        {
            DisplayFeedback.Instance.Display("la fenêtre est bloquée par une haie");
        }
        
    }

    public void ExitByDoor()
    {
        Player.Instance.coords = TileSet.map.playerCoords;

        Exit();
    }

    void Exit()
    {
        Interior.current = null;

        TileSet.SetCurrent(TileSet.map);

        DisplayDescription.Instance.UpdateDescription();

        TimeManager.Instance.ChangeMovesPerHour(10);


    }
    #endregion

    public void Genererate() {

        /// create tile set 
		tileSet = new TileSet();

        // create room types
		List<Tile.Type> roomTypes = new List<Tile.Type> ();
		Tile.Type type = Tile.Type.LivingRoom;


        /// debug interior : create fix list of rooms

        roomTypes.Add(Tile.Type.Bathroom);
        roomTypes.Add(Tile.Type.Bedroom);
        roomTypes.Add(Tile.Type.ChildBedroom);
        roomTypes.Add(Tile.Type.Kitchen);
        roomTypes.Add(Tile.Type.Toilet);

        // create hallway
        Coords hallway_Coords = tileSet.Center;

        int a = 0;

		while ( roomTypes.Count > 0 ) {

            // add new hallway tile
			Tile newHallwayTile = new Tile (hallway_Coords);
			newHallwayTile.SetType (Tile.Type.Hallway);
			tileSet.Add (hallway_Coords, newHallwayTile);

            // set entrance door
            if (a == 0)
            {
                Item doorItem = Item.FindByName("porte d’entrée (o)");

                newHallwayTile.AddItem(doorItem);
                newHallwayTile.items[0].SetAdjective(TileSet.map.GetTile(TileSet.map.playerCoords).items[0].GetAdjective);
            }

            // check if room appears
            if ( Random.value * 100 < chanceCreateRoom ) {

				Coords coords = newHallwayTile.coords + new Coords (1, 0);

				Tile newRoomTile = new Tile(coords);
				Tile.Type roomType = roomTypes [Random.Range (0, roomTypes.Count)];
				newRoomTile.SetType (roomType);

				roomTypes.Remove (roomType);

                if (Random.value < chanceClosedDoor)
                    newRoomTile.locked = true;

                tileSet.Add ( coords, newRoomTile );
			}

			hallway_Coords += new Coords (0, 1);

            ++a;

        }

        // GENERATING BUNKER

        if ( coords == ClueManager.Instance.bunkerCoords)
        {
            int i = Random.Range(1, tileSet.tiles.Count);
            Item bunkerItem = Item.FindByName("tableau");
            tileSet.tiles.Values.ElementAt(i).items.Add(bunkerItem);
        }

        if (coords== ClueManager.Instance.clueCoords)
        {
            int i = Random.Range(1, tileSet.tiles.Count);
            Item clueItem = Item.FindByName("radio");
            tileSet.tiles.Values.ElementAt(i).items.Add(clueItem);
        }

        // ADDING DOORS
        AddDoors(tileSet);

	}
    
    void AddDoors(TileSet tileset)
    {
        foreach (var tile in tileset.tiles.Values)
        {
            AddDoors(tileSet, tile);
        }
    }
    void AddDoors(TileSet tileset, Tile tile)
    {
        Direction[] surr = new Direction[4] {
                        Direction.North, Direction.West, Direction.South, Direction.East
                    };

        foreach (var dir in surr)
        {
            Coords c = tile.coords + (Coords)dir;
            Tile adjTile = tileset.GetTile(c);

            string currentTileDoorItemName = "";
            string adjTileDoorItemName = "";
            if (adjTile != null && adjTile.locked)
            {
                switch (dir)
                {
                    case Direction.North:
                        currentTileDoorItemName = "porte (o)(n)";
                        adjTileDoorItemName = "porte (o)(s)";
                        break;
                    case Direction.East:
                        currentTileDoorItemName = "porte (o)(e)";
                        adjTileDoorItemName = "porte (o)(w)";
                        break;
                    case Direction.South:
                        currentTileDoorItemName = "porte (o)(s)";
                        adjTileDoorItemName = "porte (o)(n)";
                        break;
                    case Direction.West:
                        currentTileDoorItemName = "porte (o)(w)";
                        adjTileDoorItemName = "porte (o)(e)";
                        break;
                    case Direction.None:
                        break;
                    default:
                        break;
                }

                Adjective adjective = Adjective.GetRandom(Adjective.Type.Item);

                if (tile.items.Find(x => x.word.text == currentTileDoorItemName) == null)
                {
                    //Debug.Log("adding " + currentTileDoorItemName + " to tile " + tile.type);

                    Item doorItem = Item.FindByName(currentTileDoorItemName);
                    doorItem.SetAdjective(adjective);
                    tile.AddItem(doorItem);

                }

                if (adjTile.items.Find(x => x.word.text == adjTileDoorItemName) == null)
                {
                    //Debug.Log("adding " + adjTileDoorItemName + " to adj tile " + adjTile.type);

                    Item doorItem = Item.FindByName(adjTileDoorItemName);
                    doorItem.SetAdjective(adjective);
                    adjTile.AddItem(doorItem);

                }


            }

            

        }
    }
}
