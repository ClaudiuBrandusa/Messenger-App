using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Services
{
    public interface IUserRepository
    {
        string GetIdByUsername(string username);
    }
}
