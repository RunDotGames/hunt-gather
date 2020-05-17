using System;
using DragonBones;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum EntityEventType {
  Walk, Idle, Rest, Die, Attack, Gather, Construct
}


[Serializable]
public class EntityAnimatorConfig{
  public EntityEventType eventType;
  public string animationName;
  public bool isFullStop;
  public int playTimes = -1;
}

public class EntityAnimator{

  private UnityArmatureComponent armature;
  private Dictionary<EntityEventType, EntityAnimatorConfig> animationsByType; 

  public EntityAnimator(List<EntityAnimatorConfig> config, UnityArmatureComponent armature){
    this.armature = armature;
    animationsByType = config.Aggregate(new Dictionary<EntityEventType, EntityAnimatorConfig>(), (agg, item)=>{
      agg[item.eventType] = item;
      return agg;
    });
  }

  public void HandleEvent(EntityEventType type){
    if(animationsByType.ContainsKey(type)){
      var item = animationsByType[type];
      if(item.isFullStop){
        armature.animation.Stop();
        return;
      }
      armature.animation.Play(item.animationName, item.playTimes);
    }
  }

  public void Show(){
    armature.gameObject.SetActive(true);
  }

  public void Hide(){
    armature.gameObject.SetActive(false);
  }
}