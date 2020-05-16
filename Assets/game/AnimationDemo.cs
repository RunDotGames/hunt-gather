using UnityEngine;


public class AnimationDemo: MonoBehaviour {
  public Villager villager;

  public void Update(){
    if(Input.GetKeyUp(KeyCode.Alpha1)){
      villager.HandleEntityEvent(EntityEventType.Walk);
    }

    if(Input.GetKeyUp(KeyCode.Alpha2)){
      villager.HandleEntityEvent(EntityEventType.Idle);
    }


  }

}