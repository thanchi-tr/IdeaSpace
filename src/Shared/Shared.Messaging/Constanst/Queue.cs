namespace Shared.Messaging.Constanst
{
    public static class Queue
    {
        public static string CollectionChange = "collection.change.queue";
        public static string TageChange = "tag.change.queue";
        public static string Recover = "system.recover.queue";
        public static string IdeaChange = "idea.content.change.queue";
        public static string ExpiredIdeaPass = "expiration.pass.queue";
        public static string ExpiredIdeaFail = "expiration.fail.queue";
        public static string ExpiredIdeaReschedule = "expiration.reschedule.queue";
        public static string PoolInit = "pool.init.queue";
        public static string PoolUpdate = "pool.update.queue";

    }
}
