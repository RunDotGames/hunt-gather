using UnityEngine;
using UnityEngine.UI;
using System;

public class UiFactor: MonoBehaviour{
  public Text value;
  public SeasonTask task;
  public Image fillBar;
  public float fillAlpha;
  public bool invert;

  private RectTransform fillTransform;
  
  private SeasonController season;
  
  public void Start(){
    this.season = GameObject.FindObjectOfType<SeasonController>();
    this.fillTransform = (RectTransform)fillBar.transform;
    var oldColor = fillBar.color;
    fillBar.color = new Color(oldColor.r, oldColor.g, oldColor.b, fillAlpha);
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
      fillTransform.anchorMin = new Vector2(0, 0.5f-height);
      fillTransform.anchorMax = new Vector2(1, 0.5f);
      return;
    }
    fillTransform.anchorMin = new Vector2(0, 0.5f);
    fillTransform.anchorMax = new Vector2(1, 0.5f + height);
  }
  
}