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
  
  private bool hasFruit;
  private TreeEvent onFruit;
  private TreeConfig config;
  private float nextFruitTime;

  public delegate void TreeEvent(Tree self);

  public void Init(TreeConfig config){
    this.config = config;
    if(config.withFruit){
      MakeFruit();
    } else {
      PopNextFruitTime();
    }
  }
  private bool HasFruit(){
    return hasFruit;
  }

  private void MakeFruit(){
    hasFruit = true;
    fruitSprite.color = fruitColor;
    onFruit?.Invoke(this);
  }

  public void Defruit(){
    
  }

  private void PopNextFruitTime(){
    nextFruitTime = Time.time + config.fruitRange.GetRangeValue();
  }

  public void Update(){
    if(!hasFruit && (Time.time > nextFruitTime)){
      MakeFruit();
    }
  }

  public Vector2 GetPostion() {
    return transform.position;
  }

  public void Release() {
      config.onRelease(this);
  }

  public void Harvest() {
    config.onHarvest(this);
    PopNextFruitTime();
    hasFruit = false;
    fruitSprite.color = normalColor;
  }

  public float GetHarvestTime() {
    return config.harvestRange.GetRangeValue();
  }
}