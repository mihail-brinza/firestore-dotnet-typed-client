namespace Firestore.Typed.Client.Exceptions;

public class DocumentNotFoundException : Exception
{
    private readonly string _documentId;

    public DocumentNotFoundException(string documentId) : base($"Document with id: {documentId} does not exist")
    {
        _documentId = documentId;
    }
}