using UnityEngine;
using System;
using System.Collections.Generic;


[Serializable]
public class BabiesConfig {
  public RandomFloatRange tickVariance;
  public int timeToTick;
  public int babyMakingToVillager;
  public int babyMakingPerTick;
}

public class BabiesController : MonoBehaviour {

  public BabiesConfig config;

  public event Action<float> OnProgressChange;

  private VillageController village;
  private float nextTickTime;
  private int dedicated;
  private int progress;


  public void Init(VillageController village){
    this.village = village;
    village.OnHutAllocationChange += HandleHutAllocationChange;
    nextTickTime = float.MaxValue;
    dedicated = 0;
    progress = 0;
  }

  private void HandleHutAllocationChange(Dictionary<HutType, int> old, Dictionary<HutType, int> updated){
    var oldDedicated = dedicated;
    dedicated = (updated?.ContainsKey(HutType.Housing) ?? false) ? updated[HutType.Housing] : 0;
    if(dedicated == 0){
      nextTickTime = float.MaxValue;
      return;
    }
    if(dedicated < oldDedicated){
      RollNextTick();
    }
    if(nextTickTime >= float.MaxValue){
      RollNextTick();
    }
  }

  private void RollNextTick(){
    nextTickTime = Time.time + (config.timeToTick / dedicated) * config.tickVariance.GetRangeValue();
  }

  public void Update(){
    if(Time.time < nextTickTime){
      return;
    }
    RollNextTick();
    progress = progress + config.babyMakingPerTick;
    if(progress > config.babyMakingToVillager){
      progress = progress % config.babyMakingToVillager;
      village.SpawnVillager();
    }
    FireProgressChange();
  }

  private void FireProgressChange(){
    float percent = progress / (float)config.babyMakingToVillager;
    OnProgressChange?.Invoke(percent);
  }

}