namespace MSBLOC.Core.Model.GitHub
{
    public class UserRepository
    {
        public long OwnerId { get; set; }

        public string OwnerNodeId { get; set; }

        public string Owner { get; set; }

        public long Id { get; set; }

        public string NodeId { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public AccountType OwnerType { get; set; }
    }
}