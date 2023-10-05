using System;
using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;
// ReSharper disable StringLiteralTypo

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_Query
{
    [Test]
    public static void Test_Create_Query()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        //
        var query1  = store.Query<Position>();
        var query2  = store.Query<Position, Rotation>();
        var query3  = store.Query<Position, Rotation, Scale3>();
        var query4  = store.Query<Position, Rotation, Scale3, EntityName>();
        var query5  = store.Query<Position, Rotation, Scale3, EntityName, MyComponent1>();
        var query6  = store.Query<Position, Rotation, Scale3, EntityName, MyComponent1, MyComponent2>();
        
        AreSame(query1, store.Query<Position>());
        AreSame(query2, store.Query<Position, Rotation>());
        AreSame(query3, store.Query<Position, Rotation, Scale3>());
        AreSame(query4, store.Query<Position, Rotation, Scale3, EntityName>());
        AreSame(query5, store.Query<Position, Rotation, Scale3, EntityName, MyComponent1>());
        AreSame(query6, store.Query<Position, Rotation, Scale3, EntityName, MyComponent1, MyComponent2>());
        
        AreEqual(0, query1.Archetypes.Length);
        AreEqual(0, query2.Archetypes.Length);
        AreEqual(0, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        AreEqual(0, query6.Archetypes.Length);
        
        entity.AddComponent<Position>();
        AreEqual(1, query1.Archetypes.Length);
        AreEqual(0, query2.Archetypes.Length);
        AreEqual(0, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        AreEqual(0, query6.Archetypes.Length);
        
        entity.AddComponent<Rotation>();
        AreEqual(2, query1.Archetypes.Length);
        AreEqual(1, query2.Archetypes.Length);
        AreEqual(0, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        AreEqual(0, query6.Archetypes.Length);
        
        entity.AddComponent<Scale3>();
        AreEqual(3, query1.Archetypes.Length);
        AreEqual(2, query2.Archetypes.Length);
        AreEqual(1, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        AreEqual(0, query6.Archetypes.Length);
        
        entity.AddComponent<EntityName>();
        AreEqual(4, query1.Archetypes.Length);
        AreEqual(3, query2.Archetypes.Length);
        AreEqual(2, query3.Archetypes.Length);
        AreEqual(1, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        AreEqual(0, query6.Archetypes.Length);
        
        entity.AddComponent<MyComponent1>();
        AreEqual(5, query1.Archetypes.Length);
        AreEqual(4, query2.Archetypes.Length);
        AreEqual(3, query3.Archetypes.Length);
        AreEqual(2, query4.Archetypes.Length);
        AreEqual(1, query5.Archetypes.Length);
        AreEqual(0, query6.Archetypes.Length);
        
        entity.AddComponent<MyComponent2>();
        AreEqual(6, query1.Archetypes.Length);
        AreEqual(5, query2.Archetypes.Length);
        AreEqual(4, query3.Archetypes.Length);
        AreEqual(3, query4.Archetypes.Length);
        AreEqual(2, query5.Archetypes.Length);
        AreEqual(1, query6.Archetypes.Length);
    }
    
    [Test]
    public static void Test_Signature_Query()
    {
        var store   = new EntityStore();
        var entity  = store.CreateEntity();
        
        var sig1    = Signature.Get<Position>();
        var sig2    = Signature.Get<Position, Rotation>();
        var sig3    = Signature.Get<Position, Rotation, Scale3>();
        var sig4    = Signature.Get<Position, Rotation, Scale3, MyComponent1>();
        var sig5    = Signature.Get<Position, Rotation, Scale3, MyComponent1, MyComponent2>();
        //
        var query1  = store.Query(sig1);
        var query2  = store.Query(sig2);
        var query3  = store.Query(sig3);
        var query4  = store.Query(sig4);
        var query5  = store.Query(sig5);
        
        AreSame(query1, store.Query(sig1));
        AreSame(query2, store.Query(sig2));
        AreSame(query3, store.Query(sig3));
        AreSame(query4, store.Query(sig4));
        AreSame(query5, store.Query(sig5));
        
        AreEqual(0, query1.Archetypes.Length);
        AreEqual(0, query2.Archetypes.Length);
        AreEqual(0, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<Position>();
        AreEqual(1, query1.Archetypes.Length);
        AreEqual(0, query2.Archetypes.Length);
        AreEqual(0, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<Rotation>();
        AreEqual(2, query1.Archetypes.Length);
        AreEqual(1, query2.Archetypes.Length);
        AreEqual(0, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<Scale3>();
        AreEqual(3, query1.Archetypes.Length);
        AreEqual(2, query2.Archetypes.Length);
        AreEqual(1, query3.Archetypes.Length);
        AreEqual(0, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<MyComponent1>();
        AreEqual(4, query1.Archetypes.Length);
        AreEqual(3, query2.Archetypes.Length);
        AreEqual(2, query3.Archetypes.Length);
        AreEqual(1, query4.Archetypes.Length);
        AreEqual(0, query5.Archetypes.Length);
        
        entity.AddComponent<MyComponent2>();
        AreEqual(5, query1.Archetypes.Length);
        AreEqual(4, query2.Archetypes.Length);
        AreEqual(3, query3.Archetypes.Length);
        AreEqual(2, query4.Archetypes.Length);
        AreEqual(1, query5.Archetypes.Length);
    }

    [Test]
    public static void Test_ArchetypeMask()
    {
        {
            var mask = new ArchetypeMask(Array.Empty<int>());
            AreEqual("0000000000000000", mask.ToString());
        } {
            var mask = new ArchetypeMask(new [] { 0 });
            AreEqual("0000000000000001", mask.ToString());
        } {
            var mask = new ArchetypeMask(new [] { 0, 64, 128, 192 });
            AreEqual("0000000000000001 0000000000000001 0000000000000001 0000000000000001", mask.ToString());
        } {
            var mask = new ArchetypeMask(new [] { 63, 127, 191, 255 });
            AreEqual("8000000000000000 8000000000000000 8000000000000000 8000000000000000", mask.ToString());
        } {
            var mask = new ArchetypeMask(1);
            for (int n = 0; n < 256; n++) {
                mask.SetBit(n);
            }
            AreEqual("ffffffffffffffff ffffffffffffffff ffffffffffffffff ffffffffffffffff", mask.ToString());
        }
    }
}

