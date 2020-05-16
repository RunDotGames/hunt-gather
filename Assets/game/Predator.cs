using System;
using UnityEngine;
using DragonBones;
using System.Collections.Generic;

public class Predator : MonoBehaviour, CombatTarget {

  public List<EntityAnimatorConfig> animConfig;
  public UnityArmatureComponent armature;
  
  private Agent agent;

  public event Action<CombatTarget> OnDeath;

  private bool isDead;
  private EntityAnimator entityAnimator;

  public MonoBehaviour GetBehaviour() {
      return this;
  }

  public void Start(){
    if(armature != null){
      entityAnimator = new EntityAnimator(animConfig, armature);
    }
  }

  public void Init(Combatant combatant, AgentConfigCommon common){
    var agent = new CombatAgent(common, "predator");
    agent.ProvideCombatant(combatant);
    agent.onEvent += HandleAgentEvent;
    this.agent = agent;
    
  }

  public void Kill(string reason) {
    Debug.Log("wolf died bc " + reason);
    isDead = true;
    agent.Release();
    OnDeath?.Invoke(this);
    GameObject.Destroy(gameObject);
  }

  public void Update(){
    if(isDead){
      return;
    }
    agent?.Update();
  }

  public void HandleAgentEvent(EntityEventType eventType){
    entityAnimator.HandleEvent(eventType);
  }
}