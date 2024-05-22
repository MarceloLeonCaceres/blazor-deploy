namespace DataAccessLibrary.QueriesStrategy
{
    internal class FactoryDispositivoQueries
    {
        public ITipoDispositivos Factory(string type)
        {
            switch (type)
            {
                case "ambos":
                    return new AmbosDispositivos();
                case "push":
                    return new DispositivosPush();
                default:
                    return new DispositivosZK();
            }
        }
    }
}
