using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhraseManager : MonoBehaviour
{
    public static PhraseManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public Socket[] genericSockets;
    public string[] positionPhrases;
    public string[] visionPhrases;
    public string[] locationPhrases;
}
