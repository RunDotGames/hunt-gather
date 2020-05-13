using UnityEngine;
using System;
using System.Collections.Generic;


public class BuilderConfig {
  public RandomFloatRange restRange;
  public RandomIntRange workRange;
  public float walkSpeed;
  public float arrivalDistance;
  public Transform transform;
}

public class BuilderAgent : Agent {
  
  private enum AgentState {
    Idle, GoToProject, WorkProject, Rest
  }
  
  private static Dictionary<AgentState, int> statePriority = new Dictionary<AgentState, int>() {
    {AgentState.Idle, 0},
    {AgentState.GoToProject, 100},
    {AgentState.Rest, 200},
    {AgentState.WorkProject, 300},
  };

  private Dictionary<AgentState, Action> updates;
  private AgentState state;
  private ConstructionController construction;
  private BuilderConfig config;
  private float nextActionTime;
  private AgentPather walkPather;
  private ConstructionProject active;

  public BuilderAgent(BuilderConfig config){
    this.config = config;
    updates = new Dictionary<AgentState, Action>();
    updates[AgentState.Idle] = UpdateIdle;
    updates[AgentState.GoToProject] = UpdateGoToProject;
    updates[AgentState.WorkProject] = UpdateWorkProject;
    updates[AgentState.Rest] = UpdateRest;
    construction = GameObject.FindObjectOfType<ConstructionController>();
    walkPather = new AgentPather(){arrivalDistance=config.arrivalDistance, speed=config.walkSpeed, transform=config.transform};
    ReturnToRest();
  }

  public int GetAvailability() {
      return statePriority[state];
  }

  public bool IsBusy() {
      return !(state == AgentState.Rest || state == AgentState.Idle);
  }

  public void Release() {
    if(active != null){
      active.OnComplete -= HandleProjectDone;
    }
    active = null;
  }

  public void Resume() {
      ReturnToRest();
  }

  public void Update() {
      updates[state]();
  }

  public void UpdateIdle(){
    active = construction.GetActiveProject();
    if(active == null){
      ReturnToRest();
      return;
    }
    active.OnComplete += HandleProjectDone;
    state = AgentState.GoToProject;
  }

  
  public void UpdateGoToProject(){
    if(walkPather.ToPoint(active.transform.position)){
      state = AgentState.WorkProject;
      nextActionTime = Time.time + config.workRange.GetRangeValue();
    }
  }
  
  public void UpdateWorkProject(){
    if(Time.time > nextActionTime){
      nextActionTime = Time.time + config.workRange.GetRangeValue();
      active.ContributeWork();
    }
  }
  
  public void UpdateRest(){
    if(Time.time < nextActionTime){
      state = AgentState.Idle;
    }
  }

  private void HandleProjectDone(ConstructionProject project){
    if(active == project){
      ReturnToRest();
      return;
    }
  }

  
  private void ReturnToRest(){
    state = AgentState.Rest;
    nextActionTime = Time.time + config.restRange.GetRangeValue();
  }
}