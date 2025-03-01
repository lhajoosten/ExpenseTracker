namespace ExpenseTracker.Common.Entities
{
    public abstract class AuditableEntityBase : EntityBase
    {
        public DateTime CreatedOn { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedBy { get; set; }
    }
}
