using UnityEngine;
using System;
using UnityEngine.UI;

public class UiTemp: MonoBehaviour{
  public Text value;
    
  private SeasonController season;
  
  public void Start(){
    this.season = GameObject.FindObjectOfType<SeasonController>();
  }

  public void Update(){
    var value = season?.GetTempValue() ?? 0.0f;
    this.value.text = value.ToString("F0") + "° F";
  }
}