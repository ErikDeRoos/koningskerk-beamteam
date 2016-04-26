// Copyright 2016 door Remco Veurink en Erik de Roos
using mppt.Connect;
using System.IO;

namespace mppt.RegelVerwerking
{
    abstract class VerwerkBase
    {
        protected IMppApplication _applicatie { get; }

        public VerwerkBase(IMppApplication metApplicatie)
        {
            _applicatie = metApplicatie;
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
