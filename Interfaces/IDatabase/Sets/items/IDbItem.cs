// Copyright 2016 door Erik de Roos

namespace IDatabase
{
    public interface IDbItem
    {
        /// <summary>
        /// Echte naam van het item binnen de set. Bij subs is dit de naam 
        /// van de afzonderlijke items.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// De veilige versie van Naam waarin spaties vervangen zijn en alles lowercase is.
        /// </summary>
        string SafeName { get; }
        
        /// <summary>
        /// Content
        /// </summary>
        IDbItemContent Content { get; }
    }
}
