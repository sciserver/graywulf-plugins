﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jhu.Graywulf.Components;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.Web.Security
{
    [TestClass]
    public class KeystoneIdentityProviderTest
    {
        protected void PurgeTestEntities(Jhu.Graywulf.Keystone.KeystoneClient client)
        {
            var domains = client.ListDomains();
            for (int i = 0; i < domains.Length; i++)
            {
                if (domains[i].Name.StartsWith("test"))
                {
                    client.Delete(domains[i]);
                }
            }

            var projects = client.ListProjects();
            for (int i = 0; i < projects.Length; i++)
            {
                if (projects[i].Name.StartsWith("test"))
                {
                    client.Delete(projects[i]);
                }
            }

            var roles = client.ListRoles();
            for (int i = 0; i < roles.Length; i++)
            {
                if (roles[i].Name.StartsWith("test"))
                {
                    client.Delete(roles[i]);
                }
            }

            var groups = client.ListGroups();
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i].Name.StartsWith("test"))
                {
                    client.Delete(groups[i]);
                }
            }

            var users = client.FindUsers(null, "test*", false, false);
            for (int i = 0; i < users.Length; i++)
            {
                client.Delete(users[i]);
            }
        }

        [TestMethod]
        public void ManipulateUserTest()
        {
            using (var context = ContextManager.Instance.CreateContext(ConnectionMode.AutoOpen, TransactionMode.ManualCommit))
            {
                var ip = new KeystoneIdentityProvider(context.Domain);

                // TODO: move these to a better location
                ip.Settings.KeystoneDomainID = "default";
                ip.Settings.KeystoneBaseUri = new Uri("http://192.168.170.50:5000");
                ip.Settings.KeystoneAdminToken = "e5b19f25f5d55a995a16";

                PurgeTestEntities(ip.KeystoneClient);

                // Create a new user and set password
                var user = new User(context.Domain);

                user.Name = "testuser";
                user.Email = "testuser@graywulf.org";

                ip.CreateUser(user);
                ip.ResetPassword(user, "alma");

                ip.ChangePassword(user, "alma", "alma2");

                Assert.IsTrue(user.DeploymentState == DeploymentState.New);

                ip.ActivateUser(user);

                Assert.IsTrue(user.DeploymentState == DeploymentState.Deployed);

                ip.DeactivateUser(user);

                Assert.IsTrue(user.DeploymentState == DeploymentState.Undeployed);

                ip.DeleteUser(user);
            }
        }

        [TestMethod]
        public void SerializeSettingsTest()
        {
            var p = new Parameter();

            p.Value = new KeystoneIdentityProviderSettings();

            var xml = p.XmlValue;
        }
    }
}
