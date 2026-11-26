namespace NexusPlan.Domain.Exceptions;

/// <summary>El recurso solicitado no existe (HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Se ha violado una regla de negocio (HTTP 409).</summary>
public class BusinessRuleException(string message) : Exception(message);

/// <summary>Los datos de entrada no son válidos (HTTP 400).</summary>
public class ValidationException(string message) : Exception(message);
