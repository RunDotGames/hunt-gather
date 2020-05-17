using UnityEngine;
using UnityEngine.UI;
using System;

public class UiFactor: MonoBehaviour{
  public Text value;
  public SeasonTask task;
  public RectTransform fillBar;
  public bool invert;
  
  private SeasonController season;
  
  public void Start(){
    this.season = GameObject.FindObjectOfType<SeasonController>();
  }

  public void Update(){
    var value = ((season?.GetFactor(task) ?? 1.0f)) - 1.0f;
    this.value.text = value.ToString("F2");
    var height = Mathf.Abs(value/2.0f);
    value = invert ? value * -1.0f : value;
    if(fillBar == null){
      return;
    }
    if(value < 0){
      fillBar.anchorMin = new Vector2(0, 0.5f-height);
      fillBar.anchorMax = new Vector2(1, 0.5f);
      return;
    }
    fillBar.anchorMin = new Vector2(0, 0.5f);
    fillBar.anchorMax = new Vector2(1, 0.5f + height);
  }
  
}