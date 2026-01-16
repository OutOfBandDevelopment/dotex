using OoBDev.Generations.Rules;
using System;

namespace OoBDev.Generations.Tests.TestTargets
{
    public class TestModelWithDate
    {
        [DateTime(MinimumDateTime = "1/1/2020", MaximumDateTime = "12/31/2030")]
        public DateTimeOffset UploadedDate { get; set; }
    }
}
