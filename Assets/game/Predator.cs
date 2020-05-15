using System;
using UnityEngine;

public class Predator : MonoBehaviour, CombatTarget {
  
  private Agent agent;

  public event Action<CombatTarget> OnDeath;

  private bool isDead;

  public MonoBehaviour GetBehaviour() {
      return this;
  }

  public void Init(Combatant combatant, AgentConfigCommon common){
    var agent = new CombatAgent(common, "predator");
    agent.ProvideCombatant(combatant);
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
}