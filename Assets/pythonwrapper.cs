using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pythonwrapper
{
    static class py_random
    {
        public static float random()
        {
            // 注意: python のrandom.randomは[min,max)、UnityのRandom.Rangeは[min,max]
            return Random.Range(0.0f, 0.999999999f);
        }

        public static T choice<T>(List<T> seq)
        {
            return seq[Random.Range(0, seq.Count)];
        }


        public static void shuffle<T>(this List<T> seq)
        {
            for (int i = 0; i < seq.Count; i++)
            {
                T temp = seq[i];
                int index = Random.Range(0, seq.Count);
                seq[i] = seq[index];
                seq[index] = temp;
            }
        }
    }

    static class py_copy
    {
        public static List<T> copy<T>(List<T> seq)
        {
            List<T> seq2 = new List<T>();
            for(int i=0; i<seq.Count; i++)
            {
                seq2.Add(seq[i]);
            }
            return seq2;
        }
    }
}