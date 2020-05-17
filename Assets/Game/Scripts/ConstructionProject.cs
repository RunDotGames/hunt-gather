using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ConstructionProjectConfig {
  public int totalWork;
  public int workIncrement;
  public RandomFloatRange workTime;
  
}

public class ConstructionProject : MonoBehaviour {
  public List<Sprite> progressSprites;
  public SpriteRenderer spriteRenderer;
  public event Action<ConstructionProject> OnComplete;
  
  private int currentWork;
  private ConstructionProjectConfig config;
  private Action<float> onProgressChange;
  private bool isDone = false;
  private int currentIndex = -1;
  
  public void Init(ConstructionProjectConfig config, Action<float> onProgressChange){
    this.config = config;
    this.onProgressChange = onProgressChange;
  }
  
  public void ContributeWork(){
    if(isDone){
      return;
    }
    currentWork = currentWork + config.workIncrement;
    float percent = (float)currentWork / config.totalWork;
    onProgressChange?.Invoke(percent);
    var index = (int)Math.Ceiling(percent / (1.0f / progressSprites.Count)) -1;
    if(index != currentIndex){
      currentIndex = index;
      spriteRenderer.sprite = progressSprites[index];
    }

    if(currentWork < config.totalWork){
      return;
    }

    isDone = true;
    OnComplete?.Invoke(this);
    GameObject.Destroy(gameObject);
  }

  public RandomFloatRange GetWorkTime(){
    return config.workTime;
  }

}