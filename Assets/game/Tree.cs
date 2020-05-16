using UnityEngine;
using System;

public class TreeConfig {
  public bool withFruit;
  public Action<Tree> onFruit;
  public Action<Tree> onRelease;
  public Action<Tree> onHarvest;
  public RandomFloatRange harvestRange;
  public RandomFloatRange fruitRange;
      
}

public class Tree : MonoBehaviour, HarvestTarget {
  
  public Color fruitColor;
  public Color normalColor;
  public SpriteRenderer fruitSprite;
  public GameObject viewRoot;
  
  private bool hasFruit;
  private TreeConfig config;
  private SeasonalTimeRange ripeRange;

  public delegate void TreeEvent(Tree self);

  public void Init(TreeConfig config){
    var isFlip = UnityEngine.Random.Range(0,2) > 0;
    if(isFlip) {
      var scale = viewRoot.transform.localScale;
      viewRoot.transform.localScale = new Vector3(scale.x * -1, scale.y, scale.z);
    }
    this.config = config;
    this.ripeRange = new SeasonalTimeRange(config.fruitRange);
    if(config.withFruit){
      MakeFruit();
    }
  }
  private bool HasFruit(){
    return hasFruit;
  }

  private void MakeFruit(){
    ripeRange.Stop();
    hasFruit = true;
    fruitSprite.color = fruitColor;
    config.onFruit?.Invoke(this);
  }

  public void Update(){
    if(!ripeRange.Update(SeasonTask.Ripen)){
      return;
    }
    MakeFruit();
  }

  public Vector2 GetPostion() {
    return transform.position;
  }

  public void Release() {
      config.onRelease(this);
  }

  public void Harvest() {
    config.onHarvest(this);
    ripeRange.Resume();
    hasFruit = false;
    fruitSprite.color = normalColor;
  }

  public RandomFloatRange GetHarvestTime() {
    return config.harvestRange;
  }
}