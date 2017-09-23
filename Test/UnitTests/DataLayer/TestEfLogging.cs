﻿// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Collections.Generic;
using DataLayer.EfCode;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayer
{
    public class TestEfLogging
    {
        private readonly ITestOutputHelper _output;

        public TestEfLogging(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestEfCoreLogging1()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                //ATTEMPT
                var logs = context.SetupLogging();
                context.Database.EnsureCreated();

                //VERIFY
                foreach (var log in logs)
                {
                    _output.WriteLine(log.ToString());
                }
                logs.Count.ShouldBeInRange(11, 20);
            }
        }

        [Fact]
        public void TestEfCoreLogging2()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                //ATTEMPT
                var logs = context.SetupLogging();
                context.Database.EnsureCreated();

                //VERIFY
                logs.Count.ShouldEqual(11);
            }
        }

        [Fact]
        public void TestEfCoreLoggingWithMutipleDbContexts()
        {
            //SETUP
            List<LogOutput> logs1;
            var options1 = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options1))
            {
                //ATTEMPT
                logs1 = context.SetupLogging();
                context.Database.EnsureCreated();
            }
            var logs1Count = logs1.Count;
            var options2 = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options2))
            {
                //ATTEMPT
                var logs = context.SetupLogging();
                context.Database.EnsureCreated();

                //VERIFY
                logs.Count.ShouldBeInRange(1, 100);
                logs1.Count.ShouldNotEqual(logs1Count); //The second DbContext methods are also logged to the first logger
            }
        }
    }
}