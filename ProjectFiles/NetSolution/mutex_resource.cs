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

public class mutex_resource : BaseNetLogic
{
    private IUAVariable resourceAvaliable;
    private NetLogicObject lock_manager;
    private PeriodicTask locker;

    public override void Start()
    {
        resourceAvaliable = LogicObject.GetVariable("ResourceAvailable");
        lock_manager = Project.Current.Get<NetLogicObject>("NetLogic/lock_manager");
        locker = new PeriodicTask(LockResource, 1000, LogicObject);
        locker.Start();
    }

    public override void Stop()
    {
        locker.Dispose();
    }

    private void LockResource()
    {
        lock_manager.ExecuteMethod("IsResourceAvailable", new object[] { Session.NodeId }, out object[] resourceAvailable);
        var isResourceAvailable = (bool)resourceAvailable[0];
        resourceAvaliable.Value = isResourceAvailable;
        if (isResourceAvailable)
        {
            lock_manager.ExecuteMethod("Lock", new object[] { Session.NodeId });
        }
    }
}
