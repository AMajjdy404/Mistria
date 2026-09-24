using GateOfEgypt.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateOfEgypt.Domain.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(Email email);
    }
}
