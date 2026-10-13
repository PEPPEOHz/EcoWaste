namespace EcoWaste.Api.Exceptions;

/// <summary>Recurso não encontrado (gera 404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Regra de negócio violada (gera 400/422).</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
