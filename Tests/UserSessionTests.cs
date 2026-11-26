using System;
using System.Threading;

#if NUNIT
using NUnit.Framework;
using TestClassAttribute = NUnit.Framework.TestFixtureAttribute;
using TestMethodAttribute = NUnit.Framework.TestCaseAttribute;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

using Ooui.AspNetCore;

namespace Tests
{
    [TestClass]
    public class UserSessionTests
    {
        sealed class DisposableValue : IDisposable
        {
            public int DisposeCount;

            public void Dispose () => DisposeCount++;
        }

        static OouiUserSessions CreateStore (TimeSpan idleTimeout, TimeSpan sweepInterval)
        {
            return new OouiUserSessions (new OouiUserSessionOptions {
                IdleTimeout = idleTimeout,
                SweepInterval = sweepInterval,
            });
        }

        [TestMethod]
        public void NewSessionGetsAnIdThatCanBeReused ()
        {
            var store = CreateStore (TimeSpan.FromMinutes (30), TimeSpan.FromMinutes (1));

            var session = store.GetOrCreate (null);

            Assert.AreEqual (32, session.Id.Length);
            Assert.AreEqual (1, store.Count);

            session.Set ("state", new DisposableValue ());
            var again = store.GetOrCreate (session.Id);

            Assert.AreSame (session, again);
            Assert.AreEqual (1, store.Count);
            Assert.IsTrue (again.TryGetValue<DisposableValue> ("state", out var stored));
            Assert.IsNotNull (stored);
        }

        [TestMethod]
        public void UnknownIdStartsAFreshSession ()
        {
            var store = CreateStore (TimeSpan.FromMinutes (30), TimeSpan.FromMinutes (1));

            var session = store.GetOrCreate ("does-not-exist");

            Assert.AreNotEqual ("does-not-exist", session.Id);
            Assert.AreEqual (0, session.Count);
            Assert.AreEqual (1, store.Count);
        }

        [TestMethod]
        public void IdleSessionIsReplacedByTheNextRequest ()
        {
            var store = CreateStore (TimeSpan.FromMilliseconds (20), TimeSpan.Zero);

            var session = store.GetOrCreate (null);
            session.Set ("state", new DisposableValue ());
            Thread.Sleep (80);

            var next = store.GetOrCreate (session.Id);

            Assert.AreNotEqual (session.Id, next.Id);
            Assert.AreEqual (0, next.Count);
            Assert.AreEqual (1, store.Count);
        }

        [TestMethod]
        public void ClearAndRemoveDisposeTheStoredValues ()
        {
            var store = CreateStore (TimeSpan.FromMinutes (30), TimeSpan.FromMinutes (1));
            var session = store.GetOrCreate (null);
            var cleared = new DisposableValue ();
            var removed = new DisposableValue ();
            session.Set ("a", cleared);
            session.Set ("b", removed);

            Assert.IsTrue (session.Remove ("b"));
            Assert.IsFalse (session.Remove ("b"));
            Assert.AreEqual (1, removed.DisposeCount);
            Assert.AreEqual (0, cleared.DisposeCount);

            session.Clear ();

            Assert.AreEqual (1, cleared.DisposeCount);
            Assert.AreEqual (0, session.Count);

            session.Set ("c", new DisposableValue ());
            Assert.IsTrue (store.Remove (session.Id));
            Assert.AreEqual (0, store.Count);
        }
    }
}
