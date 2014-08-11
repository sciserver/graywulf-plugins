﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jhu.Graywulf.IO.Tasks;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.RemoteService;
using Jhu.Graywulf.Test;

namespace Jhu.Graywulf.IO.Tasks
{
    [TestClass]
    public class ImportTableArchiveTest
    {
        private IImportTableArchive GetImportTableArchiveTask(string path, string table, bool remote)
        {
            var ds = new Jhu.Graywulf.Schema.SqlServer.SqlServerDataset(Jhu.Graywulf.Test.Constants.TestDatasetName, Jhu.Graywulf.Test.AppSettings.IOTestConnectionString);
            ds.IsMutable = true;

            var destination = new DestinationTable()
            {
                Dataset = ds,
                DatabaseName = ds.DatabaseName,
                SchemaName = ds.DefaultSchemaName,
                TableNamePattern = table,
            };

            IImportTableArchive it = null;
            if (remote)
            {
                it = RemoteServiceHelper.CreateObject<IImportTableArchive>(Test.Constants.Localhost);
            }
            else
            {
                it = new ImportTableArchive();
            }

            it.Uri = Util.UriConverter.FromFilePath(path);
            it.Destination = destination;

            return it;
        }

        [TestMethod]
        public void ImportTest()
        {
            var path = @"..\..\..\graywulf\test\files\csv_numbers.zip";
            var table = "TableImportArchiveTest_ImportTest";
            var it = GetImportTableArchiveTask(path, table, false);

            it.Open();
            it.Execute();

            it.Destination.GetTable().Drop();
        }

        [TestMethod]
        public void RemoteImportTest()
        {
            using (RemoteServiceTester.Instance.GetToken())
            {
                RemoteServiceTester.Instance.EnsureRunning();

                var path = @"..\..\..\graywulf\test\files\csv_numbers.zip";
                var table = "TableImportArchiveTest_RemoteImportTest";
                var it = GetImportTableArchiveTask(path, table, false);

                it.Open();
                it.Execute();

                it.Destination.GetTable().Drop();
            }
        }

        [TestMethod]
        public void ImportArchiveTest()
        {
            var path = @"..\..\..\graywulf\test\files\archive.zip";
            var table = "TableImportArchiveTest_[$BatchName]_[$ResultsetName]";
            var it = GetImportTableArchiveTask(path, table, false);

            it.Open();
            it.Execute();

            it.Destination.GetTable().Drop();
        }
    }
}
