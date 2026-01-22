using System;

namespace OoBDev.Generations.Tests.TestTargets
{
    public class ExampleCompleted : IncompleteClassBase
    {
        private readonly IProcedualGenerationContext _context;

        public ExampleCompleted(IProcedualGenerationContext context) => _context = context;

        public override int Abstract { get; set; }

        public override string AbstractFunction()
        {
            var context = _context.Provider.CreateContext(
                typeof(IncompleteClassBase).GetMethod(nameof(AbstractFunction)) ?? throw new InvalidOperationException(),
                new object[] { },
                default
,
                _context,
                default);
            var result = context.Provider.Generate(context);
            var output = (string?)result ?? throw new InvalidOperationException();
            return output;
        }

        public override string AbstractFunction2(string input1)
        {
            var context = _context.Provider.CreateContext(
                typeof(IncompleteClassBase).GetMethod(nameof(AbstractFunction2)) ?? throw new InvalidOperationException(),
                new object[] { input1 },
                default
,
                _context,
                default);
            var result = context.Provider.Generate(context);
            var output = (string?)result ?? throw new InvalidOperationException();
            return output;
        }

        public override void AbstractFunction3(string input1, string input2)
        {
            var context = _context.Provider.CreateContext(
                typeof(IncompleteClassBase).GetMethod(nameof(AbstractFunction3)) ?? throw new InvalidOperationException(),
                new object[] { input1, input2 },
                default
,
                _context,
                default);
            var result = context.Provider.Generate(context);
        }
    }
}
