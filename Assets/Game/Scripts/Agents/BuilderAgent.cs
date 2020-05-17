using UnityEngine;
using System;
using System.Collections.Generic;


public class BuilderAgent : Agent {
  
  public event Action<EntityEventType> onEvent;

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
  private AgentConfigCommon config;
  private AgentPather walkPather;
  private ConstructionProject active;
  private SeasonalTimeRange workTime;
  

  public BuilderAgent(AgentConfigCommon config){
    this.config = config;
    updates = new Dictionary<AgentState, Action>();
    updates[AgentState.Idle] = UpdateIdle;
    updates[AgentState.GoToProject] = UpdateGoToProject;
    updates[AgentState.WorkProject] = UpdateWorkProject;
    updates[AgentState.Rest] = UpdateRest;
    construction = GameObject.FindObjectOfType<ConstructionController>();
    walkPather = new AgentPather(){arrivalDistance=config.arrivalDistance, speed=config.speed, transform=config.transform};
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
    workTime = null;
    config.restRange.Pause();
  }

  public void Resume() {
      ReturnToRest();
  }

  public void Update() {
      updates[state]();
  }

  public void UpdateIdle(){
    active = construction?.GetActiveProject() ?? null;
    if(active == null){
      workTime = null;
      ReturnToRest();
      return;
    }
    workTime = new SeasonalTimeRange(active.GetWorkTime());
    active.OnComplete += HandleProjectDone;
    state = AgentState.GoToProject;
    onEvent?.Invoke(EntityEventType.Walk);
  }

  
  public void UpdateGoToProject(){
    if(!walkPather.ToPoint(active.transform.position)){
      return;
    }
    state = AgentState.WorkProject;
    workTime.Stop().Resume();
    onEvent?.Invoke(EntityEventType.Construct);
  }
  
  public void UpdateWorkProject(){
    if(!workTime.Update(SeasonTask.Construct)){
      return;
    }
    active.ContributeWork();
  }
  
  public void UpdateRest(){
    if(!config.restRange.Update(SeasonTask.Rest)){
      return;
    }
    config.restRange.Stop();
    state = AgentState.Idle;
    onEvent?.Invoke(EntityEventType.Idle);
  }

  private void HandleProjectDone(ConstructionProject project){
    if(active == project){
      ReturnToRest();
      return;
    }
  }

  
  private void ReturnToRest(){
    state = AgentState.Rest;
    config.restRange.Resume();
    onEvent?.Invoke(EntityEventType.Rest);
  }
}