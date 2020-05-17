using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UiIntro: MonoBehaviour {
  public Button startButton;
  public Button nextButton;
  public Text nextText;

  public List<GameObject> screens;

  private int screenIndex;

  public void Start(){
    screenIndex = -1;
    nextButton.onClick.AddListener(HandleNextClick);
    startButton.onClick.AddListener(HandleStartClick);
    foreach (var screen in screens) {
        screen.SetActive(false);
    }
    HandleNextClick();
  }

  public void HandleNextClick(){
    if(screenIndex > -1){
      screens[screenIndex].SetActive(false);
    }
    screenIndex = (screenIndex + 1) % screens.Count;
    screens[screenIndex].SetActive(true);
  }

  public void HandleStartClick(){
    GameObject.Destroy(gameObject);
    SceneManager.LoadScene("game-scene");
  }
}
