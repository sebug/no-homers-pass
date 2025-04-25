namespace Sebug.Function.Models;

public record EventTicket(List<Field> headerFields,
    List<Field> primaryFields,
    List<Field> secondaryFields,
    List<Field> auxiliaryFields,
    List<Field> backFields)
{

}