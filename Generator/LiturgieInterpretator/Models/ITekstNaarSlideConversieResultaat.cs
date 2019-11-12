// Copyright 2019 door Erik de Roos
using Generator.Database.Models;

namespace Generator.LiturgieInterpretator.Models
{
    /// <summary>
    /// Resultaat van omzetten basistekst naar slide inhoud
    /// </summary>
    public interface ITekstNaarSlideConversieResultaat
    {
        ILiturgieTekstObject InputTekst { get; }
        ISlideOpbouw ResultaatSlide { get; }
        DatabaseZoekStatus ResultaatStatus { get; }
    }
}
