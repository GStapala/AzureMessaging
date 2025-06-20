using System.Collections.Generic;

namespace WebApp.BackgroundWorkers;

public interface IMessageReader
{
    IEnumerable<string> PeekLatestMessages();
}