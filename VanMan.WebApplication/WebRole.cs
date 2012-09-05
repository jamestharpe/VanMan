using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Rollins.Marketing.CommandCenter.Web
{

    public class WebRole : RoleEntryPoint
    {

        public override bool OnStart()
        {
            TimeSpan tsOneMinute = TimeSpan.FromMinutes(1);

            DiagnosticMonitorConfiguration dmc = DiagnosticMonitor.GetDefaultInitialConfiguration();

            // Transfer logs to storage every minute
            dmc.Logs.ScheduledTransferPeriod = tsOneMinute;

            // Transfer verbose, critical, etc. logs
            dmc.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

            // Start up the diagnostic manager with the given configuration
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", dmc);
            
            Trace.WriteLine("Diagnostics Initialized");

            return true;
        }
    }
}