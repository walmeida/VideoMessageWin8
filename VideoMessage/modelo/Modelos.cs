using System.Runtime.Serialization;

namespace modelo
{
    public class TodoItem
    {
        public int Id { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "complete")]
        public bool Complete { get; set; }

        [DataMember(Name = "destinatario")]
        public string Destinatario { get; set; }
    }

    public class Channel
    {
        public int Id { get; set; }

        [DataMember(Name = "ident")]
        public string Ident { get; set; }

        [DataMember(Name = "uri")]
        public string Uri { get; set; }

    }
}

