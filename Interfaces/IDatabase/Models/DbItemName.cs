// Copyright 2019 door Erik de Roos

namespace IDatabase
{
    public class DbItemName
    {
        /// <summary>
        /// Naam van de dataset zoals deze voorkomt op de server (meestal directory naam, case sensitive)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// De veilige versie van Naam waarin spaties vervangen zijn en alles lowercase is.
        /// </summary>
        public string SafeName { get; set;  }
    }
}
