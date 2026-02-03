#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.WebUI;
#endregion

public class lock_manager : BaseNetLogic
{
    private IUAVariable timeStamp;
    private IUAVariable sessionId;
    private PeriodicTask checkLockTimeStamp;
    private int deltaSeconds = 5;

    public override void Start()
    {
        timeStamp = LogicObject.GetVariable("TimeStamp");
        sessionId = LogicObject.GetVariable("SessionId");

        checkLockTimeStamp = new PeriodicTask(FreeLockIfExpired, 1000, LogicObject);
        checkLockTimeStamp.Start();
    }

    public override void Stop() => checkLockTimeStamp.Dispose();

    [ExportMethod]
    public void Lock(NodeId sId)
    {
        if (IsResourceLocked(sId)) return; // resource already locked by another session
        sessionId.Value = sId.ToString();
        timeStamp.Value = DateTime.Now;
    }

    [ExportMethod]
    public void IsResourceAvailable(NodeId sId, out bool isAvailable)
    {
        isAvailable = !IsResourceLocked(sId);
    }

    private void FreeLockIfExpired()
    {
        DateTime ts = (DateTime)timeStamp.Value;
        TimeSpan diff = DateTime.Now - ts;
        if (diff.TotalSeconds > deltaSeconds)
        {
            sessionId.Value = ""; 
            timeStamp.Value = DateTime.MinValue;
        }
    }

    private bool IsResourceLocked(NodeId sId)
    {
        var activeSessionIdOnResource = sessionId.Value.Value.ToString();
        return
            activeSessionIdOnResource != sId.ToString()
            && activeSessionIdOnResource != "";
    }
}
