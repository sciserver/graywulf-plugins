﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jhu.Graywulf.Format;
using Jhu.Graywulf.IO;
using Jhu.Graywulf.IO.Jobs.ImportTables;
using Jhu.Graywulf.IO.Tasks;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.SciDrive
{
    public class SciDriveImportTablesJobFactory : ImportTablesJobFactory
    {
        public override IEnumerable<IO.Jobs.ImportTables.ImportTablesMethod> EnumerateMethods()
        {
            foreach (var method in base.EnumerateMethods())
            {
                yield return method;
            }

            yield return new ImportTablesFromSciDriveMethod();
        }
        
        public override ImportTablesParameters CreateParameters(Registry.Federation federation, Uri uri, IO.Credentials credentials, Format.DataFileBase source, IO.Tasks.DestinationTable destination)
        {
            // Intercept scidrive URIs and modify credentials

            if (SciDriveClient.IsUriSciDrive(uri))
            {
                credentials = credentials ?? new IO.Credentials();
                SciDriveClient.SetAuthenticationHeaders(credentials);
            }

            return base.CreateParameters(federation, uri, credentials, source, destination);
        }
    }
}
