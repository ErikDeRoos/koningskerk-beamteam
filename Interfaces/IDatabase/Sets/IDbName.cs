// Copyright 2019 door Erik de Roos

namespace IDatabase
{
    public interface IDbName
    {
        /// <summary>
        /// Naam van de dataset zoals deze voorkomt op de server (meestal directory naam, case sensitive)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// De veilige versie van Naam waarin spaties vervangen zijn en alles lowercase is.
        /// </summary>
        string SafeName { get; }
    }
}
