using UnityEngine;
using System.Collections.Generic;

public class UiController: MonoBehaviour {
  
  public FillBar foodBar;
  public AllocSlider villagerTypeSlider;
  
  private VillageController village;


  public void Start(){
    Debug.Log("ui init");
    village = GameObject.FindObjectOfType<VillageController>();
    village.OnFoodStorageChange += HandleFoodStorageChange;
    village.OnFoodCapacityChange += HandleFoodCapacityChange;
    village.OnAllocationChange += HandleAllocationChange;
    var foodDetails = village.GetFoodCapactiyAndStorage();
    foodBar.Init(foodDetails.Item1, foodDetails.Item2);
    villagerTypeSlider.OnChange += HandleTypeSliderChange;
    var allocation = village.GetCurrentAllocation();
    HandleAllocationChange(allocation, allocation);
  }

  private void HandleFoodStorageChange(int old, int updated){
    foodBar.SetValue(updated);
  }
  private void HandleFoodCapacityChange(int old, int updated){
    foodBar.SetCapcity(updated);
  }

  private void HandleTypeSliderChange(int[] positions, int count) {
    var newAllocation = new Dictionary<VillagerType, int>();
    newAllocation[VillagerType.Gatherer] = positions[0];
    newAllocation[VillagerType.Hunter] = positions[1] -  positions[0];
    newAllocation[VillagerType.Builder] = count -  positions[1];
    village.ReAllocate(newAllocation);
  }
  private void HandleAllocationChange(Dictionary<VillagerType, int> old, Dictionary<VillagerType, int> updated){
    int[] positions = new int[2];
    positions[0] = (updated.ContainsKey(VillagerType.Gatherer) ? updated[VillagerType.Gatherer] : 0);
    positions[1] = positions[0] + (updated.ContainsKey(VillagerType.Hunter) ? updated[VillagerType.Hunter] : 0);
    var total = positions[1] + (updated.ContainsKey(VillagerType.Builder) ? updated[VillagerType.Builder] : 0);
    villagerTypeSlider.SetState(positions, total);
  }
}