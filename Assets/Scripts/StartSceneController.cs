using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartSceneController : MonoBehaviour
{
    TextMeshProUGUI numPlayers;
    TextMeshProUGUI numNPCs;
    private void Start()
    {
        numPlayers = GameObject.Find("NumPlayers").GetComponent<TextMeshProUGUI>();
        numNPCs = GameObject.Find("NumNPCs").GetComponent<TextMeshProUGUI>();
    }
    public void Minus()
    {
        Settings.NumPlayers = Mathf.Max(2, Settings.NumPlayers - 1);
        numPlayers.text = Settings.NumPlayers.ToString();
        if (Settings.NumPlayers <= Settings.NumNPCs)
        {
            Settings.NumNPCs = Settings.NumPlayers - 1;
            numNPCs.text = Settings.NumNPCs.ToString();
        }
    }
    public void Plus()
    {
        Settings.NumPlayers = Mathf.Min(6, Settings.NumPlayers + 1);
        numPlayers.text = Settings.NumPlayers.ToString();
    }
    public void MinusNPC()
    {
        Settings.NumNPCs = Mathf.Max(0, Settings.NumNPCs - 1);
        numNPCs.text = Settings.NumNPCs.ToString();
    }
    public void PlusNPC()
    {
        Settings.NumNPCs = Mathf.Min(Settings.NumPlayers - 1, Settings.NumNPCs + 1);
        numNPCs.text = Settings.NumNPCs.ToString();
    }
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
    
}
