// Copyright 2019 door Erik de Roos

namespace IDatabase
{
    public interface IDbItem
    {
        /// <summary>
        /// Naam van het item binnen de set. Bij subs is dit de naam 
        /// van de afzonderlijke items.
        /// </summary>
        DbItemName Name { get; }
        
        /// <summary>
        /// Content
        /// </summary>
        IDbItemContent Content { get; }
    }
}
