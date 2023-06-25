namespace Externo.API.ViewModels
{
    public class EmailViewModel
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Assunto { get; set; }
        public string? Mensagem { get; set; }
    }

    public class EmailInsertViewModel
    {
        public string? Email { get; set; }
        public string? Assunto { get; set; }
        public string? Mensagem { get; set; }
    }
}
