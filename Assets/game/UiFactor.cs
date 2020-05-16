using UnityEngine;
using UnityEngine.UI;
using System;

public class UiFactor: MonoBehaviour{
  public Text value;
  public SeasonTask task;
  
  private SeasonController season;
  
  public void Start(){
    this.season = GameObject.FindObjectOfType<SeasonController>();
  }

  public void Update(){
    var value = ((season?.GetFactor(task) ?? 1.0f)) - 1.0f;
    this.value.text = (value < 0 ? "" : "+") + String.Format("{0:0%}", value);
  }
  
}