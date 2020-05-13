using UnityEngine;
using System;


[Serializable]
public class ConstructionProjectConfig {
  public int totalWork;
  public int workIncrement;
}

public class ConstructionProject : MonoBehaviour {

  public event Action<ConstructionProject> OnComplete;
  
  private int currentWork;
  private ConstructionProjectConfig config;
  
  public void Init(ConstructionProjectConfig config){
    this.config = config;
  }
  
  public void ContributeWork(){
    currentWork = currentWork + config.workIncrement;
    Debug.Log("project is now: " +  (float)currentWork / config.totalWork + " done.");
    if(currentWork > config.totalWork){
      OnComplete?.Invoke(this);
      GameObject.Destroy(gameObject);
    }
  }


}