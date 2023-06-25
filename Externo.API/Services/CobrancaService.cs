using Externo.API.ViewModels;

namespace Externo.API.Services
{
    public interface ICobrancaService
    {
        public Cobranca AdicionarCobrancaNaLista(CobrancaNova Cobranca);
    }

    public class CobrancaService : ICobrancaService
    {
        private static readonly Queue<Cobranca> FilaCobrancas = new();
        public Cobranca AdicionarCobrancaNaLista(CobrancaNova Cobranca)
        {
            var result = new Cobranca()
            {
                Id = 0,
                Status = "nova",
                HoraSolicitacao = "agora",
                HoraFinalizacao = "depois",
                Valor = Cobranca.Valor,
                Ciclista = Cobranca.Ciclista
            };

            FilaCobrancas.Enqueue(result);

            return result;
        }
    }
}
