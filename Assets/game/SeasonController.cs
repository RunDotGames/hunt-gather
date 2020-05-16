using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public enum SeasonTask {
  Rest, Construct, Harvest, Ripen, Eat,
}

[Serializable]
public class SeasonTaskConfig{
  public List<SeasonTask> names;
  public AnimationCurve curve;
}

[Serializable]
public class SeasonControllerConfig {
  public List<SeasonTaskConfig> tasks;
  public float yearLength;
  public AnimationCurve tempCurve;
  public float maxTempDegrees;
  public float minTempDegrees;
  public float startOffset;
}



public class SeasonController : MonoBehaviour {
  
  public SeasonControllerConfig config;

  private Dictionary<SeasonTask, AnimationCurve> curvesByTask;
  private Dictionary<SeasonTask, float> factorsByTask;
  private float elapsed;
  private float tempPercent;
  private float tempValue;

  public void Init(){
    curvesByTask = config.tasks.Aggregate(new Dictionary<SeasonTask, AnimationCurve>(), (agg, item) => {
      item.names.Select((name) => {
        agg[name] = item.curve;
        return true;
      }).ToArray();
      return agg;
    });
    factorsByTask = curvesByTask.Keys.Aggregate(new Dictionary<SeasonTask, float>(), (agg, item) => {
      agg[item] = 1.0f;
      return agg;
    });

    elapsed = config.startOffset * config.yearLength;
    UpdateTemp();
    UpdateTaskPercents();
  }

  public void Update(){
    elapsed = elapsed + Time.deltaTime;
    if(elapsed > config.yearLength){
      elapsed = elapsed % config.yearLength;
    }
    UpdateTemp();
    UpdateTaskPercents();
  }

  private void UpdateTemp(){
    var percent = elapsed / config.yearLength;
    tempPercent = config.tempCurve.Evaluate(elapsed / config.yearLength);
    tempValue = config.minTempDegrees + (config.maxTempDegrees - config.minTempDegrees) * tempPercent;
  }

  private void UpdateTaskPercents(){
      foreach (var item in curvesByTask.Keys){
          var curve = curvesByTask[item];
          factorsByTask[item] = curve.Evaluate(tempPercent);
      }
  }

  public float GetTempPercent(){
    return tempPercent;
  }

  public float GetTempValue(){
    return tempValue;
  }

  public float GetFactor(SeasonTask task){
    return factorsByTask[task];
  }
}