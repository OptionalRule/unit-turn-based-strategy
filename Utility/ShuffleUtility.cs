using System;
using System.Collections.Generic;
using UnityEngine;


namespace OptionalRule.Utility
{
    public static class ShuffleUtility
    {
        // Generic shuffle method for Lists that returns a new shuffled list
        public static List<T> Shuffle<T>(List<T> list)
        {
            List<T> newList = new List<T>(list); // Create a copy of the original list
            int count = newList.Count;

            for (int i = count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);

                // Swap elements in the new list
                T temp = newList[i];
                newList[i] = newList[randomIndex];
                newList[randomIndex] = temp;
            }

            return newList; // Return the new shuffled list
        }

        // Generic shuffle method for IEnumerables that returns a new shuffled list
        public static IEnumerable<T> Shuffle<T>(IEnumerable<T> sequence)
        {
            var list = new List<T>(sequence);
            return Shuffle(list); // Use the List shuffle method
        }
    }
}
