namespace Common.Model
{

    /// <summary>
    /// Depricated
    /// Пост связан с поездкой, но пока не стоит трогать, чтобы не усложнять логику
    /// </summary>
    public class Post
    {
        public Guid Id { get; set; }
        public Trip? Trip { get; set; }
        public VipStatus Status { get; set; }
    }

    public enum VipStatus
    {
        Requested,
        Vip,
        Regular
    }
}
