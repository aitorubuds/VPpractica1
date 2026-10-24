namespace www.Utilidades
{
    public static class ClavesSesion
    {
        // No es necesario registrar ClavesSesion mediante inyección
        // de dependencias porque solo contiene constantes.
        public const string EmailPendiente =
            "Login.EmailPendiente";

        public const string UsuarioId =
            "Usuario.Id";

        public const string ProyectoActualId =
            "ProyectoActual.Id";
    }
}
