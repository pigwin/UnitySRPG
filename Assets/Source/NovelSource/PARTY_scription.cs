using UnityEngine;
using System.Collections;

public class PARTY_scription : Scription
{
    [SerializeField]
    private string _scobjname;
    public string scobjname
    {
        get { return _scobjname; }
    }

    [SerializeField]
    private int _unitlevel;
    public int unitlevel
    {
        get { return _unitlevel; }
    }

    public PARTY_scription(string scobjname, int unitlevel, string str, COMMAND_TYPE t) : base(str,t)
    {
        this._scobjname = scobjname;
        this._unitlevel = unitlevel;
    }
}
