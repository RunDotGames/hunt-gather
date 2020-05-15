using UnityEngine;
using System.Collections.Generic;

public class UiController: MonoBehaviour {
  
  public FillBar foodBar;
  public FillBar babiesBar;
  public FillBar constructBar;
  public AllocSlider villagerTypeSlider;
  public AllocSlider hutTypeSlider;
  
  private VillageController village;


  public void Init(VillageController village, FoodController food, BabiesController babies, ConstructionController constructor){
    babies.OnProgressChange += HandleBabiesProgress;
    constructor.OnProgressChange += HandleConstructProgress;
    Debug.Log("ui init");
    this.village = village;
    food.OnFoodStorageChange += HandleFoodStorageChange;
    food.OnFoodCapacityChange += HandleFoodCapacityChange;
    
    village.OnVillagerAllocationChange += HandleVillagerAllocationChange;
    villagerTypeSlider.OnChange += HandleVillagerTypeSliderChange;

    village.OnHutAllocationChange += HandleHutAllocationChange;
    hutTypeSlider.OnChange += HandleHutTypeSliderChange;

    babiesBar.SetCapcity(1000);
    babiesBar.SetValue(0);
    constructBar.SetCapcity(1000);
    constructBar.SetValue(0);
  }

  private void HandleConstructProgress(float value){
    Debug.Log("c progress " + value);
    constructBar.SetValue((int)(value * 1000));
  }

  private void HandleFoodStorageChange(int old, int updated){
    foodBar.SetValue(updated);
  }
  private void HandleFoodCapacityChange(int old, int updated){
    foodBar.SetCapcity(updated);
  }

  private void HandleBabiesProgress(float value){
    babiesBar.SetValue((int)(value * 1000));
  }

  private void HandleVillagerTypeSliderChange(int[] positions, int count) {
    var newAllocation = new Dictionary<VillagerType, int>();
    newAllocation[VillagerType.Gatherer] = positions[0];
    newAllocation[VillagerType.Hunter] = positions[1] -  positions[0];
    newAllocation[VillagerType.Builder] = count -  positions[1];
    village.ReAllocateVillagers(newAllocation);
  }
  private void HandleVillagerAllocationChange(Dictionary<VillagerType, int> old, Dictionary<VillagerType, int> updated){
    int[] positions = new int[2];
    positions[0] = (updated.ContainsKey(VillagerType.Gatherer) ? updated[VillagerType.Gatherer] : 0);
    positions[1] = positions[0] + (updated.ContainsKey(VillagerType.Hunter) ? updated[VillagerType.Hunter] : 0);
    var total = positions[1] + (updated.ContainsKey(VillagerType.Builder) ? updated[VillagerType.Builder] : 0);
    villagerTypeSlider.SetState(positions, total);
  }

  private void HandleHutTypeSliderChange(int[] positions, int count) {
    var newAllocation = new Dictionary<HutType, int>();
    newAllocation[HutType.Storage] = positions[0];
    newAllocation[HutType.Housing] = count -  positions[0];
    village.ReAllocateHuts(newAllocation);
  }

  private void HandleHutAllocationChange(Dictionary<HutType, int> old, Dictionary<HutType, int> updated){
    int[] positions = new int[1];
    positions[0] = (updated.ContainsKey(HutType.Storage) ? updated[HutType.Storage] : 0);
    var total = positions[0] + (updated.ContainsKey(HutType.Housing) ? updated[HutType.Housing] : 0);
    hutTypeSlider.SetState(positions, total);
  }
}