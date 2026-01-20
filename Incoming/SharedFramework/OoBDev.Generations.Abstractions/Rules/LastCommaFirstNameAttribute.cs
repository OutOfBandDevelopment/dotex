namespace OoBDev.Generations.Rules
{
    public class LastCommaFirstNameAttribute : FirstSpaceLastNameAttribute
    {
        public LastCommaFirstNameAttribute()
        {
            WordSeperator = ", ";
            Priority--;
        }
        public override object TypeId => this;
    }
}
