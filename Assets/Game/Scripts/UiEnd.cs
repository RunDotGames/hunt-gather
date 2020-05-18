using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class UiEnd : MonoBehaviour {
  public Text villageText;
  public Text bearText;
  public Button restart;

  public void Start(){
    villageText.text = String.Format(villageText.text, ScoreController.GetMaxPop(), ScoreController.GetTotalPop());
    bearText.text = String.Format(bearText.text, ScoreController.GetBearsKilled());
    restart.onClick.AddListener(HandleRestart);
  }

  private void HandleRestart(){
    GameObject.Destroy(gameObject);
    SceneManager.LoadScene("game-scene");
  }
}