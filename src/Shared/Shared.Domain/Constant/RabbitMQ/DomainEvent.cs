namespace Shared.Domain.Constant.RabbitMQ
{
    public static class DomainEventTypes
    {
        public const string Empty = "";
        public const string SystemRecover = "sys.recover.event";
        public const string DomainDataModified = "idea.content.modified.event";
        public const string CollectionModified = "collection.modified.event";
        public const string TagModified = "tag.modified.event";
        public const string RevisePass = "revising.pass.event";
        public const string ReviseFail = "revising.fail.event";
        public const string ReviseReschedule = "revising.reschedule.event";

        public static readonly HashSet<string> All = new HashSet<string>()
        {
            SystemRecover,
            DomainDataModified,
            CollectionModified,
            TagModified,
            RevisePass,
            ReviseFail,
            ReviseReschedule
        };
    }
}
