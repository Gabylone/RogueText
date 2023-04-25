﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Interior {

	public Coords coords;

    public static Interior GetCurrent;

	public static Dictionary<Coords, Interior> interiors= new Dictionary<Coords, Interior>();

    public TileSet tileSet;

    public static float chanceLockedInterior = 0f;
    public static float chanceEnclosedRoom = 1f;
    //public static float chanceClosedDoor = 0.2f;
    public static float chanceCreateRoom = 1f;
    //public static float chanceCreateRoom = 0.65f;
    public static float chanceHallwayTurn = 0.5f;

    // tmp list for giving objets ( doors ... ) adjectives, because they_re not meant to repeat them selves
    //List<Adjective> adjectives;

    public static bool InsideInterior()
    {
        return GetCurrent != null;
    }

    public static void NewInterior ( Tile tile)
    {
        tile.SetType(tile.type);

        Interior newInterior = new Interior();

        newInterior.coords = tile.coords;
        interiors.Add(tile.coords, newInterior);

        //Debug.Log("addid interior of tile : " + tile.GetName() + " at " + tile.coords.ToString());
    }

	public static Interior Get (Coords coords)
	{
		return interiors[coords];
	}

    public static void DescribeExterior()
    {
        Cardinal dir = Cardinal.East;

        if (Player.Instance.coords.x < 0)
        {
            dir = Cardinal.West;
        }

        Coords tCoords = TileSet.map.playerCoords + (Coords)dir;

        Tile tile = TileSet.map.GetTile(tCoords);

        if (tile == null)
        {
            PhraseKey.WritePhrase("interior_exteriorDescription_blocked");
        }
        else
        {
            PhraseKey.WritePhrase("/interior_exteriorDescription_visible/" + tile.GetDescription());
        }

    }

    #region enter / exit
    public void Enter()
    {
        TileSet.map.playerCoords = Player.Instance.coords;

        GetCurrent = this;

        if (tileSet == null)
        {
            Genererate();
        }

        TileSet.SetCurrent(tileSet);

        MapTexture.Instance.UpdateInteriorMap();

        Player.Instance.coords = tileSet.Center;
        Player.Instance.Move(Cardinal.None);
        //DisplayDescription.Instance.UpdateDescription();

        TimeManager.GetInstance().ChangeMovesPerHour(4);

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
            PhraseKey.WritePhrase("interior_getout_blocked");
        }
        
    }

    public void ExitByDoor()
    {
        Player.Instance.coords = TileSet.map.playerCoords;

        Exit();
    }

    void Exit()
    {
        Interior.GetCurrent = null;

        TileSet.SetCurrent(TileSet.map);

        Player.Instance.Move(Cardinal.None);

        TimeManager.GetInstance().ChangeMovesPerHour(10);

    }
    #endregion

    public void Genererate() {

        /// create tile set 
		tileSet = new TileSet();
        tileSet.width = TileSet.map.width;
        tileSet.height = TileSet.map.height;

        // create room types
        List<Tile.Type> roomTypes = new List<Tile.Type> ();

        /// debug interior : create fix list of rooms
        roomTypes.Add(Tile.Type.Bathroom);
        roomTypes.Add(Tile.Type.Bedroom);
        roomTypes.Add(Tile.Type.ChildBedroom);
        roomTypes.Add(Tile.Type.Kitchen);
        roomTypes.Add(Tile.Type.Toilet);

        // create hallway
        Coords hallway_Coords = tileSet.Center;
        Coords hallway_Dir = new Coords(0,1);
        int a = 0;

		while ( roomTypes.Count > 0 ) {

            // add new hallway tile
			Tile newHallwayTile = new Tile (hallway_Coords);
			newHallwayTile.SetType (Tile.Type.Hallway);

            if (tileSet.tiles.ContainsKey(hallway_Coords))
            {
                hallway_Coords += hallway_Dir;
                ++a;
                continue;
            }

			tileSet.Add (hallway_Coords, newHallwayTile);

            // set entrance door
            if (a == 0)
            {
                Item doorItem = ItemManager.Instance.CreateInTile(newHallwayTile, "door");
                doorItem.CreateProperty("entrance");
                // il n'y a plus réellement de porte dehors en fait non ?
                //doorItem.word.SetAdjective(TileSet.map.GetTile(TileSet.map.playerCoords).items[0].word.GetAdjective);
                doorItem.CreateProperty("direction / to south");
            }

            // check if room appears
            if ( Random.value < chanceCreateRoom ) {

                Coords side = new Coords(hallway_Dir.x, hallway_Dir.y);
                side.Turn();

                Coords coords = newHallwayTile.coords + side
                    ;

                if (tileSet.tiles.ContainsKey(coords))
                {
                    continue;
                }

				Tile newRoomTile = new Tile(coords);
				Tile.Type roomType = roomTypes [Random.Range (0, roomTypes.Count)];
				newRoomTile.SetType (roomType);

				roomTypes.Remove (roomType);

                if (Random.value < chanceEnclosedRoom)
                    newRoomTile.enclosed = true;

                tileSet.Add ( coords, newRoomTile );
			}

            hallway_Coords += hallway_Dir;
            
            if ( Random.value < chanceHallwayTurn)
            {
                hallway_Dir.Turn();
            }

            ++a;

        }

        InitStoryTiles();

        // ADDING DOORS
        AddDoors();

	}

    void InitStoryTiles()
    {
        if (coords == ClueManager.Instance.bunkerCoords)
        {
            //Debug.Log("creating letter");
            int i = Random.Range(1, tileSet.tiles.Count);
            Item letter_item = Item.GetDataItem("lettre");
            tileSet.tiles.Values.ElementAt(i).items.Add(letter_item);
        }

        /*if (coords == ClueManager.Instance.bunkerCoords)
        {
            int i = Random.Range(1, tileSet.tiles.Count);
            Item bunkerItem = Item.FindByName("tableau");
            tileSet.tiles.Values.ElementAt(i).items.Add(bunkerItem);
        }

        if (coords == ClueManager.Instance.clueCoords)
        {
            int i = Random.Range(1, tileSet.tiles.Count);
            Item clueItem = Item.FindByName("radio");
            tileSet.tiles.Values.ElementAt(i).items.Add(clueItem);
        }*/

    }

    void AddDoors()
    {
        // reseting adjectives

        foreach (var tile in tileSet.tiles.Values)
        {
            AddDoors(tile);
        }
    }
    void AddDoors(Tile tile)
    {
        Cardinal[] surr = new Cardinal[4] {
                        Cardinal.North, Cardinal.West, Cardinal.South, Cardinal.East
                    };

        List<Adjective> adjectives = Adjective.GetAll("objet");

        foreach (var dir in surr)
        {
            Coords c = tile.coords + (Coords)dir;
            Tile adjacentTile = tileSet.GetTile(c);

            string currentDoorDirection = "";
            string adjacentDoorDirection = "";

            if (adjacentTile != null && adjacentTile.enclosed)
            {
                switch (dir)
                {
                    case Cardinal.North:
                        currentDoorDirection = "to north";
                        adjacentDoorDirection = "to south";
                        break;
                    case Cardinal.East:
                        currentDoorDirection = "to east";
                        adjacentDoorDirection = "to west";
                        break;
                    case Cardinal.South:
                        currentDoorDirection = "to south";
                        adjacentDoorDirection = "to north";
                        break;
                    case Cardinal.West:
                        currentDoorDirection = "to west";
                        adjacentDoorDirection = "to east";
                        break;
                    case Cardinal.None:
                        break;
                    default:
                        break;
                }

                // adjectives
                if (adjectives.Count == 0)
                {
                    adjectives = Adjective.GetAll("objet");
                }
                Adjective adjective = adjectives[Random.Range(0, adjectives.Count)];
                adjectives.Remove(adjective);

                // current tile door
                if (tile.items.Find(x => x.word.text == "door" && x.GetProperty("direction").GetPart(1) == currentDoorDirection) == null)
                {
                    Item doorItem = ItemManager.Instance.CreateInTile(tile, "door");
                    doorItem.word.SetAdjective(adjective);
                    doorItem.CreateProperty("direction / " + currentDoorDirection);
                }

                // adjacent tile door
                if (adjacentTile.items.Find(x => x.word.text == "porte" && x.GetProperty("direction").GetPart(1) == adjacentDoorDirection) == null )
                {
                    Item doorItem = ItemManager.Instance.CreateInTile(adjacentTile, "door");
                    doorItem.word.SetAdjective(adjective);
                    doorItem.CreateProperty("direction / " + adjacentDoorDirection);
                }

                //Debug.Log("door name : " + currentTileDoorItemName + " adjacent door name : " + adjTileDoorItemName);
            }

        }
    }
}
