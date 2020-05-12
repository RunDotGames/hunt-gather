using System;
using UnityEngine;

public class Predator : MonoBehaviour, CombatTarget {
  
  private Agent agent;

  public event Action<CombatTarget> OnDeath;

  private bool isDead;

  public MonoBehaviour GetBehaviour() {
      return this;
  }

  public void Init(CombatAgentConfig config){
    this.agent = new CombatAgent(config, "predator");
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
}