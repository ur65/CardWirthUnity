using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pythonwrapper;
using System;

class Dice
{
    public int roll(int times = 1, int sided = 6)
    {
        if (sided <= 1) return times;
        int n = 0;
        for (int i = 0; i < times; i++)
        {
            n += (int)(py_random.random() * sided) + 1; // TODO: python package
        }
        return n;
    }

    public T? choice<T>(List<T> seq) where T : struct
    {
        // TODO: Nullableも扱う必要があるかもしれない
        if (seq.Count == 0)
        {
            return null;
        }
        return py_random.choice(seq);
    }

    public List<T> shuffle<T>(List<T> seq)
    {
        List<T> seq2 = py_copy.copy(seq);
        py_random.shuffle(seq2);
        return seq2;
    }

    public T? pop<T>(List<T> seq) where T : struct
    {
        // TODO: Nullableも扱う必要があるかもしれない
        T? item = choice<T>(seq);

        if (item != null)
        {
            seq.Remove((T)item);
        }

        return item;
    }
}