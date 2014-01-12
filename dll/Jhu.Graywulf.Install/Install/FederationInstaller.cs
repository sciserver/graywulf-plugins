﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.Install
{
    public class FederationInstaller : ContextObject
    {
        private Federation federation;

        public FederationInstaller(Federation federation)
            : base(federation.Context)
        {
            this.federation = federation;
        }

        public void GenerateDefaultSettings()
        {
            federation.QueryFactory = typeof(Jhu.Graywulf.Jobs.Query.QueryFactory).AssemblyQualifiedName;
            federation.FileFormatFactory = typeof(Jhu.Graywulf.Format.FileFormatFactory).AssemblyQualifiedName;
            federation.StreamFactory = typeof(Jhu.Graywulf.IO.StreamFactory).AssemblyQualifiedName;
            federation.Copyright = Jhu.Graywulf.Copyright.InfoCopyright;
        }

        public void GenerateDefaultChildren(ServerVersion myDbServerVersion)
        {
            // MyDB database definition

            DatabaseDefinition mydbdd = new DatabaseDefinition(federation)
            {
                Name = Constants.MyDbName,
                System = federation.System,
                LayoutType = DatabaseLayoutType.Monolithic,
                DatabaseInstanceNamePattern = Constants.MyDbInstanceNamePattern,
                DatabaseNamePattern = Constants.MyDbNamePattern,
                SliceCount = 1,
                PartitionCount = 1,
            };
            mydbdd.Save();

            var mydbddi = new DatabaseDefinitionInstaller(mydbdd);
            mydbddi.GenerateDefaultChildren(myDbServerVersion, Constants.MyDbName);

            mydbdd.LoadDatabaseVersions(true);
            federation.MyDBDatabaseVersion = mydbdd.DatabaseVersions[Constants.MyDbName];

            // Job definitions
            var jd = new JobDefinition(federation)
            {
                Name = typeof(Jobs.ExportTables.ExportTablesJob).Name,
                System = federation.System,
                WorkflowTypeName = typeof(Jobs.ExportTables.ExportTablesJob).AssemblyQualifiedName,
            };
            jd.DiscoverWorkflowParameters();
            jd.Save();

            jd = new JobDefinition(federation)
            {
                Name = typeof(Jobs.ExportTables.ExportMaintenanceJob).Name,
                System = federation.System,
                WorkflowTypeName = typeof(Jobs.ExportTables.ExportMaintenanceJob).AssemblyQualifiedName,
            };
            jd.DiscoverWorkflowParameters();
            jd.Save();


            var jdi = new SqlQueryJobInstaller(federation);
            jdi.Install();

            federation.Save();
        }
    }
}
