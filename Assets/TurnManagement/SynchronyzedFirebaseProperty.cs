using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class SynchronyzedFirebaseProperty<T> : SynchronyzedProperty<T>
{
    //public abstract void BindToFirebaseValue(DatabaseReference referenceToValue);
    [ShowInInspector]
    private DatabaseReference _databaseReference;
    bool boundToFirebaseValue = false;
    public async Task<T> TaskGetValue(DatabaseReference referenceToValue)
    {
        T tt = initialValue;
        var ds = await referenceToValue.GetValueAsync();
        if (ds.Value is T t)
        {
            tt = t;
        }
        return tt;
    }
    
    public async void BindToFirebaseValue(DatabaseReference referenceToValue)
    {
            Debug.LogWarning("bindThis");
        var result = await referenceToValue.GetValueAsync();
        boundToFirebaseValue = result.Exists;
        if (!boundToFirebaseValue)
        {
            Debug.LogError($"Value for {referenceToValue} does not exist, will not bind or register callback.");
            await Task.Delay(400);
            Debug.LogWarning("bindAgain");
            BindToFirebaseValue(referenceToValue);
            return;
        }

        if (boundToFirebaseValue)
        {
            _databaseReference = referenceToValue;
            referenceToValue.ValueChanged += (sender, args) =>
            {
                var snap = args.Snapshot;
                if (args.DatabaseError != null)
                {
                    Debug.LogError(args.DatabaseError.Message);
                    return;
                }

                if (this != null)
                {
                    if (snap.Exists)
                    {

                        if (args.Snapshot.Value is T rt)
                        {
                            if (!rt.Equals(initialValue))
                            {
                                Value = rt;

                            }
                        }
                        else
                        {
                            Debug.LogWarning("Wrong return type!");
                            Debug.LogWarning($"expected type {typeof(T).FullName} but got value {args.Snapshot.Value} which is of type {snap.Value.GetType().FullName}");

                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Value does not exist (yet) for value name {referenceToValue.Key}");

                    }
                }
            };
            onValueChanged += t => _databaseReference.SetValueAsync(t);
        }
    }



    /// <summary>
    ///
    /// </summary>
    /// <param name="initialValue"> Initial value must be -1 for long values and "-1" for string, do not use boolean right now. </param>
    public SynchronyzedFirebaseProperty(T initialValue) : base(initialValue)
    {
    }
}



