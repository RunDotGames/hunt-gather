
using UnityEngine;
using System;
using System.Collections.Generic;
using DragonBones;

public class VillagerArmature: MonoBehaviour {
  public List<EntityAnimatorConfig> animConfig;
  public VillagerType type;
  public UnityArmatureComponent armature;

  public EntityAnimator animator;

  public void Init(){
    gameObject.transform.position = Vector3.zero;
    this.animator = new EntityAnimator(animConfig, armature);
    animator.Hide();
  }

  public void Show(){
    animator?.Show();
  }

  public void Hide(){
    animator?.Hide();
  }
}