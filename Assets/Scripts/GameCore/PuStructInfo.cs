using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuStructInfo
{
    public string PuName { get; private set; }
    public string displayName { get; private set; }
    public int cardToSelect { get; private set; }
    public string info { get; private set; }
    public string instructions { get; private set; }
    public string releventCards1 { get; private set; }
    public string releventCards2 { get; private set; }

    public PuStructInfo(string puName, string displayName, string info, string instructions, int cardToSelect, string releventCards1, string releventCards2)
    {

        this.PuName = puName;
        this.displayName = displayName;
        this.cardToSelect = cardToSelect;
        this.info = info;
        this.instructions = instructions;
        this.releventCards1 = releventCards1;
        this.releventCards2 = releventCards2;
    }


}