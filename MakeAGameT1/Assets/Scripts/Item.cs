using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "New Item Preset", menuName = "Item")]
public abstract class Item : ScriptableObject
{
    private int id;
    public new string name;
    public string description;
    public Sprite artwork;

    public abstract void Apply(GameObject target);

}


