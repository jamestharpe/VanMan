using System;
using System.Diagnostics;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Rollins.Marketing.CommandCenter.Web
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            DiagnosticMonitorConfiguration dmc = DiagnosticMonitor.GetDefaultInitialConfiguration();
            Trace.AutoFlush = true;
            TimeSpan tsTenSeconds = TimeSpan.FromSeconds(10);
            dmc.Logs.ScheduledTransferPeriod = tsTenSeconds;
            dmc.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", dmc);
            Trace.WriteLine("Diagnostics Initialized");
            return base.OnStart();
        }
    }
}