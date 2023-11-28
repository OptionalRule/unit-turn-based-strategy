using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OptionalRule.Utility
{
    public class DiceRoller
    {
        public enum DiceType
        {
            D4 = 4,
            D6 = 6,
            D8 = 8,
            D10 = 10,
            D12 = 12,
            D20 = 20,
            D100 = 100
        }

        public int Roll(DiceType dice)
        {
            return Random.Range(1, (int)dice + 1);
        }

        public int Roll(int diceSides)
        {
            if (diceSides < 1)
            {
                Debug.LogWarning("Roll called with invalid dice sides. Returning 0.");
                return 0;
            }

            return Random.Range(1, diceSides + 1);
        }

        public int RollDice(int diceSides, int numberOfDice)
        {
            return SumRolls(RollMultiple(diceSides, numberOfDice));
        }

        public int[] RollMultiple(DiceType dice, int numberOfDice)
        {
            int[] rolls = new int[numberOfDice];
            for (int i = 0; i < numberOfDice; i++)
            {
                rolls[i] = Roll(dice);
            }
            return rolls;
        }

        public int[] RollMultiple(int diceSides, int numberOfDice)
        {
            if (diceSides < 1)
            {
                Debug.LogWarning("RollMultiple called with invalid dice sides. Returning empty array.");
                return new int[0];
            }

            int[] rolls = new int[numberOfDice];
            for (int i = 0; i < numberOfDice; i++)
            {
                rolls[i] = Roll(diceSides);
            }
            return rolls;
        }

        public int SumRolls(int[] rolls)
        {
            int sum = 0;
            foreach (int roll in rolls)
            {
                sum += roll;
            }
            return sum;
        }
    }
}