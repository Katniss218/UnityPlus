using NUnit.Framework;
using UnityEngine.LowLevel;
using UnityPlus.PlayerLoop;
using UnityPlus.PlayerLoop.Phases;

namespace UnityPlus.Tests
{
    public class PlayerLoopManagerTests
    {
        // --- Custom Systems for Scanning --- //

        [PlayerLoopSystem( typeof( Update ) )]
        private class SimpleStaticSystem : IPlayerLoopSystem
        {
            public void Run() { }
        }

        [PlayerLoopSystem( typeof( Update ) )]
        private class SimpleClassSystem : IPlayerLoopSystem
        {
            public void Run() { }
        }

        [PlayerLoopSystem( typeof( UnityEngine.PlayerLoop.Update ) )] // RAW UNITY TYPE
        private class InvalidSystem
        {
            public static void Run() { }
        }

        [Test]
        public void Manager_DiscoversClassSystems()
        {
            var nodes = PlayerLoopManager.ScanAllAssemblies();
            Assert.That( nodes.Contains( typeof( SimpleClassSystem ) ), Is.True );
            Assert.That( nodes.Contains( typeof( SimpleStaticSystem ) ), Is.True );
        }
    }
}