using Externo.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Externo.Tests.MockData
{
    public class EmailFake
    {
        public static EmailInsertViewModel GetEmailFake() {
            return new EmailInsertViewModel
            {
                Email = "meuEmail@email.com",
                Assunto = "Assunto Interessante",
                Mensagem = "Olá"
            };
        }
    }
}
