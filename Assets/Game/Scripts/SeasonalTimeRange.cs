using UnityEngine;
using System;


public class SeasonalTimeRange{

  private readonly RandomFloatRange duration;
  private float elapsed;
  private float thisDuration;
  private bool isRunning;
  private static SeasonController season;
  public SeasonalTimeRange(RandomFloatRange duration){
    this.duration = duration;
    Stop();
    if(season == null){
      season = GameObject.FindObjectOfType<SeasonController>();
    }
  }

  public SeasonalTimeRange Stop(){
    isRunning = false;
    thisDuration = -1.0f;
    elapsed = 0;
    return this;
  }

  public SeasonalTimeRange Resume(){
    isRunning = true;
    if(thisDuration < 0){
      thisDuration = duration.GetRangeValue();  
    }
    return this;
  }

  public SeasonalTimeRange Pause(){
    isRunning = false;
    return this;
  }

  
  public bool Update(SeasonTask task){
    if(!isRunning){
      return false;
    }
    float factor = season.GetFactor(task);
    elapsed = elapsed + factor*Time.deltaTime;
    if(elapsed < thisDuration){
      return false;
    }
    
    elapsed = elapsed % thisDuration;
    thisDuration = duration.GetRangeValue();
    return true;
  }
}