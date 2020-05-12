using UnityEngine;

public class Tree : MonoBehaviour {
  
  public Color fruitColor;
  public Color normalColor;
  public SpriteRenderer fruitSprite;
  
  private bool hasFruit;
  private TreeEvent onFruit;
  private TreeConfig config;
  private float nextFruitTime;

  public delegate void TreeEvent(Tree self);

  public void Init(TreeConfig config, bool withFruit, TreeEvent onFruit){
    this.config = config;
    this.onFruit = onFruit;
    if(withFruit){
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
    PopNextFruitTime();
    hasFruit = false;
    fruitSprite.color = normalColor;
  }

  private void PopNextFruitTime(){
    nextFruitTime = Time.time + UnityEngine.Random.Range(config.minReFruit, config.maxReFruit);
  }

  public void Update(){
    if(!hasFruit && (Time.time > nextFruitTime)){
      MakeFruit();
    }
  }
}