﻿using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EditorTools : EditorWindow {
    public bool mapVisible = false;

    bool interiorVisible = false;
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Tools")]
    static void Init() {
        // Get existing open window or if none, make a new one:
        var window = (EditorTools)EditorWindow.GetWindow(typeof(EditorTools));
        window.Show();
    }

    void OnGUI() {

        DrawMap();
    }

    void DrawMap() {
        if (GUILayout.Button("Delete Player Prefs")) {
            PlayerPrefs.DeleteAll();
        }
        if ( GUILayout.Button("Show Interior")) {
            interiorVisible = !interiorVisible;
            GameObject.Find("Map Texture (Interior)").GetComponent<Image>().enabled = interiorVisible;
        }
        if (GameObject.Find("Map Texture").GetComponent<Image>().enabled) {
            mapVisible = true;
        } else {
            mapVisible = false;
        }

        if (mapVisible) {
            if (GUILayout.Button("Hide Map")) {
                mapVisible = false;
                GameObject.Find("Map Texture").GetComponent<Image>().enabled = false;
                GameObject.Find("Text Background").GetComponent<Image>().enabled = true;
            }
        } else {
            if (GUILayout.Button("Show Map")) {
                mapVisible = true;
                GameObject.Find("Map Texture").GetComponent<Image>().enabled = true;
                GameObject.Find("Text Background").GetComponent<Image>().enabled = false;
            }
        }
    }
}
