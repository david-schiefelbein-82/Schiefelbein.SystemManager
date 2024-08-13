namespace Schiefelbein.SystemManager.Models
{
#if false
    public class ServiceOperationResult
    {
        public string Name { get; set; }

        public bool Success { get; set; }

        public string Error { get; set; }

        public ServiceOperationResult(string name, bool success, string error)
        {
            Name = name;
            Success = success;
            Error = error;
        }

        public override string ToString()
        {
            return string.Format("{{ name: {0}, success: {1}, error: {2} }}", Name, Success, Error);
        }
    }
#endif
}
