﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Diagnostics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using Tests.Utils;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.ECS.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_SystemRoot
    {
        [Test]
        public static void Test_SystemRoot_Tick() {
            Tick tick = 42;
            AreEqual("deltaTime: 42", tick.ToString());
        }
            
        [Test]
        public static void Test_SystemRoot_Add_System_minimal()
        {
            var store   = new EntityStore();
            var entity  = store.CreateEntity(new Position());
            var root    = new SystemRoot(store);
            root.AddSystem(new TestSystem1());
            root.Update(42); 
            AreEqual(new Position(1,0,0), entity.Position);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_Group()
        {
            var store       = new EntityStore();
            var root        = new SystemRoot("Systems");
            var group1      = new SystemGroup("Group1");
            var testGroup   = new TestGroup();
            
            IsNull(root.FindGroup("Group1",     true));
            IsNull(root.FindGroup("TestGroup",  true));
            
            
            root.AddSystem(group1);
            group1.AddSystem(testGroup);
            
            AreEqual("'Systems' Root - child systems: 1", root.ToString());
            AreEqual("'Group1' Group - child systems: 1", group1.ToString());
            AreEqual("'TestGroup' Group - child systems: 0", testGroup.ToString());
            
            AreSame(group1,     root.FindGroup("Group1",    true));
            AreSame(group1,     root.FindGroup("Group1",    false));
            AreSame(testGroup,  root.FindGroup("TestGroup", true));
            IsNull (            root.FindGroup("TestGroup", false));
            
            AreEqual(1, root.ChildSystems.Count);
            AreEqual(1, group1.ChildSystems.Count);
            
            root.AddStore(store);
            AreEqual(1, root.Stores.Count);
            
            root.Update(new Tick(42)); // use Tick constructor to ensure its available
            
            AreEqual(1, testGroup.beginCalled);
            AreEqual(1, testGroup.endCalled);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_System()
        {
            var store       = new EntityStore();
            var entity      = store.CreateEntity(new Position(1,2,3));
            var root        = new SystemRoot(store);    // create SystemRoot with store
            var testGroup   = new TestGroup();
            root.AddSystem(testGroup);
            var testSystem1 = new TestSystem1();
            AreEqual("TestSystem1 - [Position]", testSystem1.ToString());
            AreEqual("Components: [Position]", testSystem1.ComponentTypes.ToString());
            AreEqual(0,     testSystem1.Queries.Count);
            testGroup.AddSystem(testSystem1);
            AreEqual(1,     testSystem1.Queries.Count);
            AreEqual(1,     testSystem1.EntityCount);
            AreSame(root,   testSystem1.SystemRoot);
            
            root.Update(42);
            
            AreEqual(new Scale3(4,5,6), entity.Scale3);
            AreEqual(0, testSystem1.Tick.deltaTime);
            AreEqual(0, testGroup.Tick.deltaTime);
            AreEqual(0, root.Tick.deltaTime);
        }
        
        [Test]
        public static void Test_SystemRoot_Add_RemoveStore()
        {
            var store1      = new EntityStore();
            var store2      = new EntityStore();
            store1.CreateEntity(new Position(1,2,3));
            var root        = new SystemRoot("Systems");   // create SystemRoot without store
            var group       = new SystemGroup("Group");
            var testSystem1 = new TestSystem1();
            group.AddSystem(testSystem1);
            root.AddSystem(group);
            
            // --- add store
            AreEqual(0, testSystem1.Queries.Count);
            root.AddStore(store1);                      // add store after system setup
            AreEqual(1, root.Stores.Count);
            AreEqual(1, testSystem1.Queries.Count);
            AreEqual(1, testSystem1.EntityCount);
            root.Update(42);
            
            root.AddStore(store2);                      // add store after system setup
            AreEqual(2, root.Stores.Count);
            AreEqual(2, testSystem1.Queries.Count);
            
            // --- remove store
            root.RemoveStore(store1);                   // remove store after system setup
            AreEqual(1, root.Stores.Count);
            AreEqual(1, testSystem1.Queries.Count);
            
            root.RemoveStore(store2);                   // remove store after system setup
            AreEqual(0, root.Stores.Count);
            AreEqual(0, testSystem1.Queries.Count); 
        }
        
        [Test]
        public static void Test_SystemRoot_Name()
        {
            var group = new SystemGroup("TestGroup");
            AreEqual("TestGroup", group.Name);
            
            group.SetName("changed name");
            AreEqual("changed name", group.Name);
            
            var testSystem1 = new TestSystem1();
            AreEqual("TestSystem1", testSystem1.Name);
            
            var mySystem1 = new MySystem1();
            AreEqual("MySystem1", mySystem1.Name);
            
            var mySystem2 = new MySystem2();
            AreEqual("MySystem2 - custom name", mySystem2.Name);
        }
        
        [Test]
        public static void Test_Systems_BaseSystem_coverage()
        {
            var root            = new SystemRoot("Systems");
            var customSystem    = new MySystem1();
            root.AddSystem(customSystem);
            var store = new EntityStore();
            root.AddStore(store);
            root.RemoveStore(store);
            root.Update(0);
        }
        
        [Test]
        public static void Test_SystemRoot_exceptions()
        {
            var group = new SystemRoot("Systems");
            Throws<ArgumentNullException>(() => {
                group.AddStore(null);
            });
            Throws<ArgumentNullException>(() => {
                group.RemoveStore(null);
            });
        }
        
        [Test]
        public static void Test_SystemRoot_Update_Perf()
        {
            bool perfEnabled    = false;
            int  count          = 10;
                                // 100_000_000 - perfEnabled: false ~ #PC: 3682 ms (overhead of perf conditions ~ 250 ms)
                                // 100_000_000 - perfEnabled: true  ~ #PC: 8128 ms
            var store       = new EntityStore();
            var root        = new SystemRoot("Systems");
            var testSystem2 = new TestSystem2();
            root.AddSystem(testSystem2);
            root.AddStore(store);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            root.SetPerfEnabled(perfEnabled);
            root.Update(default);

            var sw      = new Stopwatch();
            var start   = Mem.GetAllocatedBytes();
            sw.Start();
            for (int n = 0; n < count; n++) {
                root.Update(default);
            }
            Mem.AssertNoAlloc(start);
            Console.WriteLine($"Test_SystemRoot_Update_Perf - count: {count}, duration: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"SystemRoot  - DurationSumMs: {root.Perf.SumMs}");
            Console.WriteLine($"TestSystem2 - DurationSumMs: {testSystem2.Perf.SumMs}");
        }
    }
    

}