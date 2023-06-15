namespace Externo.API.ViewModels 
{
    public class EmailViewModel {
        public int id { get; set; }
        public string? email { get; set; }
        public string? assunto { get; set; }
        public string? mensagem { get; set; }
    }

    public class EmailInsertViewModel {
        public string? email { get; set; }
        public string? assunto { get; set; }
        public string? mensagem { get; set; }
    }
}
