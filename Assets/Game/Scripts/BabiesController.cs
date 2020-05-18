using UnityEngine;
using System;
using System.Collections.Generic;


[Serializable]
public class BabiesConfig {
  public RandomFloatRange workToBaby;
}

public class BabiesController : MonoBehaviour {

  public BabiesConfig config;

  public event Action<float> OnProgressChange;

  private VillageController village;
  private int dedicated;
  private SeasonalTimeRange babiesTask;

  public void Init(VillageController village){
    babiesTask = new SeasonalTimeRange(config.workToBaby);
    babiesTask.Resume();
    this.village = village;
    village.OnHutAllocationChange += HandleHutAllocationChange;
    dedicated = 0;
  }

  private void HandleHutAllocationChange(Dictionary<HutType, int> old, Dictionary<HutType, int> updated){
    dedicated = (updated?.ContainsKey(HutType.Housing) ?? false) ? updated[HutType.Housing] : 0;
  }

  
  public void Update(){
    for(int i = 0; i < dedicated; i++){
      bool isDone = babiesTask.Update(SeasonTask.Babies);
      FireProgressChange();
      if(!isDone){
        continue;
      }
      village.SpawnVillager();
    }
    
  }

  private void FireProgressChange(){
    float percent= babiesTask.GetPercent();
    Debug.Log(percent);
    OnProgressChange?.Invoke(percent);
  }

}