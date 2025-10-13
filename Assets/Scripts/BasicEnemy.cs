using UnityEngine;

public class BasicEnemy : BaseEnemy
{
    protected override void Die()
    {
        GameManager.Instance.AddMoney(moneyReward);
        base.Die(); 
    }
}