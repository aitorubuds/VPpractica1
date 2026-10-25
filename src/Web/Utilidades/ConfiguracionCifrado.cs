namespace www.Utilidades;

// Clave AES con la que se cifran los datos sensibles (se inyecta en las páginas que la necesitan)
public record ConfiguracionCifrado(string Clave);
