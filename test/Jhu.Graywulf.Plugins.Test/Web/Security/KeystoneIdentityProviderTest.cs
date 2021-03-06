﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jhu.Graywulf.Components;
using Jhu.Graywulf.Registry;
using Jhu.Graywulf.Test;

namespace Jhu.Graywulf.Web.Security
{
    [TestClass]
    public class KeystoneIdentityProviderTest : TestClassBase
    {
        protected const string TestPrefix = "__test__";
        protected const string TestPassword = "almafa";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            StartLogger();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            StopLogger();
        }

        protected void PurgeTestEntities(Jhu.Graywulf.Keystone.KeystoneClient client)
        {
            var domains = client.ListDomains();
            for (int i = 0; i < domains.Length; i++)
            {
                if (domains[i].Name.StartsWith(TestPrefix))
                {
                    client.Delete(domains[i]);
                }
            }

            var projects = client.ListProjects();
            for (int i = 0; i < projects.Length; i++)
            {
                if (projects[i].Name.StartsWith(TestPrefix))
                {
                    client.Delete(projects[i]);
                }
            }

            var roles = client.ListRoles();
            for (int i = 0; i < roles.Length; i++)
            {
                if (roles[i].Name.StartsWith(TestPrefix))
                {
                    client.Delete(roles[i]);
                }
            }

            var groups = client.ListGroups();
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i].Name.StartsWith(TestPrefix))
                {
                    client.Delete(groups[i]);
                }
            }

            var users = client.FindUsers(null, TestPrefix + "*", false, false);
            for (int i = 0; i < users.Length; i++)
            {
                if (users[i].Name.StartsWith(TestPrefix))
                {
                    client.Delete(users[i]);
                }
            }
        }

        [TestMethod]
        public void ManipulateUserTest()
        {
            using (var context = ContextManager.Instance.CreateReadWriteContext())
            {
                var ip = new KeystoneIdentityProvider(context.Domain);

                PurgeTestEntities(ip.KeystoneClient);

                // Create a new user and set password
                var user = new User(context.Domain);

                user.Name = TestPrefix + "user";
                user.Email = "testuser@graywulf.org";

                ip.CreateUser(user, "alma");

                Assert.IsTrue(user.DeploymentState == DeploymentState.Undeployed);

                ip.ActivateUser(user);

                Assert.IsTrue(user.DeploymentState == DeploymentState.Deployed);

                user.FirstName = "Modified";
                ip.ModifyUser(user);

                ip.ChangePassword(user, "alma", "alma2");
                
                ip.DeactivateUser(user);

                Assert.IsTrue(user.DeploymentState == DeploymentState.Undeployed);

                ip.DeleteUser(user);
            }
        }
    }
}
