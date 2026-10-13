namespace EcoWaste.Api.Models;

/// <summary>
/// Categorias de resíduos conforme a coleta seletiva (CONAMA 275).
/// </summary>
public enum WasteType
{
    Paper = 1,      // Azul
    Plastic = 2,    // Vermelho
    Glass = 3,      // Verde
    Metal = 4,      // Amarelo
    Organic = 5,    // Marrom
    Electronic = 6, // Laranja
    Hazardous = 7   // Preto / especial
}
