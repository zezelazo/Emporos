using System;

namespace Emporos.Core.Entities
{
    public abstract class AuditableEntity : BaseEntity, IAggregateRoot
    {
        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? DeletedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.Now;

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string DeletedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

}