namespace PhotoFilter.Job
{
    public class Image
    {
        public string Id { get; set; }
        public bool IsPhoto { get; set; }
        public string BlobName { get; set; }
        public string LeaseId { get; set; }
        public string Email { get; set; }
    }
}
