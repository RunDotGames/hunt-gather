
using UnityEngine;
using System.Collections.Generic;
using System;

public interface CombatTarget {
 event Action<CombatTarget> OnDeath;
 void Kill(string reason);
 MonoBehaviour GetBehaviour();
}


public class CombatAgentConfig {
  public float attackPace;
  public float prowlPace;
  public float arrivalDistance;
  public float attackDistance;
  public float spotDistance;
  public RandomFloatRange idleRange;
  public Transform transform;
  public Func<Vector2, CombatTarget> targetProvider;
}

public class CombatAgent: Agent {
  
  private enum AgentState {
    Idle, Prowling, Attacking
  }
  private static Dictionary<AgentState, int> statePriority = new Dictionary<AgentState, int>(){
    {AgentState.Idle, 0},
    {AgentState.Prowling, 100},
    {AgentState.Attacking, 200},
  };

  private CombatAgentConfig config;
  private AgentState state = AgentState.Idle;
  private Dictionary<AgentState, AgentUpdate> updates = new Dictionary<AgentState, AgentUpdate>();
  private AgentPather prowlPather;
  private AgentPather attackPather;
  private Vector2 prowlTo;
  private float nextProwl;
  private CombatTarget target;
  private string name;

  public CombatAgent(CombatAgentConfig config, string name){
    this.name = name;
    this.config = config;
    this.prowlPather = new AgentPather(){arrivalDistance=config.arrivalDistance, speed=config.prowlPace, transform=config.transform};
    this.attackPather = new AgentPather(){arrivalDistance=config.attackDistance, speed=config.attackPace, transform=config.transform};
    updates[AgentState.Idle] = UpdateIdle;
    updates[AgentState.Prowling] = UpdateProwling;
    updates[AgentState.Attacking] = UpdateAttacking;
    ReturnToIdle();
  }

  public void Update(){
    updates[state]();
  }

  private void UpdateIdle(){
    if(Time.time < nextProwl){
      return;
    }
    var x = UnityEngine.Random.Range(0, Screen.width);
    var y = UnityEngine.Random.Range(0, Screen.height);
    prowlTo = (Vector2)Camera.main.ScreenToWorldPoint(new Vector2(x,y));
    state = AgentState.Prowling;
  }
  private void UpdateProwling(){
    var nearest = config.targetProvider(config.transform.position);
    if(nearest != null && (nearest.GetBehaviour().transform.position - config.transform.position).magnitude < config.spotDistance){
      state = AgentState.Attacking;
      target = nearest;
      target.OnDeath += HandleTargetDead;
      return;
    }
    if(prowlPather.ToPoint(prowlTo)){
      ReturnToIdle();
      return;
    }
  }

  private void ReturnToIdle(){
    state = AgentState.Idle;
    nextProwl = Time.time + config.idleRange.GetRangeValue();
  }

  private void HandleTargetDead(CombatTarget dead){
    if(dead == target){
      target.OnDeath -= HandleTargetDead;
      target = null;
      ReturnToIdle();
    }

  }

  private void UpdateAttacking(){
    if(attackPather.ToPoint(target.GetBehaviour().transform.position)){
      target.OnDeath -= HandleTargetDead;
      target.Kill("death by combat");
      Debug.Log(name + " is killing stuff");
      ReturnToIdle();
    }
  }

  public bool IsBusy(){
    return !(state == AgentState.Idle);
  }

  public void Release(){
    if(target == null){
      return;
    }
    target.OnDeath -= HandleTargetDead;
    target = null;
  }

  public void Resume(){
    ReturnToIdle();
  }

  public int GetAvailability(){
      return statePriority[state];
  }
}