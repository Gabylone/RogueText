using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Movable
{
    public Cardinal previousCardinal;
    public Cardinal currentCarnidal;

    // STATES
    public int health = 0;
    public int maxHealth = 10;

    public Coords prevCoords = new Coords(-1, -1);
    public Coords coords = new Coords(-1, -1);
    public Coords direction = new Coords(-1, -1);

    public virtual void Init()
    {

    }

    public bool CanMoveForward(Coords c)
    {
        Tile targetTile = TileSet.current.GetTile(c);

        if (targetTile == null)
        {
            return false;
        }

        if (targetTile.HasProperty("blocking"))
        {
            return false;
        }

        return true;
    }

    public void Move(Orientation orientation)
    {
        Move(OrientationToCardinal( orientation));
    }
    public void Move(Cardinal targetCardinal)
    {
        Coords targetCoords = coords + (Coords)targetCardinal;
        Move(targetCoords);
    }

    public virtual void Move(Coords targetCoords)
    {

        // change current coords
        prevCoords = coords;
        coords = targetCoords;
        direction = coords - prevCoords;

        // set new direction
        currentCarnidal = (Cardinal)direction;
    }

    public virtual void Orient(Orientation orientation)
    {
        SetDirection(OrientationToCardinal(orientation));
    }
    
    public void SetDirection(Cardinal cardinal)
    {
        previousCardinal = currentCarnidal;
        currentCarnidal = cardinal;
    }

    public static Cardinal OrientationToCardinal( Orientation orientation)
    {

        int a = (int)Player.Instance.currentCarnidal + (int)orientation;
        if (a >= 8)
        {
            a -= 8;
        }

        return (Cardinal)a;
    }

    public static Orientation CardinalToOrientation(Cardinal cardinal)
    {

        int a = (int)cardinal - (int)Player.Instance.currentCarnidal;
        if (a < 0)
        {
            a += 8;
        }

        return (Orientation)a;
    }

    public enum Orientation
    {
        Front,
        FrontRight,
        Right,
        BackRight,
        Back,
        BackLeft,
        Left,
        FrontLeft,

        None,

        Current,
    }
}
