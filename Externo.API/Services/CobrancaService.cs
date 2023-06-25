using Externo.API.ViewModels;

namespace Externo.API.Services
{
    public interface ICobrancaService
    {
        public CobrancaViewModel AdicionarCobrancaNaLista(CobrancaNovaViewModel Cobranca);
        public CobrancaViewModel RegistrarCobranca(CobrancaNovaViewModel Cobranca, CartaoViewModel cartao);
        public CartaoViewModel GetCartao(int ciclistaId);
        public bool ValidateCreditCardNumber(string cardNumber);
    }

    public class CobrancaService : ICobrancaService
    {
        private static readonly Queue<CobrancaViewModel> FilaCobrancas = new();

        private static readonly Dictionary<int, CobrancaViewModel> DicionarioCobrancas = new();


        public CobrancaViewModel AdicionarCobrancaNaLista(CobrancaNovaViewModel Cobranca)
        {
            var result = new CobrancaViewModel()
            {
                Id = FilaCobrancas.Count,
                Status = "nova",
                HoraSolicitacao = "agora",
                HoraFinalizacao = "depois",
                Valor = Cobranca.Valor,
                Ciclista = Cobranca.Ciclista
            };

            FilaCobrancas.Enqueue(result);

            return result;
        }

        public CobrancaViewModel RegistrarCobranca(CobrancaNovaViewModel Cobranca, CartaoViewModel cartao) {

            var result = new CobrancaViewModel()
            {
                Id = DicionarioCobrancas.Count,
                Status = "nova",
                HoraSolicitacao = "agora",
                HoraFinalizacao = "depois",
                Valor = Cobranca.Valor,
                Ciclista = Cobranca.Ciclista,
                Cartao = cartao
            };

            DicionarioCobrancas.Add(DicionarioCobrancas.Count, result);

            return result;
        }

        public CartaoViewModel GetCartao(int ciclistaId) {
            return new CartaoViewModel(){
                NomeTitular = "aroldo",
                Numero = "22445818436",
                Validade = "2023-06-25",
                CVV = "9905"
            };
        }

        public bool ValidateCreditCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;
            System.Collections.Generic.IEnumerable<char> rev = cardNumber.Reverse();
            int sum = 0, i = 0;
            foreach (char c in rev)
            {
                if (c < '0' || c > '9')
                    return false;
                int tmp = c - '0';
                if ((i & 1) != 0)
                {
                    tmp <<= 1;
                    if (tmp > 9)
                        tmp -= 9;
                }
                sum += tmp;
                i++;
            }
            return ((sum % 10) == 0);
        }
    }
}
