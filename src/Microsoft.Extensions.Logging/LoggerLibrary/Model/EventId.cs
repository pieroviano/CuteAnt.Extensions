namespace Microsoft.Extensions.Logging
{
    public struct EventId
    {
        private int _id;
        private string? _name;

        public EventId(int id, string? name = null)
        {
            this._id = id;
            this._name = name;
        }

        public int Id => this._id;

        public string? Name => this._name;

        public static implicit operator EventId(int i) => new EventId(i);
    }
}
