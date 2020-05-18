

public class ScoreController {
  private static int maxPop;
  private static int totalPop;
  private static int bearsKilled;
  private static int currentPop;

  public static void Reset(){
    maxPop = 0;
    totalPop = 0;
    bearsKilled = 0;
    currentPop = 0;
  }

  public static void IncrementPop(){
    totalPop = totalPop + 1;
    currentPop = currentPop + 1;
    if(currentPop > maxPop){
      maxPop = currentPop;
    }
  }

  public static void DecrementPop(){
    currentPop = currentPop - 1;
  }

  public static void IncrementBearKill(){
    bearsKilled = bearsKilled + 1;
  }

  public static int GetMaxPop(){
    return maxPop;
  }

  public static int GetBearsKilled(){
    return bearsKilled;
  }

  public static int GetTotalPop(){
    return totalPop;
  }


}