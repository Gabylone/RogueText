﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MapTexture : MonoBehaviour {

	public static MapTexture Instance;

	public Image mainMap_Image;
    public Texture2D mainMap_Texture;

    public Image feedbackMap_Image;
    public Texture2D feedbackMap_Texture;

    public int range = 1;
    int scale = 0;
    public Color[] tileColors;
    public int testcolorx = 0;
    public int testcolory = 0;
    
    void Awake () {
		Instance = this;
	}

    public void SaveTextureToFile(Texture2D textureToSave, string fileName)
    {
        var bytes = textureToSave.EncodeToPNG();
        var file = File.Open(Application.dataPath + "/" + fileName, FileMode.Create);
        var binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();
    }
    
    public void UpdateMap()
    {
        for (int x = 0; x < feedbackMap_Texture.width; x++)
        {
            for (int y = 0; y < feedbackMap_Texture.height; y++)
            {
                Paint(new Coords(x, y), Color.clear);
            }
        }

        Paint(Player.Instance.coords, Color.blue);
        //Paint(ClueManager.Instance.clueCoords, Color.cyan);
        RefreshTexture();
    }

    #region read map from texture
    public void CreateMapFromTexture()
    {
        TileSet.map = new TileSet();
        TileSet.SetCurrent(TileSet.map);

        int w = mainMap_Texture.width;
        int h = mainMap_Texture.height;

        TileSet.map.width = w;
        TileSet.map.height = h;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Coords c = new Coords(x, y);

                Color pixelColor = mainMap_Texture.GetPixel(x, y);

                Tile.Type tileType = Tile.Type.None;

                for (int i = 0; i < tileColors.Length; i++)
                {
                   
                    // check for color match

                    Color tileColor = tileColors[i];

                    float tileColor_r = Mathf.Round(tileColor.r * 1000f);
                    float tileColor_g = Mathf.Round(tileColor.g * 1000f);
                    float tileColor_b = Mathf.Round(tileColor.b * 1000f);

                    float pixel_r = Mathf.Round(pixelColor.r * 1000f);
                    float pixel_g = Mathf.Round(pixelColor.g * 1000f);
                    float pixel_b = Mathf.Round(pixelColor.b * 1000f);

                    float tolerance = 3;

                    if (
                        pixel_r >= tileColor_r - tolerance && pixel_r <= tileColor_r + tolerance &&
                        pixel_g >= tileColor_g - tolerance && pixel_g <= tileColor_g + tolerance &&
                        pixel_b >= tileColor_b - tolerance && pixel_b <= tileColor_b + tolerance
                        )
                    {
                        Tile newTile = new Tile(c);

                        // get tile type from color
                        tileType = (Tile.Type)i;

                        if (tileType == Tile.Type.None)
                        {
                            break;
                        }

                        newTile.SetType(tileType);

                        TileSet.map.Add(c, newTile);

                        switch (tileType)
                        {
                            case Tile.Type.None:
                                break;
                            case Tile.Type.Plain:
                                break;
                            case Tile.Type.Field:
                                break;
                            case Tile.Type.Clearing:
                                break;
                            case Tile.Type.Hill:
                                break;
                            case Tile.Type.Mountain:
                                break;
                            case Tile.Type.Forest:
                                break;
                            case Tile.Type.Woods:
                                break;
                            case Tile.Type.Sea:
                                break;
                            case Tile.Type.Lake:
                                break;
                            case Tile.Type.River:
                                break;
                            case Tile.Type.Beach:
                                break;
                            case Tile.Type.Road:
                                break;
                            case Tile.Type.TownRoad:
                                break;
                            case Tile.Type.CoastalRoad:
                                break;
                            case Tile.Type.Path:
                                break;
                            case Tile.Type.Bridge:
                                break;
                            case Tile.Type.TownHouse:
                                Interior.NewInterior(newTile);
                                break;
                            case Tile.Type.Farm:
                                Interior.NewInterior(newTile);
                                break;
                            case Tile.Type.ForestCabin:
                                Interior.NewInterior(newTile);
                                break;
                            case Tile.Type.CountryHouse:
                                Interior.NewInterior(newTile);
                                break;
                            case Tile.Type.Hallway:
                                break;
                            case Tile.Type.Stairs:
                                break;
                            case Tile.Type.LivingRoom:
                                break;
                            case Tile.Type.Kitchen:
                                break;
                            case Tile.Type.DiningRoom:
                                break;
                            case Tile.Type.ChildBedroom:
                                break;
                            case Tile.Type.Bedroom:
                                break;
                            case Tile.Type.Bathroom:
                                break;
                            case Tile.Type.Toilet:
                                break;
                            case Tile.Type.Attic:
                                break;
                            case Tile.Type.Basement:
                                break;
                            case Tile.Type.Cellar:
                                break;
                            default:
                                break;
                        }
                        break;
                    }
                }

                if (tileType == Tile.Type.None)
                {

                }

            }
        }

    }
    #endregion

    #region texture
    /*public void InitTexture (int scale)
	{
		this.scale = scale;

		mainMap_Texture = new Texture2D (scale, scale);

		mainMap_Texture.filterMode = FilterMode.Point;
		mainMap_Texture.anisoLevel = 0;
		mainMap_Texture.mipMapBias = 0;
		mainMap_Texture.wrapMode = TextureWrapMode.Clamp;

		mainMap_Texture.Resize (scale,scale);
	}*/

	public void ResetTexture () {
		Color[] colors = new Color[scale*scale];
		for (int i = 0; i < colors.Length; i++) {
			colors[i] = Color.black;
		}
		feedbackMap_Texture.SetPixels(colors);
	}
	public void RefreshTexture() {
		feedbackMap_Texture.Apply ();
		feedbackMap_Image.sprite = Sprite.Create (feedbackMap_Texture, new Rect (0, 0, feedbackMap_Texture.width, feedbackMap_Texture.height) , Vector2.one * 0.5f );
	}
	public void Paint ( Coords coords , Color c ) {
		feedbackMap_Texture.SetPixel (coords.x, coords.y, c);
	}
	public void Paint ( Coords c , Tile.Type tileType ) {
		Paint (c, GetTileColor (tileType));
	}
	#endregion

	#region colors
	public Color GetTileColor ( Tile.Type type ) {
		return tileColors [(int)type];
	}
	#endregion
}
