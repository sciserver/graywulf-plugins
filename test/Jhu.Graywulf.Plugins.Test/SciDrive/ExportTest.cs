﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jhu.Graywulf.Web.Services;
using Jhu.Graywulf.Scheduler;
using Jhu.Graywulf.RemoteService;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.SciDrive
{
    [TestClass]
    public sealed class ExportTest : Jhu.Graywulf.Web.Api.V1.ApiTestBase
    {
        [ClassInitialize]
        public new static void Initialize(TestContext context)
        {
            Jhu.Graywulf.Web.Api.V1.ImportTest.Initialize(context);
        }

        [ClassCleanup]
        public new static void CleanUp()
        {
            Jhu.Graywulf.Web.Api.V1.ImportTest.CleanUp();
        }

        protected override void ExportFileHelper(string dataset, string table, string uri, string mimeType, string comments)
        {
            var scuri =  SciDriveClient.GetFilePutUri(new Uri(uri, UriKind.RelativeOrAbsolute)).ToString();
            
            base.ExportFileHelper(dataset, table, scuri, mimeType, comments);
        }

        [TestMethod]
        public void ExportToSciDriveCsvTest()
        {
            ExportFileHelper(
                Registry.Constants.UserDbName,
                "SampleData",
                "graywulf_io_test/export.zip", 
                "text/csv",
                "ExportToSciDriveCsvTest");
        }

        [TestMethod]
        public void ExportToSciDriveTxtTest()
        {
            ExportFileHelper(
                Registry.Constants.UserDbName,
                "SampleData",
                "graywulf_io_test/export.zip",
                "text/plain",
                "ExportToSciDriveTxtTest");
        }
    }
}
