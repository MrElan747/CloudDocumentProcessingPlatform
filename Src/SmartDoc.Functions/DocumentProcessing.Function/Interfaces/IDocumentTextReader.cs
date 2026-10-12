using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentProcessing.Function.Interfaces
{
    public interface IDocumentTextReader
    {
        public Task<string> ExtractAsync(Stream documentStream, string fileName, CancellationToken cancellationToken = default);
    }
}
