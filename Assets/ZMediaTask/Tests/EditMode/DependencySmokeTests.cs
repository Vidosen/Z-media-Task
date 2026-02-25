using NUnit.Framework;
using R3;
using Reflex.Core;
using Reflex.Enums;

namespace ZMediaTask.Tests.EditMode
{
    public class DependencySmokeTests
    {
        [Test]
        public void R3_Subject_EmitsValue()
        {
            using var subject = new Subject<int>();
            var received = 0;
            using var subscription = subject.Subscribe(value => received = value);

            subject.OnNext(42);

            Assert.AreEqual(42, received);
        }

        [Test]
        public void Reflex_Container_ResolvesSingletonByContract()
        {
            var builder = new ContainerBuilder();
            builder.SetName("ZMediaTask.DependencySmokeTests");
            builder.RegisterType(
                typeof(FooService),
                new[] { typeof(IFooService) },
                Lifetime.Singleton,
                Resolution.Lazy);

            using var container = builder.Build();

            var first = container.Resolve<IFooService>();
            var second = container.Resolve<IFooService>();

            Assert.AreSame(first, second);
        }

        private interface IFooService
        {
        }

        private sealed class FooService : IFooService
        {
        }
    }
}
