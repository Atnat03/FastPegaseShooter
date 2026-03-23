using System;
using UnityEngine;

[System.Serializable]
public struct SavableDateTime
{
    public string savedDate;

    public SavableDateTime SetDate(DateTime time)
    {
        savedDate = time.ToShortDateString();
        return this;
    }

    public DateTime GetDate()
    {
        return !String.IsNullOrEmpty(savedDate) ? DateTime.Parse(savedDate) : DateTime.MinValue;
    }
}
