using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpNet.Infrastructure.Messages;

namespace UpNet.Infrastructure
{
    public interface IExecutor
    {
        public Task ExecuteAll(IEnumerable<Message> messages);
    }
}