using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartSceneController : MonoBehaviour
{
    TextMeshProUGUI numPlayers;
    private void Start()
    {
        numPlayers = GameObject.Find("NumPlayers").GetComponent<TextMeshProUGUI>();
    }
    public void Minus()
    {
        Settings.NumPlayers = Mathf.Max(2, Settings.NumPlayers - 1);
        numPlayers.text = Settings.NumPlayers.ToString();
    }
    public void Plus()
    {
        Settings.NumPlayers = Mathf.Min(6, Settings.NumPlayers + 1);
        numPlayers.text = Settings.NumPlayers.ToString();
    }
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
    
}
