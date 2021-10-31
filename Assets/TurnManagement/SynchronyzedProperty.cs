using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SynchronyzedProperty<T> 
{
    public T initialValue;
    private T _value;
    public event Action<T> onValueChanged;
    
    [ShowInInspector]
    public T Value
    {
        get => _value;
        set
        {
            try
            {
                if (!(value.Equals(_value)))
                {
                    _value = value;
                    onValueChanged?.Invoke(value);
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError($"new value is {value} and old value is {_value}");
            }
            
        }
    }

    

    [FormerlySerializedAs("SynchronyzedValue")] public ReactiveProperty<T> reactiveProperty;

    public SynchronyzedProperty(T initialValue)
    {
        this.initialValue = initialValue;
        reactiveProperty = new ReactiveProperty<T>(initialValue);
        _value = initialValue;
        reactiveProperty.Select(x => x).Subscribe(x =>
        {
            if (!x.Equals(_value)) {
            Value = x;
            }
        });
        onValueChanged += v => { reactiveProperty.Value = v; };
    }
}





