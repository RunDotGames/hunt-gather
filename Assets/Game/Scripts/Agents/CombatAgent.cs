
using UnityEngine;
using System.Collections.Generic;
using System;

public interface CombatTarget {
 event Action<CombatTarget> OnDeath;
 void Kill(string reason);
 MonoBehaviour GetBehaviour();
}

public class Combatant{
  public float prowlSpeed;
  public float spotDistance;
  public float attackDistance;
  public Func<Vector2, CombatTarget> targetProvider;
  public float attackDuration;
}

public class CombatAgent: Agent {

  public event Action<EntityEventType> onEvent;
  
  private enum AgentState {
    Rest, Prowling, Attacking
  }
  private static Dictionary<AgentState, int> statePriority = new Dictionary<AgentState, int>(){
    {AgentState.Rest, 0},
    {AgentState.Prowling, 100},
    {AgentState.Attacking, 200},
  };

  private AgentConfigCommon config;
  private Combatant combatant;
  private AgentState state = AgentState.Rest;
  private Dictionary<AgentState, AgentUpdate> updates = new Dictionary<AgentState, AgentUpdate>();
  private AgentPather prowlPather;
  private AgentPather attackPather;
  private Vector2 prowlTo;
  private CombatTarget target;
  private string name;
  private MonoBehaviour parent;

  public CombatAgent(AgentConfigCommon config, string name, MonoBehaviour parent){
    this.parent = parent;
    this.name = name;
    this.config = config;
    updates[AgentState.Rest] = UpdateIdle;
    updates[AgentState.Prowling] = UpdateProwling;
    updates[AgentState.Attacking] = UpdateAttacking;
    ReturnToRest();
  }

  public void ProvideCombatant(Combatant combatant){
    this.combatant = combatant;
    this.prowlPather = new AgentPather(){arrivalDistance=config.arrivalDistance, speed=combatant.prowlSpeed, transform=config.transform};
    this.attackPather = new AgentPather(){arrivalDistance=combatant.attackDistance, speed=config.speed, transform=config.transform};
  }

  public void Update(){
    if(waiter != null){
      return;
    }
    updates[state]();
  }

  private void UpdateIdle(){
    if(!config.restRange.Update(SeasonTask.Rest)){
      return;
    }

    config.restRange.Stop();
    var x = UnityEngine.Random.Range(0, Screen.width);
    var y = UnityEngine.Random.Range(0, Screen.height);
    prowlTo = (Vector2)Camera.main.ScreenToWorldPoint(new Vector2(x,y));
    state = AgentState.Prowling;
    onEvent?.Invoke(EntityEventType.Walk);
  }
  
  private void UpdateProwling(){
    var nearest = combatant.targetProvider(config.transform.position);
    if(nearest != null && (nearest.GetBehaviour().transform.position - config.transform.position).magnitude < combatant.spotDistance){
      state = AgentState.Attacking;
      target = nearest;
      target.OnDeath += HandleTargetDead;
      return;
    }
    if(prowlPather.ToPoint(prowlTo)){
      ReturnToRest();
      return;
    }
  }

  private void ReturnToRest(){
    state = AgentState.Rest;
    onEvent?.Invoke(EntityEventType.Rest);
    config.restRange.Resume();
  }

  private void HandleTargetDead(CombatTarget dead){
    if(dead == target){
      target.OnDeath -= HandleTargetDead;
      target = null;
      ReturnToRest();
    }

  }

  private Coroutine waiter;
  private void UpdateAttacking(){
    if(attackPather.ToPoint(target.GetBehaviour().transform.position)){
      target.OnDeath -= HandleTargetDead;
      Debug.Log(name + " is killing stuff");
      target.Kill("death by combat");
      onEvent?.Invoke(EntityEventType.Attack);
      waiter = parent.StartCoroutine(WaitForAttack());
    }
  }
  
  private IEnumerator<YieldInstruction> WaitForAttack(){
    yield return new WaitForSeconds(combatant.attackDuration);
    waiter = null;
    ReturnToRest();
  }

  public bool IsBusy(){
    return !(state == AgentState.Rest);
  }

  public void Release(){
    if(waiter != null){
      parent.StopCoroutine(waiter);
      waiter = null;
    }
    config.restRange.Stop();
    if(target == null){
      return;
    }
    target.OnDeath -= HandleTargetDead;
    target = null;
  }

  public void Resume(){
    ReturnToRest();
  }

  public int GetAvailability(){
      return statePriority[state];
  }
}