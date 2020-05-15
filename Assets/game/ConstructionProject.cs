using UnityEngine;
using System;


[Serializable]
public class ConstructionProjectConfig {
  public int totalWork;
  public int workIncrement;
  public RandomFloatRange workTime;
}

public class ConstructionProject : MonoBehaviour {

  public event Action<ConstructionProject> OnComplete;
  
  private int currentWork;
  private ConstructionProjectConfig config;
  private Action<float> onProgressChange;
  private bool isDone = false;
  
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
    if(currentWork >= config.totalWork){
      isDone = true;
      OnComplete?.Invoke(this);
      GameObject.Destroy(gameObject);
    }
  }

  public float GetWorkTime(){
    return config.workTime.GetRangeValue();
  }

}