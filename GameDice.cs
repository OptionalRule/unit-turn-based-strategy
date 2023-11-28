using UnityEngine;
using OptionalRule.Utility;

public class GameDice : MonoBehaviour
{
    public static GameDice Instance { get; private set; } // Singleton instance

    private DiceRoller dice = new DiceRoller();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this); // Destroy if another instance already exists
        }
    }

    public int Roll(int diceSides, int diceNumber)
    {
        return dice.SumRolls(dice.RollMultiple(diceSides, diceNumber));
    }
}
