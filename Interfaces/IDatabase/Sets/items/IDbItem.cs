// Copyright 2016 door Erik de Roos

namespace IDatabase
{
    public interface IDbItem
    {
        /// <summary>
        /// Naam (lowercase) van het item binnen de set. Bij subs is dit de naam (lowercase) 
        /// van de afzonderlijke items.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Content
        /// </summary>
        IDbItemContent Content { get; }
    }
}
