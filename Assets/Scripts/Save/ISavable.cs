using UnityEngine;

public interface ISavable<T>
{
    public T GetFromJSon();
    
    public void SaveToJson();
}
