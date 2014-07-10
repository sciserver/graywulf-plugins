﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jhu.Graywulf.Test;

namespace Jhu.Graywulf.Data
{
    [TestClass]
    public class SmartDataReaderTest : TestClassBase
    {
        [TestMethod]
        public void LoadColumnsTest()
        {
            var sql = "SELECT * FROM SampleData";

            using (var cn = IOTestDataset.OpenConnection())
            {
                using (var cmd = IOTestDataset.CreateCommand(sql, cn))
                {
                    cmd.RecordsCounted = true;

                    using (var dr = cmd.ExecuteReader())
                    {
                        Assert.AreEqual(1, dr.RecordCount);
                        Assert.AreEqual(13, dr.Columns.Count);
                    }
                }
            }
        }
    }
}
