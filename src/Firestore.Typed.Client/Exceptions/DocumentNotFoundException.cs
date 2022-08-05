using System;

namespace Firestore.Typed.Client.Exceptions
{
    public class DocumentNotFoundException : Exception
    {
        internal DocumentNotFoundException(string documentId) : base($"Document with id: {documentId} does not exist")
        {
        }
    }
}