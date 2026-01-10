namespace OoBDev.Generations.Tests.TestTargets
{
    public class TargetFunctionWithService
    {
        private readonly ITargetInterface _service;

        public TargetFunctionWithService(
            ITargetInterface service
            )
        {
            _service = service;
        }

        public ModelWithProperties2 Action1(string a, int b) => _service.DoWork(a, b);
    }
}
