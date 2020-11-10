namespace Emporos.Core.Entities
{
    public abstract class BaseValueObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}