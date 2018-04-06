﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameState
{
    public List<GameObject> players = new List<GameObject>();

    public GameState() { }

    public bool win = false;
    public bool atMenu = true;
    public bool gameOn = false;
    public bool startingGame = false;

    public void startGame()
    {
        gameOn = true;
        win = false;
        atMenu = false;
        startingGame = false;
    }

    public void playerSelection()
    {
        gameOn = false;
        win = false;
        atMenu = false;
        startingGame = true;
    }

    public void winner()
    {
        gameOn = false;
        win = true;
        atMenu = false;
        startingGame = true;
    }

    public void inMenu()
    {
        atMenu = true;
        gameOn = false;
        win = false;
        startingGame = false;
    }

    public void inGame()
    {
        win = false;
        atMenu = false;
        startingGame = false;
        gameOn = true;
    }
    public int verifyPlayerCount()
    {
        int count = 0;
        foreach (GameObject player in players)
        {
            if (player.activeInHierarchy)
                count++;
        }
        return count;
    }

    public GameObject getLastAlivePlayer()
    {
        foreach (GameObject player in players)
        {
            if (player.activeInHierarchy)
                return player;
        }
        return null;
    }

    public bool has2Player()
    {
        int count = 0;
        foreach (GameObject player in players)
        {
            PlayerController ctrl = player.GetComponent<PlayerController>();
            if (ctrl.isRdy)
                count++;
            if (count >= 2)
                return true;
        }
        return false;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static GameState gameState;

    public static GameObject menu;
    public static GameObject ui;
    public GameObject arena;
    public GameObject lightningStrike;

    bool lightning = false;
    bool isInAnim = false;

    GameManager _instance()
    {
        if (instance == null)
            return instance = this;
        else
            return instance;
    }

    GameState _gameState()
    {
        if (gameState == null)
            return gameState = new GameState();
        else
            return gameState;
    }

    GameObject _menu()
    {
        if (menu == null)
            return menu = GameObject.Find("Menu");
        else
            return menu;
    }

    GameObject _ui()
    {
        if (ui == null)
            return ui = GameObject.Find("UI");
        else
            return ui;
    }

    void Start()
    {
        instance = _instance();
        gameState = _gameState();
        menu = _menu();
        ui = _ui();
        menu.SetActive(true);
        ui.SetActive(false);

        setupMenu();

        for (int i = 1; i <= 4; i++)
        {
            string playerName = "player_" + i;
            GameObject player = new GameObject(playerName);
            PlayerController control = player.AddComponent<PlayerController>();
            control.playerId = i;
            control.isRdy = false;
            player.tag = "Player";
            gameState.players.Add(player);
        }
    }

    void Update()
    {
        if (gameState.atMenu)
        {
            StopAllCoroutines();
            setupMenu();
        }
        if (gameState.startingGame)
        {
            if (gameState.has2Player())
            {
                foreach (GameObject player in gameState.players)
                {
                    if (!player.GetComponent<PlayerController>().isRdy)
                        player.SetActive(false);
                }
                gameState.startGame();
                StartPressed();
            }
        }

        if (gameState.gameOn)
        {
            if (!lightning)
                StartCoroutine(lightningStorm());
            if (gameState.verifyPlayerCount() <= 1)
            {
                GameObject winPlayer = gameState.getLastAlivePlayer();
                if (winPlayer != null)
                {
                    StartCoroutine(playWinAnim());
                    //wait for anim to end before going back to the main menu.
                }
                else
                {
                    playNoContestAnim();
                    //no contest.
                }
                StartCoroutine(playWinAnim());
                gameState.winner();
            }
        }

        if (gameState.win)
        {
            if (!isInAnim)
            {
                //Reset the Body to 0
                foreach (GameObject player in gameState.players)
                {
                    player.SetActive(true);
                    //player.3dBody.SetActive(false);
                }
                gameState.inMenu();
            }
        }
    }

    public void onClickWin()
    {
        int count = 4;
        foreach (GameObject player in gameState.players)
        {
            player.SetActive(false);
            count--;
            if (count == 1)
                break;
        }
    }
    
    public void onClickReturn()
    {
        gameState.winner();
    }

    public void onClickStart()
    {
        swapMenu();
        gameState.playerSelection();
    }

    void swapCanvasUIMenu()
    {
        if (menu.activeInHierarchy)
        {
            menu.SetActive(false);
            ui.SetActive(true);
        }
        else
        {
            menu.SetActive(true);
            ui.SetActive(false);
        }
    }

    void swapMenu()
    {
        foreach (Button btn in menu.GetComponentsInChildren<Button>(true))
        {
            if (btn.tag == "startMenuBtn")
                btn.gameObject.SetActive(false);
        }

        Image[] playerIcons = menu.GetComponentsInChildren<Image>(true);
        foreach (Image icon in playerIcons)
        {
            if (icon.tag == "icon")
                icon.gameObject.SetActive(true);
        }
    }

    void setupMenu()
    {
        ui.SetActive(false);
        menu.SetActive(true);

        Button[] btns = menu.GetComponentsInChildren<Button>(true);
        foreach (Button btn in btns)
            btn.gameObject.SetActive(true);

        Image[] playerIcons = menu.GetComponentsInChildren<Image>(true);
        foreach (Image icon in playerIcons)
            if (icon.tag == "icon")
                icon.gameObject.SetActive(false);

        gameState.inMenu();
    }

    public void activatePlayerRdyIcon(int playerId)
    {
        string iconName = "Player (%d)" + playerId;
        GameObject icon = GameObject.Find(iconName);
        if (icon == null)
            return;
        icon.SetActive(true);
    }

    public void playerLeave(int playerId)
    {
        if (gameState.startingGame)
        {
            string iconName = "Player (%d)" + playerId;
            GameObject icon = GameObject.Find(iconName);
            if (icon == null)
                return;
            icon.SetActive(true);       //Will just Change the look.
        }
    }

    public void StartPressed()
    {
        if (Input.GetButton("Start"))
        {
            gameState.playerSelection();
            menu.SetActive(false);
            ui.SetActive(true);
        }
    }

    IEnumerator lightningStorm()
    {
        if (!lightning)
            lightning = true;

        int freq = 2;
        while (lightning)
        {
            float x, y, z;
            float range = 8;
            x = arena.transform.position.x + Random.Range(-range, range);
            y = arena.transform.position.y + 10;
            z = arena.transform.position.z + Random.Range(-range, range);
            Vector3 pos = new Vector3(x, y, z);
            
            if (Time.time > 30.0 && Time.time < 60.0)
            {
                Instantiate<GameObject>(lightningStrike, pos, new Quaternion(0, 0, 0, 0));
            }
            else if (Time.time > 60.0 && Time.time < 90.0)
            {
                freq = 1;
                Instantiate<GameObject>(lightningStrike, pos, new Quaternion(0, 0, 0, 0));
            }
            else if(Time.time > 90)
            {
                freq = 1;
                Instantiate<GameObject>(lightningStrike, pos, new Quaternion(0, 0, 0, 0));
            }

            //Debug
            //Instantiate<GameObject>(lightningStrike, pos, new Quaternion(0, 0, 0, 0));

            yield return new WaitForSeconds(freq);
        }
    }

    IEnumerator playWinAnim()
    {
        isInAnim = true;
        Image winner = GameObject.Find("PlayerWin (" + gameState.getLastAlivePlayer().GetComponent<PlayerController>().playerId +")").GetComponent<Image>();
        float oldY = winner.rectTransform.position.y;
        float y = winner.rectTransform.position.y;
        while (isInAnim)
        {
            y = 2;
            winner.rectTransform.Translate(0, -y, 0);
            if (winner.rectTransform.position.y <= 150)
            {
                winner.rectTransform.position = new Vector3(winner.rectTransform.position.x, winner.rectTransform.position.y, winner.rectTransform.position.z);
                break;
            }
            yield return null;
        }
        yield return new WaitForSeconds(2);
        Debug.Log("it has been 2 secc");
        winner.rectTransform.position =new Vector3(winner.rectTransform.position.x, oldY, winner.rectTransform.position.z);
        isInAnim = false;
        StopCoroutine(playWinAnim());
    }

    IEnumerator playNoContestAnim()
    {
        isInAnim = true;
        while (isInAnim)
        {
            ui.GetComponentInChildren<Image>();
            //if animStopped
            // isInAnim = false;
            yield return null;
        }
    }

}
