// Copyright 2016 door Remco Veurink en Erik de Roos
using ILiturgieDatabase;
using mppt.Connect;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mppt.RegelVerwerking
{
    abstract class VerwerkBase
    {
        protected IMppApplication _applicatie { get; }

        public VerwerkBase(IMppApplication metApplicatie)
        {
            _applicatie = metApplicatie;
        }

        /// Een 'volgende' tekst is alleen relevant om te tonen op de laatste pagina binnen een item voordat 
        /// een nieuw item komt.
        /// Je kunt er echter ook voor kiezen dat een volgende item gewoon niet aangekondigd wordt. Dat gaat
        /// via 'TonenInVolgende'.
        protected static bool IsLaatsteSlide(IEnumerable<string> tekstOmTeRenderen, string huidigeTekst, ILiturgieRegel regel, ILiturgieContent deel)
        {
            return tekstOmTeRenderen.Last() == huidigeTekst && regel.Content.Last() == deel;
        }

        /// <summary>
        /// Open een presentatie op het meegegeven pad
        /// </summary>
        /// <param name="path">het pad waar de powerpointpresentatie kan worden gevonden</param>
        /// <returns>de powerpoint presentatie</returns>
        protected IMppPresentatie OpenPps(string path)
        {
            //controleer voor het openen van de presentatie op het meegegeven path of de presentatie bestaat
            return File.Exists(path) ? _applicatie.Open(path, metWindow: false) : null;
        }
    }
}
