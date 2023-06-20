using System;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject roomList;
    public GameObject roomCreate;
    public GameObject gamePole;
    public GameObject authView;

    public void MainMenu()
    {
        mainMenu.SetActive(true);
        roomList.SetActive(false);
        roomCreate.SetActive(false);
        gamePole.SetActive(false);
        authView.SetActive(false);
    }

    public void RoomList()
    {
        mainMenu.SetActive(false);
        roomList.SetActive(true);
        roomCreate.SetActive(false);
        gamePole.SetActive(false);
        authView.SetActive(false);
    }

    public void RoomCreate()
    {
        mainMenu.SetActive(false);
        roomList.SetActive(false);
        roomCreate.SetActive(true);
        gamePole.SetActive(false);
        authView.SetActive(false);
    }

    public void GmaePole()
    {
        mainMenu.SetActive(false);
        roomList.SetActive(false);
        roomCreate.SetActive(false);
        gamePole.SetActive(true);
        authView.SetActive(false);
    }

    public void AuthView()
    {
        mainMenu.SetActive(false);
        roomList.SetActive(false);
        roomCreate.SetActive(false);
        gamePole.SetActive(false);
        authView.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
